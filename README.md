# E-Learning Management System

A production-style E-Learning Management System built with C#, ASP.NET Core Web API, Blazor WebAssembly, Tailwind CSS, Entity Framework Core, SQL Server, and JWT Authentication.

---

## 🏛️ Architecture Overview

The system follows a strict **N-Layer Architecture** designed for maintainability, separation of concerns, and clean boundaries:

```
ELearningManagementSystem/
│
├── src/
│   ├── ELearningManagementSystem.App/            # Blazor WebAssembly UI (Frontend)
│   ├── ELearningManagementSystem.Api/            # ASP.NET Core Web API (HTTP Entry Point)
│   ├── ELearningManagementSystem.Application/    # Business Logic & Workflows
│   ├── ELearningManagementSystem.Domain/         # Domain Entities & Business Enums
│   ├── ELearningManagementSystem.Database/       # EF Core AppDbContext & Migrations
│   ├── ELearningManagementSystem.Infrastructure/ # Security (JWT, Hashing) & Technical Services
│   └── ELearningManagementSystem.Shared/         # Common Constants & Utilities
│
├── logs/                                         # Application logs (Serilog output)
├── docs/                                         # Project documentation
├── README.md
└── ELearningManagementSystem.slnx
```

---

## 🎯 Architectural Principles & Constraints

- **No Repository Pattern**: Entity Framework Core `AppDbContext` and `DbSet` already act as unit of work and repositories. Direct EF Core querying in Application services via `IAppDbContext`.
- **No Unit of Work Abstraction**: Direct EF Core context usage through `IAppDbContext`.
- **No CQRS / No MediatR**: Direct service method calls for simplicity and performance without unnecessary indirections.
- **Dynamic RBAC**: Fine-grained claim-based permission authorization (`Admin` vs `Student`).
- **Result Pattern**: Uniform operation result handling with `Result` and `Result<T>` types.
- **IHttpClientFactory**: Blazor WASM client communicates exclusively via API HTTP calls.

---

## 📂 Project Responsibilities

### 1. `ELearningManagementSystem.App` (Blazor WebAssembly Standalone)
- **Role**: Pure Client-Side UI (SPA)
- **Responsibilities**:
  - UI Pages, Components, Layouts
  - Interactive User Flow
  - API Integration via `IHttpClientFactory`
  - JWT storage (`Blazored.LocalStorage`) & Auth state (`AuthenticationStateProvider`)
- **Strict Boundaries**: No direct SQL, EF Core, or server-side code.

### 2. `ELearningManagementSystem.Api` (ASP.NET Core Web API)
- **Role**: Backend HTTP API Gateway
- **Responsibilities**:
  - Thin API Controllers
  - Middleware (Global Exception Handling)
  - Action Filters (`ValidateModelFilter`)
  - Authorization Handlers (`PermissionAuthorizationHandler`)
  - Serilog Logging & Swagger Documentation

### 3. `ELearningManagementSystem.Application`
- **Role**: Application Core & Business Orchestration
- **Responsibilities**:
  - Service Contracts & Implementations (`CourseService`, `AuthService`, etc.)
  - DTOs (Data Transfer Objects)
  - FluentValidation Request Validators
  - Result Pattern primitives (`Result`, `Result<T>`)

### 4. `ELearningManagementSystem.Domain`
- **Role**: Enterprise Domain Model
- **Responsibilities**:
  - Domain Entities (`User`, `Role`, `Permission`, `Course`, `Lesson`, `Quiz`, `Enrollment`, `QuizAttempt`)
  - Domain Enums (`UserStatus`, `CourseStatus`)
- **Strict Boundaries**: Zero external dependencies, framework-agnostic.

### 5. `ELearningManagementSystem.Database`
- **Role**: Data Persistence Layer
- **Responsibilities**:
  - `AppDbContext` implementing `IAppDbContext`
  - EF Core Entity Configurations & Relationships
  - Migrations & SQL Server integration

### 6. `ELearningManagementSystem.Infrastructure`
- **Role**: Technical Infrastructure & Cross-Cutting Implementations
- **Responsibilities**:
  - JWT Token Generation (`JwtTokenService`)
  - Password Hashing (`PasswordHasher` using BCrypt)
  - External Service Integration

### 7. `ELearningManagementSystem.Shared`
- **Role**: Truly Common Utilities & Constants
- **Responsibilities**:
  - Global Constants (`AppConstants`)
  - Shared Helper Extensions (`StringExtensions`)

---

## 👥 User Roles & Key Workflows

### Admin
- Dynamic Role & Permission Management
- Sub-Admin Access Management
- Course, Lesson, and Quiz Creation & Management
- User Management

### Student
- Account Registration & Login
- Course Browsing & Enrollment
- Interactive Lesson Viewing & Progress Tracking
- Taking Predefined Quizzes with Immediate Score Calculation

---

## 🛠️ Technology Stack

- **Language**: C# (.NET 8.0)
- **Web API**: ASP.NET Core Web API + Swagger / OpenAPI
- **Frontend**: Blazor WebAssembly Standalone + Tailwind CSS v4
- **Database**: SQL Server + Entity Framework Core 8
- **Authentication**: JWT Bearer Tokens + Custom Claims
- **Logging**: Serilog (Console & Rolling File Sinks)
- **Validation**: FluentValidation
