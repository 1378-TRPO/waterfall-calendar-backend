# waterfall-calendar-backend

Бэкенд «Календарика» — учебного веб-календаря для маленькой компании (методология Waterfall).
Требования: `../tec-doc-waterfall/docs/01-srs.md`.

## Стек

C# / .NET 10, ASP.NET Core Web API, EF Core, Npgsql, PostgreSQL 16, BCrypt.Net.

## Запуск

Нужны .NET SDK 10 и Docker.

```bash
# 1. База данных
docker compose up -d db

# 2. Локальные настройки (файл в .gitignore)
cp Calendar/Calendar.Api/appsettings.Development.example.json Calendar/Calendar.Api/appsettings.Development.json

# 3. Backend
cd Calendar
dotnet restore
dotnet run --project Calendar.Api
```

Строка подключения лежит в `appsettings.Development.json` (`ConnectionStrings:DefaultConnection`).
Значения по умолчанию совпадают с `docker-compose.yml`. Порт на хосте — 5433, чтобы не конфликтовать
с локальным PostgreSQL на 5432.

## Структура

```
Calendar/
├── Calendar.slnx
└── Calendar.Api/
    ├── Controllers/
    ├── Models/
    ├── Data/        # AppDbContext
    ├── Services/
    └── Program.cs
```

## Известные ограничения

- Сущности, миграции и контроллеры пока не реализованы — каркас проекта.
- Вне scope MVP: email/push/WebSocket-уведомления, интеграции с Google/Yandex Calendar,
  повторяющиеся события, часовые пояса, приватные календари, мобильная верстка (см. SRS, раздел 5).
- Авторизация в MVP мокируется пулом фикстур пользователей (FR-1).
