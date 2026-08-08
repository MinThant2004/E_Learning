# E-Learning Management System — Project Context & Implementation Guide

## 1. Project Overview

This project is an **E-Learning Management System** inspired by learning platforms such as W3Schools.

The system allows students to learn from predefined courses and lessons, enroll in courses, track lesson progress, and take predefined quizzes with immediate scoring.

There are only two main types of users from a business perspective:

- Student
- Admin

There are **no instructors/teachers**.

The system also supports **Dynamic RBAC**, so the main administrator can create/manage sub-admin roles and assign permissions dynamically.

The application is built with:

- C#
- ASP.NET Core Web API
- Blazor
- Tailwind CSS
- SQL Server
- Entity Framework Core
- N-Layer Architecture
- Feature-Based Architecture
- Dependency Injection
- IHttpClientFactory
- Serilog
- Middleware
- Action/Core Filters
- JWT Authentication
- Dynamic RBAC
- Result Pattern

---

# 2. Current Development Status

## Completed

### Phase 1 — Project Foundation

Completed.

The solution structure and N-Layer architecture have already been created.

### Phase 2 — Database / EF Core

Completed.

The SQL Server database already exists:

`ELearningManagementSystem`

The database tables were created manually in SQL Server Management Studio.

EF Core Database-First scaffolding has already been completed.

The existing `DbContext` and database models must be treated as the current source of truth.

---

# 3. Important Development Rules

These rules apply to the entire project.

## Do NOT use Repository Pattern

The project intentionally does NOT use:

- Repository Pattern
- Generic Repository
- Unit of Work

Use Entity Framework Core `DbContext` directly through Dependency Injection where appropriate.

Do not introduce repository abstractions unless explicitly requested.

## Do NOT introduce unnecessary architecture

Do not add:

- CQRS
- MediatR
- Event Sourcing
- Generic Repository
- Unit of Work
- Contracts project
- SharedKernel project
- Microservices
- Unnecessary abstractions

Keep the architecture clean, maintainable, reusable, and understandable.

---

# 4. Architecture

The solution contains these projects:

```text
ELearningManagementSystem/
│
├── src/
│
│   ├── ELearningManagementSystem.App/
│   │
│   ├── ELearningManagementSystem.Api/
│   │
│   ├── ELearningManagementSystem.Application/
│   │
│   ├── ELearningManagementSystem.Domain/
│   │
│   ├── ELearningManagementSystem.Database/
│   │
│   ├── ELearningManagementSystem.Infrastructure/
│   │
│   └── ELearningManagementSystem.Shared/
│
├── logs/
├── docs/
├── README.md
└── ELearningManagementSystem.slnx
```

## Layer Responsibilities

### App

Blazor UI.

Responsibilities:

- Pages
- Components
- Feature UI
- Layout
- Tailwind CSS
- HTTP communication with API
- Authentication state
- Client-side UI state

The App project must NOT:

- Access EF Core directly
- Access SQL Server directly
- Use `DbContext`
- Contain database business logic

Communication must go through the API.

---

### Api

ASP.NET Core Web API.

Responsibilities:

- Controllers
- HTTP request/response handling
- Middleware
- Filters
- Authorization configuration
- Authentication configuration
- API-specific requests/responses
- HTTP status code mapping

Controllers should remain thin.

Controllers should call Application services rather than contain business logic.

---

### Application

Application/business use-case layer.

Responsibilities:

- Application services
- Business/use-case orchestration
- DTOs
- Interfaces
- Validators
- Result Pattern
- Pagination
- Application-level rules

Examples:

```text
AuthenticationService
UserService
RoleService
PermissionService
CourseService
LessonService
EnrollmentService
LessonProgressService
QuizService
QuizAttemptService
```

Application services should contain application/business logic.

---

### Domain

Core business model.

Responsibilities:

- Entities
- Domain rules
- Domain enums
- Core business concepts

Examples:

```text
User
Role
Permission
UserRole
RolePermission

Course
Lesson
Enrollment
LessonProgress

Quiz
Question
QuestionOption
QuizAttempt
QuizAnswer

AuditLog
```

Domain should not depend on API, Blazor, SQL Server, or infrastructure implementation details.

---

### Database

Entity Framework Core / SQL Server persistence layer.

Responsibilities:

- `AppDbContext`
- EF Core configurations
- Database mappings
- Migrations only if explicitly required
- SQL Server persistence configuration

The existing SQL Server database already exists.

Do not recreate or replace it.

Use the existing database schema and scaffolded EF Core models.

---

### Infrastructure

External/technical implementations.

Responsibilities include:

- JWT implementation
- Password hashing
- Current-user implementation
- Serilog configuration
- External API clients
- Other technical integrations

Examples:

```text
Security/
    JwtService.cs
    PasswordHasher.cs
    CurrentUserService.cs

Logging/
    SerilogConfiguration.cs

HttpClients/
```

Infrastructure implements technical concerns required by Application/API.

---

### Shared

Small common reusable definitions only.

Examples:

```text
Constants/
Enums/
Extensions/
```

Do not turn Shared into a dumping ground.

Only genuinely cross-project common items belong here.

---

# 5. Feature-Based Architecture

Organize functionality by feature where appropriate.

Main features:

```text
Authentication
Users
Roles
Permissions
Courses
Lessons
Enrollments
LessonProgress
Quizzes
QuizAttempts
Student
Admin
```

Do not create one huge global service/controller structure if feature-based organization already exists.

Keep related DTOs, validators, services, requests, responses, and UI components grouped logically by feature.

---

# 6. Database / Business Model

The main business relationships are:

```text
User
 ├── UserRole
 │     └── Role
 │            └── RolePermission
 │                    └── Permission
 │
 ├── Enrollment
 │     └── Course
 │            └── Lesson
 │
 └── QuizAttempt

Course
 └── Lesson

Lesson
 └── LessonProgress

Quiz
 └── Question
       └── QuestionOption

QuizAttempt
 └── QuizAnswer
```

The exact existing database schema and scaffolded EF Core models must be inspected before changing code.

Do not assume that a table/column exists if it has not been verified.

---

# 7. User Model

Users have:

- UserId
- Name
- Email
- Password
- Role relationship through RBAC
- CreatedAt
- UpdatedAt
- DeleteFlag

Important:

The password must never be stored as plain text.

Public registration must always create a Student.

A public user must never be able to submit a RoleId and become an Admin.

---

# 8. Authentication Design

Authentication uses:

```text
JWT Access Token
+
Refresh Token
```

Do NOT create an unlimited JWT.

Recommended:

- Short-lived Access Token
- Longer-lived Refresh Token
- Refresh Token rotation where supported by the database
- Refresh Token revocation
- Automatic token refresh from the Blazor client

Flow:

```text
Login
  ↓
Validate credentials
  ↓
Generate Access Token
  ↓
Generate Refresh Token
  ↓
Return token pair
```

When the access token expires:

```text
API request
  ↓
401
  ↓
Refresh Token
  ↓
New Access Token
  ↓
Retry original request once
```

Do not create an infinite retry loop.

If refresh fails, clear authentication state and require login.

---

# 9. Dynamic RBAC

Dynamic RBAC is a core requirement.

The authorization relationship is:

```text
User
 ↓
UserRole
 ↓
Role
 ↓
RolePermission
 ↓
Permission
```

Roles and permissions should come from the database.

Do not hard-code permission rules throughout controllers.

Example permissions:

```text
Course.Read
Course.Create
Course.Update
Course.Delete

Lesson.Read
Lesson.Create
Lesson.Update
Lesson.Delete

Quiz.Read
Quiz.Create
Quiz.Update
Quiz.Delete

User.Read
User.Update

Role.Read
Role.Create
Role.Update
Role.Delete

Permission.Read
Permission.Assign
```

The exact permission list should be based on the existing database and requirements.

## Main Admin

The main administrator can:

- Create sub-admin roles
- Assign roles
- Assign permissions
- Manage users
- Manage courses
- Manage lessons
- Manage quizzes

## Sub Admin

A sub-admin can only perform actions allowed by assigned permissions.

Example:

```text
Course Admin

Course.Read
Course.Create
Course.Update
Course.Delete
```

The sub-admin should not automatically receive unrelated permissions.

---

# 10. Course Management

Admins create and manage courses.

Course operations:

```text
Create
Read
Update
Delete
```

Students can:

```text
Browse courses
View course details
```

A course contains:

```text
CourseId
Title
Description
CreatedAt
UpdatedAt
DeleteFlag
```

Do not add unnecessary fields such as DifficultyLevel or EstimatedHours unless explicitly requested.

---

# 11. Lesson Management

A course contains multiple lessons.

Relationship:

```text
Course
 ↓
Lesson 1
Lesson 2
Lesson 3
...
```

Lesson contains:

```text
LessonId
CourseId
Title
Content
LessonOrder
CreatedAt
UpdatedAt
DeleteFlag
```

Admins can create/update/delete lessons.

Students can read lessons.

Lesson order must be respected when displaying a course.

---

# 12. Enrollment

Students can enroll in courses.

Flow:

```text
Student
 ↓
Enroll in Course
 ↓
Enrollment
```

Rules:

- User must be authenticated.
- Only appropriate users can enroll.
- Duplicate enrollment should not be allowed.
- Deleted courses cannot be enrolled in.

Students should have a "My Courses" area.

---

# 13. Lesson Progress

Track the student's progress through lessons.

Example:

```text
Course
 ├── Lesson 1  Completed
 ├── Lesson 2  Completed
 ├── Lesson 3  Current
 ├── Lesson 4  Not Started
 └── Lesson 5  Not Started
```

The system should be able to calculate course progress.

Example:

```text
Completed lessons / Total lessons
```

The exact progress rules must follow the existing database schema.

---

# 14. Quiz System

Quizzes are predefined by Admins.

Students do not create quizzes.

Structure:

```text
Quiz
 ↓
Question
 ↓
QuestionOption
```

Student attempt:

```text
QuizAttempt
 ↓
QuizAnswer
```

Quiz can be associated with a lesson/course according to the existing database design.

Recommended learning flow:

```text
Read Lesson
 ↓
Complete Lesson
 ↓
Quiz Available
 ↓
Answer Questions
 ↓
Submit
 ↓
Server Calculates Score
 ↓
Immediate Result
```

---

# 15. Quiz Scoring

Scoring must be calculated on the server.

Never trust a score sent by the client.

Example:

```text
10 questions
8 correct
80%
```

The API should return the result immediately after submission.

Example response concept:

```json
{
  "score": 80,
  "totalQuestions": 10,
  "correctAnswers": 8,
  "passed": true
}
```

The exact DTO should follow the existing architecture.

---

# 16. Student Experience

Student flow:

```text
Register
 ↓
Login
 ↓
Browse Courses
 ↓
Select Course
 ↓
Enroll
 ↓
Start Learning
 ↓
Read Lessons
 ↓
Track Progress
 ↓
Complete Lesson
 ↓
Take Quiz
 ↓
Receive Immediate Score
 ↓
Continue Learning
```

Student dashboard should eventually show:

- Enrolled courses
- Course progress
- Completed lessons
- Available quizzes
- Quiz results

---

# 17. Admin Experience

Admin flow:

```text
Login
 ↓
Admin Dashboard
 ↓
Manage Courses
 ↓
Manage Lessons
 ↓
Manage Quizzes
 ↓
Manage Users
```

Main Admin additionally:

```text
Manage Roles
Manage Permissions
Create/Manage Sub Admins
Assign Permissions
```

---

# 18. Result Pattern

The project uses a Result Pattern.

Typical concepts:

```text
Result<T>
Error
PagedResult<T>
```

Application services should return structured results.

Normal business failures should not be implemented as exceptions.

Examples:

```text
EmailAlreadyExists
InvalidCredentials
CourseNotFound
AlreadyEnrolled
PermissionDenied
QuizNotFound
InvalidQuizAttempt
```

Unexpected system errors may use exceptions and should be handled by global exception middleware.

---

# 19. Middleware

Use middleware for cross-cutting HTTP concerns.

Expected middleware includes:

```text
ExceptionMiddleware
RequestLoggingMiddleware
```

Authentication and authorization should use ASP.NET Core's standard authentication/authorization pipeline.

Do not create custom JWT validation middleware when `AddJwtBearer()` can handle it.

---

# 20. Filters

The project can use:

```text
ValidationFilter
AuditActionFilter
```

Use filters for cross-cutting controller/action concerns.

Do not put business logic inside filters.

---

# 21. Dependency Injection

All application/infrastructure services must be registered using DI.

Avoid:

```text
new Service()
```

inside controllers or other services.

Use interfaces where an abstraction is genuinely useful, especially across architectural boundaries.

Do not create interfaces for every trivial class without a reason.

---

# 22. IHttpClientFactory

The Blazor application communicates with the API through HttpClient.

Use:

```text
IHttpClientFactory
```

Do not use:

```text
new HttpClient()
```

inside components/services.

Architecture:

```text
Blazor Component
 ↓
App Feature Service
 ↓
HttpClient
 ↓
API Controller
 ↓
Application Service
 ↓
EF Core DbContext
 ↓
SQL Server
```

The Blazor project must never access the database directly.

---

# 23. Serilog

Serilog is used for application logging.

Log useful events such as:

- Requests
- Errors
- Authentication events
- Authorization failures
- Important business actions

Never log sensitive values:

- Password
- Password hash
- JWT access token
- Refresh token
- Secrets

Use structured logging.

---

# 24. API Design

Use REST-style endpoints.

Examples:

```text
/api/auth/register
/api/auth/login
/api/auth/refresh
/api/auth/logout
/api/auth/me

/api/courses
/api/courses/{id}

/api/lessons
/api/lessons/{id}

/api/enrollments
/api/enrollments/my

/api/quizzes
/api/quizzes/{id}

/api/quiz-attempts
```

Exact endpoint design should follow the existing codebase conventions.

---

# 25. DTO Rules

Do not return EF Core entities directly from API endpoints.

Use DTOs for API contracts.

Separate:

```text
Request DTO
Response DTO
```

when their responsibilities differ.

Do not expose:

- Password
- PasswordHash
- Internal database fields that should not be public
- Sensitive security information

Keep DTOs feature-specific where practical.

---

# 26. Validation

Validation should be handled at the Application layer.

Validate:

### Authentication

- Name
- Email
- Password
- Refresh Token

### Courses

- Title
- Description

### Lessons

- CourseId
- Title
- Content
- LessonOrder

### Quiz

- Quiz data
- Questions
- Options
- Correct answer rules

Do not duplicate the same business validation in multiple layers.

---

# 27. Soft Delete

Existing entities contain:

```text
DeleteFlag
```

Use soft delete where appropriate.

Do not physically delete records when the business requirement is soft delete.

Normal queries should exclude deleted records unless an administrative/history query explicitly requires them.

---

# 28. Database Rules

The SQL Server database already exists.

Database name:

```text
ELearningManagementSystem
```

Important:

- Do not create a new database.
- Do not drop the existing database.
- Do not recreate existing tables.
- Do not change schema without verifying the current database first.
- Do not assume a table/column exists.
- Inspect the existing EF Core scaffolded models before implementing features.

When database changes are genuinely required, explicitly identify the required schema change before applying it.

---

# 29. Security Rules

Always:

- Hash passwords securely.
- Validate JWT lifetime.
- Keep JWT secrets out of source control.
- Never log tokens.
- Never trust client-side scores.
- Never allow public registration as Admin.
- Validate authorization server-side.
- Validate ownership of student resources.
- Prevent users from accessing another student's private data.
- Prevent unauthorized Admin operations.

---

# 30. Development Phases

Current status:

```text
Phase 1 — Project Foundation                 COMPLETED
Phase 2 — Database / EF Core                 COMPLETED

Phase 3 — Authentication + JWT + Refresh     NEXT
Phase 4 — Dynamic RBAC
Phase 5 — Course Management
Phase 6 — Lesson Management
Phase 7 — Enrollment
Phase 8 — Lesson Progress
Phase 9 — Quiz Engine
Phase 10 — Quiz Scoring
Phase 11 — Student Dashboard
Phase 12 — Admin Dashboard
Phase 13 — User Management
Phase 14 — Role & Permission Management
Phase 15 — Audit Logging
Phase 16 — API / Blazor Integration
Phase 17 — Middleware / Filters Refinement
Phase 18 — Result Pattern Refinement
Phase 19 — Logging Refinement
Phase 20 — Validation Refinement
Phase 21 — Tailwind UI Polish
Phase 22 — End-to-End Integration / Verification
```

Some later phases may be combined when the implementation naturally belongs to the same feature. Do not create unnecessary work solely to follow phase numbers.

---

# 31. How Codex Should Work

When asked to implement a phase:

1. Inspect the existing code first.
2. Inspect existing project references.
3. Inspect the existing EF Core models and DbContext.
4. Inspect existing configuration.
5. Inspect existing implementations before creating duplicates.
6. Reuse existing code where appropriate.
7. Implement only the requested phase.
8. Do not rewrite unrelated features.
9. Do not introduce unnecessary architecture.
10. Build the solution.
11. Fix compilation errors.
12. Run relevant tests or verification.
13. Report what was changed.
14. Report remaining issues.
15. Do not implement future phases unless explicitly requested.

## Important

Do not simply return an implementation plan when the instruction says:

**IMPLEMENT**

Actually modify the codebase.

When the instruction says:

**PLAN**

Do not modify the codebase.

---

# 32. Standard Phase Prompt

Use this pattern when continuing development:

```text
Phase [X] is the next phase.

Inspect the existing project and understand the current implementation before making changes.

Implement Phase [X] from PROJECT_CONTEXT.md.

Do not give me only an implementation plan.

Actually create/modify the required files, integrate with the existing architecture, build the solution, and verify the implementation.

Do not implement future phases.

Do not introduce Repository Pattern, Unit of Work, CQRS, MediatR, or unnecessary abstractions.

Reuse existing code where appropriate.

Respect the existing SQL Server database and EF Core scaffolded models.

If a database schema change is required, do not silently modify it. Clearly report the required database change first.

At the end, report:
1. Files created/modified
2. Main functionality implemented
3. API endpoints added/changed
4. Database changes, if any
5. Build result
6. Verification/test result
7. Remaining issues
```

For the current project, the next command should be:

```text
Phase 3 is the next phase.

Implement Authentication + JWT + Refresh Token from PROJECT_CONTEXT.md.

Do not implement Dynamic RBAC yet.
```

---

# 33. Current Immediate Next Step

The project is currently at:

```text
Phase 2 — COMPLETED
```

Therefore the next implementation is:

```text
Phase 3
Authentication + JWT + Refresh Token
```

The expected Phase 3 flow is:

```text
Register
   ↓
Password Hashing
   ↓
Create User
   ↓
Assign Student Role
   ↓
Login
   ↓
JWT Access Token
   +
Refresh Token
   ↓
Authenticated API
   ↓
Automatic Refresh
   ↓
Logout / Revoke Refresh Token
```

After Phase 3 is stable:

```text
Phase 4
Dynamic RBAC
```

Then:

```text
User
 ↓
Role
 ↓
Permission
 ↓
Authorization
```

Only after authentication and authorization are stable should the main learning features be implemented.

---

# 34. Final Architecture Principle

Keep the project:

- Simple
- Maintainable
- Reusable
- Feature-oriented
- Secure
- Easy to understand
- Appropriate for a single E-Learning application

Prefer simple code over unnecessary enterprise patterns.

The goal is not to maximize the number of layers or abstractions.

The goal is to maintain a clear separation:

```text
UI
 ↓
API
 ↓
Application
 ↓
Domain
 ↓
Database

Infrastructure
 └── technical implementations
```

and:

```text
Authentication
 ↓
Authorization
 ↓
Business Features
```

This architecture should be preserved throughout the project.
