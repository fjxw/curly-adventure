# HR Management System

Микросервисное серверное приложение для кадрового учета на предприятии.

## Архитектура

Система состоит из 3 микросервисов:

### 1. Employees API (Порт 5001)
Сервис управления сотрудниками и организационной структурой:
- Управление сотрудниками (CRUD)
- Управление отделами
- Управление должностями
- Аутентификация и авторизация (JWT + ASP.NET Core Identity)

### 2. Payroll API (Порт 5002)
Сервис управления заработной платой и учетом рабочего времени:
- Штатное расписание
- Табель учета рабочего времени
- Расчет заработной платы
- Нормы труда

### 3. Recruitment API (Порт 5003)
Сервис управления рекрутингом и обучением:
- Управление вакансиями
- Управление кандидатами
- Планирование собеседований
- Управление обучением персонала

## Технологический стек

- **Framework**: ASP.NET Core 8.0 (Minimal API)
- **ORM**: Entity Framework Core с Fluent API
- **Database**: SQLite (для разработки)
- **Authentication**: JWT Bearer + ASP.NET Core Identity
- **Documentation**: Swagger/OpenAPI
- **Caching**: In-Memory Cache
- **Message Broker**: In-Memory Event Bus (для межсервисного взаимодействия)
- **Containerization**: Docker + Docker Compose

## Структура проекта

```
HRManagement/
├── src/
│   ├── Services/
│   │   ├── Employees/
│   │   │   └── HRManagement.Employees.Api/
│   │   ├── Payroll/
│   │   │   └── HRManagement.Payroll.Api/
│   │   └── Recruitment/
│   │       └── HRManagement.Recruitment.Api/
│   └── Shared/
│       ├── HRManagement.Shared.Common/
│       ├── HRManagement.Shared.Contracts/
│       └── HRManagement.Shared.MessageBus/
├── docker-compose.yml
└── HRManagement.slnx
```

## Запуск

### Локальный запуск

```bash
# Сборка всего решения
cd HRManagement
dotnet build HRManagement.slnx

# Запуск каждого сервиса отдельно
cd src/Services/Employees/HRManagement.Employees.Api
dotnet run

# В отдельных терминалах:
cd src/Services/Payroll/HRManagement.Payroll.Api
dotnet run

cd src/Services/Recruitment/HRManagement.Recruitment.Api
dotnet run
```

### Запуск через Docker Compose

```bash
cd HRManagement
docker-compose up --build
```

После запуска сервисы будут доступны:
- Employees API: http://localhost:5001
- Payroll API: http://localhost:5002
- Recruitment API: http://localhost:5003

## API Endpoints

### Employees API

#### Аутентификация
- `POST /api/auth/register` - Регистрация пользователя
- `POST /api/auth/login` - Вход в систему

#### Сотрудники
- `GET /api/employees` - Список сотрудников
- `GET /api/employees/{id}` - Получить сотрудника
- `POST /api/employees` - Создать сотрудника
- `PUT /api/employees/{id}` - Обновить сотрудника
- `DELETE /api/employees/{id}` - Удалить сотрудника

#### Отделы
- `GET /api/departments` - Список отделов
- `POST /api/departments` - Создать отдел

#### Должности
- `GET /api/positions` - Список должностей
- `POST /api/positions` - Создать должность

### Payroll API

#### Штатное расписание
- `GET /api/staffing` - Штатное расписание
- `POST /api/staffing` - Добавить позицию

#### Табель
- `GET /api/timesheet/{employeeId}` - Табель сотрудника
- `POST /api/timesheet` - Добавить запись

#### Зарплата
- `GET /api/salary/{employeeId}` - Расчеты зарплаты
- `POST /api/salary/calculate` - Рассчитать зарплату

### Recruitment API

#### Вакансии
- `GET /api/vacancies` - Все вакансии
- `GET /api/vacancies/open` - Открытые вакансии
- `POST /api/vacancies` - Создать вакансию

#### Кандидаты
- `GET /api/candidates` - Все кандидаты
- `POST /api/candidates` - Добавить кандидата
- `POST /api/candidates/{id}/hire` - Принять кандидата

#### Собеседования
- `GET /api/interviews` - Все собеседования
- `POST /api/interviews` - Назначить собеседование

#### Обучение
- `GET /api/trainings` - Все обучения
- `POST /api/trainings` - Создать обучение
- `POST /api/trainings/{id}/participants` - Добавить участника

## Аутентификация

Все endpoints (кроме `/api/auth/*` и `/api/vacancies/open`) требуют JWT токен в заголовке:

```
Authorization: Bearer <token>
```

Для получения токена:
1. Зарегистрируйтесь через `POST /api/auth/register`
2. Войдите через `POST /api/auth/login`
3. Используйте полученный токен в заголовке Authorization

## Конфигурация

Настройки хранятся в `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=employees.db"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "HRManagement",
    "Audience": "HRManagement",
    "ExpirationMinutes": 60
  }
}
```

## Особенности реализации

1. **Clean Architecture**: Разделение на Domain, Application, Infrastructure слои
2. **Minimal API**: Endpoints вынесены в отдельные классы расширения
3. **Fluent API**: Конфигурация моделей EF Core через Fluent API
4. **Event-Driven**: Межсервисное взаимодействие через события (CandidateHiredEvent, SalaryCalculatedEvent и т.д.)
5. **Caching**: Кэширование часто запрашиваемых данных
6. **Soft Delete**: Логическое удаление записей

## Лицензия

MIT
