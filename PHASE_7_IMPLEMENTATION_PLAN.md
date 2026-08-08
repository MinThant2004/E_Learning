# Phase 7 — Enrollment and My Courses: Detailed Implementation Plan

## 1. Objective

Allow an authenticated Student to enroll once in an active course and view only their own enrolled courses in a My Courses area. This phase establishes the prerequisite ownership boundary for lesson progress and final-course quizzes.

## 2. Current State

Phases 1–6 provide JWT authentication, database-backed RBAC, active/archived category/course/lesson management, public course pages, course learning pages, and `Enrollment`/`LessonProgress` scaffolded entities. The current repository has no Enrollment Application feature, controller, App API client, or My Courses page.

## 3. Scope

- Enroll current user in an active course.
- List current user’s active enrollments with safe course summaries.
- Show enrollment state on course detail and a My Courses page.
- Prevent duplicate enrollment and enrollment in archived courses.

Out of scope: lesson completion/progress, unenrollment, quiz access, certificates, dashboards, and schema redesign.

## 4. Business Rules

1. Caller must be authenticated and an active, non-archived user.
2. Only active (`DeleteFlag = 0`) courses can be enrolled in.
3. A student may have only one enrollment per course.
4. A student may read only their own enrollments.
5. Archived courses remain historically recoverable but must not appear as normal learning content.
6. Do not accept a `UserId` from the UI; derive it from the validated JWT/current-user service.

## 5. Database Impact

Use existing `Enrollments`, `Users`, `Courses`, and later `LessonProgress` tables. Inspect the scaffolded `Enrollment` fields, keys, timestamps, status fields, constraints, and navigation properties before coding. No schema change is planned. If no uniqueness constraint exists for `(UserId, CourseId)`, handle duplicates transactionally and report a constraint proposal rather than silently changing the database.

## 6. Domain Layer

Reuse the scaffolded `Enrollment` entity. Do not create duplicate entities or repositories. Add a domain enum only if the existing schema has an explicit enrollment-state field that needs a safe enum mapping.

## 7. Application Layer

Create feature-oriented `Enrollments` DTOs, validator(s), `IEnrollmentService`, and `EnrollmentService` using injected `IAppDbContext`/EF Core.

Expected DTOs:

- `EnrollCourseRequest` (course identifier only if route does not supply it)
- `EnrollmentSummaryResponse`
- `MyCourseResponse` (course metadata plus future-safe progress placeholder only when calculated server-side)
- `EnrollmentDetailResponse` when required

Expected Results: `CourseNotFound`, `CourseArchived`, `AlreadyEnrolled`, `EnrollmentNotFound`, `AccountNotFound`, `PermissionDenied`.

Use `Result<T>`/`PagedResult<T>` conventions already in the solution; expected failures are not exceptions.

## 8. API Layer

Add thin controller actions following existing routing style:

| Endpoint | Purpose | Authorization |
| --- | --- | --- |
| `POST /api/courses/{courseId}/enrollments` | Enroll the authenticated user | Authenticated Student flow |
| `GET /api/enrollments/my` | List own enrolled active courses | Authenticated |
| `GET /api/courses/{courseId}/enrollment` | Return current-user enrollment state, if needed | Authenticated |

Map validation errors to `400`, missing/invalid token to `401`, RBAC denial to `403`, unavailable course to `404`, and duplicate business result according to existing Result conventions.

## 9. Infrastructure Layer

Reuse `ICurrentUserService`, JWT authentication, Dynamic RBAC, `IHttpClientFactory`, Serilog, and global exception middleware. Add no new authentication system or database abstraction. Log enrollment events with safe IDs only.

## 10. Database Layer

Use EF Core directly through `DbContext`. Query only active courses. Create the enrollment inside a transaction/duplicate-safe operation where concurrent requests can occur. Use `AsNoTracking` and projection for read lists. Do not physically delete records.

## 11. Blazor App

Add an `EnrollmentApiClient` registered through existing DI/`IHttpClientFactory`. Update course details with an authenticated Enroll action and enrollment status. Add `/my-courses` with responsive course cards/list.

Provide loading, empty, success, duplicate, unavailable, `401`, `403`, and unexpected-error states. Keep archive/restore controls admin-only; students never see them. Do not access EF Core from Blazor.

## 12. RBAC / Permissions

Enrollment is a student-owned authenticated action; server ownership validation is required. Reuse existing permissions if the database already contains enrollment permissions; otherwise do not invent an unrelated authorization system. Any future admin enrollment reporting must wait for an explicitly seeded permission such as an existing `Enrollment.Read`.

## 13. Validation

- Valid positive course ID.
- Course exists and is active.
- Current user exists, is active, and is not archived.
- No existing enrollment for current user/course.
- List queries have bounded page/page-size if pagination is added.

## 14. Result Pattern

Successful enrollment returns a safe enrollment/course DTO. Failures return existing structured Result errors; UI translates `AlreadyEnrolled` into a friendly message without exposing internal data.

## 15. Security Considerations

All private enrollment queries filter by current JWT user ID. The API, not client UI state, enforces archived-course checks and duplicate prevention. Do not leak another student's enrollment or progress.

## 16. Implementation Steps

1. Inspect `Enrollment` model/DbContext and existing database constraints.
2. Inspect current course detail and authentication-state patterns.
3. Add enrollment DTOs, validators, service/interface, and DI registration.
4. Implement duplicate-safe EF Core enrollment and own-enrollment queries.
5. Add API endpoints and Result-to-HTTP mapping.
6. Add App API client and My Courses page.
7. Add course-detail enrollment state/action and UI states.
8. Add logging and automated/manual tests.
9. Build the solution and verify against the actual SQL Server schema.

## 17. Verification Checklist

- Authenticated student enrolls in an active course.
- Duplicate enrollment is rejected without duplicate row creation.
- Archived/missing course cannot be enrolled in.
- My Courses returns only current user's records.
- Unauthenticated request returns `401`; cross-user data cannot be read.
- Student UI does not expose archive/restore operations.
- Solution builds with no Phase 7 regressions.

## 18. Definition of Done

Students can securely enroll in active courses and use My Courses; duplicates, archived content, and cross-user access are blocked server-side; no schema change or repository pattern was introduced.
