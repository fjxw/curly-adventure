#!/bin/bash

# Скрипт для заполнения HR Management тестовыми данными
# Использование: ./seed-data.sh

set -e

EMPLOYEES_API="http://localhost:5001"
PAYROLL_API="http://localhost:5002"
RECRUITMENT_API="http://localhost:5003"
ATTENDANCE_API="http://localhost:5004"
DOCUMENTS_API="http://localhost:5005"

echo "========================================"
echo "  HR Management - Заполнение данными"
echo "========================================"
echo ""

# Цвета для вывода
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Функция для красивого вывода
log_step() {
    echo -e "${BLUE}➤ $1${NC}"
}

log_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

log_info() {
    echo -e "${YELLOW}  $1${NC}"
}

# ============================================
# 1. РЕГИСТРАЦИЯ И АВТОРИЗАЦИЯ
# ============================================
log_step "Регистрация администратора..."

curl -s -X POST "$EMPLOYEES_API/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@hrmanagement.ru",
    "password": "Admin123!",
    "firstName": "Администратор",
    "lastName": "Системы"
  }' > /dev/null 2>&1 || true

log_success "Администратор зарегистрирован"

log_step "Авторизация..."
LOGIN_RESPONSE=$(curl -s -X POST "$EMPLOYEES_API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@hrmanagement.ru",
    "password": "Admin123!"
  }')

# Токен находится в data.token
TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.data.token')

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
    echo "Ошибка: не удалось получить токен авторизации"
    echo "Ответ: $LOGIN_RESPONSE"
    exit 1
fi

log_success "Токен получен"
echo ""

# ============================================
# 2. СОЗДАНИЕ ОТДЕЛОВ
# ============================================
log_step "Создание отделов..."

# IT отдел
IT_DEPT=$(curl -s -X POST "$EMPLOYEES_API/api/departments" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "IT-отдел",
    "description": "Отдел информационных технологий"
  }')
IT_DEPT_ID=$(echo $IT_DEPT | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "IT-отдел: $IT_DEPT_ID"

# HR отдел
HR_DEPT=$(curl -s -X POST "$EMPLOYEES_API/api/departments" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "HR-отдел",
    "description": "Управление персоналом"
  }')
HR_DEPT_ID=$(echo $HR_DEPT | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "HR-отдел: $HR_DEPT_ID"

# Финансовый отдел
FIN_DEPT=$(curl -s -X POST "$EMPLOYEES_API/api/departments" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Финансовый отдел",
    "description": "Бухгалтерия и финансы"
  }')
FIN_DEPT_ID=$(echo $FIN_DEPT | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Финансовый отдел: $FIN_DEPT_ID"

# Отдел продаж
SALES_DEPT=$(curl -s -X POST "$EMPLOYEES_API/api/departments" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Отдел продаж",
    "description": "Продажи и работа с клиентами"
  }')
SALES_DEPT_ID=$(echo $SALES_DEPT | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Отдел продаж: $SALES_DEPT_ID"

log_success "Создано 4 отдела"
echo ""

# ============================================
# 3. СОЗДАНИЕ ДОЛЖНОСТЕЙ
# ============================================
log_step "Создание должностей..."

# Senior Developer
SR_DEV=$(curl -s -X POST "$EMPLOYEES_API/api/positions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"name\": \"Senior Developer\",
    \"description\": \"Старший разработчик\",
    \"departmentId\": \"$IT_DEPT_ID\",
    \"minSalary\": 150000,
    \"maxSalary\": 250000
  }")
SR_DEV_ID=$(echo $SR_DEV | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Senior Developer: $SR_DEV_ID"

# Middle Developer
MID_DEV=$(curl -s -X POST "$EMPLOYEES_API/api/positions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"name\": \"Middle Developer\",
    \"description\": \"Разработчик\",
    \"departmentId\": \"$IT_DEPT_ID\",
    \"minSalary\": 100000,
    \"maxSalary\": 150000
  }")
MID_DEV_ID=$(echo $MID_DEV | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Middle Developer: $MID_DEV_ID"

# Junior Developer
JR_DEV=$(curl -s -X POST "$EMPLOYEES_API/api/positions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"name\": \"Junior Developer\",
    \"description\": \"Младший разработчик\",
    \"departmentId\": \"$IT_DEPT_ID\",
    \"minSalary\": 60000,
    \"maxSalary\": 100000
  }")
JR_DEV_ID=$(echo $JR_DEV | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Junior Developer: $JR_DEV_ID"

# HR Manager
HR_MGR=$(curl -s -X POST "$EMPLOYEES_API/api/positions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"name\": \"HR Manager\",
    \"description\": \"Менеджер по персоналу\",
    \"departmentId\": \"$HR_DEPT_ID\",
    \"minSalary\": 80000,
    \"maxSalary\": 120000
  }")
HR_MGR_ID=$(echo $HR_MGR | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "HR Manager: $HR_MGR_ID"

# Бухгалтер
ACCOUNTANT=$(curl -s -X POST "$EMPLOYEES_API/api/positions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"name\": \"Бухгалтер\",
    \"description\": \"Специалист по бухгалтерскому учёту\",
    \"departmentId\": \"$FIN_DEPT_ID\",
    \"minSalary\": 70000,
    \"maxSalary\": 100000
  }")
ACCOUNTANT_ID=$(echo $ACCOUNTANT | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Бухгалтер: $ACCOUNTANT_ID"

# Менеджер по продажам
SALES_MGR=$(curl -s -X POST "$EMPLOYEES_API/api/positions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"name\": \"Менеджер по продажам\",
    \"description\": \"Специалист по продажам\",
    \"departmentId\": \"$SALES_DEPT_ID\",
    \"minSalary\": 60000,
    \"maxSalary\": 150000
  }")
SALES_MGR_ID=$(echo $SALES_MGR | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Менеджер по продажам: $SALES_MGR_ID"

log_success "Создано 6 должностей"
echo ""

# ============================================
# 4. СОЗДАНИЕ СОТРУДНИКОВ
# ============================================
log_step "Создание сотрудников..."

# Сотрудник 1 - Senior Developer
EMP1=$(curl -s -X POST "$EMPLOYEES_API/api/employees" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"firstName\": \"Александр\",
    \"lastName\": \"Иванов\",
    \"middleName\": \"Сергеевич\",
    \"email\": \"a.ivanov@company.ru\",
    \"phone\": \"+79001234501\",
    \"dateOfBirth\": \"1988-03-15\",
    \"address\": \"г. Москва, ул. Пушкина, д. 10\",
    \"departmentId\": \"$IT_DEPT_ID\",
    \"positionId\": \"$SR_DEV_ID\",
    \"hireDate\": \"2020-01-15\"
  }")
EMP1_ID=$(echo $EMP1 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Александр Иванов (Senior Dev): $EMP1_ID"

# Сотрудник 2 - Middle Developer
EMP2=$(curl -s -X POST "$EMPLOYEES_API/api/employees" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"firstName\": \"Мария\",
    \"lastName\": \"Петрова\",
    \"middleName\": \"Александровна\",
    \"email\": \"m.petrova@company.ru\",
    \"phone\": \"+79001234502\",
    \"dateOfBirth\": \"1992-07-22\",
    \"address\": \"г. Москва, ул. Лермонтова, д. 5\",
    \"departmentId\": \"$IT_DEPT_ID\",
    \"positionId\": \"$MID_DEV_ID\",
    \"hireDate\": \"2021-06-01\"
  }")
EMP2_ID=$(echo $EMP2 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Мария Петрова (Middle Dev): $EMP2_ID"

# Сотрудник 3 - Junior Developer
EMP3=$(curl -s -X POST "$EMPLOYEES_API/api/employees" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"firstName\": \"Дмитрий\",
    \"lastName\": \"Сидоров\",
    \"middleName\": \"Николаевич\",
    \"email\": \"d.sidorov@company.ru\",
    \"phone\": \"+79001234503\",
    \"dateOfBirth\": \"1998-11-30\",
    \"address\": \"г. Москва, ул. Гоголя, д. 15\",
    \"departmentId\": \"$IT_DEPT_ID\",
    \"positionId\": \"$JR_DEV_ID\",
    \"hireDate\": \"2024-03-01\"
  }")
EMP3_ID=$(echo $EMP3 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Дмитрий Сидоров (Junior Dev): $EMP3_ID"

# Сотрудник 4 - HR Manager
EMP4=$(curl -s -X POST "$EMPLOYEES_API/api/employees" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"firstName\": \"Елена\",
    \"lastName\": \"Козлова\",
    \"middleName\": \"Владимировна\",
    \"email\": \"e.kozlova@company.ru\",
    \"phone\": \"+79001234504\",
    \"dateOfBirth\": \"1985-05-18\",
    \"address\": \"г. Москва, ул. Чехова, д. 8\",
    \"departmentId\": \"$HR_DEPT_ID\",
    \"positionId\": \"$HR_MGR_ID\",
    \"hireDate\": \"2019-02-10\"
  }")
EMP4_ID=$(echo $EMP4 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Елена Козлова (HR Manager): $EMP4_ID"

# Сотрудник 5 - Бухгалтер
EMP5=$(curl -s -X POST "$EMPLOYEES_API/api/employees" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"firstName\": \"Ольга\",
    \"lastName\": \"Новикова\",
    \"middleName\": \"Игоревна\",
    \"email\": \"o.novikova@company.ru\",
    \"phone\": \"+79001234505\",
    \"dateOfBirth\": \"1990-09-25\",
    \"address\": \"г. Москва, ул. Толстого, д. 20\",
    \"departmentId\": \"$FIN_DEPT_ID\",
    \"positionId\": \"$ACCOUNTANT_ID\",
    \"hireDate\": \"2022-08-15\"
  }")
EMP5_ID=$(echo $EMP5 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Ольга Новикова (Бухгалтер): $EMP5_ID"

# Сотрудник 6 - Менеджер по продажам
EMP6=$(curl -s -X POST "$EMPLOYEES_API/api/employees" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"firstName\": \"Андрей\",
    \"lastName\": \"Морозов\",
    \"middleName\": \"Петрович\",
    \"email\": \"a.morozov@company.ru\",
    \"phone\": \"+79001234506\",
    \"dateOfBirth\": \"1993-12-05\",
    \"address\": \"г. Москва, ул. Достоевского, д. 3\",
    \"departmentId\": \"$SALES_DEPT_ID\",
    \"positionId\": \"$SALES_MGR_ID\",
    \"hireDate\": \"2023-04-01\"
  }")
EMP6_ID=$(echo $EMP6 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Андрей Морозов (Продажи): $EMP6_ID"

log_success "Создано 6 сотрудников"
echo ""

# ============================================
# 5. СОЗДАНИЕ ВАКАНСИЙ (Recruitment)
# ============================================
log_step "Создание вакансий..."

VAC1=$(curl -s -X POST "$RECRUITMENT_API/api/vacancies" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"title\": \"Senior .NET Developer\",
    \"description\": \"Ищем опытного .NET разработчика для работы над микросервисной архитектурой\",
    \"requirements\": \"5+ лет опыта, C#, ASP.NET Core, Docker, Kubernetes, PostgreSQL\",
    \"departmentId\": \"$IT_DEPT_ID\",
    \"departmentName\": \"IT-отдел\",
    \"positionId\": \"$SR_DEV_ID\",
    \"positionName\": \"Senior Developer\",
    \"salaryFrom\": 180000,
    \"salaryTo\": 250000
  }")
VAC1_ID=$(echo $VAC1 | jq -r '.data.id // empty')
log_info "Вакансия Senior .NET Developer: $VAC1_ID"

VAC2=$(curl -s -X POST "$RECRUITMENT_API/api/vacancies" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"title\": \"Middle Frontend Developer\",
    \"description\": \"Разработчик интерфейсов на React/Vue\",
    \"requirements\": \"3+ лет опыта, React или Vue, TypeScript, REST API\",
    \"departmentId\": \"$IT_DEPT_ID\",
    \"departmentName\": \"IT-отдел\",
    \"positionId\": \"$MID_DEV_ID\",
    \"positionName\": \"Middle Developer\",
    \"salaryFrom\": 120000,
    \"salaryTo\": 180000
  }")
VAC2_ID=$(echo $VAC2 | jq -r '.data.id // empty')
log_info "Вакансия Middle Frontend Developer: $VAC2_ID"

VAC3=$(curl -s -X POST "$RECRUITMENT_API/api/vacancies" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"title\": \"HR-специалист\",
    \"description\": \"Специалист по подбору IT-персонала\",
    \"requirements\": \"2+ лет опыта в IT-рекрутинге\",
    \"departmentId\": \"$HR_DEPT_ID\",
    \"departmentName\": \"HR-отдел\",
    \"positionId\": \"$HR_MGR_ID\",
    \"positionName\": \"HR Manager\",
    \"salaryFrom\": 80000,
    \"salaryTo\": 120000
  }")
VAC3_ID=$(echo $VAC3 | jq -r '.data.id // empty')
log_info "Вакансия HR-специалист: $VAC3_ID"

log_success "Создано 3 вакансии"
echo ""

# ============================================
# 6. СОЗДАНИЕ КАНДИДАТОВ
# ============================================
log_step "Создание кандидатов..."

CAND1=$(curl -s -X POST "$RECRUITMENT_API/api/candidates" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"firstName\": \"Игорь\",
    \"lastName\": \"Волков\",
    \"email\": \"i.volkov@gmail.com\",
    \"phone\": \"+79005551001\",
    \"resumeUrl\": \"https://hh.ru/resume/12345\",
    \"coverLetter\": \"Имею 6 лет опыта в разработке на .NET, работал в крупных проектах\",
    \"yearsOfExperience\": 6,
    \"education\": \"МГУ, Факультет ВМК\",
    \"skills\": \"C#, .NET Core, Docker, PostgreSQL, RabbitMQ\",
    \"vacancyId\": \"$VAC1_ID\"
  }")
CAND1_ID=$(echo $CAND1 | jq -r '.data.id // empty')
log_info "Кандидат Игорь Волков: $CAND1_ID"

CAND2=$(curl -s -X POST "$RECRUITMENT_API/api/candidates" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"firstName\": \"Анна\",
    \"lastName\": \"Соколова\",
    \"email\": \"a.sokolova@mail.ru\",
    \"phone\": \"+79005551002\",
    \"resumeUrl\": \"https://hh.ru/resume/67890\",
    \"coverLetter\": \"Frontend разработчик с опытом работы на React\",
    \"yearsOfExperience\": 4,
    \"education\": \"МФТИ, Прикладная математика\",
    \"skills\": \"React, TypeScript, Redux, REST API, Git\",
    \"vacancyId\": \"$VAC2_ID\"
  }")
CAND2_ID=$(echo $CAND2 | jq -r '.data.id // empty')
log_info "Кандидат Анна Соколова: $CAND2_ID"

CAND3=$(curl -s -X POST "$RECRUITMENT_API/api/candidates" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"firstName\": \"Виктор\",
    \"lastName\": \"Кузнецов\",
    \"email\": \"v.kuznetsov@yandex.ru\",
    \"phone\": \"+79005551003\",
    \"resumeUrl\": \"https://hh.ru/resume/11111\",
    \"coverLetter\": \"7 лет опыта в backend разработке, последние 3 года на .NET\",
    \"yearsOfExperience\": 7,
    \"education\": \"СПбГУ, Информационные системы\",
    \"skills\": \"C#, ASP.NET Core, Microservices, Azure, SQL Server\",
    \"vacancyId\": \"$VAC1_ID\"
  }")
CAND3_ID=$(echo $CAND3 | jq -r '.data.id // empty')
log_info "Кандидат Виктор Кузнецов: $CAND3_ID"

log_success "Создано 3 кандидата"
echo ""

# ============================================
# 7. НАЗНАЧЕНИЕ СОБЕСЕДОВАНИЙ
# ============================================
log_step "Назначение собеседований..."

curl -s -X POST "$RECRUITMENT_API/api/interviews" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"candidateId\": \"$CAND1_ID\",
    \"scheduledDate\": \"2025-01-10T10:00:00Z\",
    \"interviewerName\": \"Александр Иванов\",
    \"notes\": \"Техническое собеседование на позицию Senior Developer\"
  }" > /dev/null

curl -s -X POST "$RECRUITMENT_API/api/interviews" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"candidateId\": \"$CAND2_ID\",
    \"scheduledDate\": \"2025-01-12T14:00:00Z\",
    \"interviewerName\": \"Мария Петрова\",
    \"notes\": \"Техническое собеседование Frontend\"
  }" > /dev/null

curl -s -X POST "$RECRUITMENT_API/api/interviews" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"candidateId\": \"$CAND3_ID\",
    \"scheduledDate\": \"2025-01-15T11:00:00Z\",
    \"interviewerName\": \"Александр Иванов\",
    \"notes\": \"Финальное собеседование с тимлидом\"
  }" > /dev/null

log_success "Назначено 3 собеседования"
echo ""

# ============================================
# 8. СОЗДАНИЕ ОБУЧЕНИЙ
# ============================================
log_step "Создание обучений..."

TRAIN1=$(curl -s -X POST "$RECRUITMENT_API/api/trainings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title": "Docker и Kubernetes для разработчиков",
    "description": "Практический курс по контейнеризации приложений",
    "type": "Technical",
    "startDate": "2025-02-01T09:00:00Z",
    "endDate": "2025-02-03T18:00:00Z",
    "durationHours": 24,
    "provider": "Internal",
    "location": "Офис, конференц-зал А",
    "cost": 0,
    "maxParticipants": 15
  }')
TRAIN1_ID=$(echo $TRAIN1 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Обучение Docker/K8s: $TRAIN1_ID"

TRAIN2=$(curl -s -X POST "$RECRUITMENT_API/api/trainings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title": "Soft Skills для IT-специалистов",
    "description": "Развитие коммуникативных навыков и работа в команде",
    "type": "Soft",
    "startDate": "2025-02-15T10:00:00Z",
    "endDate": "2025-02-15T18:00:00Z",
    "durationHours": 8,
    "provider": "External",
    "location": "Онлайн",
    "cost": 15000,
    "maxParticipants": 30
  }')
TRAIN2_ID=$(echo $TRAIN2 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Обучение Soft Skills: $TRAIN2_ID"

log_success "Создано 2 обучения"
echo ""

# ============================================
# 9. ШТАТНОЕ РАСПИСАНИЕ (Payroll)
# ============================================
log_step "Создание штатного расписания..."

curl -s -X POST "$PAYROLL_API/api/staffing" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"positionId\": \"$SR_DEV_ID\",
    \"departmentId\": \"$IT_DEPT_ID\",
    \"headcount\": 3,
    \"baseSalary\": 200000,
    \"effectiveDate\": \"2024-01-01\"
  }" > /dev/null

curl -s -X POST "$PAYROLL_API/api/staffing" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"positionId\": \"$MID_DEV_ID\",
    \"departmentId\": \"$IT_DEPT_ID\",
    \"headcount\": 5,
    \"baseSalary\": 130000,
    \"effectiveDate\": \"2024-01-01\"
  }" > /dev/null

curl -s -X POST "$PAYROLL_API/api/staffing" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"positionId\": \"$JR_DEV_ID\",
    \"departmentId\": \"$IT_DEPT_ID\",
    \"headcount\": 3,
    \"baseSalary\": 80000,
    \"effectiveDate\": \"2024-01-01\"
  }" > /dev/null

curl -s -X POST "$PAYROLL_API/api/staffing" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"positionId\": \"$HR_MGR_ID\",
    \"departmentId\": \"$HR_DEPT_ID\",
    \"headcount\": 2,
    \"baseSalary\": 100000,
    \"effectiveDate\": \"2024-01-01\"
  }" > /dev/null

log_success "Штатное расписание создано"
echo ""

# ============================================
# 10. ТАБЕЛЬ УЧЁТА ВРЕМЕНИ
# ============================================
log_step "Заполнение табеля за декабрь 2024..."

for day in {1..20}; do
    DATE=$(printf "2024-12-%02d" $day)
    
    # Для каждого сотрудника
    for EMP_ID in $EMP1_ID $EMP2_ID $EMP3_ID $EMP4_ID $EMP5_ID $EMP6_ID; do
        if [ ! -z "$EMP_ID" ]; then
            curl -s -X POST "$PAYROLL_API/api/timesheet" \
              -H "Content-Type: application/json" \
              -H "Authorization: Bearer $TOKEN" \
              -d "{
                \"employeeId\": \"$EMP_ID\",
                \"date\": \"$DATE\",
                \"hoursWorked\": 8,
                \"type\": \"Regular\"
              }" > /dev/null 2>&1 || true
        fi
    done
done

log_success "Табель заполнен (20 рабочих дней)"
echo ""

# ============================================
# 11. РАСЧЁТ ЗАРПЛАТЫ
# ============================================
log_step "Расчёт зарплаты за декабрь 2024..."

for EMP_ID in $EMP1_ID $EMP2_ID $EMP3_ID $EMP4_ID $EMP5_ID $EMP6_ID; do
    if [ ! -z "$EMP_ID" ]; then
        curl -s -X POST "$PAYROLL_API/api/salary/calculate" \
          -H "Content-Type: application/json" \
          -H "Authorization: Bearer $TOKEN" \
          -d "{
            \"employeeId\": \"$EMP_ID\",
            \"month\": 12,
            \"year\": 2024
          }" > /dev/null 2>&1 || true
    fi
done

log_success "Зарплата рассчитана"
echo ""

# ============================================
# 12. ГРАФИКИ РАБОТЫ (Attendance)
# ============================================
log_step "Создание графиков работы..."

SCHED1=$(curl -s -X POST "$ATTENDANCE_API/api/schedules" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Стандартный график 5/2",
    "description": "Пятидневная рабочая неделя 9:00-18:00",
    "workStartTime": "09:00:00",
    "workEndTime": "18:00:00",
    "breakDurationMinutes": 60,
    "workDays": [1, 2, 3, 4, 5]
  }')
SCHED1_ID=$(echo $SCHED1 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "График 5/2: $SCHED1_ID"

SCHED2=$(curl -s -X POST "$ATTENDANCE_API/api/schedules" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Гибкий график",
    "description": "Гибкое начало с 8:00 до 11:00",
    "workStartTime": "08:00:00",
    "workEndTime": "20:00:00",
    "breakDurationMinutes": 60,
    "workDays": [1, 2, 3, 4, 5]
  }')
SCHED2_ID=$(echo $SCHED2 | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
log_info "Гибкий график: $SCHED2_ID"

log_success "Создано 2 графика работы"
echo ""

# ============================================
# 13. ШАБЛОНЫ ДОКУМЕНТОВ (Documents)
# ============================================
log_step "Создание шаблонов документов..."

TPL1=$(curl -s -X POST "$DOCUMENTS_API/api/templates" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Трудовой договор",
    "description": "Стандартный трудовой договор",
    "category": "Contract",
    "content": "ТРУДОВОЙ ДОГОВОР №{{number}}\n\nг. Москва\n\n{{company_name}} в лице {{director_name}}, именуемый \"Работодатель\", и {{employee_name}}, именуемый \"Работник\", заключили настоящий договор о нижеследующем...",
    "requiredFields": ["number", "company_name", "director_name", "employee_name", "position", "salary", "start_date"]
  }')
log_info "Шаблон: Трудовой договор"

TPL2=$(curl -s -X POST "$DOCUMENTS_API/api/templates" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Заявление на отпуск",
    "description": "Заявление на ежегодный оплачиваемый отпуск",
    "category": "Application",
    "content": "Генеральному директору\n{{company_name}}\n{{director_name}}\nот {{employee_name}}\n\nЗАЯВЛЕНИЕ\n\nПрошу предоставить мне ежегодный оплачиваемый отпуск с {{start_date}} по {{end_date}} продолжительностью {{days}} календарных дней.",
    "requiredFields": ["company_name", "director_name", "employee_name", "start_date", "end_date", "days"]
  }')
log_info "Шаблон: Заявление на отпуск"

TPL3=$(curl -s -X POST "$DOCUMENTS_API/api/templates" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Приказ о приёме на работу",
    "description": "Приказ о приёме сотрудника",
    "category": "Order",
    "content": "ПРИКАЗ №{{number}}\n\nо приёме на работу\n\nПринять {{employee_name}} на должность {{position}} в {{department}} с окладом {{salary}} руб. с {{start_date}}.",
    "requiredFields": ["number", "employee_name", "position", "department", "salary", "start_date"]
  }')
log_info "Шаблон: Приказ о приёме"

log_success "Создано 3 шаблона документов"
echo ""

# ============================================
# ИТОГОВАЯ СТАТИСТИКА
# ============================================
echo "========================================"
echo -e "${GREEN}  ✓ Данные успешно загружены!${NC}"
echo "========================================"
echo ""
echo "Созданные данные:"
echo "  • 4 отдела"
echo "  • 6 должностей"
echo "  • 6 сотрудников"
echo "  • 3 вакансии"
echo "  • 3 кандидата"
echo "  • 3 собеседования"
echo "  • 2 обучения"
echo "  • 4 штатные единицы"
echo "  • Табель за 20 дней"
echo "  • 6 расчётов зарплаты"
echo "  • 2 графика работы"
echo "  • 3 шаблона документов"
echo ""
echo "Учётные данные для входа:"
echo "  Email: admin@hrmanagement.ru"
echo "  Пароль: Admin123!"
echo ""
echo "API эндпоинты:"
echo "  • Сотрудники: http://localhost:5001/swagger"
echo "  • Зарплата:   http://localhost:5002/swagger"
echo "  • Рекрутинг:  http://localhost:5003/swagger"
echo "  • Посещаемость: http://localhost:5004/swagger"
echo "  • Документы:  http://localhost:5005/swagger"
echo "  • API Шлюз:   http://localhost:5000/swagger"
echo ""
