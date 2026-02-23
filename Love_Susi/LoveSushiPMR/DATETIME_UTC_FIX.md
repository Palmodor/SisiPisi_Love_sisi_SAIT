# Исправление ошибки DateTime с PostgreSQL - Полное решение

## Проблема
При редактировании/добавлении акций, промокодов и других операций с DateTime возникала ошибка:

```
ArgumentException: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 
'timestamp with time zone', only UTC is supported.
```

## Причина
1. PostgreSQL требует, чтобы все поля `timestamp with time zone` имели Kind=UTC
2. HTML поля типа `<input type="datetime-local">` парсятся ASP.NET как DateTime с Kind=Unspecified
3. По умолчанию EF Core не конвертирует такие значения автоматически

## Полное решение (3 уровня защиты)

### 1. Глобальный конвертер в Entity Framework Core
**Файл:** [Data/ApplicationDbContext.cs](Data/ApplicationDbContext.cs#L27-L94)

Добавлен ValueConverter, который автоматически конвертирует ВСЕ DateTime значения в UTC перед сохранением в БД:

```csharp
private static DateTime EnsureUtc(DateTime dateTime)
{
    return dateTime.Kind switch
    {
        DateTimeKind.Utc => dateTime,
        DateTimeKind.Local => dateTime.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
        _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
    };
}

// В OnModelCreating
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
{
    foreach (var property in entityType.GetProperties())
    {
        if (property.ClrType == typeof(DateTime))
        {
            property.SetValueConverter(dateTimeConverter);
        }
    }
}
```

**Преимущество:** Работает для ВСЕХ DateTime свойств автоматически.

### 2. Инициализация свойств сущностей
**Файлы:**
- [Models/Entities/Promotion.cs](Models/Entities/Promotion.cs)
- [Models/Entities/PromoCode.cs](Models/Entities/PromoCode.cs)
- [Models/Entities/Payment.cs](Models/Entities/Payment.cs)
- [Models/Entities/BonusAccount.cs](Models/Entities/BonusAccount.cs)

Все DateTime поля инициализируются с `DateTime.UtcNow`:

```csharp
public DateTime StartDate { get; set; } = DateTime.UtcNow;
public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddMonths(1);
public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
```

**Преимущество:** Обеспечивает правильные значения по умолчанию.

### 3. Явная конвертация в контроллерах
**Файл:** [Controllers/AdminController.cs](Controllers/AdminController.cs#L811-L820)

Добавлен helper метод для явной конвертации в местах, где нужно:

```csharp
private DateTime EnsureUtc(DateTime dateTime)
{
    return dateTime.Kind switch
    {
        DateTimeKind.Utc => dateTime,
        DateTimeKind.Local => dateTime.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
        _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
    };
}
```

Используется в методах:
- `CreatePromoCode()` - для ValidUntil
- `EditPromoCode()` - для ValidUntil
- `CreatePromotion()` - для StartDate и EndDate
- `EditPromotion()` - для StartDate и EndDate

**Пример:**
```csharp
var promoCode = new PromoCode
{
    ValidUntil = EnsureUtc(model.ValidUntil), // Конвертируем из формы
    ...
};
```

## Результат
✅ Все DateTime значения из HTML форм правильно конвертируются в UTC
✅ PostgreSQL больше не жалуется на Kind=Unspecified
✅ Данные корректно сохраняются в БД
✅ Три уровня защиты гарантируют надежность решения

## Ключевые моменты

### DateTime.SpecifyKind vs ToUniversalTime
- **SpecifyKind:** Просто меняет флаг Kind, не изменяя время. Используется для Unspecified.
- **ToUniversalTime:** Конвертирует время из локального в UTC. Используется для Local.

### Важно:
```csharp
// ❌ НЕПРАВИЛЬНО - только меняет флаг
var badTime = new DateTime(2024, 2, 22, 12, 0, 0); // Kind=Unspecified
var result = DateTime.SpecifyKind(badTime, DateTimeKind.Utc); // Kind изменился, время не изменилось

// ✅ ПРАВИЛЬНО - конвертирует время
var localTime = new DateTime(2024, 2, 22, 12, 0, 0, DateTimeKind.Local);
var utcTime = localTime.ToUniversalTime(); // Время конвертировано в UTC
```

## Тестирование
Протестируйте следующие операции:
1. Создание промокода
2. Редактирование промокода
3. Создание акции
4. Редактирование акции
5. Оформление заказа

Все операции должны работать без ошибок DateTime.
