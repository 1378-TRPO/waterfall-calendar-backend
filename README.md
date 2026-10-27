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

Секция `Jwt` в `appsettings.Development.json` (ключ подписи не короче 32 символов) задаёт параметры токена;
пример значений — в `appsettings.Development.example.json`. Без неё приложение не запустится.

Строка подключения лежит в `appsettings.Development.json` (`ConnectionStrings:DefaultConnection`).
Значения по умолчанию совпадают с `docker-compose.yml`. Порт на хосте — 5433, чтобы не конфликтовать
с локальным PostgreSQL на 5432.

Миграции применяются командой (локальный `dotnet-ef`, один раз `dotnet tool restore`):

```bash
cd Calendar
dotnet tool restore
dotnet ef database update --project Calendar.Api
```

## Тестовые пользователи (seed)

| Email | Пароль |
|---|---|
| alice@local | alice123 |
| bob@local | bob123 |
| carol@local | carol123 |

## API (аутентификация)

| Метод и путь | Доступ | Описание |
|---|---|---|
| `POST /api/auth/register` | все | регистрация (`email`, `password` 6–72 символа, `displayName`), возвращает токен; 409, если email занят |
| `POST /api/auth/login` | все | вход (`email`, `password`), возвращает токен; 401 при неверных данных |
| `GET /api/auth/me` | токен | текущий пользователь |
| `GET /api/users` | токен | список пользователей для выбора участников |

Токен передаётся в заголовке `Authorization: Bearer <token>`, живёт 8 часов. Ошибки — `ProblemDetails`.

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

- Реализованы схема БД, seed и аутентификация; события, RSVP и уведомления пока нет.
- Вне scope MVP: email/push/WebSocket-уведомления, интеграции с Google/Yandex Calendar,
  повторяющиеся события, часовые пояса, приватные календари, мобильная верстка (см. SRS, раздел 5).
- Авторизация в MVP мокируется пулом фикстур пользователей (FR-1).
