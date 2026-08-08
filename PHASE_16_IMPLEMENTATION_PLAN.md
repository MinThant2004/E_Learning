# Phase 16 — API and Blazor Integration: Detailed Implementation Plan

## 1. Objective

Complete and standardize end-to-end integration between the ASP.NET Core API and Blazor WebAssembly App across all completed features, ensuring consistent contracts, authentication refresh, authorization behavior, error mapping, loading states, and feature navigation.

## 2. Current State

Phases 1–15 implement the core business features. The App already uses `IHttpClientFactory`, authentication state, API client services, and an `AuthHttpHandler`; the API uses DTOs, Result Pattern, JWT, Dynamic RBAC, middleware, and controllers. Integration must be audited for inconsistent client URL conventions, duplicate error handling, missing token refresh behavior, mismatched DTOs, and unauthorized UI paths.

## 3. Scope

- Audit all API endpoints and corresponding Blazor clients/pages.
- Standardize feature client services and HTTP response handling.
- Verify access-token attachment, one-time refresh/retry, logout, and 401/403 behavior.
- Standardize Result Pattern/API error display, route protection, loading/empty/error states, and navigation.
- Verify archive/restore, enrollment, progress, quizzes, dashboards, user/RBAC, and audit flows end to end.

Excluded: new business features, database schema changes, a replacement HTTP stack, new authentication mechanism, or unrelated UI redesign.

## 4. Business Rules

1. Blazor never accesses EF Core, SQL Server, or entities directly.
2. Every feature request goes through configured `IHttpClientFactory` clients/handlers.
3. On `401`, client attempts refresh once, retries original request once, then clears session/redirects if refresh fails.
4. On `403`, client shows a permission-denied state and does not refresh/retry repeatedly.
5. API remains source of truth for ownership, Active/Archived filtering, permissions, progress, and scores.
6. API Result errors are shown as useful messages without exposing implementation details.

## 5. Database Impact

No database change. Use this phase to verify that all existing API queries apply server-side ownership and `DeleteFlag` filters.

## 6. Domain Layer

No new domain entities. Correct DTO/contract mapping without leaking entities or sensitive fields.

## 7. Application Layer

Audit each completed Application service and its DTOs/results for API contract completeness. Add no duplicate services; only consolidate shared mapping/error conventions where existing code allows. Confirm every service returns structured Results and cancellation tokens are propagated.

## 8. API Layer

Create an endpoint inventory for Auth, Categories, Courses, Lessons, Enrollments, Progress, Quizzes, Attempts, Student/Admin Dashboards, Users, Roles/Permissions, and Audit Logs. Verify routes, verbs, status codes, authorization policies, request binding, validation, and response DTOs. Normalize only documented inconsistencies.

## 9. Infrastructure Layer

Audit JWT configuration, refresh-token route/transport, `AuthHttpHandler`, `CustomAuthStateProvider`, current-user service, CORS, Serilog, exception middleware, and validation filter. Reuse them; do not add a second token store/handler. Confirm sensitive data is never logged.

## 10. Database Layer

Verify EF Core query behavior and no-tracking projections through integration tests. Confirm all student APIs filter by current user and active content, and administration lists apply Active/Archived/All at query level.

## 11. Blazor App

For every feature client/page:

- Use a single feature API client and typed DTOs.
- Handle successful Result, validation error, `401`, `403`, `404`, and unexpected failure consistently.
- Implement cancellation/disposal where existing patterns support it.
- Ensure routes/navigation are protected appropriately and permission-based nav is a UX aid only.
- Reuse shared loading, empty-state, confirmation-modal, and notification/message patterns.
- Verify Tailwind UI does not show students admin/archive/restore actions.

## 12. RBAC / Permissions

Audit endpoint policy coverage against live permission codes. Verify Blazor `AuthorizeView`/client policy behavior matches server policy names but never substitutes for API protection. Ensure permission changes take effect after refresh/current-user reload according to Phase 4 design.

## 13. Validation

Verify DTO validation/error shape is consistent across JSON and multipart endpoints. Validate client route parameters before sending where useful, while retaining Application layer as authority. Prevent duplicate client submits for mutations.

## 14. Result Pattern

Define/use one API error-response convention based on existing `Result` mappings. Feature clients should parse the common safe error shape rather than each page inventing a different one. Do not introduce a duplicate Result library.

## 15. Security Considerations

Test token expiry, refresh rotation/revocation, logout, direct API calls, IDOR attempts, stale permissions, CORS, and archive filtering. Do not store refresh tokens insecurely or expose passwords, hashes, JWTs, answer keys, or audit-sensitive data in App state/logs.

## 16. Implementation Steps

1. Inventory API endpoints, Application DTOs, App clients/pages, and authorization policies.
2. Document mismatches and select existing conventions to retain.
3. Refine common client error/response handling and one-time refresh behavior.
4. Correct route/DTO/status/authorization mismatches feature by feature.
5. Add shared UI states only where existing components do not already cover them.
6. Run endpoint-to-page integration tests and manual flows for every feature.
7. Build all projects and fix only integration-scope regressions.

## 17. Verification Checklist

- Every navigation action calls the intended API endpoint with valid DTO.
- 401 refresh retries once; failed refresh signs out; 403 does not loop.
- API enforces ownership/RBAC even if UI is bypassed.
- Active/Archived filters are server-side and UI states match returned data.
- All mutations show success/friendly Result errors and prevent double-submit.
- Student, Admin, and restricted sub-admin flows each render correct navigation/actions.
- No DTO exposes entities, tokens, hashes, answer keys, or private cross-user data.
- Solution build and integration/manual tests pass.

## 18. Definition of Done

All completed feature flows work consistently from Blazor through API/Application/EF Core, authentication and RBAC behavior is reliable, Result errors and UI states are standardized, and no alternate database/HTTP/auth architecture was introduced.
