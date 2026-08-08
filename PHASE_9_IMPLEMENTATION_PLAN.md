# Phase 9 — Course Final Quiz Engine: Detailed Implementation Plan

## 1. Objective

Implement administrator management for predefined **final quizzes attached to courses**. Admins can create, edit, archive, restore, and view a course quiz with questions and answer options. Students do not create or administer quizzes; student attempts and scoring belong to Phase 10.

## 2. Current State

Phases 1–8 establish EF Core database-first persistence, JWT, Dynamic RBAC, active/archived content, enrollment, and lesson progress/course completion. The repository contains scaffolded quiz-related entities (`Quiz`, `Question`, `QuestionOption`, and/or legacy-named quiz equivalents), but no verified Quiz Application feature, API controller, admin client, or UI. The exact models must be inspected before implementation.

## 3. Scope

- Manage a quiz at the course level only.
- Manage quiz title/settings supported by the existing schema.
- Manage questions and their answer options.
- Validate correct-answer configuration.
- Archive/restore quiz content if existing `DeleteFlag` fields support it.
- Provide secure admin UI and RBAC-protected APIs.

Excluded: student quiz taking, answer submission, scoring, attempts/history, lesson quizzes, or database changes without approval.

## 4. Business Rules

1. A quiz belongs to an active course, not an individual lesson.
2. Prefer one active final quiz per course; verify whether database constraints/model already enforce this.
3. Archived course/quiz/question/option data is not presented as active student content.
4. A question must have valid non-empty text and at least the required number of active options.
5. Correct-answer rules must match the existing question model (single choice versus multi-select); do not assume either model.
6. Admin changes must never expose correctness metadata through any future student-safe response.
7. Archive/restore uses `DeleteFlag` where verified; never physically delete quiz data.

## 5. Database Impact

Inspect `AppDbContext` and the actual scaffolded tables/entities first: `Quizzes`, `Questions`, `QuestionOptions`, possible `QuizQuestions`/`QuizOptions`, `Courses`, and related attempts. Confirm keys, foreign keys, `CourseId`, `DeleteFlag`, timestamps, option correctness field, ordering columns, max lengths, and cascade behavior. No schema change is planned. If the database cannot support course-level quiz linkage or correct option data, document the smallest required change before implementation.

## 6. Domain Layer

Reuse existing scaffolded entities. Add a small enum only where the existing schema clearly represents a shared domain concept such as question type. Do not create parallel Quiz/Question entities or a repository layer.

## 7. Application Layer

Create a feature-oriented `Quizzes` folder with `IQuizService`/`QuizService`, DTOs, validators, and EF Core use cases.

Expected DTOs:

- `CreateQuizRequest`, `UpdateQuizRequest`, `QuizResponse`, `QuizDetailResponse`
- `CreateQuestionRequest`, `UpdateQuestionRequest`, `QuestionResponse`
- `CreateQuestionOptionRequest`, `UpdateQuestionOptionRequest`, `QuestionOptionResponse`
- Admin list/filter query DTOs where needed

Expected Result errors include: `QuizNotFound`, `CourseNotFound`, `CourseArchived`, `QuizAlreadyExistsForCourse`, `QuestionNotFound`, `OptionNotFound`, `InvalidQuestionConfiguration`, `InvalidCorrectAnswer`, `CannotRestoreArchivedCourse`, and `PermissionDenied`.

Use direct injected `IAppDbContext`; transactions are required for multi-row question/option changes.

## 8. API Layer

Use thin REST controllers following established route conventions:

| Endpoint | Purpose | Permission |
| --- | --- | --- |
| `GET /api/courses/{courseId}/quizzes` | Admin course quiz list | `Quiz.Read` |
| `POST /api/courses/{courseId}/quizzes` | Create final quiz | `Quiz.Create` |
| `GET /api/quizzes/{quizId}` | Admin detail | `Quiz.Read` |
| `PUT /api/quizzes/{quizId}` | Update quiz | `Quiz.Update` |
| `POST /api/quizzes/{quizId}/archive` | Archive | `Quiz.Delete` |
| `POST /api/quizzes/{quizId}/restore` | Restore | `Quiz.Update` or verified existing convention |
| nested question/option endpoints | Manage aggregate items | matching `Quiz.*` permission |

Map Results consistently: validation `400`, absent resources `404`, unauthenticated `401`, missing permission `403`, and conflict according to current Result conventions.

## 9. Infrastructure Layer

Reuse current JWT/current-user service, Dynamic RBAC policy provider, Serilog, exception middleware, validation filter, and DI. Log safe quiz administration actions using IDs/action names only. Do not add external services or duplicate authorization/logging systems.

## 10. Database Layer

Use EF Core `DbContext` projections for list/detail reads and transactions for aggregate mutation. Filter active versus archived records server-side. Confirm that archives do not break historical `QuizAttempts`; do not cascade physical deletion. Apply no migrations unless an approved schema change is needed.

## 11. Blazor App

Add `QuizApiClient` via `IHttpClientFactory` and admin pages/components:

- Course quiz list/status view.
- Create/edit quiz form.
- Question editor with option editor and correct-answer selection.
- Active/Archived/All filter if schema supports archive.
- Reuse `ConfirmationModal` for Archive/Restore.
- Clear loading, saving, validation, empty, 401, 403, and error states.

Use Tailwind and existing shared components. Do not create student attempt UI in this phase.

## 12. RBAC / Permissions

Use database-backed existing `Quiz.Read`, `Quiz.Create`, `Quiz.Update`, and `Quiz.Delete` permission codes. Do not hard-code Admin checks. UI visibility is convenience only; every endpoint must use Phase 4 server authorization.

## 13. Validation

- Parent course exists and is active.
- Quiz title/settings fit verified schema limits.
- Question belongs to the requested quiz.
- Options belong to the requested question.
- Required active options exist before a quiz can be made available later.
- Correct option references only an option in that question.
- Archive/restore dependencies are checked safely.

## 14. Result Pattern

Application methods return existing `Result<T>`/`Result`; normal business failures never use exceptions. API and Blazor translate stable errors into user-friendly messages while retaining error codes.

## 15. Security Considerations

Only authorized quiz administrators manage content. Do not accept parent IDs blindly—validate relationships server-side. Correct-answer flags/options must be excluded from all future student quiz DTOs. Prevent archived course content from being restored into an inconsistent state.

## 16. Implementation Steps

1. Audit actual quiz/question/option models and current database schema.
2. Confirm course-final quiz cardinality and question correctness model.
3. Add feature DTOs, validators, Result errors, service interface, and service implementation.
4. Implement transaction-safe quiz/question/option operations.
5. Register DI and add RBAC-protected API endpoints.
6. Build App client and Tailwind admin pages using reusable modal.
7. Add logging and API/Application tests.
8. Build solution and verify active/archived/RBAC flows.

## 17. Verification Checklist

- Authorized admin can create one valid final course quiz.
- Unauthorized/student requests receive `401`/`403`.
- Invalid question/option/correct-answer configurations are rejected.
- Nested resource ID tampering is rejected.
- Archive hides active quiz data without physical deletion.
- Restore respects active parent-course dependency.
- Correct-answer data is not present in student-safe contracts.
- Build and relevant tests pass.

## 18. Definition of Done

Authorized admins can securely manage valid course-final quiz content using existing EF Core, Result Pattern, RBAC, archive terminology, and shared Blazor UI patterns. No student attempt/scoring behavior or unapproved schema change has been introduced.
