# Phase 19 — Logging Refinement: Detailed Implementation Plan

## 1. Objective

Refine Serilog and application logging so production support can trace requests, security events, business mutations, failures, and authorization decisions using structured, safe, actionable records.

## 2. Current State

The project uses Serilog, exception middleware, authentication logging, and Phase 15 audit logs. Logging exists across services but likely varies in event names, levels, properties, correlation values, duplication, and redaction. Audit logs and operational logs must remain separate but complementary.

## 3. Scope

- Establish structured logging conventions and consistent levels.
- Improve request/correlation context and exception logging.
- Add/normalize key security and business mutation events.
- Redact sensitive fields and prevent noisy duplicate logs.
- Verify log configuration, sinks, retention, and environment behavior already present.

Excluded: a new logging platform, external telemetry/SIEM integration, database audit redesign, logging every sensitive payload, or schema changes.

## 4. Business Rules

1. Log meaningful operational events: request completion, unexpected error, login outcome, refresh/logout, authorization denial, archive/restore, enrollment/progress, quiz submit/result, and admin RBAC changes.
2. Audit logs record business history; Serilog records diagnostics/operations. Do not create duplicate audit systems.
3. Never log passwords, password hashes, JWTs, refresh tokens, signing keys, connection strings, correct answer keys, or raw sensitive request bodies.
4. Normal expected Result failures should not be logged as unhandled errors.
5. Logs should include safe correlation/request IDs, route, status, actor ID where available, entity ID/action, and elapsed time.

## 5. Database Impact

No database change. Continue using `AuditLogs` only for Phase 15 audit history; Serilog sink changes must not require database writes unless an existing approved sink is configured.

## 6. Domain Layer

No domain model changes. Logging remains Infrastructure/API/Application cross-cutting concern and must not pollute domain entities.

## 7. Application Layer

Audit service logging. Standardize event templates/properties for successful important mutations and expected failures. Use `ILogger<T>` injected by DI. Avoid logging duplicate controller/service messages for one action; designate the business-service outcome as primary mutation event.

## 8. API Layer

Refine request logging middleware/Serilog request logging, exception middleware, controller endpoint context, and authentication/authorization failure logging. Ensure response status and trace identifier are available in safe error responses or logs according to existing conventions.

## 9. Infrastructure Layer

Review Serilog configuration (`appsettings`, sinks, minimum levels, enrichment, file rolling/retention) and environment overrides. Reuse existing configuration; add enrichers/redaction helpers only if necessary and compatible. Confirm DI logging works in Application/Infrastructure.

## 10. Database Layer

No EF Core mappings. Ensure EF Core command logging is appropriately configured: useful diagnostics in development, not excessive/sensitive SQL/parameter logging in production.

## 11. Blazor App

Review client logging/error reporting. Keep user-facing messages separate from diagnostic details. Do not send tokens or sensitive response bodies to browser console/logs. Ensure client 401/403/result errors remain user-friendly and correlation information can be reported safely if needed.

## 12. RBAC / Permissions

No new permissions. Log authorization denials with safe actor ID, requested permission, route/action, and correlation ID. Do not expose permission internals to unauthorized UI or logs accessible to users.

## 13. Validation

- Validate logging configuration at startup where current pattern supports it.
- Verify required sink paths/config values exist and are environment appropriate.
- Bound/sanitize structured fields from user input.
- Redaction tests cover every sensitive field category.

## 14. Result Pattern

Use Result category to select logging level: validation/not-found may be Debug/Information as appropriate; security denial Warning; unexpected exception Error. Do not turn normal Result failures into exception stack traces.

## 15. Security Considerations

Protect log files/sinks, apply retention/rotation, restrict production log access, redact secrets, and prevent log injection by structured logging rather than string-concatenated untrusted text. Ensure errors do not surface internal logging paths/configuration.

## 16. Implementation Steps

1. Inspect Serilog configuration, middleware, filters, and existing log templates.
2. Define approved event naming, levels, common properties, and redaction matrix.
3. Refine request/exception/authentication/authorization logging.
4. Normalize core Application feature mutation logs.
5. Confirm Phase 15 audit and Serilog responsibilities do not overlap incorrectly.
6. Review production/development sink/minimum-level/retention settings.
7. Add redaction and log-behavior tests.
8. Build and manually inspect representative logs.

## 17. Verification Checklist

- Request logs include method/route/status/elapsed/correlation safely.
- Login/refresh/logout/archive/enrollment/quiz/RBAC mutation events are useful.
- Expected validation/Result failures do not produce noisy error traces.
- Unexpected exceptions produce one actionable error with trace context.
- 401/403 denials log safe diagnostics.
- No password/hash/token/secret/answer key/connection string appears in logs.
- File sink rolling/retention/environment configuration works.
- Build and logging tests pass.

## 18. Definition of Done

The application emits consistent, structured, secure, operationally useful logs across API, Application, and Infrastructure while preserving audit-log boundaries, redacting all sensitive data, and requiring no new logging architecture or database change.
