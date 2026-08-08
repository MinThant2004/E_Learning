# Phase 14 — Role and Permission Management: Detailed Implementation Plan

## 1. Objective

Complete the administrator-facing Dynamic RBAC management feature: authorized Main Admin users can manage roles, view permission catalog entries, assign permissions to roles, and assign roles to users through the existing database-backed RBAC model.

## 2. Current State

Phase 4 established `User → UserRole → Role → RolePermission → Permission` evaluation and live permission policies. Phase 13 adds safe user management. The database contains Roles, Permissions, UserRoles, and RolePermissions; however, no verified Role/Permission management Application feature or Blazor administration UI should be assumed complete.

## 3. Scope

- Role list/detail/create/update/archive/restore.
- Permission catalog read-only list.
- Assign/remove active permissions from a role.
- Assign/remove active roles for a user through controlled admin UI.
- Live authorization effect on the next protected request.

Excluded: changing permission-code meaning, public role assignment, physical deletes, new authorization framework, and schema changes without approval.

## 4. Business Rules

1. Permissions and roles remain database data; code must not add hard-coded role bypasses.
2. Only Main Admin-equivalent authority, determined by existing live permissions, may manage roles/assign permissions/users.
3. Prevent duplicate RoleName, UserRole, and RolePermission relationships.
4. Student public registration remains server-assigned Student only.
5. Do not remove/archived the final role/permission assignment needed to retain Main Admin access; define safe guard based on verified bootstrap role/data.
6. Archived roles/permissions grant no access; restore uses existing active dependencies.
7. A permission change must affect the next protected API request through Phase 4 live evaluation.

## 5. Database Impact

Use `Roles`, `Permissions`, `UserRoles`, `RolePermissions`, and `Users` tables. Inspect actual `DeleteFlag` support (Roles/Permissions may not have it), unique indexes, FKs, and audit fields. No schema change planned; document any missing soft-delete field before proposing alteration.

## 6. Domain Layer

Reuse scaffolded Role, Permission, UserRole, RolePermission entities. No repository/Unit of Work, duplicate authorization entities, or generic assignment service.

## 7. Application Layer

Add feature services/interfaces for Roles and Permissions using injected `IAppDbContext`.

DTOs: `RoleListQuery`, `RoleResponse`, `RoleDetailResponse`, `CreateRoleRequest`, `UpdateRoleRequest`, `PermissionResponse`, `AssignPermissionsRequest`, `AssignUserRolesRequest`, `UserRoleResponse`.

Results: `RoleNotFound`, `PermissionNotFound`, `UserNotFound`, `RoleAlreadyExists`, `DuplicateRoleAssignment`, `DuplicatePermissionAssignment`, `CannotRemoveLastMainAdminAccess`, `RoleInUse`, `PermissionDenied`.

Use transactions for assignment changes and Result Pattern for expected failures.

## 8. API Layer

| Endpoint | Purpose | Permission |
| --- | --- | --- |
| `GET /api/roles` / `{id}` | List/detail roles | `Role.Read` |
| `POST /api/roles` | Create role | `Role.Create` |
| `PUT /api/roles/{id}` | Update role | `Role.Update` |
| archive/restore role endpoints | Lifecycle actions | `Role.Delete` / verified restore convention |
| `GET /api/permissions` | Catalog | `Permission.Read` |
| `PUT /api/roles/{id}/permissions` | Set/assign permissions | `Permission.Assign` |
| `PUT /api/users/{id}/roles` | Set/assign roles | `Role.Update` or verified existing assignment permission |

Keep controllers thin and use existing Dynamic RBAC policies.

## 9. Infrastructure Layer

Reuse current permission resolver/policy provider, JWT/current-user service, Serilog, middleware, and DI. No cache initially because live permission revocation is more important; if cache exists, implement explicit invalidation only after verification.

## 10. Database Layer

Use EF Core projections for lists and transactions for multi-row role/permission/user assignments. Enforce server-side active-record validation. Do not expose direct DbContext to Blazor.

## 11. Blazor App

Add Role/Permission API clients through `IHttpClientFactory` and admin pages:

- Roles list with search and Active/Archived/All if supported.
- Role create/edit and permission checkbox/grouped assignment interface.
- User role assignment panel integrated safely with Phase 13 user detail.
- Permission catalog read-only list.
- Shared ConfirmationModal for Archive/Restore.
- Tailwind responsive tables/forms, loading/empty/error/401/403 states.

## 12. RBAC / Permissions

Use existing `Role.Read/Create/Update/Delete`, `Permission.Read`, and `Permission.Assign`. Do not create duplicate permission codes unless the verified database lacks an essential operation and user approves seed/data work.

## 13. Validation

- Role names required, trimmed, schema-length safe, unique.
- Every assigned permission/role exists and is active.
- No duplicates in request or existing relationship.
- Guard protected bootstrap/Main Admin access and self-impact changes.
- Bounded search/paging input.

## 14. Result Pattern

All expected management conflicts return existing `Result<T>`/`PagedResult<T>` errors. UI displays clear messages instead of exceptions.

## 15. Security Considerations

Always derive actor from JWT and authorize server-side. Treat role/permission IDs from UI as untrusted. Prevent privilege escalation, last-admin lockout, assignment of archived records, and sensitive field leakage. Log safe assignment actions without tokens/secrets.

## 16. Implementation Steps

1. Audit schema, seed data, actual Phase 4 authorization behavior, and Main Admin bootstrap rules.
2. Define approved guard rules and DTOs/validators.
3. Implement Application Role/Permission/assignment use cases with transactions.
4. Add DI and RBAC-protected API endpoints.
5. Build App clients/pages and integrate user-role UI.
6. Reuse confirmation modal and standardized result UI.
7. Test live permission changes, duplicates, last-admin guards, and unauthorized requests.
8. Build and verify.

## 17. Verification Checklist

- Main Admin creates a restricted Course Admin role and grants only Course permissions.
- Restricted sub-admin can use only allowed endpoints after sign-in/refresh.
- Duplicate assignments/names are rejected.
- Student/public registration cannot become Admin.
- Removing critical last-admin access is blocked.
- Archived role grants no access; restored role works per assignment.
- UI/API return 401/403 appropriately; build/tests pass.

## 18. Definition of Done

Main Admin can safely manage database-backed roles, permissions, and assignments with immediate live authorization effects, protected guardrails, no hard-coded bypasses, no physical deletes, and no new persistence/authorization architecture.
