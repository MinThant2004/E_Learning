# Phase 3 — Authentication, JWT, and Refresh Tokens: Implementation Plan

## Purpose

Implement secure student authentication for the existing E-Learning Management System. Phase 3 establishes registration, login, JWT access tokens, refresh-token rotation, authenticated-user access, logout, and automatic token refresh in the Blazor application.

This phase must be completed before Dynamic RBAC (Phase 4). It must not implement permission-based authorization, course management, lessons, quizzes, or any later phase.

## Phase Boundaries

### Included

- Public student registration
- Secure password hashing and verification
- Login with JWT access-token issuance
- Refresh-token issuance, storage, rotation, revocation, and expiry validation
- Authenticated `me` endpoint
- Logout / refresh-token revocation
- JWT bearer authentication configuration
- Blazor authentication state and one-time automatic token refresh
- Feature-specific DTOs, validation, result handling, logging, and tests for authentication

### Explicitly excluded

- Dynamic permissions and permission policies (Phase 4)
- Admin/sub-admin role management (Phase 4 and later)
- Course, lesson, enrollment, progress, or quiz features
- Repository Pattern, Unit of Work, CQRS, MediatR, or new generic abstractions
- Recreating or replacing the existing SQL Server database

## Guiding Rules

- Inspect the existing scaffolded `DbContext`, entity classes, application services, configuration, and project references before adding anything.
- Use EF Core `DbContext` through dependency injection; do not introduce repositories or a Unit of Work.
- Keep controllers thin. Place use-case logic in the Application layer.
- Do not expose EF Core entities, password hashes, access tokens in logs, refresh tokens in logs, or JWT signing secrets in API responses.
- Public registration must always assign the existing **Student** role server-side. It must not accept a client-provided role identifier.
- Use the existing database schema as the source of truth. Do not apply a schema change until the model and database have been inspected and the required change is explicitly documented.

## Implementation Sequence

### 1. Perform a read-only implementation audit

Inspect and document the current implementation before changing files:

1. Solution and project references, including which layers already reference the Database and Infrastructure projects.
2. `AppDbContext`, all scaffolded user/role-related entities, configurations, table names, primary keys, nullability, and soft-delete fields.
3. Existing tables or entities for refresh tokens, sessions, token expiry, revocation, or user authentication.
4. Existing user, role, and user-role records, especially the exact representation of the Student role.
5. Current `appsettings*.json`, secret-management approach, API startup (`Program.cs`), middleware order, Serilog configuration, and CORS configuration.
6. Existing Result Pattern, DTO conventions, validation approach, controller conventions, error-response mapping, and HTTP client configuration in Blazor.
7. Existing authentication components, `AuthenticationStateProvider`, storage mechanism, route guards, or HTTP message handlers to reuse rather than duplicate.

**Output of this step:** a short implementation note listing what already exists, what can be reused, and any required schema gap. No schema change is made during the audit.

### 2. Confirm the minimal persistence design

Use the existing schema if it supports refresh tokens. If it does not, prepare a minimal schema-change proposal for approval before implementing it.

The persistence design must support the following information per refresh token:

| Data | Purpose |
| --- | --- |
| Token identifier | Uniquely identifies the stored refresh-token record. |
| User identifier | Associates the token with its owner. |
| Token value or secure hash | Allows a submitted refresh token to be validated without logging it. |
| Expiry time (UTC) | Prevents use after the configured refresh-token lifetime. |
| Created time (UTC) | Supports auditability and lifecycle checks. |
| Revoked time / revoke state | Supports logout and invalidation. |
| Replacement reference, if supported | Connects rotated tokens and supports reuse detection. |

If a schema addition is needed, it should be limited to the refresh-token persistence required above, include a foreign key to the existing user table, and follow the current database naming and soft-delete conventions. Do not create a new database.

### 3. Define authentication configuration and secrets

Add or reuse configuration settings for:

- JWT issuer and audience
- JWT signing key or secure key-source configuration
- Access-token lifetime (short-lived)
- Refresh-token lifetime (longer-lived)
- Refresh-token cookie settings if the chosen existing application design uses secure cookies

Implementation requirements:

1. Keep secrets out of source control. Use the project’s existing secret-management mechanism; use local development secrets or environment variables where appropriate.
2. Validate required JWT configuration at startup and fail clearly when it is invalid or missing.
3. Use UTC consistently for issued-at, expiry, and revocation timestamps.
4. Do not make access tokens unlimited-lived.
5. Do not hard-code signing keys, issuer, audience, or lifetime values in services/controllers.

### 4. Establish authentication contracts and results

Create feature-scoped request/response DTOs in the Application/API conventions already present in the solution.

Expected request contracts:

- `RegisterRequest`: name, email, password
- `LoginRequest`: email, password
- `RefreshTokenRequest`: only if refresh tokens are sent in a request body rather than an HttpOnly cookie
- `LogoutRequest`: only if required by the chosen transport design

Expected response contracts:

- `AuthResponse`: access token, access-token expiry, authenticated user summary, and the appropriate refresh-token transport behavior
- `CurrentUserResponse`: non-sensitive authenticated user fields (for example user ID, name, email, and role names as available)
- Standard validation/business-failure response using the existing Result Pattern

Define/reuse structured business errors, such as:

- `EmailAlreadyExists`
- `InvalidCredentials`
- `AccountNotFound`
- `AccountDeleted`
- `StudentRoleNotConfigured`
- `InvalidRefreshToken`
- `ExpiredRefreshToken`
- `RevokedRefreshToken`
- `AuthenticationRequired`

Do not return whether a login email exists; return a generic invalid-credentials result for failed login.

### 5. Implement password security services

Implement or reuse a focused infrastructure service for password hashing and verification.

Requirements:

1. Use a modern, salted password-hashing algorithm provided by the approved platform/library already compatible with the solution.
2. Hash the password at registration; never save plaintext passwords.
3. Verify using the password-hashing service at login.
4. Keep hashing details outside controllers and do not expose hashes through DTOs.
5. Add tests for successful verification, invalid-password rejection, and password hashes differing from the plaintext input.

### 6. Implement JWT and refresh-token services

Create or complete focused authentication infrastructure services and register them through DI.

JWT issuance must:

1. Create a signed, short-lived access token using configured issuer, audience, key, and expiry.
2. Include only necessary identity claims (at minimum a stable user identifier; include name/email/role claims only when supported and necessary).
3. Avoid adding dynamic permission claims in this phase; that work belongs to Phase 4.
4. Return the token and its expiry to the application service without logging the raw value.

Refresh-token lifecycle must:

1. Generate cryptographically secure, unpredictable token values.
2. Persist the token according to the confirmed schema.
3. Validate ownership, expiry, and revocation when it is presented.
4. Rotate the refresh token after successful refresh: revoke the old token and issue/persist a replacement.
5. Reject revoked, expired, unknown, malformed, and reused tokens.
6. Revoke the current refresh token on logout.
7. Define whether logout revokes one session or all of the user’s active sessions; use the simplest behavior supported by the existing schema and document it.

### 7. Implement Application-layer authentication use cases

Implement feature-oriented application services using `AppDbContext` directly through DI and returning the established Result Pattern.

#### Registration

1. Validate name, email, and password using the project’s validation conventions.
2. Normalize the email according to existing project conventions and check for a non-deleted duplicate user.
3. Hash the password.
4. Create the user with required audit fields.
5. Locate the existing Student role server-side and create the user-role relationship.
6. If the Student role is not correctly configured, fail safely with a clear result and do not create a partly configured account.
7. Decide whether registration immediately authenticates the user; prefer the existing app convention. If no convention exists, return a normal registration success and require login to keep the flow simple.

#### Login

1. Locate the non-deleted user by normalized email.
2. Verify the password using the password-hashing service.
3. Return generic `InvalidCredentials` for either an unknown email or an incorrect password.
4. Issue an access token and a refresh token after successful verification.
5. Persist the refresh-token record.
6. Return the established authentication response.

#### Refresh

1. Receive the refresh token only from the approved transport mechanism.
2. Validate the stored record, user state, token expiry, and revocation state.
3. Rotate the token atomically as supported by EF Core/database transaction conventions.
4. Issue a new access token and a replacement refresh token.
5. Return the new authentication response without exposing internal token record details.

#### Current user

1. Read the authenticated user identifier from validated JWT claims.
2. Fetch the non-deleted user and return only safe profile fields.
3. Return an authentication failure if the user no longer exists or has been soft-deleted.

#### Logout

1. Identify the presented/current refresh token using the approved transport mechanism.
2. Revoke it if found and active.
3. Clear the client refresh-token transport where relevant.
4. Return a safe, idempotent success response; logout must not disclose token validity details.

### 8. Configure API authentication and endpoints

Configure ASP.NET Core JWT bearer authentication using standard `AddAuthentication().AddJwtBearer()` and the standard authorization pipeline.

Middleware order must preserve the expected flow:

```text
Exception handling
→ Request logging
→ Routing
→ Authentication
→ Authorization
→ Endpoint execution
```

Add thin API endpoints using current route conventions. The expected endpoint set is:

| Endpoint | Purpose | Access |
| --- | --- | --- |
| `POST /api/auth/register` | Register a public Student account | Anonymous |
| `POST /api/auth/login` | Authenticate and receive a token pair | Anonymous |
| `POST /api/auth/refresh` | Rotate a valid refresh token and issue a new pair | Anonymous, token validated by endpoint logic |
| `POST /api/auth/logout` | Revoke the current refresh token | Authenticated or token-transport-aware, per chosen design |
| `GET /api/auth/me` | Return the authenticated user profile | Authenticated |

Endpoint requirements:

- Map Application Result outcomes to consistent status codes using existing conventions.
- Use `[Authorize]` or equivalent standard authorization for `me` and appropriate protected logout behavior.
- Do not introduce custom JWT-validation middleware.
- Apply request validation through the established validation mechanism.

### 9. Implement Blazor authentication integration

Keep all database access in the API. The App communicates through `IHttpClientFactory` and feature-specific API/client services.

Implement or complete:

1. Registration and login UI/service requests.
2. Safe client-side storage/transport of the access token according to the existing application approach.
3. Refresh-token storage/transport using the approved design; prefer an HttpOnly, Secure cookie when the deployment design supports it, rather than exposing the refresh token to application JavaScript.
4. An `AuthenticationStateProvider` (or existing equivalent) that derives the current state from a valid access token and refreshes `me` when needed.
5. An authorized HTTP message handler that attaches the access token.
6. A one-time refresh-and-retry flow on an API `401` response:
   - Attempt refresh once.
   - On success, store/use the new access token and retry the original request once.
   - On failure, clear local authentication state and redirect the user to login.
   - Never loop indefinitely.
7. Protected-route behavior for authenticated pages.
8. Logout UI behavior that calls the API, clears local access-token state, and redirects to login.

### 10. Add logging, security checks, and error handling

Use structured Serilog events for relevant authentication events:

- Registration succeeded/failed (without password data)
- Login succeeded/failed
- Refresh succeeded/failed
- Logout/revocation succeeded
- Invalid or expired JWT/refresh-token conditions at an appropriate severity

Never log:

- Passwords or password hashes
- JWT access tokens
- Refresh tokens
- Signing keys or configuration secrets

Ensure expected business failures are returned through the Result Pattern. Reserve exception middleware for unexpected failures.

### 11. Verify the feature

Build and test Phase 3 in layers.

#### Application/API test cases

| Scenario | Expected result |
| --- | --- |
| Valid registration | User is created, password is hashed, and Student role is assigned server-side. |
| Duplicate email | Structured `EmailAlreadyExists` failure; no duplicate user. |
| Client attempts to submit an admin role | Ignored/rejected; public registration still cannot create an Admin. |
| Valid login | Short-lived access token and active refresh token are issued. |
| Invalid email or password | Generic `InvalidCredentials` failure. |
| Valid authenticated `me` request | Returns non-sensitive user information. |
| Missing/invalid/expired access token | Protected endpoint returns `401`. |
| Valid refresh token | Old token is revoked and a new token pair is issued. |
| Reused/revoked/expired refresh token | Refresh is rejected; no new token pair is issued. |
| Logout | Current refresh token cannot refresh a session afterward. |
| Soft-deleted user | Cannot log in or use/refresh a session. |

#### Blazor verification

1. Register and login through the UI.
2. Visit a protected page successfully.
3. Simulate or wait for access-token expiry and verify exactly one refresh/retry occurs.
4. Confirm a failed refresh returns the user to login and clears local authenticated state.
5. Log out and confirm protected pages are inaccessible after logout.

#### Build and configuration verification

1. Build the complete solution.
2. Run relevant automated tests, if the solution has a test project; otherwise document manual verification results.
3. Verify no connection strings, signing keys, or tokens were added to tracked source files.
4. Confirm that the existing SQL Server database was not recreated or modified without approved schema work.

## Completion Criteria

Phase 3 is complete when all of the following are true:

- A public user can register only as a Student.
- Passwords are securely hashed and are never returned or logged.
- A valid user can log in and receive a short-lived JWT access token plus a refresh-token mechanism.
- Protected endpoints require a valid JWT.
- A valid refresh token produces a new access token and rotates the refresh token.
- Logout prevents the current refresh token from being used again.
- The Blazor client refreshes once after a `401`, retries once, and signs out cleanly if refresh fails.
- Result Pattern, validation, logging, soft-delete handling, DI, and layer boundaries follow the project context.
- The solution builds successfully and the verification scenarios above pass.

## Required Handoff Report After Implementation

When Phase 3 is implemented, report:

1. Files created and modified.
2. Authentication functionality implemented.
3. API endpoints added or changed.
4. Any database change proposed/applied, with exact reason.
5. Build result.
6. Automated and manual verification results.
7. Remaining issues or prerequisites before Phase 4.

