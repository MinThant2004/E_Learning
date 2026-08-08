# Phase 20 — Validation Refinement: Detailed Implementation Plan

## 1. Objective

Review and standardize validation across all completed features so invalid, inconsistent, unauthorized, archived, or unsafe input is rejected at the correct layer with clear Result Pattern errors and reliable Blazor feedback.

## 2. Current State

The project has feature DTOs, authentication validators, Application services, a validation filter, Result Pattern, and numerous completed features. Validation likely varies by feature and may duplicate UI/controller/Application checks or omit database-length, relationship, archive-state, and pagination constraints.

## 3. Scope

- Inventory requests and validators for all features.
- Make Application-layer validation authoritative.
- Align API model/filter validation and Blazor UX validation.
- Validate business relationships, state transitions, paging/filtering, uploads, and security-sensitive inputs.
- Standardize validation Result/error UI behavior.

Excluded: new business features, replacing the existing validation approach, schema changes, introducing a second Result system, or moving business rules into Blazor.

## 4. Business Rules

1. Application layer is the final authority for business validation.
2. UI validation improves usability but never replaces server validation.
3. Controller/filter validation rejects malformed models before use-case invocation.
4. Database constraints remain a final safety net, not the primary user-feedback mechanism.
5. Active/Archived, ownership, enrollment, RBAC, and relationship rules are checked server-side for every mutation.
6. Validation messages are actionable but do not leak private/deleted resources or internal implementation details.

## 5. Database Impact

No planned database change. Inspect scaffolded maximum lengths, nullability, FKs, unique indexes, numeric types, and check constraints; align validators with verified schema rather than guessing.

## 6. Domain Layer

No new domain entities. Reuse domain enums/constraints where already present. Do not create generic validation entities or repositories.

## 7. Application Layer

Audit and complete feature validators/services for:

- Authentication/register/login/refresh.
- Categories, courses, thumbnail upload metadata, lessons/order.
- Enrollment and lesson progress ownership/state.
- Quiz/question/options/attempt submission.
- User profile/archive/restore.
- Roles, permissions, assignments.
- Audit-log filters and dashboard query parameters.

Use existing Result Pattern to return field/general errors. Consolidate shared simple rules only when genuinely cross-feature; avoid a huge generic validator abstraction.

## 8. API Layer

Audit JSON, multipart/form, route, query, and pagination binding. Ensure validation filter/model state response uses Phase 18 standardized shape. Verify controllers do not duplicate Application business checks or accept hidden IDs/fields that should come from routes/JWT.

## 9. Infrastructure Layer

Reuse existing validation filter, exception middleware, logging redaction, DI, and upload security handling. No new validation framework unless current project already references and uses it consistently.

## 10. Database Layer

Use EF Core to validate existence/relationships/state where database lookups are required. Avoid time-of-check/time-of-use races for duplicate/assignment/attempt mutations: rely on verified uniqueness constraints and transaction-safe rechecks. No migrations without explicit approval.

## 11. Blazor App

Audit every form and mutation UI:

- Required/format/length feedback before submit.
- Disabled/in-progress submit to prevent duplicates.
- Display server validation field/general errors consistently.
- Preserve safe input where appropriate after failure.
- Accessible labels, error associations, focus management, and mobile-friendly messages.

Use existing shared UI and `IHttpClientFactory` clients; do not put business validation/database access in components.

## 12. RBAC / Permissions

Validation never replaces authorization. Check valid IDs/relationships after Dynamic RBAC/ownership scope is established. Do not disclose whether a restricted resource exists. No new permission codes are required.

## 13. Validation Matrix

Validate at minimum:

| Area | Key rules |
| --- | --- |
| IDs/queries | positive IDs, bounded page/page-size/date/search values |
| Strings | required/trimmed/schema-length-safe; safe rendering |
| Uploads | allowed MIME/extension, size, image content policy, filename safety |
| Active state | prevent use of archived course/category/lesson/quiz/user/role as appropriate |
| Relationships | child belongs to specified parent; ownership/enrollment verified |
| Duplicates | email, role/permission assignment, enrollment, completion, answer selection |
| Scores | answers only; score/correctness server calculated |

## 14. Result Pattern

Validation errors use the Phase 18 Result/error convention: stable code, safe message, field errors where supported, API `400`, and consistent Blazor rendering. Expected conflicts/not-found/permission failures remain distinct from input validation.

## 15. Security Considerations

Prevent mass assignment, IDOR, over-posting, upload attacks, log injection, XSS-prone content, oversized input, duplicate submissions, and client-controlled privilege/progress/score data. Do not echo sensitive input in errors/logs.

## 16. Implementation Steps

1. Inventory all request DTOs, validators, model filters, schema constraints, and forms.
2. Create a validation matrix based on verified database/entity rules.
3. Fix/complete Application validators and business-state checks feature by feature.
4. Align API filter/model binding/error shapes.
5. Align Blazor form feedback and mutation guards.
6. Add duplicate/race-safe checks for critical mutations.
7. Add unit/integration tests for valid/invalid/boundary/unauthorized inputs.
8. Build and run regression verification.

## 17. Verification Checklist

- All DTOs reject missing/oversized/invalid data before unsafe persistence.
- Schema limits and unique constraints have matching validation.
- Archived/dependent/foreign-owned resources are rejected safely.
- Multipart upload validation blocks invalid/oversized input.
- Quiz score/ownership/answer integrity cannot be client-controlled.
- Forms show accessible server/client errors and prevent double submit.
- API returns standardized `400` Result errors without sensitive leakage.
- Existing successful flows remain unchanged; build/tests pass.

## 18. Definition of Done

All completed features have authoritative, consistent, schema-aware, security-conscious validation across Application/API/Blazor, with standardized Result feedback, no duplicate validation architecture, and no unapproved database change.
