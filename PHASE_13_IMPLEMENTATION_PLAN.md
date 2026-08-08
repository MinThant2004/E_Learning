# Phase 13 — User Management: Detailed Implementation Plan

## 1. Objective

Provide authorized administrators with secure user search, profile viewing, controlled status/archive actions, and role-assignment visibility while protecting accounts, credentials, and Dynamic RBAC integrity.

## 2. Current State

Authentication, JWT refresh, Dynamic RBAC, student registration, and an existing `Users`/`UserRoles` model are available. Public registration always assigns Student. No verified User Management Application feature, API controller, administration page, or user list exists.

## 3. Scope

- Paginated/searchable administrative user list and safe user detail.
- View active/archived state and assigned roles.
- Update safe profile/status fields permitted by verified schema.
- Archive/restore accounts using existing `DeleteFlag`, with reusable confirmation modal.
- Prepare safe integration points for Phase 14 role assignment.

Excluded: public registration changes, plaintext password access, password reset UI unless explicitly requested, role/permission CRUD (Phase 14), physical deletion, and new schema.

## 4. Business Rules

1. Public users cannot submit RoleId or elevate themselves.
2. User list/detail excludes `PasswordHash`, refresh tokens, secrets, and private data not needed by admins.
3. Archiving sets `DeleteFlag = 1`; restore sets it to `0`; use Active/Archived/Archive/Restore UI terminology.
4. Archived accounts cannot log in, refresh, or access protected resources; verify existing Phase 3 behavior remains effective.
5. Administrators must not be able to accidentally archive/disable their own only administrative access or remove the final Main Admin; exact guard follows verified role model.
6. A sub-admin may only perform user actions permitted by live RBAC, and cannot manage users/roles beyond scope.

## 5. Database Impact

Use existing `Users`, `UserRoles`, `Roles`, `RefreshTokens`, and `AuditLogs` where present. Inspect fields/constraints/soft-delete representation first. No schema change is planned. Do not physically delete users or refresh-token history; revoke active refresh tokens when archiving if the existing service/schema supports it.

## 6. Domain Layer

Reuse scaffolded entities and `UserStatus` only if it matches persistence. Add no repository/Unit of Work or duplicate identity entity.

## 7. Application Layer

Add `Users` feature service/interface, DTOs, validators, and Result Pattern operations using injected `IAppDbContext`.

DTOs: `UserListQuery`, `AdminUserSummaryResponse`, `AdminUserDetailResponse`, `UpdateUserRequest`, `UserRoleSummaryResponse`.

Results: `UserNotFound`, `UserArchived`, `CannotArchiveSelf`, `CannotArchiveLastAdmin`, `CannotRestoreUser`, `InvalidUserStatus`, `PermissionDenied`. Project safe fields only; use transactions for state/revocation changes.

## 8. API Layer

| Endpoint | Purpose | Permission |
| --- | --- | --- |
| `GET /api/users` | Paged/searchable Active/Archived/All list | `User.Read` |
| `GET /api/users/{id}` | Safe admin detail | `User.Read` |
| `PUT /api/users/{id}` | Safe profile/status update | `User.Update` |
| `POST /api/users/{id}/archive` | Archive/revoke access | `User.Update` |
| `POST /api/users/{id}/restore` | Restore account | `User.Update` |

Controllers remain thin; validate server-side and map Result errors consistently.

## 9. Infrastructure Layer

Reuse current-user/JWT, password service unchanged, refresh-token lifecycle service, Serilog, middleware, and Dynamic RBAC. Do not log credentials/token values. Use existing audit mechanism if implemented; otherwise defer detailed audit feature to Phase 15.

## 10. Database Layer

Use EF Core projections and server-side pagination/search/filtering. Default query is Active; archived/all is explicitly requested by authorized admin. Filter roles by active records according to verified schema. No migration is expected.

## 11. Blazor App

Add `UserApiClient` via `IHttpClientFactory` and `/admin/users` management page:

- Search, pagination, Active/Archived/All filter.
- Columns: name, email, roles, status, created date, actions.
- Detail/edit form for permitted safe fields.
- Shared ConfirmationModal for Archive/Restore.
- Friendly Result errors, loading, empty, 401, 403 states.

No password-hash display or direct database access.

## 12. RBAC / Permissions

Use existing `User.Read` and `User.Update`. Live Phase 4 authorization applies to every endpoint; do not hard-code role names. Role assignment itself remains Phase 14 unless it is strictly required for an already-existing user workflow.

## 13. Validation

- Positive valid user ID.
- Bounded search/page sizes.
- Email/name updates obey verified existing validation/unique index.
- No archived/nonexistent target operation.
- Self/last-admin safety guards are applied server-side.

## 14. Result Pattern

Use existing `Result<T>` and `PagedResult<T>`; expected conflicts/permission failures are Results, not exceptions. UI maps codes to friendly messages.

## 15. Security Considerations

Identity for safety checks comes from JWT. Prevent IDOR by requiring User permissions and guard scopes. Never return password hashes, refresh tokens, security stamps, or sensitive audit metadata. Revoke session access when archive policy requires it.

## 16. Implementation Steps

1. Inspect entities, existing auth/revocation behavior, permissions, and audit support.
2. Define safe DTOs/validators/results and safety rules.
3. Implement EF query, update, archive/restore/revocation use cases.
4. Add DI/API authorization/endpoints.
5. Add App client and responsive management UI.
6. Reuse modal and add states/messages.
7. Test RBAC, self/last-admin guards, archive login/refresh behavior, and pagination.
8. Build solution and verify.

## 17. Verification Checklist

- User.Read admin can list/detail but not update.
- User.Update admin can update only allowed fields.
- Student cannot access management API/page.
- Archived user is hidden by default and cannot authenticate/refresh.
- Restore makes account eligible under normal authentication rules.
- No password hash/token leaks in API/UI/logs.
- Self/last-admin unsafe operations are blocked.
- Active/Archived/All filters, modal cancel/confirm, build/tests pass.

## 18. Definition of Done

Authorized admins securely manage safe user information and account lifecycle with server-side RBAC, archive terminology, privacy protection, and no database schema change or duplicated identity/authorization architecture.
