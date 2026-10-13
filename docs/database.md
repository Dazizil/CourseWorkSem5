# База данных

PostgreSQL 17. Схема создаётся миграциями EF Core
(`backend/src/SocialNetwork.Infrastructure/Persistence/Migrations`), которые применяются
автоматически при старте API (`DbInitializer`).

## ER-диаграмма

```mermaid
erDiagram
    users ||--o{ user_roles : "имеет"
    roles ||--o{ user_roles : "назначена"
    users ||--o{ refresh_tokens : "владеет"
    users ||--o{ posts : "пишет"

    users {
        uuid Id PK
        varchar Email UK
        varchar UserName UK
        varchar PasswordHash
        varchar DisplayName
        varchar Bio
        varchar AvatarUrl
        bool IsBlocked
        timestamptz CreatedAt
        timestamptz UpdatedAt
    }
    roles {
        uuid Id PK
        varchar Name UK
        varchar Description
    }
    user_roles {
        uuid UserId PK, FK
        uuid RoleId PK, FK
        timestamptz AssignedAt
        uuid AssignedById
    }
    refresh_tokens {
        uuid Id PK
        varchar TokenHash UK
        timestamptz ExpiresAt
        timestamptz RevokedAt
        uuid UserId FK
    }
    posts {
        uuid Id PK
        varchar Content
        uuid AuthorId FK
        timestamptz CreatedAt
    }
```

Все внешние ключи — `ON DELETE CASCADE`. Email и имя пользователя хранятся в нижнем регистре.

## Миграции

Команды выполняются из каталога `backend/` (инструмент `dotnet-ef` подключён локально через
`dotnet-tools.json`, перед первым использованием — `dotnet tool restore`):

```bash
# создать миграцию после изменения сущностей/конфигураций
dotnet ef migrations add <Name> -p src/SocialNetwork.Infrastructure -s src/SocialNetwork.Api -o Persistence/Migrations

# применить вручную (обычно не требуется — применяются при старте API)
dotnet ef database update -p src/SocialNetwork.Infrastructure -s src/SocialNetwork.Api

# SQL-скрипт для приложения к отчёту
dotnet ef migrations script -p src/SocialNetwork.Infrastructure -s src/SocialNetwork.Api
```

## Redis

| Ключ | Значение | TTL |
|---|---|---|
| `socialnetwork:user:{id}` | `UserDto` в JSON | 10 мин |
