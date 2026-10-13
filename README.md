# Автоматизированная информационная система социальной сети

Курсовая работа: веб-приложение социальной сети с регистрацией, JWT-авторизацией, системой ролей
(User / Moderator / Admin), профилями пользователей и лентой публикаций.

## Технологический стек

| Слой | Технологии |
|---|---|
| Бэкенд | C# / ASP.NET Core 10 (Web API), Entity Framework Core 10, Clean Architecture |
| Фронтенд | Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS 4 |
| База данных | PostgreSQL 17 |
| Кэш | Redis 8 |
| Развёртывание | Docker, Docker Compose |

## Возможности

- Регистрация и вход по email или имени пользователя
- JWT access-токены (15 мин) + refresh-токены (7 дней) с ротацией и отзывом при выходе
- Ролевая модель: **User**, **Moderator**, **Admin**; назначение и отзыв ролей администратором
- Блокировка пользователей модератором/администратором (с отзывом всех сессий)
- Профиль пользователя (имя, описание, аватар), поиск пользователей
- Лента публикаций; удаление своих постов, модераторы удаляют любые
- Кэширование профилей пользователей в Redis
- Документация API (OpenAPI + Scalar UI)

## Быстрый старт (Docker)

```bash
cp .env.example .env          # задайте JWT_SECRET (минимум 32 символа)
docker compose up -d --build
```

| Сервис | Адрес |
|---|---|
| Фронтенд | http://localhost:3000 |
| API | http://localhost:8080 |
| API документация (Scalar) | http://localhost:8080/scalar |
| Adminer (по запросу) | `docker compose --profile tools up -d adminer` → http://localhost:8081 |

Администратор создаётся автоматически при первом запуске:
`admin@socialnetwork.local` / `Admin12345!` (меняется через `SEED_ADMIN_*` в `.env`).

## Локальная разработка

Инфраструктура в Docker, приложения — локально:

```bash
docker compose up -d postgres redis

# Бэкенд (http://localhost:5273, документация — /scalar)
cd backend
dotnet run --project src/SocialNetwork.Api

# Фронтенд (http://localhost:3000), проксирует /api/* на http://localhost:5273
cd frontend
npm install
npm run dev
```

Тесты и проверки:

```bash
cd backend && dotnet test
cd frontend && npm run lint && npm run build
```

## Структура репозитория

```
backend/                 ASP.NET Core решение (SocialNetwork.slnx)
  src/SocialNetwork.Domain          сущности и константы, без зависимостей
  src/SocialNetwork.Application     бизнес-логика, DTO, интерфейсы
  src/SocialNetwork.Infrastructure  EF Core + PostgreSQL, Redis, JWT, BCrypt
  src/SocialNetwork.Api             контроллеры, аутентификация, обработка ошибок
  tests/SocialNetwork.UnitTests     xUnit-тесты
frontend/                Next.js приложение
docs/                    документация проекта
docker-compose.yml       PostgreSQL, Redis, API, фронтенд, Adminer
```

## Документация

- [Архитектура](docs/architecture.md)
- [Авторизация и роли](docs/auth-and-roles.md)
- [REST API](docs/api.md)
- [База данных](docs/database.md)
- [Развёртывание и конфигурация](docs/deployment.md)
