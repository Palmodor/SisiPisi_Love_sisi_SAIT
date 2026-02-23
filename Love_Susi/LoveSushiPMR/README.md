# LoveSushi PMR - Сервис доставки еды

## Описание проекта

MVC веб-приложение на C# ASP.NET Core для сервиса доставки суши и роллов в Приднестровье.

## Технологии

- **Backend**: ASP.NET Core 8.0 MVC
- **База данных**: PostgreSQL
- **ORM**: Entity Framework Core 8.0
- **Frontend**: Bootstrap 5, jQuery
- **Аутентификация**: ASP.NET Core Identity + Cookie

## Структура базы данных

База данных реализована согласно ER-диаграмме и включает следующие сущности:
- **Users** - пользователи системы
- **Roles** - роли (Администратор, Пользователь)
- **Categories** - категории блюд
- **Dishes** - блюда
- **Orders** - заказы
- **OrderItems** - позиции заказа
- **CartItems** - корзина
- **Couriers** - курьеры
- **DeliveryZones** - зоны доставки
- **DeliveryAddresses** - адреса доставки
- **PromoCodes** - промокоды
- **Promotions** - акции
- **Reviews** - отзывы
- **BonusAccounts** - бонусные счета
- **Payments** - платежи

## Установка и запуск

### 1. Предварительные требования

- Visual Studio Community 2022 или VS Code
- .NET 8.0 SDK
- PostgreSQL 15+

### 2. Настройка базы данных

1. Создайте базу данных в PostgreSQL:
```sql
CREATE DATABASE lovesushi_db;
```

2. Обновите строку подключения в `appsettings.json`:
```json
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=lovesushi_db;Username=postgres;Password=ВАШ_ПАРОЛЬ"
}
```

### 3. Миграции

Миграции применяются автоматически при запуске приложения через:
```csharp
// В Program.cs
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}
```

### 4. Запуск

1. Откройте проект в Visual Studio
2. Нажмите F5 или выполните:
```bash
dotnet run
```

Приложение будет доступно по адресу: `https://localhost:7001` или `http://localhost:5001`

## Функционал

### Для клиентов:
- 🍣 Просмотр меню с категориями
- 🔍 Поиск блюд
- 🛒 Корзина с управлением товарами
- 📦 Оформление заказов
- 👤 Личный кабинет
- 💰 Бонусная система
- 📜 История заказов
- 🏷️ Применение промокодов

### Для администраторов:
- 📊 Панель управления (Dashboard)
- 🍽️ Управление блюдами (CRUD)
- 📂 Управление категориями
- 📦 Управление заказами (изменение статусов, назначение курьеров)
- 👥 Просмотр пользователей
- 🚚 Управление курьерами
- 🏷️ Управление промокодами

## Регистрация администратора

При регистрации введите секретный код: **LOVESUSHI2024_ADMIN** для получения прав администратора.

## Тестовые данные

При первом запуске автоматически создаются:
- 2 роли (Администратор, Пользователь)
- 4 зоны доставки (Центр, Спутник, Западный, Северный)
- 8 категорий блюд
- 13 тестовых блюд
- 2 курьера
- 2 акции

## Структура проекта

```
LoveSushiPMR/
├── Controllers/          # Контроллеры
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── CartController.cs
│   ├── HomeController.cs
│   ├── MenuController.cs
│   └── OrderController.cs
├── Data/                 # Контекст БД
│   └── ApplicationDbContext.cs
├── Models/
│   ├── Entities/         # Сущности БД
│   └── ViewModels/       # Модели представлений
├── Views/                # Представления
│   ├── Account/
│   ├── Admin/
│   ├── Cart/
│   ├── Home/
│   ├── Menu/
│   ├── Order/
│   └── Shared/
├── wwwroot/              # Статические файлы
│   ├── css/
│   ├── js/
│   └── images/
├── appsettings.json      # Конфигурация
└── Program.cs            # Точка входа
```

## Автор

Курсовая работа на тему: "Разработка и защита БД для ПО 'Сервис доставки еды'"
