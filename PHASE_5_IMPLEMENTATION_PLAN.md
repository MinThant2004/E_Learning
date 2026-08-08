# Phase 5 — Course Management: Implementation Plan

## Purpose

Implement course management as the first learning-content feature after Phases 3 and 4 are complete.

Admins and authorized sub-admins will be able to create, view, update, and soft-delete courses. Students and anonymous visitors will be able to browse active courses and view active course details. Course management must use the Dynamic RBAC permission mechanism from Phase 4 and the existing SQL Server/EF Core database-first model.

This phase creates courses only. It does not create lessons, enrollment, progress, or quizzes. The final course quiz requested for this project belongs to later phases and must not be implemented here.

## Phase Boundaries

### Included

- Read-only audit of the existing course schema and codebase conventions.
- Course create, read, update, and soft-delete use cases.
- Public/student course browsing and active-course details.
- Admin course list/detail views, including deleted-course handling only where required for administration.
- `Course.Read`, `Course.Create`, `Course.Update`, and `Course.Delete` enforcement through Phase 4 Dynamic RBAC.
- Feature-specific DTOs, validation, Result Pattern responses, pagination/search where supported by existing conventions, structured logging, API endpoints, Blazor feature UI, and verification.

### Explicitly excluded

- Lesson creation, display, ordering, or content management (Phase 6).
- Enrollment and My Courses (Phase 7).
- Lesson completion and course progress (Phase 8).
- Course-final quiz management, questions, attempts, or scoring (Phases 9–10).
- Student dashboard and formal Admin dashboard work (Phases 11–12).
- User, role, and permission management workflows (Phases 13–14).
- Audit logging as a business feature (Phase 15).
- New fields such as `DifficultyLevel`, `EstimatedHours`, publish state, categories, thumbnails, ratings, tags, or certificates unless the existing verified schema already contains them and they are explicitly in scope.
- Repository Pattern, Unit of Work, CQRS, MediatR, or unnecessary abstractions.

## Expected Course Model

The project context defines the required business fields as:

```text
CourseId
Title
Description
CreatedAt
UpdatedAt
DeleteFlag
```

The exact names, types, nullability, keys, and relationships must be taken from the existing scaffolded EF Core model—not assumed from this list.

## Target Access Rules

| Action | Who may perform it | Required authorization |
| --- | --- | --- |
| Browse active courses | Anonymous or authenticated visitor | Public endpoint; no administrative permission required |
| View active course details | Anonymous or authenticated visitor | Public endpoint; no administrative permission required |
| View administrative course list/details, including deleted records if supplied | Authorized admin/sub-admin | `Course.Read` |
| Create a course | Authorized admin/sub-admin | `Course.Create` |
| Update a course | Authorized admin/sub-admin | `Course.Update` |
| Soft-delete a course | Authorized admin/sub-admin | `Course.Delete` |

The API is always the security boundary. Blazor may hide admin actions based on effective permissions, but it must never be relied on to protect data or actions.

## Implementation Sequence

### 1. Perform a read-only course feature audit

Before modifying code or database data, inspect and document:

1. The solution’s feature-folder conventions, Application service patterns, API controller patterns, DTO conventions, validators, Result Pattern types, pagination support, and mapping style.
2. The existing `AppDbContext` and scaffolded course entity: actual table name, primary key, all scalar fields, navigation properties, nullability, default values, concurrency fields if any, and `DeleteFlag` representation.
3. Existing course-related code, endpoints, DTOs, Blazor pages/components/services, and incomplete stubs to reuse rather than duplicate.
4. The Phase 4 permission policy/handler convention and actual permission-code records for `Course.Read`, `Course.Create`, `Course.Update`, and `Course.Delete`.
5. Existing global query-filter behavior or explicit soft-delete-query conventions.
6. Existing database indexes/constraints that affect title or search behavior.
7. Existing API error mapping, authentication state, `IHttpClientFactory` configuration, Serilog enrichment, and Tailwind UI patterns.

**Required output:** a concise audit note identifying reusable patterns, exact database facts, and any schema gap. The audit itself must not change the schema.

### 2. Confirm the persistence and soft-delete design

Use the current course table and scaffolded entity as the source of truth.

Confirm the following before implementation:

| Concern | Required decision/behavior |
| --- | --- |
| Course key | Use the existing course identifier and its actual type. |
| Title/description columns | Use only verified existing fields; confirm maximum lengths and nullability. |
| Audit fields | Set/update `CreatedAt` and `UpdatedAt` only when those fields exist and follow current conventions. |
| Soft delete | Set existing `DeleteFlag`; do not physically delete the course. |
| Relationships | Identify dependent lessons/enrollments/quizzes but do not implement those phases. |
| Query filtering | Normal/public queries exclude deleted courses; administrative history views include them only when intentional. |
| Concurrency | Reuse an existing row version/concurrency mechanism if the schema already provides one; do not add one silently. |

If the existing schema lacks an essential field required by the project context, prepare a minimal database-change proposal before applying any change. The proposal must name the exact column/constraint/index, explain why it is essential, describe the data impact, and preserve the current database. No schema change is permitted without explicit agreement.

### 3. Define course contracts and Result outcomes

Create or complete feature-scoped DTOs in the existing Application/API layout. Do not expose EF Core entities directly.

Expected request contracts:

| Contract | Required fields |
| --- | --- |
| `CreateCourseRequest` | Title, Description |
| `UpdateCourseRequest` | Title, Description; include concurrency data only if the verified schema/application already uses it |
| `CourseListQuery` | Page, page size, optional search term; optional deleted-record inclusion only for authorized admin use |

Expected response contracts:

| Contract | Purpose |
| --- | --- |
| `CourseSummaryResponse` | List-safe course ID, title, short/appropriate description representation, and safe audit metadata if useful. |
| `CourseDetailResponse` | Full course fields appropriate for public or admin detail views. |
| `PagedResult<CourseSummaryResponse>` | Paginated course browsing/admin listing when the existing Result Pattern supports pagination. |

Request and response DTOs may be shared only when their responsibilities are genuinely identical; otherwise keep separate contracts.

Define/reuse structured business failures such as:

- `CourseNotFound`
- `CourseDeleted`
- `InvalidCourseId`
- `InvalidCourseTitle`
- `InvalidCourseDescription`
- `CourseTitleAlreadyExists` only if a verified existing uniqueness rule/business requirement requires it
- `PermissionDenied`

Use normal Result Pattern failures for expected business outcomes. Reserve exceptions and exception middleware for unexpected system failures.

### 4. Define validation and normalization rules

Place business/input validation in the Application layer using the project’s established validator approach.

Validate:

1. **Title:** required, trimmed, non-whitespace, and within the verified database length.
2. **Description:** required or optional exactly as the verified schema/business rule requires; trim and enforce database-safe length.
3. **Identifier:** valid existing course identifier for detail/update/delete operations.
4. **List query:** safe page number/page size values, existing maximum page size, and safe bounded search input.
5. **Delete state:** reject normal update/delete operations against an already deleted course unless an intentional administrative restore feature is later approved.

Do not invent title-uniqueness behavior unless it is already enforced by the schema or explicitly selected as a business rule. Do not duplicate the same validation in Blazor, controller, and application service; UI validation is for user experience while the Application layer is authoritative.

### 5. Implement Application-layer course use cases

Implement a feature-oriented `CourseService` (or extend the existing course application service) using injected `AppDbContext` directly through DI.

#### Public/student browse active courses

1. Query only active (not soft-deleted) courses.
2. Apply a stable ordering based on verified existing fields; use title/creation order only if no current convention exists.
3. Apply safe optional search and pagination using the shared existing pagination pattern.
4. Project directly to summary DTOs; use no-tracking queries where appropriate.
5. Do not reveal deleted courses or administrative-only fields.

#### Public/student view course details

1. Retrieve one active course by its verified identifier.
2. Return `CourseNotFound` for unknown or soft-deleted courses so public callers cannot distinguish record history/state.
3. Return only course-level data in this phase. Do not include lessons, enrollment state, student progress, or quiz data.

#### Administrative course list/detail read

1. Support `Course.Read` users viewing active courses and, only if needed by the verified UI conventions, a deliberate include-deleted/history option.
2. Keep the public browse endpoint/query separate from administrative history behavior to prevent deleted-content leakage.
3. Project only fields needed by the administration UI.

#### Create course

1. Validate the request and normalize values according to the agreed rules.
2. Create a course using only existing verified fields.
3. Set audit values according to the current entity/database conventions.
4. Save through `AppDbContext` and return a course response/result.
5. Do not create related lessons, enrollments, progress records, or quizzes automatically.

#### Update course

1. Validate the request and locate the active course.
2. Return `CourseNotFound` for unknown/deleted courses according to the established error convention.
3. Update only title and description (plus verified update audit field) in this phase.
4. Respect existing optimistic concurrency if present; return a clear business/conflict result if another update wins.
5. Save and return the updated course response.

#### Soft-delete course

1. Locate the active course by ID.
2. Set the verified `DeleteFlag` and audit fields; do not use physical deletion.
3. Do not cascade deletes into lessons, enrollments, progress, quizzes, attempts, or users. Those dependent feature rules are not implemented yet.
4. Ensure the course disappears from public browse/details and future enrollment eligibility.
5. Return a safe success result. Repeated delete behavior must follow existing Result Pattern conventions (normally `CourseNotFound` or a clear already-deleted result).

### 6. Apply Phase 4 Dynamic RBAC to admin use cases

Protect administrative endpoints with the standardized Phase 4 permission policy convention:

| Operation | Required permission |
| --- | --- |
| Administrative list/detail | `Course.Read` |
| Create | `Course.Create` |
| Update | `Course.Update` |
| Soft delete | `Course.Delete` |

Requirements:

1. Let ASP.NET Core authentication establish identity and the Phase 4 authorization handler evaluate live database permissions.
2. Do not add controller checks such as `role == "Admin"`.
3. Do not place an unconditional Main Admin bypass in course code.
4. A permission change/revocation in the database must affect the next protected request, according to Phase 4 behavior.
5. Public browse/detail operations must explicitly return only active courses and must not become administrative endpoints merely because a user is logged in.

### 7. Implement REST-style API endpoints

Follow current route naming and response conventions. The expected endpoint set is:

| Method and route | Purpose | Access |
| --- | --- | --- |
| `GET /api/courses` | Browse active courses, search, and paginate | Public |
| `GET /api/courses/{id}` | View one active course | Public |
| `GET /api/admin/courses` or approved equivalent | Administrative listing, optionally including deleted items | `Course.Read` |
| `GET /api/admin/courses/{id}` or approved equivalent | Administrative course detail | `Course.Read` |
| `POST /api/courses` | Create a course | `Course.Create` |
| `PUT /api/courses/{id}` | Update a course | `Course.Update` |
| `DELETE /api/courses/{id}` | Soft-delete a course | `Course.Delete` |

The final route layout must follow existing codebase conventions. If the project already uses a single `/api/courses` controller with endpoint-specific policies, retain it instead of adding a parallel route scheme.

API requirements:

- Keep controllers thin: bind request, call the Application service, and map the Result Pattern to HTTP status codes.
- Return `200 OK` for successful reads/updates, `201 Created` with a location for creates where current conventions support it, and the established success result for soft deletes.
- Return `400 Bad Request` for validation failures, `401 Unauthorized` for no/invalid authentication on protected endpoints, `403 Forbidden` for missing permission, and `404 Not Found` for unavailable course resources.
- Never return scaffolded entities, internal EF tracking state, deleted-course information to public callers, or information about future lesson/enrollment/quiz relations.

### 8. Implement Blazor course feature UI and API client integration

The App must use `IHttpClientFactory` and feature-specific API services. It must not reference `DbContext`, SQL Server, or database entities directly.

#### Public/student browsing UI

1. Add or complete a course catalog page that loads `GET /api/courses`.
2. Display course title and description using safe rendering; never render admin-entered content as untrusted raw HTML without an approved sanitization approach.
3. Support available search and pagination behavior.
4. Add a course details page that loads the active course endpoint.
5. Show a clear unavailable/not-found state for deleted or unknown courses.
6. Do not show enrollment, lesson navigation, progress, or final quiz controls until their later phases.

#### Admin course UI

1. Add/complete a course management list accessible only when the current API-provided effective permissions include the relevant course permissions.
2. Add create and edit forms with client-side usability validation matching, but not replacing, server-side validation.
3. Add a soft-delete confirmation that clearly states the course will be hidden rather than physically removed.
4. Show meaningful API validation, `403`, concurrency, and unexpected-error states.
5. On `401`, retain the Phase 3 one-time refresh/retry behavior. On `403`, show a permission-denied state and do not retry refresh.
6. Do not create a full dashboard or role-management UI in this phase.

### 9. Add logging and operational safeguards

Use structured Serilog events for important administrative course actions:

- Course created
- Course updated
- Course soft-deleted
- Permission denied for a course administration action
- Unexpected persistence/concurrency failures

Include safe values such as course ID, authenticated user ID, action, and correlation/request ID where available. Do not log tokens, secrets, password-related information, or full unbounded course descriptions.

Maintain existing exception middleware and request logging. Expected validation/not-found/authorization outcomes should be handled by Result Pattern/API response mapping rather than exception-driven control flow.

### 10. Test and verify Phase 5

#### Application-layer test cases

| Scenario | Expected result |
| --- | --- |
| Valid create request | Active course is persisted with verified audit values. |
| Missing/whitespace title | Structured validation failure; no course is created. |
| Oversized title/description | Validation failure before database error. |
| Public active-course list | Includes only non-deleted courses. |
| Public active-course detail | Returns the requested active course. |
| Unknown/deleted public detail | `CourseNotFound`; deleted state is not disclosed. |
| Valid update of active course | Updates only allowed course fields. |
| Update of unknown/deleted course | Structured not-found/deleted outcome, per convention. |
| Soft delete of active course | Sets `DeleteFlag`; row remains in the database. |
| After soft delete | Course is absent from public browse/detail results. |
| Existing concurrency conflict, if supported | Returns the agreed conflict result; no accidental overwrite. |

#### API/RBAC integration test cases

| Caller/request | Expected result |
| --- | --- |
| Anonymous `GET /api/courses` | Active-course results are returned. |
| Anonymous `GET /api/courses/{id}` for active course | Course details are returned. |
| Anonymous admin mutation request | `401 Unauthorized`. |
| Authenticated Student admin mutation request | `403 Forbidden`. |
| Sub-admin with only `Course.Read` | Can read admin views but cannot create/update/delete. |
| Sub-admin with `Course.Create` | Can create but cannot update/delete unless separately granted. |
| Sub-admin with correct update/delete permission | Can perform only the separately granted action. |
| Main Admin role via database permissions | Can complete allowed operations through normal Phase 4 RBAC evaluation. |
| Permission revoked after login | Next protected request is denied. |

#### Blazor manual verification

1. Browse and search active courses while anonymous.
2. Confirm a deleted course no longer appears and its public URL shows an unavailable/not-found state.
3. Sign in as a Student and confirm administration controls are not displayed; direct admin API access is still denied.
4. Sign in as an authorized course sub-admin and create, edit, and soft-delete a course.
5. Confirm only controls for granted permissions are visible.
6. Confirm server validation errors, `401`, `403`, and unexpected errors are understandable to the user.

#### Build/database/security checks

1. Build the entire solution and resolve errors within Phase 5 scope.
2. Run automated tests if a test project exists; otherwise document manual verification results.
3. Confirm no database was created, dropped, recreated, or modified outside approved changes.
4. Confirm soft delete is used instead of physical deletion.
5. Confirm no authentication secrets/tokens are logged or added to source control.
6. Confirm no course fields or future-feature behavior were invented beyond the verified schema and Phase 5 scope.

## Completion Criteria

Phase 5 is complete when:

- Authorized admins/sub-admins can create, read, update, and soft-delete courses through API and Blazor UI.
- Public/student users can browse and view only active course records.
- Course authorization uses the live database-backed `Course.*` permissions from Phase 4.
- Students and callers without relevant permissions cannot perform administrative course operations.
- All normal course queries consistently exclude soft-deleted records.
- DTOs, validation, Result Pattern, DI, logging, and layer responsibilities follow `PROJECT_CONTEXT.md`.
- No lessons, enrollment, progress, final course quizzes, or other future-phase functionality has been introduced.
- The solution builds successfully and the planned verification scenarios pass.

## Required Handoff Report After Implementation

When Phase 5 is implemented, report:

1. Files created and modified.
2. Course functionality implemented.
3. API endpoints added or changed, including required permissions.
4. Database changes proposed/applied, with exact reason and deployment instructions.
5. Build result.
6. Automated and manual verification results.
7. Remaining issues or prerequisites before Phase 6.

