# JobPortal — Звіт про виконану роботу

**Курсова робота з дисципліни «Архітектура та проектування програмного забезпечення»**
Варіант 5 — База резюме та вакансій
НАУ, кафедра ІПЗ, викладач Дишлевий О.П.

---

## Зміст

1. [Загальна архітектура проекту](#1-загальна-архітектура-проекту)
2. [Структура рішення](#2-структура-рішення)
3. [Опис виконаних завдань](#3-опис-виконаних-завдань)
4. [Деталі реалізації](#4-деталі-реалізації)
5. [Тестування](#5-тестування)
6. [Інструкція з розгортання та ручного тестування](#6-інструкція-з-розгортання-та-ручного-тестування)

---

## 1. Загальна архітектура проекту

Застосунок реалізовано за принципом **тришарової архітектури** (Layered Architecture):

```
┌─────────────────────────────────────────┐
│  PL  — JobPortal.PL  (ASP.NET WebAPI)   │  ← HTTP-запити, маршрутизація
├─────────────────────────────────────────┤
│  BLL — JobPortal.BLL (Class Library)    │  ← Бізнес-логіка, валідація
├─────────────────────────────────────────┤
│  DAL — JobPortal.DAL (Class Library)    │  ← Доступ до даних, EF6, MS SQL
└─────────────────────────────────────────┘
```

**Принцип взаємодії:** PL використовує BLL через інтерфейси сервісів. BLL використовує DAL через інтерфейс `IUnitOfWork`. Жодний шар не знає про реалізацію нижчого шару — лише про його контракт (інтерфейс).

**Передача даних між шарами** здійснюється через об'єкти DTO (Data Transfer Objects), які виключають пряму залежність PL та BLL від сутностей DAL.

**Ізоляція шарів** забезпечується через Dependency Injection (бібліотека Ninject).

---

## 2. Структура рішення

```
Б-121-24-1-ПІ test 52.slnx
│
├── JobPortal.DAL/                    ← Шар доступу до даних
│   ├── Context/
│   │   └── JobPortalContext.cs       ← DbContext (EF6 Code-First)
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   ├── Vacancy.cs
│   │   ├── Resume.cs
│   │   └── JobApplication.cs
│   ├── Interfaces/
│   │   ├── IRepository.cs            ← Узагальнений репозиторій
│   │   └── IUnitOfWork.cs            ← Одиниця роботи
│   └── Repositories/
│       ├── Repository.cs             ← Реалізація IRepository<T>
│       └── UnitOfWork.cs             ← Реалізація IUnitOfWork
│
├── JobPortal.BLL/                    ← Шар бізнес-логіки
│   ├── DTOs/
│   │   ├── VacancyDto.cs
│   │   ├── ResumeDto.cs
│   │   ├── UserDTO.cs
│   │   └── JobApplicationDto.cs
│   ├── Infrastructure/
│   │   ├── RoleIds.cs                ← Константи ідентифікаторів ролей
│   │   ├── ValidationException.cs
│   │   ├── NotFoundException.cs
│   │   └── AccessDeniedException.cs
│   ├── Interfaces/
│   │   ├── IVacancyService.cs
│   │   ├── IResumeService.cs
│   │   └── IJobApplicationService.cs
│   └── Services/
│       ├── VacancyService.cs
│       ├── ResumeService.cs
│       └── JobApplicationService.cs
│
├── JobPortal.PL/                     ← Шар представлення (WebAPI)
│   ├── App_Start/
│   │   ├── NinjectWebCommon.cs       ← Налаштування DI
│   │   └── WebApiConfig.cs           ← Маршрутизація
│   ├── Controllers/
│   │   ├── VacanciesController.cs
│   │   ├── ResumesController.cs
│   │   └── JobApplicationsController.cs
│   ├── Filters/
│   │   └── ApiExceptionFilterAttribute.cs  ← Глобальна обробка виключень
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── Global.asax
│   ├── Global.asax.cs
│   └── JobPortal.PL.csproj           ← C# проект (замінено з .vbproj)
│
└── JobPortal.BLL.Tests/              ← Модульні тести
    ├── VacancyServiceTest.cs         ← 13 тестів
    ├── ResumeServiceTests.cs         ← 11 тестів
    └── JobApplicationServiceTests.cs ← 7 тестів
```

---

## 3. Опис виконаних завдань

### Завдання 1 — Виправлення критичної помилки компіляції PL

**Проблема:** Файл `JobPortal.PL.vbproj` був проектом VB.NET. Усі контролери (`.cs` файли) були перераховані як `<Content>` (вміст), а не `<Compile>` (код для компіляції). Унаслідок цього жоден контролер не компілювався.

**Що зроблено:**
- Видалено `JobPortal.PL.vbproj`
- Створено `JobPortal.PL.csproj` — повноцінний C# WebApplication проект з правильним `ProjectTypeGuid` (`FAE04EC0` замість `F184B08F`) та імпортом `Microsoft.CSharp.targets`
- Усі `.cs` файли тепер в `<Compile>` елементах
- Створено `Properties/AssemblyInfo.cs`
- Створено `Global.asax` (markup файл для IIS)
- Виправлено `Global.asax.cs` — тепер використовує `GlobalConfiguration.Configure(WebApiConfig.Register)` замість дублювання маршрутів напряму
- Оновлено `Б-121-24-1-ПІ test 52.slnx` — посилання з `.vbproj` на `.csproj`

---

### Завдання 2 — Усунення магічних чисел у ролях

**Проблема:** Код містив числа `2` та `3` безпосередньо у перевірках (`user.RoleId != 2`), без жодного пояснення що вони означають.

**Що зроблено:**
Створено `JobPortal.BLL/Infrastructure/RoleIds.cs`:

```csharp
public static class RoleIds
{
    public const int Admin    = 1;
    public const int Employer = 2;
    public const int Employee = 3;
}
```

Замінено магічні числа у `VacancyService` та `ResumeService` на константи:
- `user.RoleId != 2` → `user.RoleId != RoleIds.Employer`
- `user.RoleId != 3` → `user.RoleId != RoleIds.Employee`

---

### Завдання 3 — Додано редагування та видалення вакансій

**Проблема:** Завдання вимагає підсистему «додавання/редагування». `VacancyService` мав лише `CreateVacancy` — без `UpdateVacancy` та `DeleteVacancy`.

**Що зроблено:**

Розширено `IVacancyService`:
```csharp
void UpdateVacancy(VacancyDto vacancyDto);
void DeleteVacancy(int id, int requesterId);
```

Реалізовано в `VacancyService` з повноцінними перевірками:
- `UpdateVacancy` — перевіряє існування вакансії, що редагувальник є роботодавцем і власником вакансії
- `DeleteVacancy` — адміністратор може видаляти будь-що; роботодавець — лише власні вакансії

Додано ендпоінти в `VacanciesController`:
```
PUT    /api/vacancies/{id}              ← редагування
DELETE /api/vacancies/{id}?requesterId  ← видалення
```

---

### Завдання 4 — Додано редагування та видалення резюме

Аналогічно до завдання 3 для `ResumeService` та `ResumesController`.

Розширено `IResumeService`:
```csharp
void UpdateResume(ResumeDto resumeDto);
void DeleteResume(int id, int requesterId);
```

Додано ендпоінти:
```
PUT    /api/resumes/{id}
DELETE /api/resumes/{id}?requesterId
```

---

### Завдання 5 — Розширення тестів VacancyService

**Проблема:** `VacancyServiceTests` мав лише 2 тести. Вимога завдання — покрити усі методи сервісу з найбільшою кількістю бізнес-операцій.

Додано 11 нових тестів (усього 13):

| Метод | Тест |
|---|---|
| `CreateVacancy` | EmptyTitle → ValidationException |
| `GetVacancies` | Без фільтрів — порядок по даті (новіші першими) |
| `GetVacancies` | З пошуком — повертає лише відповідні |
| `GetVacancies` | З мінімальною зарплатою — фільтрує нижчі |
| `GetVacanciesByResumeSkills` | Резюме не знайдено → NotFoundException |
| `GetVacanciesByResumeSkills` | Збіг навичок — повертає відповідні вакансії |
| `UpdateVacancy` | Валідний роботодавець — оновлює та зберігає |
| `UpdateVacancy` | Вакансія не знайдена → NotFoundException |
| `UpdateVacancy` | Чужа вакансія → AccessDeniedException |
| `DeleteVacancy` | Валідний роботодавець — видаляє та зберігає |
| `DeleteVacancy` | Вакансія не знайдена → NotFoundException |

---

### Завдання 6 — Нові тести ResumeService

Створено `ResumeServiceTests.cs` з 11 тестами, що покривають усі 5 методів:

| Метод | Тест |
|---|---|
| `CreateResume` | Валідний здобувач — додає та зберігає |
| `CreateResume` | Порожній заголовок → ValidationException |
| `CreateResume` | Неправильна роль → AccessDeniedException |
| `GetResumes` | Без фільтрів — порядок по даті |
| `GetResumes` | З пошуком — фільтрує за назвою/навичками |
| `GetResumesByVacancyRequirements` | Вакансія не знайдена → NotFoundException |
| `GetResumesByVacancyRequirements` | Збіг вимог — повертає відповідні резюме |
| `UpdateResume` | Валідний здобувач — оновлює та зберігає |
| `UpdateResume` | Резюме не знайдено → NotFoundException |
| `DeleteResume` | Валідний здобувач — видаляє та зберігає |
| `DeleteResume` | Резюме не знайдено → NotFoundException |

---

### Завдання 7 — Нові тести JobApplicationService

Створено `JobApplicationServiceTests.cs` з 7 тестами:

| Метод | Тест |
|---|---|
| `ApplyForVacancy` | Ініціатор — здобувач → статус "Applied" |
| `ApplyForVacancy` | Ініціатор — роботодавець → статус "Offered" |
| `ApplyForVacancy` | Резюме не знайдено → NotFoundException |
| `ApplyForVacancy` | Вакансія не знайдена → NotFoundException |
| `GetApplicationsForVacancy` | Повертає відповідні заявки з назвами |
| `GetApplicationsForVacancy` | Немає заявок → порожній список |
| `GetApplicationsForResume` | Повертає відповідні заявки |

---

### Завдання 8 — Виправлення консистентності повідомлень

- Замінено рядок `"Внутренняя ошибка сервера"` (російська) на `"Внутрішня помилка сервера"` в `ApiExceptionFilterAttribute`
- Замінено `"Связь успешно зарегистрирована"` на `"Зв'язок успішно зареєстровано"` в `JobApplicationsController`

---

## 4. Деталі реалізації

### 4.1 Шаблони Repository та Unit of Work

**IRepository\<T\>** — узагальнений інтерфейс для доступу до колекції сутностей певного типу:
```csharp
IEnumerable<T> GetAll();
T Get(int id);
IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
IEnumerable<T> FindWithIncludes(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
void Create(T item);
void Update(T item);
void Delete(int id);
```

**IUnitOfWork** — єдина точка доступу до всіх репозиторіїв та контексту EF. Усі зміни зберігаються одним викликом `Save()`, що гарантує транзакційність.

**Ізоляція DAL від BLL:** BLL залежить лише від `IUnitOfWork` (інтерфейс з DAL). Це дозволяє підміняти реалізацію в тестах через Mock-об'єкти без звернення до реальної БД.

### 4.2 Ізоляція DAL від BLL

BLL отримує `IUnitOfWork` через конструктор (DI). Сервіси не знають про `JobPortalContext`, `DbSet<T>` чи будь-що з Entity Framework — лише про контракт `IUnitOfWork`.

### 4.3 Передача даних між шарами (Mapping)

- **DAL → BLL:** `Repository<T>` повертає сутності DAL (`Vacancy`, `Resume` тощо). Сервіси BLL вручну відображають їх у DTO перед поверненням.
- **BLL → PL:** Контролери отримують DTO від сервісів і повертають їх у HTTP-відповіді як JSON.
- **PL → BLL:** Вхідні дані від клієнта (JSON body) десеріалізуються в DTO і передаються до сервісів.

Приклад маппінгу у `VacancyService`:
```csharp
return items.Select(v => new VacancyDto {
    Id = v.Id, Title = v.Title, Salary = v.Salary, ...
}).ToList();
```

### 4.4 Ізоляція BLL від PL

PL залежить лише від інтерфейсів сервісів (`IVacancyService`, `IResumeService`, `IJobApplicationService`). Конкретні класи `VacancyService` тощо інжектуються Ninject.

### 4.5 Структура HTTP-запитів та маршрутизація PL

Усі контролери використовують атрибутну маршрутизацію:

| Метод | URL | Призначення |
|---|---|---|
| GET | `/api/vacancies` | Список вакансій (з фільтрами: `search`, `sortBy`, `minSalary`) |
| POST | `/api/vacancies` | Створити вакансію (JSON body: `VacancyDto`) |
| PUT | `/api/vacancies/{id}` | Оновити вакансію (JSON body: `VacancyDto`) |
| DELETE | `/api/vacancies/{id}?requesterId` | Видалити вакансію |
| GET | `/api/vacancies/matched-for-resume/{resumeId}` | Вакансії за навичками резюме |
| GET | `/api/resumes` | Список резюме (з фільтрами: `search`, `sortBy`) |
| POST | `/api/resumes` | Створити резюме |
| PUT | `/api/resumes/{id}` | Оновити резюме |
| DELETE | `/api/resumes/{id}?requesterId` | Видалити резюме |
| GET | `/api/resumes/matched-for-vacancy/{vacancyId}` | Резюме за вимогами вакансії |
| POST | `/api/applications/connect?resumeId&vacancyId&initiatorRole` | Пов'язати резюме з вакансією |
| GET | `/api/applications/vacancy/{vacancyId}` | Заявки по вакансії |
| GET | `/api/applications/resume/{resumeId}` | Заявки по резюме |

### 4.6 Обробка виняткових ситуацій

Кастомні виключення BLL:
- `ValidationException(message, property)` → HTTP 400 Bad Request
- `AccessDeniedException(message)` → HTTP 403 Forbidden
- `NotFoundException(message)` → HTTP 404 Not Found
- Усі інші виключення → HTTP 500 Internal Server Error

`ApiExceptionFilterAttribute` на рівні PL перехоплює всі ці виключення й автоматично формує правильну HTTP-відповідь. Жодна бізнес-помилка не «вилетить» до клієнта як необроблений 500.

### 4.7 Dependency Injection (Ninject)

Зв'язування інтерфейсів з реалізаціями в `NinjectWebCommon.cs`:
```csharp
kernel.Bind<IUnitOfWork>().To<UnitOfWork>().InRequestScope();
kernel.Bind<IVacancyService>().To<VacancyService>();
kernel.Bind<IResumeService>().To<ResumeService>();
kernel.Bind<IJobApplicationService>().To<JobApplicationService>();
```

`InRequestScope()` для `IUnitOfWork` гарантує, що один HTTP-запит використовує один і той самий `DbContext` — це запобігає конфліктам між репозиторіями.

### 4.8 Тестовий фреймворк

Використовується **NUnit 4.6** з **Moq 4.20**.

Вибір NUnit обґрунтований тим, що він широко використовується у .NET-спільноті, має зручну атрибутну модель (`[TestFixture]`, `[SetUp]`, `[Test]`) та повну підтримку в Visual Studio через `NUnit3TestAdapter`.

Усі тести ізольовані від реальних даних: `IUnitOfWork` та `IRepository<T>` замінені Moq-об'єктами. Реальна БД у тестах не задіяна.

Усі тести дотримуються принципу **Triple A (Arrange-Act-Assert)**:
```csharp
[Test]
public void CreateVacancy_ValidEmployer_ShouldAddAndSave()
{
    // Arrange
    var dto = new VacancyDto { Title = "Dev", EmployerId = 10, Salary = 3000 };
    var employer = new User { Id = 10, RoleId = RoleIds.Employer };
    _userRepoMock.Setup(r => r.Get(10)).Returns(employer);

    // Act
    _service.CreateVacancy(dto);

    // Assert
    _vacancyRepoMock.Verify(r => r.Create(It.Is<Vacancy>(v => v.Title == "Dev")), Times.Once);
    _uowMock.Verify(u => u.Save(), Times.Once);
}
```

---

## 5. Тестування

### 5.1 Запуск модульних тестів

**Крок 1.** Відкрити рішення у Visual Studio.

**Крок 2.** Зібрати рішення: **Збірка → Перезібрати рішення** (або `Ctrl+Shift+B`).
Очікуваний результат: `========== Перезбірка: успішно 4, з помилками 0 ==========`

**Крок 3.** Запустити тести: **Тест → Виконати всі тести** (або `Ctrl+R, A`).

**Очікуваний результат: 31 тест пройшов, 0 провалено.**

| Тестовий клас | Кількість тестів |
|---|---|
| `VacancyServiceTests` | 13 |
| `ResumeServiceTests` | 11 |
| `JobApplicationServiceTests` | 7 |
| **Разом** | **31** |

### 5.2 Опис покриття тестами

**VacancyServiceTests (13 тестів):**
- `CreateVacancy_ValidEmployer_ShouldAddAndSave` — успішне створення
- `CreateVacancy_InvalidRole_ShouldThrowAccessDeniedException` — неправильна роль
- `CreateVacancy_EmptyTitle_ShouldThrowValidationException` — порожня назва
- `GetVacancies_NoFilters_ReturnsAllOrderedByDateDesc` — список без фільтрів
- `GetVacancies_WithSearch_ReturnsOnlyMatching` — пошук за назвою
- `GetVacancies_WithMinSalary_ReturnsOnlyAboveThreshold` — фільтр за зарплатою
- `GetVacanciesByResumeSkills_ResumeNotFound_ThrowsNotFoundException` — резюме не існує
- `GetVacanciesByResumeSkills_MatchingSkills_ReturnsMatched` — збіг навичок
- `UpdateVacancy_ValidEmployer_ShouldUpdateAndSave` — успішне оновлення
- `UpdateVacancy_NotFound_ThrowsNotFoundException` — вакансія не існує
- `UpdateVacancy_WrongOwner_ThrowsAccessDeniedException` — чужа вакансія
- `DeleteVacancy_ValidEmployer_ShouldDeleteAndSave` — успішне видалення
- `DeleteVacancy_NotFound_ThrowsNotFoundException` — вакансія не існує

---

## 6. Інструкція з розгортання та ручного тестування

### 6.1 Передумови

- Visual Studio 2022
- SQL Server LocalDB (входить до складу VS)
- Postman або аналог для тестування API

### 6.2 Налаштування бази даних

Відкрити `JobPortal.DAL/App.config` та перевірити рядок підключення:
```xml
<connectionStrings>
  <add name="JobPortalDb"
       connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=JobPortalDb;Integrated Security=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

EF Code-First автоматично створить базу даних при першому запуску. Додаткових дій не потрібно.

### 6.3 Запуск застосунку

1. У Visual Studio встановити `JobPortal.PL` як стартовий проект (правий клік → «Встановити як стартовий проект»)
2. Натиснути `F5` або кнопку «Запустити»
3. IIS Express запустить застосунок на `http://localhost:52459`

### 6.4 Початкові дані (seed)

Після першого запуску підключитися до LocalDB через **Оглядач сервера** (View → Server Explorer) та виконати:

```sql
INSERT INTO Roles (Name) VALUES ('Admin'), ('Employer'), ('Employee')

INSERT INTO Users (Name, Email, RoleId)
VALUES ('Адміністратор', 'admin@test.com', 1)

INSERT INTO Users (Name, Email, RoleId)
VALUES ('Роботодавець', 'employer@test.com', 2)

INSERT INTO Users (Name, Email, RoleId)
VALUES ('Здобувач', 'worker@test.com', 3)
```

Запам'ятати ID вставлених записів (зазвичай 1, 2, 3).

### 6.5 Тестування API через Postman

Для всіх запитів встановити заголовок:
```
Content-Type: application/json
```

---

#### Підсистема 1 — Управління вакансіями

**Створити вакансію (роботодавець userId=2):**
```
POST http://localhost:52459/api/vacancies
Body:
{
  "title": "C# Backend Developer",
  "description": "Розробка серверної частини",
  "requirements": "C#, SQL, REST",
  "salary": 45000,
  "employerId": 2
}
Очікуваний результат: 201 Created
```

**Спроба створити вакансію здобувачем (userId=3) — перевірка заборони:**
```
POST http://localhost:52459/api/vacancies
Body: { "title": "Test", "employerId": 3, "salary": 1000 }
Очікуваний результат: 403 Forbidden
```

**Спроба з порожньою назвою — перевірка валідації:**
```
POST http://localhost:52459/api/vacancies
Body: { "title": "", "employerId": 2, "salary": 1000 }
Очікуваний результат: 400 Bad Request
```

**Отримати список вакансій:**
```
GET http://localhost:52459/api/vacancies
Очікуваний результат: 200 OK, масив з 1 вакансією
```

**Отримати список з фільтрами:**
```
GET http://localhost:52459/api/vacancies?search=C%23&minSalary=10000&sortBy=salary_desc
Очікуваний результат: 200 OK, вакансія "C# Backend Developer"
```

**Оновити вакансію (id=1):**
```
PUT http://localhost:52459/api/vacancies/1
Body:
{
  "title": "Senior C# Developer",
  "description": "Оновлений опис",
  "requirements": "C#, SQL, Azure",
  "salary": 60000,
  "employerId": 2
}
Очікуваний результат: 200 OK
```

**Спроба оновити чужу вакансію:**
```
PUT http://localhost:52459/api/vacancies/1
Body: { "title": "X", "employerId": 99, "salary": 1 }
Очікуваний результат: 403 Forbidden (користувач не є власником)
```

**Видалити вакансію:**
```
DELETE http://localhost:52459/api/vacancies/1?requesterId=2
Очікуваний результат: 204 No Content

(повторний запит)
GET http://localhost:52459/api/vacancies/matched-for-resume/1
після видалення вакансії — список буде порожнім
```

---

#### Підсистема 2 — Управління резюме

**Створити резюме (здобувач userId=3):**
```
POST http://localhost:52459/api/resumes
Body:
{
  "title": "Резюме C# розробника",
  "skills": "C#, SQL, React",
  "experience": "3 роки комерційного досвіду",
  "description": "Готовий до роботи в команді",
  "employeeId": 3
}
Очікуваний результат: 201 Created
```

**Отримати список резюме:**
```
GET http://localhost:52459/api/resumes
Очікуваний результат: 200 OK, масив з 1 резюме

GET http://localhost:52459/api/resumes?search=C%23
Очікуваний результат: 200 OK, те саме резюме (збіг за навичкою)
```

**Оновити резюме (id=1):**
```
PUT http://localhost:52459/api/resumes/1
Body:
{
  "title": "Senior C# Developer CV",
  "skills": "C#, SQL, React, Azure",
  "experience": "5 років",
  "description": "Досвідчений розробник",
  "employeeId": 3
}
Очікуваний результат: 200 OK
```

---

#### Підсистема 3 — Зв'язок резюме та вакансій

Спочатку переконатися, що є хоча б одне резюме і одна вакансія.

**Знайти вакансії за навичками резюме:**
```
GET http://localhost:52459/api/vacancies/matched-for-resume/1
Очікуваний результат: 200 OK, список вакансій, чиї вимоги перетинаються
з навичками резюме №1 (наприклад, вакансія вимагає "C#", резюме має "C#")
```

**Знайти резюме за вимогами вакансії:**
```
GET http://localhost:52459/api/resumes/matched-for-vacancy/1
Очікуваний результат: 200 OK, список резюме, чиї навички відповідають
вимогам вакансії №1
```

**Подати заявку (здобувач подає резюме на вакансію):**
```
POST http://localhost:52459/api/applications/connect?resumeId=1&vacancyId=1&initiatorRole=Employee
Очікуваний результат: 200 OK, "Зв'язок успішно зареєстровано"
```

**Роботодавець пропонує вакансію здобувачу:**
```
POST http://localhost:52459/api/applications/connect?resumeId=1&vacancyId=1&initiatorRole=Employer
Очікуваний результат: 200 OK (статус буде "Offered")
```

**Переглянути заявки по вакансії:**
```
GET http://localhost:52459/api/applications/vacancy/1
Очікуваний результат: 200 OK, масив заявок з назвами вакансії та резюме
```

**Переглянути заявки по резюме:**
```
GET http://localhost:52459/api/applications/resume/1
Очікуваний результат: 200 OK, масив заявок
```

**Запит на неіснуючий ресурс — перевірка 404:**
```
GET http://localhost:52459/api/vacancies/matched-for-resume/9999
Очікуваний результат: 404 Not Found
```

---

### 6.6 Очікувані результати тестування

| Перевірка | Очікуваний HTTP-статус |
|---|---|
| Успішне створення | 201 Created |
| Успішне оновлення | 200 OK |
| Успішне видалення | 204 No Content |
| Успішне отримання даних | 200 OK |
| Порожнє обов'язкове поле | 400 Bad Request |
| Доступ з неправильною роллю | 403 Forbidden |
| Звернення до неіснуючого запису | 404 Not Found |

---

*Документ підготовлено на основі реалізованого коду проекту. Версія від 05.06.2026.*
