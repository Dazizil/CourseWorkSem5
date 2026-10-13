# REST API

Базовый адрес: `http://localhost:8080` (через фронтенд — `http://localhost:3000`).
Интерактивная документация (в окружении Development): `/scalar`, спецификация — `/openapi/v1.json`.

Все эндпоинты, кроме `/api/auth/register|login|refresh` и `/health`, требуют заголовок
`Authorization: Bearer <accessToken>`. Ошибки возвращаются в формате RFC 7807 ProblemDetails.

## Аутентификация — `/api/auth`

| Метод | Путь | Доступ | Тело | Ответ |
|---|---|---|---|---|
| POST | `/register` | все | `{ email, userName, password, displayName? }` | `AuthResponse` |
| POST | `/login` | все | `{ login, password }` (login — email или userName) | `AuthResponse` |
| POST | `/refresh` | все | `{ refreshToken }` | `AuthResponse` |
| POST | `/logout` | авторизованные | `{ refreshToken }` | 204 |

`AuthResponse`: `{ accessToken, accessTokenExpiresAt, refreshToken, refreshTokenExpiresAt, user }`.

## Пользователи — `/api/users`

| Метод | Путь | Доступ | Описание |
|---|---|---|---|
| GET | `/me` | авторизованные | Текущий пользователь |
| PUT | `/me` | авторизованные | Обновить профиль `{ displayName?, bio?, avatarUrl? }` |
| GET | `/{id}` | авторизованные | Профиль пользователя (кэшируется в Redis) |
| GET | `?query=&page=1&pageSize=20` | авторизованные | Поиск по имени |
| POST | `/{id}/block` | Moderator, Admin | Заблокировать |
| POST | `/{id}/unblock` | Moderator, Admin | Разблокировать |

## Роли — `/api/roles` (только Admin)

| Метод | Путь | Описание |
|---|---|---|
| GET | `/` | Список ролей с количеством пользователей |
| POST | `/users/{userId}` | Назначить роль `{ roleName }` |
| DELETE | `/users/{userId}/{roleName}` | Отозвать роль |

## Публикации — `/api/posts`

| Метод | Путь | Доступ | Описание |
|---|---|---|---|
| GET | `?authorId=&page=1&pageSize=20` | авторизованные | Лента (новые сверху) |
| POST | `/` | авторизованные | Создать `{ content }` |
| DELETE | `/{id}` | автор, Moderator, Admin | Удалить |

## Коды ответов

| Код | Когда |
|---|---|
| 400 | Ошибка валидации или бизнес-правила |
| 401 | Нет/истёк токен, неверный логин или пароль |
| 403 | Недостаточно прав, пользователь заблокирован |
| 404 | Объект не найден |
| 409 | Конфликт (email или имя уже заняты, роль уже назначена) |
