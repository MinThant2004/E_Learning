# Phase 6 — Lesson Management: Implementation Plan

## Purpose

Implement lesson management for courses completed in Phase 5. Authorized admins/sub-admins manage lessons; students and visitors can view active lessons in the correct course order.

This phase creates and manages lesson content only. It does **not** enroll students, mark lessons complete, calculate progress, or show/take quizzes. The requested quiz remains a final **course** quiz in later phases, never a quiz after each lesson.

## Scope

### Included

- Lesson create, list, read, update, reorder, and soft-delete use cases.
- Course-to-lesson relationship validation.
- Public/student read-only lesson navigation for active courses.
- Dynamic RBAC enforcement: `Lesson.Read`, `Lesson.Create`, `Lesson.Update`, `Lesson.Delete`.
- Application DTOs, validators, Result Pattern outcomes, REST endpoints, Blazor UI, logging, and tests.

### Excluded

- Enrollment, lesson-progress records, completion buttons, course-progress calculation (Phases 7–8).
- Quizzes, questions, attempts, and scoring (Phases 9–10).
- A per-lesson quiz or any quiz unlock behavior.
- Course CRUD changes beyond verifying the parent course.
- Schema redesign, repositories, Unit of Work, CQRS, MediatR, and unrelated UI/dashboard work.

## Required Model

The context defines a lesson as:

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

The existing EF Core-scaffolded entity and SQL Server schema are authoritative. Verify real field names/types, `DeleteFlag`, audit fields, maximum lengths, indexes, and foreign keys before implementation.

## Access Rules

| Action | Access |
| --- | --- |
| Browse/view active lesson content | Public or authenticated student; only under an active course |
| View administrative lesson list/details | `Lesson.Read` |
| Create lesson | `Lesson.Create` |
| Update lesson/reorder | `Lesson.Update` |
| Soft-delete lesson | `Lesson.Delete` |

The Phase 4 database-backed permission handler is the security boundary. Do not add `role == "Admin"` checks or an admin bypass.

## Ordered Implementation Plan

### 1. Read-only audit

Inspect before modifying anything:

1. `AppDbContext`, scaffolded `Lesson` and `Course` entities, and their relationship.
2. Existing Course Management feature patterns, DTOs, validators, Result Pattern mapping, pagination, controller routes, API client services, and Tailwind components.
3. Existing lesson code/stubs to reuse.
4. Exact Dynamic RBAC policy/attribute usage and the database records for `Lesson.*` permissions.
5. Existing soft-delete and timestamp conventions.
6. Whether `LessonOrder` has a unique/indexed constraint per course and whether content is plain text, Markdown, or HTML.

Document schema facts and any necessary schema gap first. Do not change the database without explicit approval.

### 2. Confirm lesson-order rules

Define one stable ordering rule for all active lessons in a course:

- `LessonOrder` is a positive integer.
- Active lessons display ascending by `LessonOrder`.
- No two active lessons in one course may share an order.
- Creating a lesson at an occupied position shifts later active lessons forward, or append is used when no position is supplied; choose one rule and apply it consistently.
- Moving a lesson adjusts the affected range atomically.
- Soft-deleting a lesson must preserve remaining valid order; either close the gap immediately or tolerate gaps while always sorting correctly. Prefer closing gaps if it matches existing conventions.

Perform create/reorder/delete order updates inside an EF Core transaction where multiple rows change.

### 3. Define DTOs and validation

Create feature-scoped contracts, following existing project style:

| Contract | Fields |
| --- | --- |
| `CreateLessonRequest` | CourseId, Title, Content, LessonOrder/desired position |
| `UpdateLessonRequest` | Title, Content, LessonOrder/desired position; concurrency value only if schema already supports it |
| `LessonSummaryResponse` | LessonId, CourseId, Title, LessonOrder |
| `LessonDetailResponse` | Summary fields plus safe lesson content and audit fields appropriate to caller |
| `LessonListQuery` | Page/page size/search if needed by the established UI convention |

Application-layer validation must cover:

- Parent course exists and is active.
- Title/content are required, trimmed where appropriate, and fit verified database sizes.
- Order is positive and handled according to the selected ordering policy.
- Target lesson exists, is active, and belongs to the supplied/route course.
- Update/delete reject soft-deleted lessons.

Expected Result failures: `CourseNotFound`, `CourseDeleted`, `LessonNotFound`, `LessonDeleted`, `InvalidLessonOrder`, `InvalidLessonTitle`, `InvalidLessonContent`, and `PermissionDenied`.

### 4. Implement Application use cases

Use injected EF Core `DbContext` directly—no repository.

1. **Public course lessons:** retrieve only active lessons under an active course, ordered by `LessonOrder`; return no deleted lessons.
2. **Public lesson detail:** retrieve one active lesson only when its parent course is active; return not-found for unknown/deleted resources without leaking deleted state.
3. **Admin read:** provide administrative active list/detail and optional explicit history view only if the existing feature conventions require it.
4. **Create:** validate parent course, normalize input, insert lesson, set audit values, and maintain ordering atomically.
5. **Update:** update title/content and optionally order; preserve order integrity atomically.
6. **Soft delete:** set `DeleteFlag` rather than physically deleting; update ordering according to the chosen rule; do not alter future progress/quiz records.

Use no-tracking projection for read-only queries. Do not automatically create enrollment, progress, or quiz data.

### 5. Add API endpoints and authorization

Follow existing route conventions. Expected endpoints:

| Endpoint | Purpose | Access |
| --- | --- | --- |
| `GET /api/courses/{courseId}/lessons` | Active ordered lesson list | Public |
| `GET /api/courses/{courseId}/lessons/{lessonId}` | Active lesson detail | Public |
| `GET /api/admin/courses/{courseId}/lessons` or existing equivalent | Admin lesson list | `Lesson.Read` |
| `POST /api/courses/{courseId}/lessons` | Create | `Lesson.Create` |
| `PUT /api/courses/{courseId}/lessons/{lessonId}` | Update/reorder | `Lesson.Update` |
| `DELETE /api/courses/{courseId}/lessons/{lessonId}` | Soft delete | `Lesson.Delete` |

Controllers bind, delegate to Application services, and map Results. Return `401` for missing authentication on protected calls, `403` for missing permissions, `404` for unavailable course/lesson resources, and normal validation responses for bad input.

### 6. Implement Blazor UI

Use feature API services and `IHttpClientFactory`; never access `DbContext` from App.

1. Add a course-learning view with an ordered lesson sidebar/list and active lesson-content area.
2. Include previous/next lesson navigation using current ordered results.
3. Clearly show loading, empty-course, not-found, API-error, `401`, and `403` states.
4. Do not show “complete lesson,” progress percentage, enrollment, or quiz buttons yet.
5. Add authorized admin lesson list/create/edit UI, including an understandable reorder control/input and delete confirmation.
6. Render lesson content safely: do not inject unsanitized admin HTML. Follow the existing content format/sanitization approach.
7. Hide unavailable administration controls based on effective permissions, while relying on the API for enforcement.

### 7. Logging and tests

Log structured create/update/reorder/soft-delete actions with safe lesson/course/user IDs; do not log unbounded lesson content or tokens.

Verify:

| Scenario | Expected result |
| --- | --- |
| Create under active course | Lesson persists in valid order. |
| Create under missing/deleted course | Structured failure; no lesson created. |
| Active list | Only active lessons, ascending order. |
| Public detail of deleted lesson/course | Not found. |
| Move/reorder lesson | Unique, stable order after operation. |
| Soft delete | Row remains; public list/detail exclude it; ordering remains valid. |
| Student/anonymous admin mutation | `401`/`403` as applicable. |
| Sub-admin with individual Lesson permissions | Can perform only granted actions. |
| Permission revoked after login | Next protected request is denied. |

Build the full solution, run relevant automated tests, then complete manual UI checks on desktop and mobile layouts.

## Completion Criteria

Phase 6 is complete when authorized users can manage ordered, soft-deleted lessons under active courses; visitors/students can read active lessons in order; `Lesson.*` permissions protect all admin actions; and no enrollment, completion/progress, or quiz behavior has been added.

## Required Handoff Report

Report created/modified files, lesson functionality, endpoints and permissions, database changes (if approved), build/test results, and prerequisites for Phase 7.
