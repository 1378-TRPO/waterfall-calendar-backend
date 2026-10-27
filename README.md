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

CORS: фронтенд (Vite) обращается к API с другого порта. Разрешённые источники заданы в `appsettings.json`
(`Cors:AllowedOrigins`, по умолчанию `http://localhost:5173` и `http://127.0.0.1:5173`). Если фронтенд запускается
на другом адресе, добавьте его в этот список.

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

## API

| Метод и путь | Доступ | Описание |
|---|---|---|
| `POST /api/auth/register` | все | регистрация (`email`, `password` 6–72 символа, `displayName`), возвращает токен; 409, если email занят |
| `POST /api/auth/login` | все | вход (`email`, `password`), возвращает токен; 401 при неверных данных |
| `GET /api/auth/me` | токен | текущий пользователь |
| `GET /api/users` | токен | список пользователей для выбора участников |

Токен передаётся в заголовке `Authorization: Bearer <token>`, живёт 8 часов. Ошибки — `ProblemDetails`.

### События и встречи

Все эндпоинты требуют токен. Запись видна организатору и участникам. Даты — ISO 8601 в UTC
(`2026-11-01T10:00:00Z`), значения enum'ов в JSON — строки: `type` = `event` / `meeting`,
`rsvpStatus` = `pending` / `accepted` / `declined`.

| Метод и путь | Описание |
|---|---|
| `GET /api/events?from=&to=` | мои записи за период (организатор или участник), `from` и `to` обязательны; отменённые тоже возвращаются (`isCancelled`) |
| `GET /api/events/upcoming?limit=` | ближайшие будущие записи по времени (по умолчанию 10, максимум 50); отменённые и отклонённые мной скрыты |
| `GET /api/events/{id}` | карточка с участниками; статусы RSVP видит организатор, участник — только свой; 403, если нет доступа |
| `POST /api/events` | создать событие или встречу; 201 |
| `PUT /api/events/{id}` | изменить (только организатор); список участников заменяется целиком, ответы оставшихся сохраняются |
| `POST /api/events/{id}/cancel` | отменить (только организатор) |
| `PUT /api/events/{id}/rsvp` | ответить `accepted` или `declined` (только участник) |
| `POST /api/events/{id}/participants/{userId}/ping` | пингнуть участника, создаёт ему уведомление `pinged` (только организатор); 204. 404, если пользователь не участник; 409 для отменённой записи |

Тело `POST`/`PUT`: `title`, `description`, `startAt`, `endAt`, `meetingUrl`, `participantIds`; при создании ещё `type`.

Правила: название не пустое, `endAt` позже `startAt`, `meetingUrl` начинается с `http://` или `https://`.
У события (`event`) нет участников и ссылки; у встречи (`meeting`) нужен хотя бы один участник, организатор в
участники не входит. Отменённую запись нельзя изменить, отменить повторно или ответить на неё (409).
Тип записи после создания не меняется.

### Уведомления

Все эндпоинты требуют токен, пользователь видит только свои уведомления.

| Метод и путь | Описание |
|---|---|
| `GET /api/notifications` | последние 50 уведомлений, новые сверху |
| `GET /api/notifications/unread-count` | число непрочитанных: `{ "count": 3 }` (для колокольчика, фронтенд опрашивает раз в 30 секунд) |
| `POST /api/notifications/{id}/read` | отметить прочитанным; повторный вызов безопасен; 404 для чужого или несуществующего |

Уведомление: `id`, `type` (`invited` / `pinged` / `changed` / `cancelled`), `message`, `isRead`, `createdAt`, `eventId`.

Кому и когда они создаются (организатор о своих действиях уведомлений не получает):

| Тип | Когда | Кому |
|---|---|---|
| `invited` | создана встреча; участник добавлен при изменении | приглашённым участникам |
| `changed` | встреча изменена (поля или состав участников); без реальных изменений не создаётся | участникам, которые остались в списке |
| `cancelled` | встреча отменена | всем участникам |
| `pinged` | организатор пингнул участника | этому участнику |

Убранному из встречи участнику уведомление не создаётся. Уведомления сохраняются в одной транзакции с изменением записи.

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

- Бэкенд реализован: схема БД, seed, аутентификация, события, встречи, RSVP, уведомления и пинг
  (FR-1…FR-10, FR-12, FR-13; FR-11, демо-ссылка, делается на фронтенде).
- Уведомления in-app: фронтенд получает их опросом, без WebSocket; список ограничен последними 50.
- Вне scope MVP: email/push/WebSocket-уведомления, интеграции с Google/Yandex Calendar,
  повторяющиеся события, часовые пояса, приватные календари, мобильная верстка (см. SRS, раздел 5).
- Авторизация в MVP мокируется пулом фикстур пользователей (FR-1).
