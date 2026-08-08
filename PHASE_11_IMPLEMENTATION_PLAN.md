# Phase 11 — Student Dashboard: Detailed Implementation Plan

## 1. Objective

Create a secure, responsive Student Dashboard that gives each authenticated student a useful overview of their active learning: enrolled courses, server-calculated progress, lesson-completion state, final-quiz availability, and own quiz results.

## 2. Current State

Phases 1–10 provide authentication, Dynamic RBAC, active/archived content, enrollment/My Courses, lesson progress/course completion, and final course quiz attempts/results. The current App has course pages and learning UI, but no dedicated dashboard aggregation endpoint, dashboard page, or dashboard navigation should be assumed complete without inspecting the repository.

## 3. Scope

- One dashboard summary for the current student only.
- Enrolled active course cards with progress and next action.
- Lesson progress/completed-course indicators.
- Final quiz availability and latest/most relevant own result.
- Links to My Courses, learning, final quiz, and result history.

Out of scope: admin dashboard, user management, new student-created content, grade exports, certificates, or duplicating feature pages.

## 4. Business Rules

1. Dashboard data is private: a student sees only records tied to their own active enrollment/attempts.
2. Archived courses/lessons/quizzes are not shown as active learning actions.
3. Progress, completion, quiz availability, and score are calculated on the server; Blazor only displays returned values.
4. Dashboard must gracefully handle no enrollments, no active lessons, no available final quiz, and no prior attempts.
5. A completed course without a final-quiz attempt should show the appropriate final-quiz action; attempt behavior follows Phase 10 rules.

## 5. Database Impact

Use existing `Users`, `Enrollments`, `Courses`, `Lessons`, `LessonProgress`, `Quizzes`, `QuizAttempts`, and related question/answer tables only through read projections. No schema change is planned. Inspect actual model names/relationships and indexes before implementation; do not load or expose unnecessary answer data.

## 6. Domain Layer

Reuse existing entities. Dashboard is a read-model/use-case projection, not a new persistent domain entity. Add no repository, aggregate duplication, or new database table.

## 7. Application Layer

Add a feature-oriented `StudentDashboardService`/interface using injected `IAppDbContext` and the current-user abstraction.

Expected DTOs:

- `StudentDashboardResponse`
- `StudentCourseDashboardItem`
- `StudentQuizSummaryResponse`
- `StudentLearningActionResponse`

Each course item should include safe metadata, progress counts/percentage, completion state, next active lesson if determinable, final-quiz availability, and own latest result summary only. Return `Result<T>` failures such as `AccountNotFound` or `PermissionDenied`; normal empty collections are successful results.

## 8. API Layer

Add a thin authenticated endpoint, following current routing conventions:

| Endpoint | Purpose | Authorization |
| --- | --- | --- |
| `GET /api/student/dashboard` | Current user’s dashboard summary | Authenticated student ownership |

Derive identity from validated JWT/current-user service. Do not allow a requested user ID. Map missing/invalid authentication to `401`, authorization denial to `403`, and structured Application results consistently.

## 9. Infrastructure Layer

Reuse JWT, `ICurrentUserService`, Serilog, middleware, Result Pattern mapping, DI, and Dynamic RBAC. No new HTTP/security framework. Log only safe dashboard failures/diagnostic metrics; do not log tokens, scores with sensitive answer content, or full private payloads.

## 10. Database Layer

Use efficient EF Core projections and `AsNoTracking` queries. Filter all records by current user through enrollment/attempt relationships and by active `DeleteFlag` content. Avoid N+1 queries: aggregate progress/attempt values in a bounded set of projections or carefully batched queries. No migration/configuration change is expected.

## 11. Blazor App

Add `StudentDashboardApiClient` through existing `IHttpClientFactory`/DI and a `/student/dashboard` page.

UI sections:

- Welcome/header and concise learning summary.
- Enrolled course cards: thumbnail, title, progress bar, completed/total lessons, primary Continue/Start action.
- Final quiz availability/result badge/action.
- Empty state with Browse Courses action.
- Loading skeleton, API error, 401 refresh/login, and 403 states.

Use Tailwind, existing shared components, accessible labels/progress semantics, responsive grid/card layouts, and existing navigation conventions. Students must not see Archive/Restore/admin controls.

## 12. RBAC / Permissions

This is an authenticated student-owned read use case. Ownership filtering is mandatory. Reuse existing authorization infrastructure; do not grant students administrative Course/Lesson/Quiz permissions and do not create a second RBAC mechanism.

## 13. Validation

- Validate current authenticated user exists/is active/not archived.
- Apply safe pagination/limit only if the dashboard needs a bounded course count.
- Verify any linked next lesson/quiz belongs to an active course and active enrollment.
- Treat missing optional quiz/result data as an empty state, not a system error.

## 14. Result Pattern

Return `Result<StudentDashboardResponse>`. Business failures use existing Result error conventions. Empty dashboard is a valid success response containing empty lists and clear counts.

## 15. Security Considerations

The server determines user identity and filters every query by it. Do not accept student, enrollment, or attempt identifiers from the dashboard UI. Do not expose correct answers, other students’ results, archived content, password-related fields, refresh tokens, or internal EF entities.

## 16. Implementation Steps

1. Inspect completed Phase 7–10 services/DTOs and actual entity relationships.
2. Define dashboard read DTOs and action-selection rules.
3. Implement efficient Application projection/service and Result handling.
4. Register DI and add protected API endpoint.
5. Add App client, dashboard route, navigation link, and Tailwind UI.
6. Implement loading/empty/error/authorization states.
7. Add tests for ownership, archived filtering, progress, availability, and empty dashboard.
8. Build solution and perform responsive manual verification.

## 17. Verification Checklist

- New student with no enrollment sees friendly empty state.
- Student sees only own active enrolled courses.
- Progress matches Phase 8 server calculation.
- Completed course shows final quiz availability only when Phase 10 says available.
- Latest own attempt/result is accurate and no answer key appears.
- Archived course/lesson/quiz no longer appears as an active dashboard action.
- Direct request cannot retrieve another user’s dashboard.
- 401/403/loading/error and mobile layouts are usable.
- Build and relevant tests pass with no schema changes.

## 18. Definition of Done

Authenticated students have a fast, responsive, private dashboard that accurately summarizes their server-calculated learning state and safely links to existing course, lesson, final-quiz, and result flows without duplicating business logic or introducing new persistence architecture.
