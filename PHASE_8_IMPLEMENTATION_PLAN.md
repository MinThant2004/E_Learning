# Phase 8 — Lesson Progress and Course Completion: Detailed Implementation Plan

## 1. Objective

Track each enrolled student’s completion of active lessons, calculate course progress, and provide a reliable course-completion state. This phase makes the learning flow measurable and supplies the prerequisite for unlocking the final course quiz in later phases.

## 2. Current State

Phases 1–7 provide authentication, Dynamic RBAC, active/archived course and lesson management, lesson learning pages, and student enrollment/My Courses. The database-first model includes `LessonProgress`, but no dedicated progress Application feature, API endpoints, client service, completion controls, or progress UI should be assumed complete until inspected.

## 3. Scope

- Read a current student’s lesson status and course progress.
- Mark an active lesson completed for the current enrolled student.
- Make completion idempotent; optionally support undo only if the existing product behavior requires it.
- Calculate progress using active lessons only.
- Show completion state in the course-learning and My Courses experiences.

Out of scope: quiz creation/taking/scoring, certificates, a separate course-completion table, and changing the database schema without prior approval.

## 4. Business Rules

1. Only an authenticated student enrolled in the course may update that course’s lesson progress.
2. The student may complete only an active lesson belonging to the active enrolled course.
3. Completion records are unique per enrollment/lesson according to the existing schema.
4. Course progress is `completed active lessons / total active lessons`.
5. Archived lessons do not appear in learning navigation and do not count toward progress.
6. A course is complete when all required active lessons are complete. This phase must expose that state but must not implement the quiz itself.
7. Never accept a completion status, percentage, enrollment owner, or user ID as trusted client data.

## 5. Database Impact

Use existing `Enrollments`, `LessonProgress`, `Lessons`, and `Courses` tables. Before implementation, inspect the scaffolded `LessonProgress` entity for its primary key, foreign keys, completion/status/date fields, nullability, and uniqueness constraints. No database change is planned. If the current table cannot represent completion timestamp/status, document the exact schema gap before any change.

## 6. Domain Layer

Reuse the scaffolded `LessonProgress` entity and any existing completion enum. Add no duplicate progress entity, repository, or generic abstraction. Any derived course-completion value should remain an Application-layer calculation unless the verified existing schema explicitly persists it.

## 7. Application Layer

Add a feature-oriented `LessonProgressService` and interface using injected `IAppDbContext` directly.

Expected DTOs:

- `CompleteLessonRequest` (only when route does not provide lesson ID)
- `LessonProgressResponse`
- `CourseProgressResponse` (completed count, total active lessons, percentage, course-complete flag, lesson states)
- `MyCourseProgressResponse` where Phase 7 list needs progress data

Implement validators and Result Pattern outcomes such as `EnrollmentNotFound`, `CourseNotFound`, `CourseArchived`, `LessonNotFound`, `LessonArchived`, `LessonNotInCourse`, `ProgressAlreadyCompleted`, and `PermissionDenied`.

Use efficient projections/no-tracking reads; use an EF Core transaction for creation if duplicate completion race conditions are possible.

## 8. API Layer

Follow existing route conventions; expected endpoints are:

| Endpoint | Purpose | Access |
| --- | --- | --- |
| `GET /api/courses/{courseId}/progress` | Current student’s course/lesson progress | Authenticated + enrollment ownership |
| `POST /api/courses/{courseId}/lessons/{lessonId}/complete` | Mark active lesson complete | Authenticated + enrollment ownership |
| `GET /api/enrollments/my/progress` | Optional My Courses summary projection | Authenticated |

Controllers remain thin: derive identity from JWT/current-user service, call Application service, and map Result outcomes to HTTP statuses. Return `401` for no valid session, `403` for authorization denial, and `404` for unavailable/non-owned resources according to existing response conventions.

## 9. Infrastructure Layer

Reuse JWT/current-user services, standard authorization pipeline, Serilog, middleware, and existing DI registration. Do not add a new token, cache, or event system. Log safe structured events such as student ID, enrollment/course/lesson ID, and action—never tokens or full lesson content.

## 10. Database Layer

Use `DbContext`/EF Core directly. Verify enrollment ownership and active course/lesson state in the same business operation. Project only fields needed for progress. If database constraints do not prevent duplicate completion records, use a transaction and re-check before insertion; separately report a recommended unique constraint rather than silently applying it.

## 11. Blazor App

Create a `LessonProgressApiClient` through `IHttpClientFactory` and register it with existing App DI.

Update the course-learning page to:

- Load lesson completion states and course percentage.
- Mark the active lesson completed through the API.
- Show an ordered lesson sidebar with Active/Completed states.
- Show a progress bar and clear course-complete message.
- Keep archived lessons hidden.
- Provide loading, updating, empty, unavailable, 401, 403, and error states.

Update My Courses cards/list to show server-calculated progress. Client UI remains a presentation layer; it must not calculate authoritative completion or access the database.

## 12. RBAC / Permissions

Student progress is protected primarily by authenticated ownership validation: a student may affect only their own enrollment. Reuse existing Dynamic RBAC behavior for admin-only views if such a permission already exists; do not invent a separate authorization system. Do not grant students admin `Lesson.Update` merely to complete learning progress.

## 13. Validation

- Positive, valid course and lesson identifiers.
- Current user is active/not archived.
- Enrollment exists for the current user and course.
- Course and lesson are active and related.
- Completion operation is idempotent or returns the selected `ProgressAlreadyCompleted` Result consistently.
- Progress denominator uses active lessons only and handles zero active lessons safely.

## 14. Result Pattern

Successful operations return progress snapshots. Expected business failures return `Result<T>` rather than exceptions. The API and Blazor UI display friendly messages while retaining standardized error codes for diagnostics.

## 15. Security Considerations

The API derives the user identity from the validated JWT. It must verify resource ownership on every read/update, prevent cross-student progress access, and ignore client-supplied percentages/statuses. Archived course/lesson rules must be enforced server-side, not merely hidden in the UI.

## 16. Implementation Steps

1. Inspect current scaffolded `LessonProgress`, `Enrollment`, lesson/course entities, and actual constraints.
2. Inspect Phase 7 enrollment endpoints/DTOs and the current course-learning page.
3. Define progress DTOs, validators, service interface, and Result error conventions.
4. Implement ownership-aware read and completion use cases using EF Core.
5. Register service through DI and add thin API controller endpoints.
6. Add `LessonProgressApiClient` and update course-learning/My Courses UI.
7. Add structured logging and safe UI success/error states.
8. Build the solution and run automated/manual verification.

## 17. Verification Checklist

- Enrolled student can complete an active lesson.
- Repeating completion does not create duplicate progress rows.
- Course percentage correctly reflects active completed lessons.
- Archived lessons disappear and do not count in the denominator.
- Non-enrolled student cannot read/update progress.
- Student cannot update another student’s progress by altering route IDs.
- Course complete state appears only after all active lessons are complete.
- JWT refresh/401 handling and existing RBAC continue working.
- Solution builds successfully; no database schema is changed without approval.

## 18. Definition of Done

An enrolled student can safely complete active lessons, see accurate server-calculated course progress and course-complete state, and no student can access another student’s records. The feature uses the existing architecture, existing database model, Result Pattern, and no new repository or authorization system.
