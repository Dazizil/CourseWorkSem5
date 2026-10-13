# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Проект

Курсовая работа «Автоматизированная информационная система социальной сети».
Бэкенд — ASP.NET Core 10 + EF Core 10 (Clean Architecture), фронтенд — Next.js 16 (App Router),
PostgreSQL 17, Redis 8, всё запускается через Docker Compose. Пользователь общается на русском;
документация (`README.md`, `docs/`), сообщения об ошибках API и тексты UI — на русском.

При изменении поведения обновляй соответствующий файл в `docs/` (архитектура, роли, API, БД, деплой).

## Команды

```bash
# Весь стек
cp .env.example .env && docker compose up -d --build     # фронт :3000, API :8080 (/scalar)
docker compose up -d postgres redis                       # только инфраструктура для локальной разработки

# Бэкенд (из backend/)
dotnet build
dotnet test
dotnet test --filter "FullyQualifiedName~AuthServiceTests.Login_WrongPassword_Throws"   # один тест
dotnet run --project src/SocialNetwork.Api                # http://localhost:5273
dotnet tool restore                                       # dotnet-ef (локальный инструмент)
dotnet ef migrations add <Name> -p src/SocialNetwork.Infrastructure -s src/SocialNetwork.Api -o Persistence/Migrations

# Фронтенд (из frontend/)
npm run dev        # проксирует /api/* на API_INTERNAL_URL (по умолчанию http://localhost:5273)
npm run lint
npm run build      # обязательно прогонять: cacheComponents ловит ошибки пререндера только на сборке
```

## Архитектура

### Бэкенд (`backend/`, решение `SocialNetwork.slnx`)

Зависимости: `Api → Infrastructure → Application → Domain`; Api также ссылается на Application.

- **Domain** — сущности и `RoleNames` (константы ролей). Класс назван `RoleNames`, а не `Roles`, потому что
  `Roles` конфликтует с пространством имён `SocialNetwork.Application.Roles`.
- **Application** — сервисы по фичам (`Auth/`, `Users/`, `Roles/`, `Posts/`), каждый со своими DTO
  (валидация через DataAnnotations — её выполняет `[ApiController]`). Доступ к БД через `IApplicationDbContext`
  (DbSet'ы, без репозиториев), поэтому Application зависит от пакета `Microsoft.EntityFrameworkCore`.
  Только провайдер-независимый LINQ (никаких `EF.Functions.ILike`) — тесты идут на InMemory-провайдере.
  Ошибки — исключения из `Common/Exceptions` (`NotFoundException` → 404, `ConflictException` → 409,
  `UnauthorizedException` → 401, `ForbiddenException` → 403, `BadRequestException` → 400);
  маппинг в ProblemDetails делает `Api/Infrastructure/AppExceptionHandler`.
  Новые сервисы регистрируются в `Application/DependencyInjection.cs`.
- **Infrastructure** — `ApplicationDbContext` + `IEntityTypeConfiguration` в `Persistence/Configurations`
  (таблицы в snake_case: `users`, `user_roles`, …), миграции в `Persistence/Migrations`,
  `DbInitializer` (применяет миграции, создаёт роли и админа из `SeedAdmin` при старте API),
  `TokenService`/`JwtOptions`, BCrypt, `RedisCacheService` (глотает ошибки Redis — кэш не должен ронять запросы).
- **Api** — тонкие контроллеры, JWT Bearer, `CurrentUserService`.

### Авторизация и роли

- Claims JWT короткие (`sub`, `role`) — см. `Infrastructure/Identity/JwtClaimNames`. В `Program.cs`
  `MapInboundClaims = false`, `NameClaimType = "sub"`, `RoleClaimType = "role"`. Если менять имена claims —
  менять в обоих местах, иначе `[Authorize(Roles = ...)]` молча перестанет работать.
- Refresh-токены хранятся только как SHA-256 хэш, ротируются при каждом `/refresh`.
- Роли — таблицы `roles` + `user_roles`; роли попадают в access-токен при выдаче, так что изменение ролей
  вступает в силу после refresh.
- Права проверяются атрибутами `[Authorize(Roles = $"{RoleNames.Moderator},{RoleNames.Admin}")]`, а правила
  «владелец или модератор» — внутри сервисов через `ICurrentUserService`.

### Кэш

Cache-aside только для `UserService.GetByIdAsync` (ключ `CacheKeys.User(id)`). Любой код, меняющий
пользователя, его роли или блокировку, обязан вызвать `cache.RemoveAsync(CacheKeys.User(id))`.

### Фронтенд (`frontend/`)

- Браузер ходит только на Next.js. `src/app/api/[...path]/route.ts` — серверный прокси на API
  (`API_INTERNAL_URL` читается в рантайме). Не заменять на `rewrites` в `next.config.ts` — они фиксируются при сборке.
- `src/lib/api.ts` — fetch-обёртка с Bearer-токеном и автоматическим refresh при 401;
  `src/lib/auth-context.tsx` — `useAuth()` (`user`, `hasRole`, `login`, `logout`…). Токены в `localStorage`.
- Страницы — клиентские компоненты под `<RequireAuth roles={[...]}>`.
- Включён `cacheComponents`: хуки runtime-данных (`useParams`, `useSearchParams`) должны быть внутри
  `<Suspense>` (пример — `src/app/users/[id]/`), иначе `next build` падает.
- ESLint (`react-hooks/set-state-in-effect`) запрещает вызывать функции, синхронно делающие setState, в теле
  `useEffect`: загружать данные чистой async-функцией и вызывать setState в `.then(...)`.

## Docker

Используются уже скачанные локально образы: `postgres:17.11-alpine3.24`, `redis:8.2.10-alpine3.22`,
`mcr.microsoft.com/dotnet/sdk:10.0`, `mcr.microsoft.com/dotnet/aspnet:10.0`, `adminer:latest`, `node:24-alpine`.
При смене образа проверить `docker images` и обновить `docs/deployment.md`.
`.env` (с `JWT_SECRET`) в git не коммитится; шаблон — `.env.example`.
