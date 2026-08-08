# Phase 15 — Audit Logging: Detailed Implementation Plan

## 1. Objective

Implement secure, searchable audit logging for important administrative and security-relevant business actions using the existing `AuditLogs` database model. Administrators can review authorized audit history without exposing secrets or private student data.

## 2. Current State

The project has an `AuditLog` entity and DbSet plus Serilog/request logging infrastructure. Phases 1–14 implement authentication, Dynamic RBAC, Active/Archived lifecycle actions, content administration, student learning, user management, and role/permission assignments. No complete verified audit-write service/filter, audit API, or audit UI should be assumed.

## 3. Scope

- Record significant archive/restore, create/update, user lifecycle, role/permission assignment, authentication/session, and quiz administration actions.
- Read paginated, filtered audit history through authorized API/UI.
- Reuse existing Result Pattern, EF Core, JWT/current-user, Serilog, and RBAC.

Excluded: full event sourcing, logging every ordinary read request, storing secrets, external SIEM integration, changing schema without approval, or replacing Serilog.

## 4. Business Rules

1. Audit records are append-only application history; ordinary users cannot edit/delete them.
2. Log actor, action, safe entity/table reference, target identifier, timestamp, and safe metadata only when fields exist in verified schema.
3. Required audited actions include Archive/Restore Category/Course/Lesson/Quiz, user archive/restore, role/permission assignment, and privileged create/update operations.
4. Never record passwords, password hashes, JWTs, refresh tokens, signing keys, answer keys, or full sensitive request bodies.
5. Audit write failure handling must be explicit: critical business operation/audit atomicity follows agreed policy and schema capability; do not silently swallow failures.
6. Audit viewing is admin-only and must respect a verified existing permission; do not expose logs to students.

## 5. Database Impact

Use existing `AuditLogs` table/entity and `Users` relationship. Inspect exact columns (`AuditLogId`, `UserId`, `Action`, `TableName`, timestamps, old/new values or metadata) plus nullability/length constraints and FKs. No schema change planned. If the table cannot store needed safe action data, document the minimal proposed change before implementation.

## 6. Domain Layer

Reuse scaffolded `AuditLog`. Add a small Shared constant/action catalog only if genuinely cross-feature and consistent with current project conventions; do not add event-sourcing entities or generic audit repository.

## 7. Application Layer

Add focused `IAuditLogService`/`AuditLogService` using injected `IAppDbContext`, and audit DTOs:

- `AuditLogListQuery`
- `AuditLogResponse`
- internal/safe `CreateAuditLogRequest` only where needed

Create a reusable application helper/service call used by business services after successful actions. Use Result Pattern for list/view outcomes: `AuditLogNotFound`, `InvalidAuditQuery`, `PermissionDenied`. Audit writes must sanitize metadata before persistence.

## 8. API Layer

| Endpoint | Purpose | Authorization |
| --- | --- | --- |
| `GET /api/audit-logs` | Paged/filterable audit history | Existing verified audit/admin read permission |
| `GET /api/audit-logs/{id}` | Safe audit detail | Same permission |

Do not add write endpoints for normal clients. Controllers remain thin. If existing RBAC catalog has no audit permission, identify it for explicit approval/seed rather than bypassing authorization.

## 9. Infrastructure Layer

Reuse current-user identity, Serilog, middleware, DI, and existing filters where appropriate. A narrowly scoped `AuditActionFilter` may record controller/action context only if it does not duplicate business audit records or embed business logic; primary actions should be recorded in Application services where outcome is known.

## 10. Database Layer

Use EF Core directly and no tracking for audit reads. Apply indexed/paged server-side filtering by action, table/entity, actor, and date range where columns exist. Write audit records in the same transaction as critical data changes only when transaction boundaries can be safely shared; otherwise document the consistency policy.

## 11. Blazor App

Add `AuditLogApiClient` through `IHttpClientFactory` and an authorized `/admin/audit-logs` page:

- Paged table: timestamp, actor, action, entity/table, safe target reference.
- Filters for action, entity/table, actor/date where supported.
- Safe detail drawer/page with no secrets.
- Loading, empty, validation, 401, 403, and error states.
- Responsive Tailwind UI with accessible filter labels.

No audit write control is shown to users.

## 12. RBAC / Permissions

Use existing Dynamic RBAC. Prefer a verified existing `AuditLog.Read`/administrative read permission; otherwise explicitly report the required permission data addition. Audit generation itself is internal trusted application behavior, not a client-controlled permission bypass.

## 13. Validation

- Bound page size/date range/filter string lengths.
- Validate filter values against safe action/entity catalogs where appropriate.
- Validate actor/target IDs before resolving display data.
- Enforce safe metadata length and redaction.

## 14. Result Pattern

Audit list/detail methods return existing `Result<T>`/`PagedResult<T>`. Expected filter/authorization errors are structured results; unexpected persistence failures use current exception middleware/logging policy.

## 15. Security Considerations

Derive actor identity from validated JWT/current-user context. Never trust actor ID/action supplied by Blazor. Restrict log viewing, redact sensitive fields, avoid exposing another student's private data through metadata, and prevent audit records from becoming an injection/XSS channel by encoding/rendering values safely.

## 16. Implementation Steps

1. Inspect `AuditLog` schema/entity, existing Serilog/actions, filters, and permission catalog.
2. Define audited business action catalog and redaction/transaction policy.
3. Add DTOs/query validator/service and safe EF projections.
4. Integrate audit writes into successful Application-layer operations incrementally.
5. Add DI and protected audit read endpoints.
6. Add App client and Tailwind audit-history page.
7. Test redaction, authorization, pagination/filtering, and action coverage.
8. Build and verify production-safe logging behavior.

## 17. Verification Checklist

- Archive/Restore content creates correct safe audit entries.
- User and RBAC assignment operations are auditable.
- No password/hash/token/answer key appears in database/API/UI/log output.
- Students and unauthorized sub-admins cannot read logs.
- Paging/filtering returns stable server-side results.
- Audit write failures follow documented consistency policy.
- UI handles empty/loading/401/403/error states.
- Build/tests pass without schema changes.

## 18. Definition of Done

Important administrative actions are safely auditable, authorized administrators can review paged history, and the solution retains existing EF Core, Result Pattern, RBAC, Serilog, and N-layer boundaries without secrets leakage or duplicate logging architecture.
