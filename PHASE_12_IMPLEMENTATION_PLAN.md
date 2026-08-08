# Phase 12 — Admin Dashboard: Detailed Implementation Plan

## 1. Objective

Create a secure, permission-aware Admin Dashboard that summarizes E-Learning system content and learning activity and provides clear navigation to authorized management features.

## 2. Current State

Phases 1–11 provide authentication, live database-backed RBAC, archived/active management for content, enrollment, progress, final quizzes/results, and Student Dashboard. The App has admin category/course/lesson pages but no verified aggregated admin dashboard endpoint, dashboard page, or metrics projection.

## 3. Scope

- Summarize active/archived categories, courses, lessons, and final quizzes.
- Show enrollment and quiz-attempt totals only where appropriate.
- Present permission-aware quick actions/navigation.
- Provide recent operational items only when supported by existing safe data.

Out of scope: user/role/permission CRUD, full audit-log UI, BI reporting, export, chart library adoption, and modifying the database schema.

## 4. Business Rules

1. Dashboard requires authentication and must be visible only to users with at least one appropriate administrative permission.
2. Each metric/action must respect current effective database permissions; a Course Admin must not receive User/Role/Permission information.
3. Active and Archived counts reflect `DeleteFlag` server-side.
4. Dashboard must not expose student private records, passwords, hashes, tokens, or answer keys.
5. An empty system is a valid dashboard state, not an error.

## 5. Database Impact

Use read projections over existing `Categories`, `Courses`, `Lessons`, `Quizzes`, `Enrollments`, `QuizAttempts`, and RBAC tables. Inspect scaffolded relationships/field names first. No new table, column, migration, or schema modification is planned.

## 6. Domain Layer

Reuse existing entities. Dashboard metrics are Application read models, not persistent entities. Do not add repositories, generic metric entities, or a second authorization system.

## 7. Application Layer

Add `AdminDashboardService`/interface using injected `IAppDbContext` and current-user/permission service.

Expected DTOs:

- `AdminDashboardResponse`
- `ContentMetricResponse` (active/archived/total only where caller is allowed)
- `AdminQuickActionResponse`
- `RecentAdminItemResponse` only if a safe existing source supports it

The service determines effective permissions from Phase 4, then queries only permitted metric groups. Return `Result<AdminDashboardResponse>` with `PermissionDenied`/`AccountNotFound` where applicable.

## 8. API Layer

Add a thin endpoint:

| Endpoint | Purpose | Authorization |
| --- | --- | --- |
| `GET /api/admin/dashboard` | Current authorized admin summary | Authenticated + dashboard eligibility check |

Use existing Dynamic RBAC policy approach. If no existing `Dashboard.Read` permission exists, base eligibility on one verified management read permission and ensure each returned section is independently permission filtered. Do not silently seed an unrelated permission without approval.

## 9. Infrastructure Layer

Reuse JWT, Dynamic RBAC policy handler, current-user service, DI, Serilog, middleware, and existing Result/HTTP mapping. Add only structured safe dashboard diagnostics; no new external chart/telemetry framework.

## 10. Database Layer

Use `AsNoTracking`, efficient aggregate queries, bounded recent-item queries, and server-side `DeleteFlag` filters. Avoid loading full tables or student details merely to count records. No EF configuration change is expected.

## 11. Blazor App

Add `AdminDashboardApiClient` through `IHttpClientFactory`, an `/admin` or established dashboard route, and a responsive Tailwind layout:

- Summary metric cards visible only for permitted features.
- Active/Archived badge/count distinction using business terminology.
- Quick-action buttons only for permissions available in current auth state.
- Empty, loading skeleton, 401, 403, and API-error states.
- Responsive cards/grid and accessible labels.

Do not replicate management data tables; link to existing admin pages.

## 12. RBAC / Permissions

Use live existing permissions such as `Course.Read`, `Lesson.Read`, `Quiz.Read`, `User.Read`, `Role.Read`, and `Permission.Read`. No hard-coded Admin role bypass. UI visibility is convenience; Application/API must filter and authorize metrics server-side.

## 13. Validation

- Current user must exist, be active, and not archived.
- Validate any optional dashboard query ranges/limits.
- Protect recent-item queries from exposing records outside granted feature permissions.
- Treat zero counts as valid results.

## 14. Result Pattern

Return structured `Result<AdminDashboardResponse>`. Expected permission/account failures are Result errors, not exceptions. Client shows a clear permission-denied state instead of retrying token refresh after a `403`.

## 15. Security Considerations

Derive identity from JWT only. Re-check live permissions on each request. Never include sensitive user data, answer correctness, tokens, or audit values beyond safe display fields. Avoid metric inference across permissions by omitting unauthorized sections, not returning hidden totals.

## 16. Implementation Steps

1. Inspect existing permission catalog, admin pages, and entity relationships.
2. Define permitted metric sections and DTOs.
3. Implement permission-aware Application projections with efficient EF queries.
4. Add DI registration and protected dashboard API endpoint.
5. Add App client, route/navigation, Tailwind metric cards, and quick actions.
6. Add loading/empty/error/401/403 states.
7. Test sub-admin visibility, active/archived counts, and data isolation.
8. Build and manually verify responsive UI.

## 17. Verification Checklist

- Main Admin sees all authorized feature metric cards.
- Restricted Course Admin sees course-only metrics/actions.
- Student cannot access endpoint/page.
- Active/Archived content counts are accurate.
- Dashboard does not reveal private student data or answer keys.
- Empty database has stable zero/empty states.
- Permissions revoked after login affect the next dashboard request.
- Build/test and mobile accessibility checks pass.

## 18. Definition of Done

Authorized admins have a responsive, accurate, live-permission-scoped operational dashboard that links to existing management features and exposes no unauthorized or sensitive information. It uses the existing N-layer architecture, EF Core/Result Pattern/RBAC infrastructure, and requires no schema change.
