# AGENTS.md â€” E-Learning Management System

Persistent project knowledge + active skill instructions for every session. READ THIS FIRST.

## Active Skills (must apply in every relevant prompt)

Loaded and stored at `.opencode/skills/`:

1. **blazor-expert** â€” `.opencode/skills/blazor-expert/SKILL.md`. Comprehensive Blazor (Server/WASM/Hybrid) expertise: components, lifecycle, state, routing, forms/validation, auth/authorization, performance. Follow its Orchestration Protocol:
   - UI Building â†’ `resources/components-lifecycle.md`
   - State Handling â†’ `resources/state-management-events.md`
   - Navigation â†’ `resources/routing-navigation.md`
   - Data Input â†’ `resources/forms-validation.md`
   - User Access â†’ `resources/authentication-authorization.md`
   - Speed/Efficiency â†’ `resources/performance-advanced.md`
2. **ui-ux-designer** â€” `.opencode/skills/ui-ux-designer/SKILL.md`. Design systems, accessibility-first (WCAG 2.1 AA), Tailwind v4 styling, states/feedback/responsive. Apply to every UI change.

Always consult these before/while writing or modifying Blazor components, pages, or UI styling. Do not invent patterns the skills forbid or that contradict the project conventions below.

## Project Overview

E-Learning Management System (W3Schools-inspired). Two business user types: **Student** and **Admin** (plus sub-admins via dynamic RBAC). No instructors. Built with C#/.NET 8, ASP.NET Core Web API, Blazor WebAssembly, Tailwind CSS v4, EF Core 8, SQL Server, JWT auth, Serilog.

Full context: `PROJECT_CONTEXT.md` (architecture rules, phase plan 3â€“22, security rules, development conventions). Phase status: through Phase 22 implementation (logging/refinement), `PHASE_*.md` files at repo root are the per-phase plans.

## Solution Structure (src/)

```
ELearningManagementSystem.slnx
src/
â”œâ”€â”€ ELearningManagementSystem.App            # Blazor WASM UI (frontend)
â”œâ”€â”€ ELearningManagementSystem.Api            # ASP.NET Core Web API (thin controllers)
â”œâ”€â”€ ELearningManagementSystem.Application    # Business logic/services/DTOs/validators
â”œâ”€â”€ ELearningManagementSystem.Domain         # Entities + enums (framework-free)
â”œâ”€â”€ ELearningManagementSystem.Database       # AppDbContext + EF configs (database-first)
â”œâ”€â”€ ELearningManagementSystem.Infrastructure # JWT, BCrypt, CurrentUser, RBAC pipeline
â””â”€â”€ ELearningManagementSystem.Shared         # AppConstants, StringExtensions
logs/   docs/   README.md   PROJECT_CONTEXT.md
```

## HARD Architecture Rules (never violate)

- **NO Repository Pattern, NO Unit of Work, NO CQRS, NO MediatR, NO unnecessary abstractions.** Query EF Core `IAppDbContext` directly from Application services.
- **No Repository/Contracts/SharedKernel projects.** Feature-based folders in Application: each feature = `DTOs/`, `Services/`, `Validators/`.
- **Layers only depend downward:** App â†’ Api â†’ Application â†’ Domain + Infrastructure (technical). App NEVER touches EF Core/DB â€” only via HTTP API.
- **Controllers must be thin** â€” delegate to Application services, no business logic.
- **DTO-only API contracts** â€” never return entities; never expose password/hash/tokens.
- **Result Pattern** (`Application/Common/Result.cs`): `Result` / `Result<T>` with string error codes (e.g. `"PermissionDenied"`, `"CourseNotFound"`, `"AlreadyEnrolled"`). Business failures are NOT exceptions. Unexpected errors â†’ exceptions â†’ global middleware.
- **Soft delete**: all entities have `DeleteFlag`. Archive/restore endpoints. Normal queries filter `!DeleteFlag`; some "archive" queries filter `DeleteFlag == isArchived`.
- **Dynamic RBAC**: permissions come from DB (User â†’ UserRole â†’ Role â†’ RolePermission â†’ Permission). Authorization via JWT `permission` claims + policies `Permission:<Code>`.
- **JWT + Refresh token rotation** (refresh tokens hashed in DB, rotation via ReplacedByTokenId, revocation on logout).
- **Server-side scoring** for quizzes â€” never trust client scores.
- **Public registration always = Student**; never trust client RoleId.

## Auth & Security Details

- Access token 60 min (JwtSettings), refresh token 7 days, stored hashed (SHA256 base64) in `RefreshTokens`.
- JWT claims: NameIdentifier, Email, Name, Jti, Iat, one `role` claim per role, one `permission` claim per permission.
- API `Program.cs`: Serilog, Swagger w/ Bearer, memory cache, JWT bearer (strict `ClockSkew = TimeSpan.Zero`), CORS `BlazorClient` (origins from `AllowedOrigins`), middleware order: ExceptionHandling â†’ SerilogRequestLogging â†’ HTTPS â†’ StaticFiles â†’ CORS â†’ Authentication â†’ Authorization â†’ Controllers.
- Blazor client: tokens in localStorage (`authToken`, `refreshToken`); `AuthHttpHandler` injects Bearer and does ONE 401â†’refreshâ†’retry; `CustomAuthStateProvider` builds principal from JWT claims; `ClientPermissionPolicyProvider` evaluates `Permission:*` policies client-side (`Permission.Read` satisfied by `Permission.Assign`).
- `IPermissionService` (server) caches per-user permissions (10-min, versioned cache key; `InvalidatePermissionCache()` bumps version).
- Protected-account rules in UserService/RoleService: only System Administrator can modify Administrator/System Administrator accounts/roles; last-admin & last-sysadmin guards; privilege-escalation guard (non-SuperAdmin can only assign permissions they possess); `SuperAdmin` role permissions locked to prevent lockout.

## Domain Entities (Database namespace `ELearningManagementSystem.Domain.Entities`)

- **User**: UserId, FullName, Email (unique), PasswordHash (BCrypt), Status(bool), CreatedAt, UpdatedAt?, DeleteFlag. Navs: AuditLogs, Courses, Enrollments, QuizAttempts, RefreshTokens, UserRole (assigned-by + users).
- **Role**: RoleId, RoleName (unique), Description?, CreatedAt. Built-ins: `Student`, `Administrator`, `System Administrator` (renamed from Admin/SuperAdmin in Aug 2026; Content Manager + Auditor sub-admin roles removed).
- **Permission**: PermissionId, PermissionCode (unique), PermissionName, Module.
- **UserRole**: UserId, RoleId, AssignedBy?, AssignedAt.
- **RolePermission**: RoleId, PermissionId.
- **Category**: CategoryId, CategoryName, Description?, CreatedAt, UpdatedAt?, DeleteFlag. (courses FK)
- **Course**: CourseId, CategoryId, Title, Description?, ThumbnailUrl?, Status(bool, default true), CreatedBy, CreatedAt, UpdatedAt?, DeleteFlag. Navs: Lessons, Quizzes, Enrollments.
- **Lesson**: LessonId, CourseId, Title, Content, DisplayOrder, CreatedAt, UpdatedAt?, DeleteFlag. (progress nav)
- **Enrollment**: EnrollmentId, UserId, CourseId, EnrollDate, Completed, CompletedDate?.
- **LessonProgress**: LessonProgressId, EnrollmentId, LessonId, Completed, CompletedDate?.
- **Quiz**: QuizId, CourseId, Title, PassingScore, CreatedAt, UpdatedAt?, DeleteFlag. (One final quiz per course, admin-defined.)
- **Question**: QuestionId, QuizId, QuestionText, DisplayOrder. â†’ **QuestionOption**: OptionId, QuestionId, OptionText, IsCorrect.
- **QuizAttempt**: AttemptId, QuizId, UserId, Score(decimal(5,2)), CorrectAnswers, TotalQuestions, Passed, SubmittedAt. â†’ **QuizAnswer**: QuizAnswerId, AttemptId, QuestionId, SelectedOptionId.
- **RefreshToken**: RefreshTokenId, UserId, TokenHash (unique-ish), ExpiresAt, CreatedAt, RevokedAt?, ReplacedByTokenId?. Computed: IsExpired/IsRevoked/IsActive.
- **AuditLog**: AuditLogId, UserId, Action, TableName, RecordId, CreatedAt.
- Enums: `UserStatus`, `CourseStatus` (both in Domain/Enums; `Status` bool on entities).

## Permission Codes (used across codebase)

`Course.Read/Create/Update/Delete`, `Lesson.Read/Create/Update/Delete`, `Quiz.Read/Create/Update/Delete`, `Category.Read/Create/Update/Delete`, `User.Read/Update`, `Role.Read/Create/Update/Delete`, `Permission.Read/Assign`, `AuditLog.Read`.

## API Endpoints (controllers in Api/Controllers)

- **AuthController** `api/auth`: `POST register`(public, â†’ Student), `POST login`(â†’ access+refresh), `POST refresh`(rotation), `POST logout`(revoke, AllowAnonymous), `GET me`(Authorize). FluentValidation injected into controller.
- **CoursesController** `api/courses`: GET list (`Permission:Course.Read`), GET `{id}`, POST (multipart w/ thumbnail, `Course.Create`), PUT `{id}` (multipart, `Course.Update`), POST `{id}/archive` (`Course.Delete`), POST `{id}/restore` (`Course.Update`). Thumbnails: `wwwroot/uploads/courses/{GUID}{ext}`, â‰¤5MB, jpg/jpeg/png/webp, MIME image/*.
- **CategoriesController** `api/categories`: full CRUD + archive/restore (`Category.*` policies). `ToActionResult()` maps errors: PermissionDeniedâ†’403, *NotFound*â†’404, *Already/Conflict/CategoryInUse*â†’409, else 400.
- **LessonsController** `api/courses/{courseId}/lessons`: list/get/create/update/archive/restore (`Lesson.*`). DisplayOrder auto-appends (0/neg = max+1), reorder shifts, then resequence to 1..N.
- **EnrollmentsController** (`[Authorize]` class): POST `api/courses/{courseId:int}/enrollments`, GET `api/enrollments/my`, GET `api/courses/{courseId:int}/enrollment` (â†’ `{isEnrolled}`). Error body shape is `{ error }` (lowercase â€” inconsistent with rest).
- **LessonProgressController** (`[Authorize]`): GET `api/courses/{courseId}/progress`, POST `api/courses/{courseId}/lessons/{lessonId}/complete`.
- **QuizzesController** `api` mixed routes: GET `api/courses/{courseId}/quizzes`, POST (create), GET `api/quizzes/{quizId}`, PUT `api/quizzes/{quizId}`, POST archive/restore; questions: POST `api/quizzes/{quizId}/questions`, PUT `api/questions/{questionId}`, DELETE; options: GET `api/questions/{questionId}/options`, POST, PUT `api/options/{optionId}`, DELETE, POST `{optionId}/archive|restore`.
- **QuizAttemptsController** (`[Authorize]` class, student): GET `api/courses/{courseId}/final-quiz`, POST `api/courses/{courseId}/final-quiz/attempts` (submit â†’ server scores), GET `.../final-quiz/attempts` (history), GET `api/quiz-attempts/{attemptId}` (ownership-checked).
- **RolesController** `api/Roles`: GET list (`Role.Read`), GET `{id}`, POST (create, privilege guards), PUT `{id}`, PUT `{id}/permissions` (`Permission.Assign`).
- **UsersController** `api/Users`: GET list (`User.Read`), GET `roles`, GET `{id}`, PUT `{id}`, POST `{id}/archive|restore` (`User.Update`). Admin protections (last-admin/superadmin, protected accounts).
- **PermissionsController** `api/Permissions`: GET all (manual gate: `Permission.Read|Permission.Assign` claim OR Administrator/System Administrator role).
- **AuditLogsController** `api/audit-logs`: GET list w/ filters (search/action/table/userId/date-range), GET `{id}` (`AuditLog.Read`).
- **AdminDashboardController** GET `api/admin/dashboard` (permission-gated metrics + quick actions).
- **StudentDashboardController** GET `api/student/dashboard` (per-course progress, next lesson, quiz availability).
- **HealthController** GET `api/Health/db-check` (public, CanConnect + counts).
- **TestAuthController** `api/test-auth` (`[Authorize]` + `[HasPermission(...)]` attribute) â€” used by Profile page diagnostics.

## Application Services (business logic owners)

Key Result error codes per service (strings): CourseService `PermissionDenied, InvalidCourseTitle, CategoryNotFound, CourseNotFound, CannotRestoreArchivedCategory`; CategoryService `CategoryNameRequired, CategoryAlreadyExists, CategoryNotFound, CategoryInUse`; LessonService `CourseNotFound, InvalidLessonTitle, InvalidLessonContent, LessonNotFound, CannotUpdateLessonOfArchivedCourse, CannotRestoreArchivedCourse`; EnrollmentService `AccountNotFound, CourseNotFound, CourseArchived, AlreadyEnrolled`; LessonProgressService `LessonProgress.EnrollmentNotFound, LessonProgress.LessonNotFound, LessonProgress.LessonArchived`; RoleService `RoleNotFound, RoleAlreadyExists, CannotCreateSuperAdminRole, Unauthorized, PrivilegeEscalation, ProtectedRole, Cannot change the name of the built-in role...`; UserService `Unauthorized, UserNotFound, UserArchived, ProtectedAccount, PrivilegeEscalation, RoleNotFound, CannotRemoveLastAdmin, CannotRemoveLastSuperAdmin, CannotArchiveSelf, UserAlreadyArchived, CannotArchiveLastAdmin, CannotArchiveLastSuperAdmin, UserNotArchived`; QuizAttemptService `CourseNotFound, EnrollmentNotFound, CourseNotCompleted, QuizNotFound, ValidationError, InvalidQuestion, InvalidOption, AttemptNotFound`.

Quiz flow rules: final quiz unlocks only after ALL active lessons completed (`CourseNotCompleted` gate). Scoring on server: `score = correct/total*100`, `passed = score >= PassingScore`; passing also marks Enrollment.Completed=true + CompletedDate. Student quiz DTOs omit `IsCorrect` (admin DTOs include it).

Admin dashboard gate: user must have at least one of `User.Read, Role.Read, Category.Read, Course.Update, Lesson.Update, Quiz.Update`. Metrics per module gated individually.

## Blazor App Conventions (App project)

- **Program.cs**: Blazored.LocalStorage; `ClientPermissionPolicyProvider`; `CustomAuthStateProvider`; `AuthHttpHandler`; named HttpClient `"ELearningApi"` (+ `"AuthApi"` unauthenticated for auth service); feature ApiClients registered scoped. `ApiBaseUrl` from `wwwroot/appsettings.json` = `https://localhost:5001`.
- **Routing/auth pattern**: pages use `[Authorize(Policy = "Permission:X.Y")]`; UI buttons wrapped in `<AuthorizeView Policy="Permission:X.Y">`; `App.razor` redirects unauthenticated to `/login?returnUrl=`; admin pages route from `Pages/Admin/**`, student from `Pages/Student/**`.
- **Shared UI components** (Components/UI): `PageHeader`, `SkeletonCard`, `EmptyState`; Components/Shared: `ConfirmationModal`; Layout: `MainLayout` (NavMenu + body), `AuthLayout` (login/register), `NavMenu` (dashboard/courses/manage-*/audit-logs links, profile dropdown, logout confirm).
- **Page state pattern** (all admin/CRUD pages): `IsLoading` skeleton â†’ `EmptyState` when no data â†’ table/grid; dismissible `ErrorMessage` banner; `IsSaving`/`IsDeleting` disable buttons; 400â€“500 ms debounced search (`System.Timers.Timer`, `IDisposable`); page-size select (10/15/25/50); ConfirmationModal for archive/restore/delete; row number = `(Page-1)*PageSize + i + 1`.
- **ApiClient pattern**: return `PagedResult<T>` for lists, raw `HttpResponseMessage` for admin CRUD, `(bool Success, T? Data, string? Error)` tuples for dashboards/attempts/enrollment, `ApiResult<T>` for lesson completion. Errors normalized via `ApiResponseHelper` / `AuthApiService.GetErrorMessage`. `CustomApiException` carries HttpStatusCode.
- **Tailwind v4**: source `Styles/app.css` (with `@theme`/tokens) compiled to `wwwroot/css/app.css`; npm `package.json` in App project; `@tailwindcss/cli`. Keep custom component classes consistent.
- Routes: `/courses`, `/courses/{Id}`, `/courses/{CourseId}/lessons/{LessonId}`, `/courses/{CourseId}/quiz`, `/courses/{CourseId}/quiz/results/{AttemptId}`, `/my-courses`, `/dashboard` + `/`, `/profile`, `/login`, `/register`, `/admin/dashboard` (component), `/admin/courses`, `/admin/courses/{CourseId}/lessons`, `/admin/courses/{CourseId}/quizzes`, `/admin/categories`, `/admin/users`, `/admin/roles`, `/admin/audit-logs`.

## Config

- DB: `Server=localhost;Database=ELearningManagementSystem;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true` (Windows auth, SQL Server).
- JwtSettings: Issuer `ELearningManagementSystem`, Audience `ELearningManagementSystem.Client`, ExpirationInMinutes 60. SecretKey is a dev secret in appsettings.json.
- CORS AllowedOrigins: `https://localhost:7133`, `http://localhost:5137`, `https://localhost:5002`.
- Serilog: console + rolling file `../../logs/log-.txt` (30 files). Never log passwords/tokens/secrets.

## Standard Workflow for Changes

1. Inspect existing code/conventions first; reuse existing services/clients/components.
2. Respect layer boundaries & PROJECT_CONTEXT rules.
3. Verify DB schema assumptions against `AppDbContext`/entities before writing queries.
4. Use Result pattern + existing error codes; map to HTTP via controller conventions.
5. For UI: apply ui-ux-designer skill (states, accessibility, consistency) + blazor-expert patterns.
6. Build (`dotnet build`) and verify; report files changed, functionality, endpoints, DB changes, build/verification result, remaining issues.

## Repository / Git

- Branch `master`, currently ahead of `origin/master`. Commit style: `# NN <short description>` (e.g. "19 Logging Refinement").
- Only commit when explicitly asked.
