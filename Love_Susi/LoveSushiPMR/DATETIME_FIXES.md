# Исправление ошибки DateTime с PostgreSQL

## Проблема
Ошибка при редактировании/добавлении акций и промокодов:
```
ArgumentException: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported.
```

## Причина
PostgreSQL требует, чтобы все `timestamp with time zone` поля имели Kind=Utc. Когда DateTime значения поступают из HTML форм, они парсятся как Kind=Unspecified (неквалифицированное время), что вызывает ошибку.

## Решение

### 1. Исправлены сущности (Entities)
Добавлена инициализация DateTime полей с `DateTime.UtcNow`:

- **Promotion.cs**
  - `StartDate = DateTime.UtcNow`
  - `EndDate = DateTime.UtcNow.AddMonths(1)`

- **PromoCode.cs**
  - `ValidUntil = DateTime.UtcNow.AddMonths(1)`

- **Payment.cs**
  - `PaymentDate = DateTime.UtcNow`

- **BonusAccount.cs**
  - `LastUpdated = DateTime.UtcNow`

### 2. Добавлен helper метод в AdminController

```csharp
private DateTime EnsureUtc(DateTime dateTime)
{
    return dateTime.Kind switch
    {
        DateTimeKind.Utc => dateTime,
        DateTimeKind.Local => dateTime.ToUniversalTime(),
        _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
    };
}
```

### 3. Обновлены методы контроллера

Все методы для создания и редактирования акций и промокодов теперь используют `EnsureUtc()`:

- `CreatePromotion()` - конвертирует StartDate и EndDate
- `EditPromotion()` - конвертирует StartDate и EndDate
- `CreatePromoCode()` - конвертирует ValidUntil
- `EditPromoCode()` - конвертирует ValidUntil

## Результат
✅ Ошибки при рабое с DateTime устранены
✅ Все DateTime значения в PostgreSQL теперь имеют Kind=Utc
✅ Форомы для акций и промокодов работают корректно
