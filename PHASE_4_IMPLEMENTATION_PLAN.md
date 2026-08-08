# Phase 4 — Dynamic RBAC: Implementation Plan

## Purpose

Implement the authorization foundation for database-driven Role-Based Access Control (RBAC) after Phase 3 authentication is complete.

The resulting authorization decision must follow this relationship:

```text
Authenticated User
→ UserRole
→ Role
→ RolePermission
→ Permission
→ Allow or deny the requested action
```

Roles and permissions are data in SQL Server. The API must validate permissions server-side for every protected action. A sub-admin receives only the permissions assigned through their roles; being an admin must not automatically grant unrelated capabilities.

## Phase Boundaries

### Included in Phase 4

- Inspect and confirm the existing RBAC database model.
- Define the permission-code catalog and initial role/permission assignments required by the system.
- Establish a safe Main Admin bootstrap/identification approach.
- Implement database-backed permission evaluation for the authenticated current user.
- Integrate dynamic permission authorization with the ASP.NET Core authorization pipeline.
- Provide reusable permission-protection conventions for present and future API endpoints.
- Return consistent `401` and `403` outcomes.
- Add RBAC authorization data to authenticated-user responses as appropriate for UI visibility.
- Add validation, Result Pattern failures, structured logging, and tests for authorization.

### Explicitly deferred

- Course, lesson, enrollment, progress, and quiz business features (Phases 5–10).
- Student or Admin dashboard UI (Phases 11–12).
- Full role/permission-management screens and CRUD workflows (Phase 14).
- Broad user-management screens/workflows (Phase 13).
- Audit-log feature implementation (Phase 15).
- Repository Pattern, Unit of Work, CQRS, MediatR, custom JWT-validation middleware, or unnecessary generic abstractions.

Phase 4 creates the authorization mechanism and initial RBAC data necessary to use it. Phase 14 later provides the complete admin-facing management feature for roles and permissions.

## Non-Negotiable Rules

- Start with a read-only audit of the existing scaffolded `AppDbContext`, RBAC entities, relationships, keys, constraints, soft-delete fields, and current data.
- The existing database and EF Core scaffolded models are the source of truth. Do not assume tables or columns exist.
- Use injected EF Core `DbContext` directly in application services; do not add repositories or Unit of Work.
- Authentication from Phase 3 remains responsible for identity validation. Authorization decides what an authenticated identity may do.
- Do not hard-code a global `IsAdmin` bypass. Main Admin powers must be represented by database-backed roles and permissions.
- Do not trust roles, permissions, or user identifiers sent by Blazor clients.
- Do not place authorization business logic in controllers, filters, or Blazor components.
- Keep permission checks current against the database rather than permanently embedding permissions in long-lived JWT claims.

## Target Behavior

| User type | Authorization behavior |
| --- | --- |
| Anonymous user | Has no authenticated identity; protected API returns `401 Unauthorized`. |
| Student | Receives only public/authenticated actions and future student-specific permissions, if defined. |
| Main Admin | Has a database-assigned role containing all required administrative permissions. |
| Sub-admin | Is allowed only when at least one assigned active role grants the required active permission. |
| Soft-deleted/inactive user, role, or permission | Must not produce an authorization grant. |

## Permission Catalog Strategy

Permissions are stable capability codes, not display text. Store the code in the database and use it as the authorization key. Human-readable names/descriptions may be stored separately if supported by the existing schema.

The initial catalog should be confirmed against the current database before insertion. The intended feature-oriented codes are:

| Feature | Permissions |
| --- | --- |
| Courses | `Course.Read`, `Course.Create`, `Course.Update`, `Course.Delete` |
| Lessons | `Lesson.Read`, `Lesson.Create`, `Lesson.Update`, `Lesson.Delete` |
| Quizzes | `Quiz.Read`, `Quiz.Create`, `Quiz.Update`, `Quiz.Delete` |
| Users | `User.Read`, `User.Update` |
| Roles | `Role.Read`, `Role.Create`, `Role.Update`, `Role.Delete` |
| Permissions | `Permission.Read`, `Permission.Assign` |

Additional permission codes may be proposed only when later feature rules require them. For example, `Enrollment.Create` and `LessonProgress.Update` belong to the relevant later phases rather than being invented prematurely.

## Implementation Sequence

### 1. Perform a read-only RBAC and authentication audit

Before creating or modifying code/data, inspect and document:

1. The solution’s current layers, feature folder conventions, and project references.
2. The Phase 3 authentication implementation: JWT claim type for user ID, current-user service, `me` endpoint, authentication/authorization middleware order, and Result Pattern conventions.
3. `AppDbContext` plus the scaffolded `User`, `Role`, `Permission`, `UserRole`, and `RolePermission` models.
4. Exact table names, primary keys, foreign keys, navigation properties, nullable fields, unique constraints/indexes, audit fields, and `DeleteFlag` behavior.
5. Existing role and permission records, existing user-role assignments, and the actual Student/Main Admin naming or identifiers.
6. Existing data-seeding mechanism, migration approach, SQL scripts, application configuration, and database deployment conventions.
7. Existing API controller styles, error mapping, validation mechanism, Serilog configuration, and Blazor authorization/UI patterns.

**Required output before implementation:** a concise audit note stating which schema elements and application patterns can be reused, plus any database gap. Do not change the schema during this audit.

### 2. Confirm the RBAC persistence model and integrity rules

Use the existing schema if it represents the required relationships. Confirm it can enforce or safely implement these rules:

| Relationship/data rule | Required behavior |
| --- | --- |
| User ↔ Role | A user may have one or more roles if the current schema supports it. |
| Role ↔ Permission | A role may have one or more permissions. |
| Permission code | Must uniquely identify a permission, case-handling defined consistently. |
| UserRole duplicate | The same user-role pair must not be duplicated. |
| RolePermission duplicate | The same role-permission pair must not be duplicated. |
| Soft deletion | Deleted roles/permissions/assignments must not grant access. |
| Referential integrity | Assignments must reference valid active user/role/permission records. |

If the schema is missing an essential capability—such as `RolePermission`, a unique permission code, or a user-role relationship—prepare a minimal database-change proposal first. It must identify the exact table/columns/constraints required, why they are required, and how existing data is preserved. Do not apply a change without agreement.

### 3. Define initial roles, permissions, and Main Admin bootstrap

After verifying the schema, produce an idempotent data plan using the project’s approved data-seeding/deployment approach.

#### Initial role policy

- **Student:** the role assigned only server-side during public registration in Phase 3. It starts with no administrative permissions.
- **Main Admin:** a protected administrative role with the permissions needed to administer the system.
- **Sub-admin roles:** custom roles are supported by the data model and runtime authorization, but their full management UI/workflows are deferred to Phase 14.

#### Main Admin bootstrap decision

Choose and document one safe, repeatable approach that matches existing deployment practices:

1. Assign the Main Admin role to an explicitly identified existing administrator account through an approved deployment SQL script; or
2. Seed a configured initial administrator only if the project already has a secure, non-source-controlled bootstrap configuration pattern.

Never create a public registration path that makes a user an administrator. Never store a default administrator password in source control. Never select the Main Admin by email/name checks embedded in authorization code.

#### Permission data rules

1. Insert missing permissions by stable code only; do not create duplicates on repeated deployments.
2. Assign the intended full administrative permission set to the Main Admin role in the database.
3. Preserve existing roles/permissions unless an approved migration explicitly changes them.
4. Include the exact SQL/data action in the implementation report if the project does not have a safe existing seeding path.

### 4. Define RBAC DTOs, contracts, and Result outcomes

Keep DTOs feature-specific and follow current Application/API conventions. Do not return EF entities.

Potential read-only contracts needed in this phase:

- `CurrentUserAuthorizationResponse`: current user summary, role names/codes, and effective permission codes appropriate for the client.
- `PermissionResponse`: permission ID/code/name/description where supported by the schema.
- `RoleSummaryResponse`: role ID/name and optionally its assigned permission codes.

The Phase 3 `GET /api/auth/me` response may be extended to include safe role/permission information, or a separate authenticated endpoint may be used if that better matches existing conventions. It must never expose internal linkage IDs unnecessarily or treat returned permission data as a server-side security decision.

Define/reuse Result Pattern failures such as:

- `AuthenticationRequired`
- `PermissionDenied`
- `UserNotFound`
- `UserDeleted`
- `RoleNotFound`
- `PermissionNotFound`
- `RoleDeleted`
- `PermissionDeleted`
- `InvalidPermissionCode`
- `DuplicateRoleAssignment`
- `DuplicatePermissionAssignment`

`PermissionDenied` should map to `403 Forbidden` for an authenticated user. Missing or invalid authentication should map to `401 Unauthorized` through the standard authentication pipeline.

### 5. Implement current-user and permission-resolution services

Implement focused Application-layer services using injected `AppDbContext` and the existing Phase 3 current-user abstraction where one exists.

The authorization resolver must:

1. Obtain the stable authenticated user ID from the validated JWT claims, not from request input.
2. Verify that the user exists and is not soft-deleted.
3. Query active user-role, role, role-permission, and permission records.
4. Resolve a distinct set of effective permission codes for the user.
5. Treat missing/deleted associations and invalid permission codes as no grant.
6. Return an allow decision only when the requested permission is in the effective set.
7. Use async EF Core queries and no tracking for pure reads when appropriate.
8. Avoid circular service dependencies and avoid a generic authorization framework beyond what ASP.NET Core already provides.

Recommended service responsibilities:

| Service concern | Responsibility |
| --- | --- |
| Current-user accessor | Extract authenticated user ID from trusted claims. |
| Authorization query/service | Resolve effective permissions and answer whether one permission is granted. |
| Role/permission application service | Read/validate roles and permissions as needed for Phase 4 setup; full management stays in Phase 14. |

### 6. Integrate database-backed permission checks with ASP.NET Core authorization

Use the standard ASP.NET Core authorization system rather than custom JWT middleware.

Implement a small, reusable permission-authorization convention containing only the pieces genuinely required:

1. A permission requirement that contains a requested permission code.
2. An authorization handler that calls the database-backed authorization service for the current authenticated user.
3. A dynamic policy provider, **only if needed**, that creates policies from a consistent permission policy name such as `Permission:Course.Create`.
4. A concise endpoint/controller annotation or equivalent endpoint convention that declares the needed permission.

The handler—not the controller—performs the effective-permission lookup. Controllers remain thin and declare only the required capability. For example, a future course creation endpoint would require `Course.Create`; it would not contain `if (role == "Admin")` logic.

Authorization pipeline rules:

```text
Request
→ JWT bearer authentication validates token
→ authenticated user identity is established
→ permission policy resolves the requested permission
→ handler queries active RBAC relationships in SQL Server
→ request continues or returns 403
```

Do not rely only on role claims embedded during login: role/permission assignments can change while an access token is still valid. Live database evaluation ensures revocation and role changes take effect on the next protected request.

### 7. Establish endpoint-protection and ownership conventions

Phase 4 may not yet have course/lesson endpoints to protect, but it must establish a consistent mapping for later phases.

| Future action | Required permission |
| --- | --- |
| View/admin-list courses | `Course.Read` |
| Create course | `Course.Create` |
| Update course | `Course.Update` |
| Soft-delete course | `Course.Delete` |
| Create/update/delete lessons | Corresponding `Lesson.*` permission |
| Create/update/delete course-final quizzes | Corresponding `Quiz.*` permission |
| View/manage users | Corresponding `User.*` permission |
| Manage roles | Corresponding `Role.*` permission |
| Assign/manage permissions | `Permission.Assign` (and `Permission.Read` to view) |

Two checks are often required and must remain distinct:

1. **Permission authorization:** does the caller have the capability to perform this kind of action?
2. **Resource/ownership validation:** may the caller access this specific student-owned or tenant-sensitive record?

The first is established in Phase 4. Later student features must implement their own ownership checks in Application services; a permission alone must never let one student read or alter another student’s data.

### 8. Integrate with Phase 3 tokens and the Blazor application

#### API/JWT integration

1. Keep Phase 3 token validation configuration intact.
2. Continue including a stable user ID claim in the access token; this is the required link to RBAC lookup.
3. Role claims may be included only for UI display/convenience if already present, but they must not replace database-backed permission evaluation.
4. If the current user becomes soft-deleted or loses a role/permission, the next permission-protected API request must be denied without waiting for token expiry.

#### Blazor integration

1. Fetch current user/profile authorization data only through the API using `IHttpClientFactory`.
2. Use returned effective permissions to hide or disable unavailable admin navigation/actions for usability.
3. Treat UI visibility as convenience only: every sensitive API action remains protected by the API authorization handler.
4. On `401`, retain the Phase 3 one-time refresh behavior.
5. On `403`, show a clear “You do not have permission to perform this action” state; do not attempt token refresh or repeatedly retry.
6. Do not query SQL Server or `DbContext` from Blazor.

### 9. Add validation, logging, and operational safeguards

#### Validation

- Validate requested permission codes against the defined code format and database records when accepting role/permission setup input.
- Validate role and permission IDs before associations are created in any setup/administrative operation.
- Enforce duplicate prevention for user-role and role-permission associations.
- Enforce soft-delete rules consistently.
- Do not duplicate business validation across controller, application service, and handler layers.

#### Logging

Use structured Serilog events for meaningful authorization activity:

- Authorization success/failure where useful for operational diagnosis.
- Permission denied, including safe user identifier, requested permission code, route/action, and correlation/request ID.
- RBAC data setup/assignment events.
- Unexpected RBAC query/configuration failures.

Never log passwords, password hashes, JWTs, refresh tokens, signing keys, or full sensitive request bodies. Avoid noisy success logs for every normal request if standard request logging already captures sufficient information.

#### Caching decision

Do not introduce caching in the first RBAC implementation unless an existing, validated caching pattern is already in place. Correct, immediate permission revocation is more important than premature optimization. If caching is later required, it needs explicit invalidation for every role/permission assignment change.

### 10. Test and verify Phase 4

#### Unit/application test cases

| Scenario | Expected result |
| --- | --- |
| User has a role with required permission | Authorization succeeds. |
| User has role but lacks required permission | Authorization fails with `PermissionDenied`. |
| User has multiple roles and one grants permission | Authorization succeeds. |
| Duplicate permission granted through multiple roles | Effective permission set is distinct; decision still succeeds. |
| User has no roles | Authorization fails. |
| Role or permission is soft-deleted | It grants no permission. |
| User is soft-deleted | Authorization fails even with a valid JWT. |
| Invalid/missing user ID claim | Protected action is unauthenticated/denied safely. |
| Main Admin role has configured permissions | Required admin actions are permitted through normal database evaluation. |
| Sub-admin assigned only course permissions | Can use course actions but cannot use lesson/quiz/user/role actions. |
| Role/permission assignment changes after login | Next protected request reflects the database change. |

#### API integration test cases

| Request state | Expected HTTP response |
| --- | --- |
| No access token on protected endpoint | `401 Unauthorized` |
| Invalid/expired access token | `401 Unauthorized` |
| Valid token but no matching permission | `403 Forbidden` |
| Valid token with matching active permission | Endpoint is reached successfully |
| Valid token after role/permission revocation | `403 Forbidden` on the next protected request |

#### Manual Blazor checks

1. Sign in as a Student and confirm unavailable admin controls are hidden/disabled.
2. Sign in as Main Admin and confirm permitted controls are shown.
3. Sign in as a restricted sub-admin and confirm only assigned feature actions appear.
4. Attempt restricted API actions directly or through UI and confirm server `403` responses.
5. Change a user’s assignment through the approved setup path, then retry without re-login; confirm the next API request reflects the change.

#### Build and security checks

1. Build the complete solution and resolve compilation errors within Phase 4 scope.
2. Run relevant automated tests; document manual verification where a test project does not exist.
3. Confirm no SQL Server database was recreated, dropped, or silently modified.
4. Confirm no hard-coded Administrator email, password, role bypass, permission list bypass, JWT, refresh token, or secret was introduced.
5. Confirm public registration remains incapable of assigning an Admin role.

## Completion Criteria

Phase 4 is complete when:

- The API identifies the authenticated user from a validated Phase 3 JWT.
- Effective permissions are resolved from the current database relationships: User → Role → Permission.
- A valid authenticated user without a required permission receives `403`; an unauthenticated caller receives `401`.
- Main Admin and sub-admin access use the same database-backed authorization path—no hidden admin bypass exists.
- Soft-deleted users, roles, permissions, and associations do not grant access.
- Permission changes take effect on the next protected API request without requiring access-token expiry.
- Blazor uses API-provided permission information only for UX and never as the security boundary.
- The permission convention is ready for Phases 5–10 to protect administrative endpoints.
- Build and verification results are successful, and any required database change has been separately approved and documented.

## Required Handoff Report After Implementation

When Phase 4 is implemented, report:

1. Files created and modified.
2. RBAC functionality implemented and the authorization flow used.
3. Permission codes and initial role assignments created/changed.
4. API endpoints and authorization policies added/changed.
5. Database changes proposed/applied, with exact reason and deployment instructions.
6. Build result.
7. Automated and manual verification results.
8. Remaining issues or prerequisites before Phase 5.

