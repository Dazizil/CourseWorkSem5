# Развёртывание и конфигурация

## Docker Compose

| Сервис | Образ | Порт (хост) |
|---|---|---|
| `postgres` | `postgres:17.11-alpine3.24` | 5432 |
| `redis` | `redis:8.2.10-alpine3.22` (AOF включён) | 6379 |
| `api` | собирается из `backend/Dockerfile` (`dotnet/sdk:10.0` → `dotnet/aspnet:10.0`) | 8080 |
| `frontend` | собирается из `frontend/Dockerfile` (`node:24-alpine`, Next.js standalone) | 3000 |
| `adminer` | `adminer:latest`, профиль `tools` | 8081 |

Данные хранятся в томах `postgres-data` и `redis-data`. API стартует после healthcheck'ов
PostgreSQL и Redis.

```bash
docker compose up -d --build              # собрать и запустить
docker compose logs -f api                # логи API
docker compose --profile tools up -d      # + Adminer (сервер: postgres)
docker compose down                       # остановить
docker compose down -v                    # остановить и удалить данные
```

## Переменные окружения (`.env`)

| Переменная | По умолчанию | Описание |
|---|---|---|
| `JWT_SECRET` | — (обязательна) | Ключ подписи JWT, ≥ 32 символов (`openssl rand -base64 48`) |
| `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD` | `socialnetwork` | Параметры БД |
| `SEED_ADMIN_EMAIL` / `SEED_ADMIN_PASSWORD` | `admin@socialnetwork.local` / `Admin12345!` | Первый администратор |
| `ASPNETCORE_ENVIRONMENT` | `Development` | В `Development` доступна документация `/scalar` |
| `FRONTEND_PORT`, `API_PORT`, `POSTGRES_PORT`, `REDIS_PORT`, `ADMINER_PORT` | 3000, 8080, 5432, 6379, 8081 | Порты на хосте |

## Конфигурация API

`appsettings.json` → `appsettings.{Environment}.json` → переменные окружения (`Секция__Ключ`).

| Ключ | Описание |
|---|---|
| `ConnectionStrings:Postgres`, `ConnectionStrings:Redis` | Подключения |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Secret`, `Jwt:AccessTokenMinutes`, `Jwt:RefreshTokenDays` | JWT; длина `Secret` проверяется при старте |
| `Cors:Origins` | Разрешённые origin для прямых запросов из браузера |
| `SeedAdmin:Email`, `SeedAdmin:UserName`, `SeedAdmin:Password` | Администратор, создаваемый при первом запуске |

Для локального запуска `appsettings.Development.json` уже содержит dev-секрет и данные администратора.

## Фронтенд

| Переменная | Описание |
|---|---|
| `API_INTERNAL_URL` | Адрес API для серверного прокси `/api/*` (в compose — `http://api:8080`, локально по умолчанию `http://localhost:5273`) |

## Известные сообщения в логах

- `Failed executing DbCommand ... FROM "__EFMigrationsHistory"` при первом запуске — EF Core
  проверяет таблицу истории миграций до её создания. Не ошибка.
- `libgssapi_krb5.so.2: cannot open shared object file` — Npgsql ищет необязательную библиотеку
  Kerberos. На работу не влияет.
