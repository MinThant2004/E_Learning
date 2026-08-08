# Phase 17 — Middleware and Filters Refinement: Detailed Implementation Plan

## 1. Objective

Harden and standardize cross-cutting HTTP behavior: global exception handling, request logging, validation filtering, authentication/authorization pipeline ordering, consistent error responses, and safe audit-action integration where already available.

## 2. Current State

The API includes exception middleware, Serilog, JWT bearer authentication, Dynamic RBAC policies, and a validation filter. Phases 1–16 have introduced numerous endpoints and clients. Their cross-cutting behavior must now be audited for consistency rather than replaced with new frameworks.

## 3. Scope

- Inspect and refine middleware registration/order.
- Standardize unexpected exception/error response shape.
- Refine validation filter behavior and API model-state handling.
- Confirm JWT authentication/authorization placement and challenge/forbid behavior.
- Integrate audit action filter only if it complements—not duplicates—Phase 15 business audit records.
- Verify correlation/request logging, safe error handling, and CORS/HTTPS behavior.

Excluded: business rules in middleware, custom JWT validation middleware, replacing Result Pattern, database schema changes, or new generic pipeline frameworks.

## 4. Business Rules

1. Expected business failures come from Application Result Pattern, not exceptions.
2. Unexpected exceptions are centrally logged and mapped to a safe, consistent error response.
3. Validation occurs before controller business execution; invalid model requests never reach mutation services.
4. Authentication establishes identity; authorization enforces live permissions; neither is bypassed by custom middleware.
5. Client receives `401` for unauthenticated and `403` for authenticated but unauthorized requests.
6. Request/exception logs must never reveal secrets, passwords, hashes, JWTs, refresh tokens, answer keys, or sensitive bodies.

## 5. Database Impact

No database change. Audit persistence is used only through the existing Phase 15 model/service where applicable.

## 6. Domain Layer

No domain entity or rule changes. Domain/Application business logic must remain outside middleware and filters.

## 7. Application Layer

Audit Result error mapping contracts, validators, cancellation behavior, and Application exception boundaries. Ensure validators return structured failures. Do not add a second Result type or move business logic into filters.

## 8. API Layer

Audit `Program.cs`, controllers, filters, endpoint routing, authorization policies, and response mapping. Confirm intended pipeline order:

```text
Exception handling → HTTPS/CORS/routing → Authentication → Authorization → endpoint/controller
```

Retain standard `AddJwtBearer()` validation. Ensure API responses consistently carry safe Result/validation/error shapes.

## 9. Infrastructure Layer

Reuse Serilog configuration, JWT service, permission policy provider/handler, current-user service, and DI. Ensure middleware receives dependencies through DI. Do not introduce custom token parser middleware.

## 10. Database Layer

No DbContext responsibility in middleware. Any audit write must go through approved Application/Infrastructure service and avoid creating unexpected transaction/connection lifetime issues.

## 11. Blazor App

Verify clients correctly interpret standardized errors: validation display for `400`, refresh flow for `401`, permission state for `403`, unavailable state for `404`, and generic safe error for `500`. No App database access or browser confirmation changes belong in this phase.

## 12. RBAC / Permissions

Do not alter permission catalog or authorization model. Verify endpoints use existing policies consistently and that authentication/authorization middleware executes before protected endpoints. Audit-log viewing/action filters remain subject to verified permissions.

## 13. Validation

- Audit each request DTO and multipart endpoint for correct validation invocation.
- Ensure validation messages are bounded/safe and field errors map consistently.
- Confirm invalid JSON/form/model state returns `400` without controller execution.
- Do not duplicate Application validation in filters.

## 14. Result Pattern

Define a single mapping between Result success/failure and API status/body conventions. Middleware handles only unhandled exceptions; it must not transform normal Result failures into `500`.

## 15. Security Considerations

Test malformed JWT, expired token, invalid refresh flow, permission denial, oversized/malformed request, exception redaction, CORS origin policy, and HTTP logging redaction. Production errors must not return stack traces or connection strings.

## 16. Implementation Steps

1. Inspect middleware, filters, `Program.cs`, Serilog, and all controller error patterns.
2. Document current pipeline and inconsistent response behaviors.
3. Refine exception middleware response/log redaction and correlation behavior.
4. Refine validation filter/model-state conventions.
5. Confirm standard authentication/authorization/CORS order and protected route coverage.
6. Integrate audit filter only if no duplicate business audit entries result.
7. Update App common error handling only for verified API contract changes.
8. Run API integration/security tests and build solution.

## 17. Verification Checklist

- Validation failure returns consistent safe `400` and does not mutate data.
- Application Result failure is not converted to `500`.
- Unexpected exception is logged and returns safe error without stack trace.
- Missing/invalid JWT returns `401`; valid JWT lacking permission returns `403`.
- Permission handler and current-user service receive expected identity.
- Request logs redact sensitive fields/tokens.
- CORS/HTTPS behavior matches configured Blazor origins.
- Existing archive, enrollment, quiz, RBAC, and dashboard endpoints keep working.
- Build and API integration tests pass.

## 18. Definition of Done

The API has a predictable, secure cross-cutting pipeline: validation and Result failures are consistent, unexpected errors are safely handled/logged, standard JWT/RBAC authorization remains authoritative, and Blazor receives reliable HTTP behavior without new architecture or database changes.
