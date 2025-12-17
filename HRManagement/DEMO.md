# 🏢 HR Management System - Полная документация API

## Обзор системы

HR Management — это микросервисная система управления персоналом, построенная на **.NET 8** с использованием современных практик разработки.

---

## 🏗️ Архитектура

```
┌─────────────────────────────────────────────────────────────────┐
│                        API Gateway (:5000)                       │
│                    Единая точка входа в систему                  │
└─────────────────────────────────────────────────────────────────┘
                                 │
        ┌────────────────────────┼────────────────────────┐
        │            │           │           │            │
        ▼            ▼           ▼           ▼            ▼
   ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐
   │Employees│ │ Payroll │ │Recruit- │ │Attend-  │ │Documents│
   │  :5001  │ │  :5002  │ │  ment   │ │  ance   │ │  :5005  │
   │         │ │         │ │  :5003  │ │  :5004  │ │         │
   └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘
        │            │           │           │            │
        └────────────┴───────────┼───────────┴────────────┘
                                 │
                    ┌────────────┴────────────┐
                    │                         │
               ┌────┴────┐              ┌─────┴─────┐
               │PostgreSQL│              │ RabbitMQ  │
               │  :5432   │              │   :5672   │
               └──────────┘              └───────────┘
```

---

## 🚀 Быстрый старт

### Запуск системы
```bash
cd /workspaces/dotnet-codespaces/HRManagement
docker-compose up -d
./seed-data.sh  # Заполнение тестовыми данными
```

### Учётные данные
```
Email: admin@hrmanagement.ru
Пароль: Admin123!
```

### Swagger UI
| Сервис | URL |
|--------|-----|
| Сотрудники | http://localhost:5001/swagger |
| Зарплата | http://localhost:5002/swagger |
| Рекрутинг | http://localhost:5003/swagger |
| Посещаемость | http://localhost:5004/swagger |
| Документы | http://localhost:5005/swagger |

---

# 📚 API Reference

## 1. 👥 Сервис Сотрудники (Employees) — порт 5001

### 1.1 🔐 Аутентификация

#### Регистрация
```http
POST http://localhost:5001/api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "Иван",
  "lastName": "Иванов"
}
```

**Ответ:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "user@example.com",
    "firstName": "Иван",
    "lastName": "Иванов",
    "expiresAt": "2025-12-18T22:00:00Z"
  },
  "message": "Регистрация выполнена успешно"
}
```

#### Вход
```http
POST http://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "admin@hrmanagement.ru",
  "password": "Admin123!"
}
```

**Ответ:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "admin@hrmanagement.ru",
    "firstName": "Администратор",
    "lastName": "Системы",
    "expiresAt": "2025-12-18T22:00:00Z"
  },
  "message": "Вход выполнен успешно"
}
```

#### Выход
```http
POST http://localhost:5001/api/auth/logout
Authorization: Bearer {{token}}
```

#### Текущий пользователь
```http
GET http://localhost:5001/api/auth/me
Authorization: Bearer {{token}}
```

---

### 1.2 🏢 Отделы

#### Получить все отделы
```http
GET http://localhost:5001/api/departments
Authorization: Bearer {{token}}
```

**Ответ:**
```json
{
  "success": true,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "IT-отдел",
      "description": "Отдел информационных технологий",
      "parentId": null,
      "employeesCount": 3
    }
  ]
}
```

#### Создать отдел
```http
POST http://localhost:5001/api/departments
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "name": "Отдел разработки",
  "description": "Разработка программного обеспечения",
  "parentDepartmentId": "550e8400-e29b-41d4-a716-446655440001"
}
```

#### Обновить отдел
```http
PUT http://localhost:5001/api/departments/{{departmentId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "name": "IT-департамент",
  "description": "Обновлённое описание",
  "parentDepartmentId": null
}
```

#### Удалить отдел
```http
DELETE http://localhost:5001/api/departments/{{departmentId}}
Authorization: Bearer {{token}}
```

---

### 1.3 👤 Сотрудники

#### Получить всех сотрудников (с пагинацией)
```http
GET http://localhost:5001/api/employees?pageNumber=1&pageSize=10
Authorization: Bearer {{token}}
```

**Ответ:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440101",
        "firstName": "Александр",
        "lastName": "Иванов",
        "middleName": "Сергеевич",
        "email": "a.ivanov@company.ru",
        "phone": "+79001234567",
        "dateOfBirth": "1990-05-15",
        "departmentId": "550e8400-e29b-41d4-a716-446655440001",
        "departmentName": "IT-отдел",
        "positionId": "550e8400-e29b-41d4-a716-446655440011",
        "positionName": "Senior Developer",
        "hireDate": "2023-01-15",
        "status": "Active"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 6,
    "totalPages": 1
  }
}
```

#### Получить сотрудника по ID
```http
GET http://localhost:5001/api/employees/{{employeeId}}
Authorization: Bearer {{token}}
```

#### Получить сотрудников отдела
```http
GET http://localhost:5001/api/employees/department/{{departmentId}}
Authorization: Bearer {{token}}
```

#### Создать сотрудника
```http
POST http://localhost:5001/api/employees
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "firstName": "Пётр",
  "lastName": "Петров",
  "middleName": "Иванович",
  "email": "p.petrov@company.ru",
  "phone": "+79009876543",
  "dateOfBirth": "1995-03-20",
  "address": "г. Москва, ул. Ленина, д. 1",
  "inn": "123456789012",
  "snils": "123-456-789 01",
  "departmentId": "550e8400-e29b-41d4-a716-446655440001",
  "positionId": "550e8400-e29b-41d4-a716-446655440012",
  "hireDate": "2025-01-15"
}
```

#### Обновить сотрудника
```http
PUT http://localhost:5001/api/employees/{{employeeId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "firstName": "Пётр",
  "lastName": "Петров",
  "middleName": "Иванович",
  "email": "petr.petrov@company.ru",
  "phone": "+79009876544",
  "dateOfBirth": "1995-03-20",
  "address": "г. Москва, ул. Пушкина, д. 5",
  "departmentId": "550e8400-e29b-41d4-a716-446655440001",
  "positionId": "550e8400-e29b-41d4-a716-446655440011"
}
```

#### Уволить сотрудника
```http
POST http://localhost:5001/api/employees/{{employeeId}}/terminate
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "terminationDate": "2025-12-31",
  "reason": "Собственное желание"
}
```

#### Удалить сотрудника
```http
DELETE http://localhost:5001/api/employees/{{employeeId}}
Authorization: Bearer {{token}}
```

---

### 1.4 💼 Должности

#### Получить все должности
```http
GET http://localhost:5001/api/positions?pageNumber=1&pageSize=20
Authorization: Bearer {{token}}
```

**Ответ:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440011",
        "title": "Senior Developer",
        "description": "Старший разработчик",
        "minSalary": 180000,
        "maxSalary": 300000,
        "departmentId": "550e8400-e29b-41d4-a716-446655440001",
        "departmentName": "IT-отдел"
      }
    ],
    "totalCount": 6
  }
}
```

#### Создать должность
```http
POST http://localhost:5001/api/positions
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "title": "DevOps Engineer",
  "description": "Инженер по автоматизации развёртывания",
  "minSalary": 150000,
  "maxSalary": 250000,
  "departmentId": "550e8400-e29b-41d4-a716-446655440001"
}
```

#### Добавить должностную обязанность
```http
POST http://localhost:5001/api/positions/{{positionId}}/responsibilities
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "description": "Настройка CI/CD пайплайнов",
  "priority": 1
}
```

---

### 1.5 📅 Отпуска

#### Получить отпуска сотрудника
```http
GET http://localhost:5001/api/leaves/employee/{{employeeId}}
Authorization: Bearer {{token}}
```

#### Получить заявки на рассмотрении
```http
GET http://localhost:5001/api/leaves/pending
Authorization: Bearer {{token}}
```

#### Получить остаток дней отпуска
```http
GET http://localhost:5001/api/leaves/employee/{{employeeId}}/balance/2025
Authorization: Bearer {{token}}
```

#### Создать заявку на отпуск
```http
POST http://localhost:5001/api/leaves
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "employeeId": "550e8400-e29b-41d4-a716-446655440101",
  "leaveType": "Annual",
  "startDate": "2025-02-01",
  "endDate": "2025-02-14",
  "reason": "Ежегодный отпуск"
}
```

**Типы отпусков:** `Annual`, `Sick`, `Unpaid`, `Maternity`, `Study`

#### Одобрить/отклонить заявку
```http
POST http://localhost:5001/api/leaves/{{leaveId}}/approve
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "approved": true,
  "comments": "Одобрено"
}
```

#### Отменить заявку
```http
POST http://localhost:5001/api/leaves/{{leaveId}}/cancel
Authorization: Bearer {{token}}
```

---

### 1.6 🎯 Навыки

#### Получить все навыки
```http
GET http://localhost:5001/api/skills
Authorization: Bearer {{token}}
```

#### Создать навык
```http
POST http://localhost:5001/api/skills
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "name": "Docker",
  "description": "Контейнеризация приложений",
  "category": "DevOps"
}
```

#### Получить навыки сотрудника
```http
GET http://localhost:5001/api/skills/employee/{{employeeId}}
Authorization: Bearer {{token}}
```

#### Назначить навык сотруднику
```http
POST http://localhost:5001/api/skills/assign
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "employeeId": "550e8400-e29b-41d4-a716-446655440101",
  "skillId": "550e8400-e29b-41d4-a716-446655440201",
  "level": "Advanced",
  "yearsOfExperience": 3,
  "certifiedDate": "2024-06-01",
  "notes": "Сертификат Docker DCA"
}
```

**Уровни навыков:** `Beginner`, `Intermediate`, `Advanced`, `Expert`

---

### 1.7 📈 История должностей

#### Получить историю должностей сотрудника
```http
GET http://localhost:5001/api/position-history/employee/{{employeeId}}
Authorization: Bearer {{token}}
```

#### Изменить должность сотрудника
```http
POST http://localhost:5001/api/position-history/change
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "employeeId": "550e8400-e29b-41d4-a716-446655440101",
  "newPositionId": "550e8400-e29b-41d4-a716-446655440011",
  "newDepartmentId": "550e8400-e29b-41d4-a716-446655440001",
  "newSalary": 250000,
  "effectiveDate": "2025-02-01",
  "reason": "Повышение"
}
```

---

### 1.8 📷 Фотографии сотрудников

#### Загрузить фото
```http
POST http://localhost:5001/api/photos/{{employeeId}}
Authorization: Bearer {{token}}
Content-Type: multipart/form-data

file: [binary]
```

#### Получить фото
```http
GET http://localhost:5001/api/photos/{{employeeId}}
Authorization: Bearer {{token}}
```

---

## 2. 💰 Сервис Зарплата (Payroll) — порт 5002

### 2.1 📊 Штатное расписание

#### Получить все штатные расписания
```http
GET http://localhost:5002/api/staffing
Authorization: Bearer {{token}}
```

#### Получить активное расписание
```http
GET http://localhost:5002/api/staffing/active
Authorization: Bearer {{token}}
```

#### Создать штатное расписание
```http
POST http://localhost:5002/api/staffing
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "name": "Штатное расписание 2025",
  "effectiveDate": "2025-01-01",
  "expirationDate": "2025-12-31"
}
```

#### Добавить штатную единицу
```http
POST http://localhost:5002/api/staffing/{{staffingId}}/positions
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "departmentId": "550e8400-e29b-41d4-a716-446655440001",
  "positionId": "550e8400-e29b-41d4-a716-446655440011",
  "departmentName": "IT-отдел",
  "positionName": "Senior Developer",
  "headcount": 5,
  "baseSalary": 200000
}
```

#### Получить штатные единицы
```http
GET http://localhost:5002/api/staffing/{{staffingId}}/positions
Authorization: Bearer {{token}}
```

---

### 2.2 📋 Табель рабочего времени

#### Получить табели сотрудника
```http
GET http://localhost:5002/api/timesheets/employee/{{employeeId}}
Authorization: Bearer {{token}}
```

#### Получить табели за период
```http
GET http://localhost:5002/api/timesheets/period/2025/1
Authorization: Bearer {{token}}
```

#### Создать табель
```http
POST http://localhost:5002/api/timesheets
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "employeeId": "550e8400-e29b-41d4-a716-446655440101",
  "employeeName": "Иванов Александр",
  "year": 2025,
  "month": 1,
  "workDays": 20,
  "workedDays": 20,
  "sickDays": 0,
  "vacationDays": 0,
  "absentDays": 0,
  "overtimeHours": 8
}
```

#### Утвердить табель
```http
POST http://localhost:5002/api/timesheets/{{timesheetId}}/approve
Authorization: Bearer {{token}}
```

---

### 2.3 💵 Расчёт зарплаты

#### Получить расчёты сотрудника
```http
GET http://localhost:5002/api/salary/employee/{{employeeId}}
Authorization: Bearer {{token}}
```

#### Получить расчёты за период
```http
GET http://localhost:5002/api/salary/period/2025/1
Authorization: Bearer {{token}}
```

#### Рассчитать зарплату
```http
POST http://localhost:5002/api/salary/calculate
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "employeeId": "550e8400-e29b-41d4-a716-446655440101",
  "employeeName": "Иванов Александр",
  "year": 2025,
  "month": 1,
  "baseSalary": 200000,
  "bonuses": 20000,
  "deductions": 5000
}
```

**Ответ:**
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440301",
    "employeeId": "550e8400-e29b-41d4-a716-446655440101",
    "employeeName": "Иванов Александр",
    "year": 2025,
    "month": 1,
    "baseSalary": 200000,
    "bonuses": 20000,
    "deductions": 5000,
    "taxAmount": 27950,
    "netSalary": 187050,
    "status": "Draft"
  },
  "message": "Зарплата рассчитана"
}
```

#### Утвердить расчёт
```http
POST http://localhost:5002/api/salary/{{salaryId}}/approve
Authorization: Bearer {{token}}
```

#### Отметить как выплаченную
```http
POST http://localhost:5002/api/salary/{{salaryId}}/pay
Authorization: Bearer {{token}}
```

---

## 3. 🎯 Сервис Рекрутинг (Recruitment) — порт 5003

### 3.1 📋 Вакансии

#### Получить все вакансии
```http
GET http://localhost:5003/api/vacancies
Authorization: Bearer {{token}}
```

#### Получить открытые вакансии (без авторизации)
```http
GET http://localhost:5003/api/vacancies/open
```

**Ответ:**
```json
{
  "success": true,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440401",
      "title": "Senior .NET Developer",
      "description": "Ищем опытного разработчика",
      "requirements": "5+ лет опыта, C#, ASP.NET Core",
      "departmentId": "550e8400-e29b-41d4-a716-446655440001",
      "departmentName": "IT-отдел",
      "positionId": "550e8400-e29b-41d4-a716-446655440011",
      "positionName": "Senior Developer",
      "salaryFrom": 180000,
      "salaryTo": 300000,
      "status": 1,
      "candidatesCount": 2,
      "createdAt": "2025-01-01T00:00:00Z"
    }
  ]
}
```

#### Создать вакансию
```http
POST http://localhost:5003/api/vacancies
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "title": "Middle Python Developer",
  "description": "Разработка backend сервисов на Python",
  "requirements": "3+ лет опыта, Python, FastAPI, PostgreSQL",
  "departmentId": "550e8400-e29b-41d4-a716-446655440001",
  "departmentName": "IT-отдел",
  "positionId": "550e8400-e29b-41d4-a716-446655440012",
  "positionName": "Middle Developer",
  "salaryFrom": 120000,
  "salaryTo": 180000
}
```

#### Обновить вакансию
```http
PUT http://localhost:5003/api/vacancies/{{vacancyId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "title": "Middle Python Developer",
  "description": "Обновлённое описание",
  "requirements": "3+ лет опыта",
  "salaryFrom": 130000,
  "salaryTo": 200000,
  "status": 1
}
```

**Статусы вакансий:** `1` = Open, `2` = Closed, `3` = OnHold

#### Удалить вакансию
```http
DELETE http://localhost:5003/api/vacancies/{{vacancyId}}
Authorization: Bearer {{token}}
```

---

### 3.2 👥 Кандидаты

#### Получить всех кандидатов
```http
GET http://localhost:5003/api/candidates
Authorization: Bearer {{token}}
```

#### Получить кандидатов вакансии
```http
GET http://localhost:5003/api/candidates/vacancy/{{vacancyId}}
Authorization: Bearer {{token}}
```

#### Создать кандидата (без авторизации — для внешних заявок)
```http
POST http://localhost:5003/api/candidates
Content-Type: application/json

{
  "firstName": "Игорь",
  "lastName": "Волков",
  "email": "igor.volkov@gmail.com",
  "phone": "+79001234567",
  "resumeUrl": "https://hh.ru/resume/12345",
  "coverLetter": "Имею 6 лет опыта в разработке на .NET",
  "yearsOfExperience": 6,
  "education": "МГУ, Факультет ВМК",
  "skills": "C#, .NET Core, Docker, PostgreSQL",
  "vacancyId": "550e8400-e29b-41d4-a716-446655440401"
}
```

#### Обновить кандидата
```http
PUT http://localhost:5003/api/candidates/{{candidateId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "firstName": "Игорь",
  "lastName": "Волков",
  "email": "igor.volkov.updated@gmail.com",
  "phone": "+79001234568",
  "yearsOfExperience": 7,
  "skills": "C#, .NET Core, Docker, PostgreSQL, Kubernetes"
}
```

#### Принять кандидата на работу
```http
POST http://localhost:5003/api/candidates/{{candidateId}}/hire?departmentId={{departmentId}}&positionId={{positionId}}
Authorization: Bearer {{token}}
```

> ⚡ **Событие:** После найма публикуется `CandidateHiredEvent` в RabbitMQ, и сервис Employees автоматически создаёт нового сотрудника!

#### Отклонить кандидата
```http
POST http://localhost:5003/api/candidates/{{candidateId}}/reject
Authorization: Bearer {{token}}
```

---

### 3.3 📅 Собеседования

#### Получить все собеседования
```http
GET http://localhost:5003/api/interviews
Authorization: Bearer {{token}}
```

#### Получить собеседования кандидата
```http
GET http://localhost:5003/api/interviews/candidate/{{candidateId}}
Authorization: Bearer {{token}}
```

#### Назначить собеседование
```http
POST http://localhost:5003/api/interviews
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "candidateId": "550e8400-e29b-41d4-a716-446655440501",
  "scheduledDate": "2025-01-20T14:00:00Z",
  "interviewerName": "Александр Иванов",
  "notes": "Техническое собеседование, Python + FastAPI"
}
```

#### Обновить собеседование (добавить результат)
```http
PUT http://localhost:5003/api/interviews/{{interviewId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "scheduledDate": "2025-01-20T14:00:00Z",
  "interviewerName": "Александр Иванов",
  "notes": "Техническое собеседование",
  "result": "Passed"
}
```

**Результаты:** `Passed`, `Failed`, `Pending`

---

### 3.4 📚 Обучение

#### Получить все обучения
```http
GET http://localhost:5003/api/trainings
Authorization: Bearer {{token}}
```

#### Получить предстоящие обучения
```http
GET http://localhost:5003/api/trainings/upcoming
Authorization: Bearer {{token}}
```

#### Создать обучение
```http
POST http://localhost:5003/api/trainings
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "title": "Kubernetes для разработчиков",
  "description": "Практический курс по K8s",
  "type": 1,
  "startDate": "2025-02-01T09:00:00Z",
  "endDate": "2025-02-03T18:00:00Z",
  "durationHours": 24,
  "instructor": "Внешний тренер",
  "location": "Онлайн",
  "cost": 50000,
  "maxParticipants": 15
}
```

**Типы обучения:** `1` = Internal, `2` = External, `3` = Online, `4` = Conference

#### Добавить участника
```http
POST http://localhost:5003/api/trainings/{{trainingId}}/participants?employeeId={{employeeId}}
Authorization: Bearer {{token}}
```

#### Отметить завершение обучения
```http
POST http://localhost:5003/api/trainings/{{trainingId}}/participants/{{employeeId}}/complete?certificate=K8S-CERT-001
Authorization: Bearer {{token}}
```

#### Получить обучения сотрудника
```http
GET http://localhost:5003/api/trainings/employee/{{employeeId}}
Authorization: Bearer {{token}}
```

---

### 3.5 📎 Файлы кандидатов

#### Загрузить фото кандидата
```http
POST http://localhost:5003/api/candidates/{{candidateId}}/photo
Authorization: Bearer {{token}}
Content-Type: multipart/form-data

file: [binary]
```

#### Загрузить резюме (без авторизации)
```http
POST http://localhost:5003/api/candidates/{{candidateId}}/resume
Content-Type: multipart/form-data

file: [binary]
```

#### Скачать резюме
```http
GET http://localhost:5003/api/candidates/{{candidateId}}/resume
Authorization: Bearer {{token}}
```

---

## 4. ⏰ Сервис Посещаемость (Attendance) — порт 5004

### 4.1 ✅ Отметки посещаемости

#### Получить записи посещаемости сотрудника
```http
GET http://localhost:5004/api/attendance/employee/{{employeeId}}?from=2025-01-01&to=2025-01-31
Authorization: Bearer {{token}}
```

#### Получить записи за дату
```http
GET http://localhost:5004/api/attendance/date/2025-01-15
Authorization: Bearer {{token}}
```

#### Создать запись посещаемости
```http
POST http://localhost:5004/api/attendance
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "employeeId": "550e8400-e29b-41d4-a716-446655440101",
  "date": "2025-01-15",
  "checkInTime": "09:00:00",
  "checkOutTime": "18:00:00",
  "status": "Present",
  "notes": "Работа в офисе"
}
```

**Статусы:** `Present`, `Absent`, `Late`, `OnLeave`, `Remote`

#### Отметка прихода
```http
POST http://localhost:5004/api/attendance/checkin
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "employeeId": "550e8400-e29b-41d4-a716-446655440101",
  "notes": "Пришёл в офис"
}
```

#### Отметка ухода
```http
POST http://localhost:5004/api/attendance/checkout
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "employeeId": "550e8400-e29b-41d4-a716-446655440101",
  "notes": "Ушёл домой"
}
```

---

### 4.2 📅 Графики работы

#### Получить график сотрудника
```http
GET http://localhost:5004/api/schedules/employee/{{employeeId}}
Authorization: Bearer {{token}}
```

**Ответ:**
```json
{
  "success": true,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440601",
      "employeeId": "550e8400-e29b-41d4-a716-446655440101",
      "dayOfWeek": 1,
      "startTime": "09:00:00",
      "endTime": "18:00:00",
      "breakDuration": "01:00:00",
      "isWorkingDay": true
    }
  ]
}
```

#### Создать график работы
```http
POST http://localhost:5004/api/schedules
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "employeeId": "550e8400-e29b-41d4-a716-446655440101",
  "dayOfWeek": 1,
  "startTime": "09:00:00",
  "endTime": "18:00:00",
  "breakDuration": "01:00:00",
  "isWorkingDay": true
}
```

**DayOfWeek:** `0` = Sunday, `1` = Monday, ..., `6` = Saturday

#### Создать стандартный график (Пн-Пт 9-18)
```http
POST http://localhost:5004/api/schedules/employee/{{employeeId}}/default
Authorization: Bearer {{token}}
```

---

### 4.3 📊 Табели (Attendance)

#### Получить табели сотрудника
```http
GET http://localhost:5004/api/timesheets/employee/{{employeeId}}
Authorization: Bearer {{token}}
```

#### Получить табель за месяц
```http
GET http://localhost:5004/api/timesheets/employee/{{employeeId}}/2025/1
Authorization: Bearer {{token}}
```

#### Сформировать табель
```http
POST http://localhost:5004/api/timesheets/generate
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "employeeId": "550e8400-e29b-41d4-a716-446655440101",
  "year": 2025,
  "month": 1
}
```

#### Подать на утверждение
```http
POST http://localhost:5004/api/timesheets/{{timesheetId}}/submit
Authorization: Bearer {{token}}
```

#### Утвердить табель
```http
POST http://localhost:5004/api/timesheets/{{timesheetId}}/approve
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "approverId": "550e8400-e29b-41d4-a716-446655440001"
}
```

---

## 5. 📄 Сервис Документы (Documents) — порт 5005

### 5.1 📝 Шаблоны документов

#### Получить все шаблоны
```http
GET http://localhost:5005/api/templates
Authorization: Bearer {{token}}
```

**Ответ:**
```json
{
  "success": true,
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440701",
      "name": "Трудовой договор",
      "documentType": "Contract",
      "content": "ТРУДОВОЙ ДОГОВОР №{{number}}\n\nг. {{city}} {{date}}\n\n{{company}} именуемое...",
      "description": "Стандартный трудовой договор",
      "isActive": true
    }
  ]
}
```

#### Создать шаблон
```http
POST http://localhost:5005/api/templates
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "name": "Заявление на отпуск",
  "documentType": "Application",
  "content": "Директору {{company}}\n{{director}}\n\nот {{employee}}\n\nЗАЯВЛЕНИЕ\n\nПрошу предоставить мне ежегодный оплачиваемый отпуск с {{startDate}} по {{endDate}} продолжительностью {{days}} календарных дней.\n\n{{date}}\n{{signature}}",
  "description": "Заявление на ежегодный отпуск"
}
```

#### Обновить шаблон
```http
PUT http://localhost:5005/api/templates/{{templateId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "name": "Заявление на отпуск (обновлённое)",
  "content": "Обновлённый текст шаблона...",
  "description": "Обновлённое описание",
  "isActive": true
}
```

---

### 5.2 📋 Документы

#### Получить документы сотрудника
```http
GET http://localhost:5005/api/documents/employee/{{employeeId}}
Authorization: Bearer {{token}}
```

#### Получить документы по типу
```http
GET http://localhost:5005/api/documents/type/Contract
Authorization: Bearer {{token}}
```

#### Получить документы на подпись
```http
GET http://localhost:5005/api/documents/pending/{{signerId}}
Authorization: Bearer {{token}}
```

#### Создать документ
```http
POST http://localhost:5005/api/documents
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "employeeId": "550e8400-e29b-41d4-a716-446655440101",
  "title": "Трудовой договор №123",
  "documentType": "Contract",
  "content": "ТРУДОВОЙ ДОГОВОР №123\n\nг. Москва 15.01.2025\n\nООО \"Компания\" именуемое...",
  "description": "Трудовой договор с Ивановым А.С.",
  "validFrom": "2025-01-15",
  "validTo": null,
  "templateId": "550e8400-e29b-41d4-a716-446655440701"
}
```

#### Добавить подписанта
```http
POST http://localhost:5005/api/documents/{{documentId}}/signers
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "signerId": "550e8400-e29b-41d4-a716-446655440001",
  "signerName": "Петров Иван Сергеевич",
  "signerRole": "Генеральный директор",
  "signOrder": 1
}
```

#### Отправить на подпись
```http
POST http://localhost:5005/api/documents/{{documentId}}/submit
Authorization: Bearer {{token}}
```

#### Подписать документ
```http
POST http://localhost:5005/api/documents/{{documentId}}/sign
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "signerId": "550e8400-e29b-41d4-a716-446655440001",
  "comment": "Согласовано"
}
```

#### Отклонить документ
```http
POST http://localhost:5005/api/documents/{{documentId}}/reject
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "signerId": "550e8400-e29b-41d4-a716-446655440001",
  "reason": "Требуется корректировка пункта 3.1"
}
```

#### Архивировать документ
```http
POST http://localhost:5005/api/documents/{{documentId}}/archive
Authorization: Bearer {{token}}
```

---

## 🔄 Событийная архитектура (RabbitMQ)

### События в системе

```
┌──────────────┐                         ┌──────────────┐
│  Recruitment │  ──CandidateHiredEvent─►│  Employees   │
│              │                         │              │
│  "Кандидат   │                         │ "Создаю      │
│   принят!"   │                         │  сотрудника" │
└──────────────┘                         └──────────────┘

┌──────────────┐                         ┌──────────────┐
│  Employees   │ ──EmployeeCreatedEvent─►│   Payroll    │
│              │                         │              │
│ "Сотрудник   │                         │ "Добавляю в  │
│  создан!"    │                         │  штатное"    │
└──────────────┘                         └──────────────┘
```

| Событие | Источник | Подписчики | Описание |
|---------|----------|------------|----------|
| `EmployeeCreatedEvent` | Employees | Payroll, Attendance | Создан новый сотрудник |
| `EmployeeTerminatedEvent` | Employees | Payroll, Attendance | Сотрудник уволен |
| `CandidateHiredEvent` | Recruitment | Employees | Кандидат принят на работу |
| `SalaryCalculatedEvent` | Payroll | Documents | Зарплата рассчитана |
| `DocumentSignedEvent` | Documents | - | Документ подписан |

---

## 📊 Тестовые данные

После запуска `./seed-data.sh`:

| Сущность | Количество |
|----------|------------|
| Отделы | 4 (IT, HR, Финансы, Продажи) |
| Должности | 6 |
| Сотрудники | 6 |
| Вакансии | 3 |
| Кандидаты | 3 |
| Собеседования | 3 |
| Обучения | 2 |
| Графики работы | 2 |
| Шаблоны документов | 3 |

---

## 🐛 Устранение неполадок

### Сервисы не запускаются
```bash
docker-compose logs -f
docker-compose down && docker-compose up -d
```

### Ошибка 401 Unauthorized
1. Выполните вход: `POST /api/auth/login`
2. Используйте токен: `Authorization: Bearer <token>`
3. Токен действителен 24 часа

### Ошибка подключения к БД
```bash
docker-compose ps hr-postgres
docker-compose logs hr-postgres
```

---

## 🛠️ Технологический стек

| Компонент | Технология |
|-----------|------------|
| Backend | .NET 8, ASP.NET Core Minimal APIs |
| База данных | PostgreSQL 16 |
| Брокер сообщений | RabbitMQ 3.12 |
| Контейнеризация | Docker, Docker Compose |
| Аутентификация | JWT (HTTP-only cookies) |
| ORM | Entity Framework Core 8 |
| Документация API | Swagger / OpenAPI |
