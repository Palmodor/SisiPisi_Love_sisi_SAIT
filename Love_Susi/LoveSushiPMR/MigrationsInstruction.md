# Инструкция по миграциям Entity Framework Core

## Автоматические миграции

В проекте настроены автоматические миграции. При запуске приложения база данных создаётся и обновляется автоматически.

## Ручное создание миграций

Если вам нужно создать новую миграцию после изменения моделей:

1. Откройте Package Manager Console в Visual Studio:
   ```
   Tools → NuGet Package Manager → Package Manager Console
   ```

2. Создайте миграцию:
   ```powershell
   Add-Migration InitialCreate
   ```

3. Примените миграцию:
   ```powershell
   Update-Database
   ```

## Командная строка (CLI)

```bash
# Установить EF Core tools (если не установлены)
dotnet tool install --global dotnet-ef

# Создать миграцию
dotnet ef migrations add InitialCreate

# Применить миграции
dotnet ef database update

# Удалить последнюю миграцию
dotnet ef migrations remove

# Создать SQL скрипт
dotnet ef migrations script
```

## Создание дампа базы данных PostgreSQL

```bash
# Экспорт
pg_dump -U postgres -d lovesushi_db > lovesushi_db_backup.sql

# Импорт
psql -U postgres -d lovesushi_db < lovesushi_db_backup.sql
```
