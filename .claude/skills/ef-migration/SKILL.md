---
name: ef-migration
description: Изменить модель данных бэкенда и создать EF Core миграцию (новая сущность, поле, индекс, связь). Использовать при любом изменении Domain-сущностей или конфигураций EF.
---

# Изменение схемы БД

1. Изменить сущность в `backend/src/SocialNetwork.Domain/Entities/`.
2. Настроить маппинг в `backend/src/SocialNetwork.Infrastructure/Persistence/Configurations/`
   (таблица в snake_case через `ToTable`, длины строк, индексы, `OnDelete`).
3. Для новой сущности — добавить `DbSet` в `IApplicationDbContext` (Application) и `ApplicationDbContext` (Infrastructure).
4. Из каталога `backend/`:
   ```bash
   dotnet tool restore
   dotnet ef migrations add <PascalCaseName> -p src/SocialNetwork.Infrastructure -s src/SocialNetwork.Api -o Persistence/Migrations
   ```
5. Просмотреть сгенерированную миграцию (нет ли неожиданных `DropColumn`/`DropTable`).
6. `dotnet build && dotnet test`.
7. Миграция применится при старте API (`DbInitializer`). Для Docker: `docker compose up -d --build api`.
8. Обновить ER-диаграмму и описание таблиц в `docs/database.md`.
