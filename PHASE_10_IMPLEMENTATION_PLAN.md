# Phase 10 — Final Quiz Attempts, Scoring, and Results: Detailed Implementation Plan

## 1. Objective

Implement the student-facing final course quiz flow: determine availability after required lesson completion, deliver safe questions, accept answers, calculate the score on the server, persist attempts/answers, and return immediate results.

## 2. Current State

Phases 1–9 provide authenticated students, live Dynamic RBAC, active/archived courses/lessons, enrollment, lesson progress/course completion, and administrator-managed course-final quizzes. Quiz, question, option, attempt, and answer entities are database-first scaffolded, but no student quiz-attempt/scoring feature should be assumed complete until current models are inspected.

## 3. Scope

- Determine whether a current student may take a final quiz for an enrolled course.
- Return student-safe quiz questions/options.
- Submit and persist a student’s answers.
- Calculate correct count, percentage/score, and pass status on the API.
- Return immediate result and own attempt history.

Excluded: quiz authoring, client-side scoring, certificates, retake policy beyond the verified schema/requirements, and unapproved database changes.

## 4. Business Rules

1. A student must be authenticated, active, and enrolled in the active course.
2. The course’s final quiz is available only after all required active lessons are complete.
3. Archived courses, lessons, quizzes, questions, and options cannot be used in a new attempt.
4. The client submits selected option identifiers only; it never submits correctness or score.
5. Every submitted question and option must belong to the available quiz and question respectively.
6. The API calculates score and pass status in one server-side operation.
7. The student may read only their own attempts/results.
8. Retake limits/one-attempt behavior must follow verified existing database fields and an explicit business decision; do not invent a limit.

## 5. Database Impact

Use existing `Enrollments`, `LessonProgress`, `Courses`, `Lessons`, `Quizzes`, question/option tables, `QuizAttempts`, and `QuizAnswers`. Inspect actual scaffolded entity/table names, keys, question correctness representation, attempt timestamp/score/pass fields, and FK constraints first. No schema change is planned. Report any missing required attempt/answer field before changing schema.

## 6. Domain Layer

Reuse scaffolded `QuizAttempt` and `QuizAnswer` entities and existing question/option model. Add no repository or duplicate scoring entity. Introduce an enum only when it maps a verified persisted concept.

## 7. Application Layer

Create a feature-oriented `QuizAttempts` service/interface using `IAppDbContext` directly.

Expected DTOs:

- `AvailableQuizResponse`, `StudentQuizQuestionResponse`, `StudentQuizOptionResponse` (never include correct-answer fields)
- `SubmitQuizAttemptRequest`, `SubmitQuizAnswerRequest`
- `QuizAttemptResultResponse` (score, total questions, correct answers, pass status, submitted time)
- `QuizAttemptHistoryResponse`

Expected Results: `CourseNotFound`, `EnrollmentNotFound`, `CourseNotCompleted`, `QuizNotFound`, `QuizNotAvailable`, `InvalidQuizAttempt`, `InvalidQuestion`, `InvalidOption`, `DuplicateAnswer`, `AttemptNotFound`, and `PermissionDenied`.

Use a transaction to validate availability, save attempt/answers, and calculate/persist result atomically.

## 8. API Layer

Expected routes, adjusted to established conventions:

| Endpoint | Purpose | Access |
| --- | --- | --- |
| `GET /api/courses/{courseId}/final-quiz` | Current student’s available safe quiz | Authenticated + ownership |
| `POST /api/courses/{courseId}/final-quiz/attempts` | Submit answers and calculate result | Authenticated + ownership |
| `GET /api/courses/{courseId}/final-quiz/attempts` | Current student attempt history | Authenticated + ownership |
| `GET /api/quiz-attempts/{attemptId}` | Current student result detail | Authenticated + ownership |

Controllers derive current user from JWT, delegate to Application services, and map Results: `400` validation, `401` no valid token, `403` denial, `404` unavailable/non-owned resources, and consistent conflict behavior for duplicate/concurrent attempt rules.

## 9. Infrastructure Layer

Reuse JWT/current-user, EF Core, Serilog, middleware, DI, and existing authorization pipeline. Log attempt created/submitted and scoring outcome with safe IDs/counts; never log raw tokens, correct answers, or full answer bodies unnecessarily.

## 10. Database Layer

Use EF Core transaction and relationship checks. Load correct-option information only inside the protected server scoring operation. Use no-tracking projection for result/history reads. Do not physically delete content or mutate quiz authoring data. Enforce active/archived filtering in database queries.

## 11. Blazor App

Add `QuizAttemptApiClient` via `IHttpClientFactory`.

Implement:

- Final-quiz availability card on completed course learning page.
- Quiz-taking page with question navigation, option selection, unanswered indication, and one submit action.
- Disabled/in-progress submit state to prevent duplicate clicks.
- Immediate result page with score, totals, correct count, pass state, and link back to course.
- Own attempt-history display where appropriate.
- Loading, unavailable/incomplete-course, empty, validation, 401, 403, and unexpected-error states.

Do not calculate or reveal correct answers in Blazor unless later requirements explicitly permit review.

## 12. RBAC / Permissions

Student access is enforced through authentication plus enrollment/attempt ownership. Reuse quiz administration permissions only for Phase 9 admin endpoints; students must not require or receive `Quiz.Update`/`Quiz.Delete`. No new authorization system is introduced.

## 13. Validation

- Valid course, quiz, question, and option identifiers.
- Active enrolled course and all required active lessons complete.
- Quiz belongs to course and is active.
- Submitted question IDs are unique and belong to quiz.
- Selected option IDs belong to submitted question and are active.
- Submission completeness rules follow verified quiz configuration.
- Attempt ownership and retake policy are checked server-side.

## 14. Result Pattern

Return existing `Result<T>` structures. Success returns the authoritative result DTO. Expected failures are stable Result errors, not exceptions; Blazor converts them into readable messages such as “Complete all lessons to unlock the final quiz.”

## 15. Security Considerations

Never trust a client score, correctness flag, answer key, user ID, enrollment ID, or attempt ID. Validate all relationships server-side and filter every history/detail query by current user. Keep answer keys out of student DTOs and structured logs. Re-check eligibility inside the submission transaction to resist stale UI/concurrent changes.

## 16. Implementation Steps

1. Inspect exact quiz/attempt/answer schema and Phase 8 progress contracts.
2. Agree/verify quiz availability and retake rules.
3. Add student-safe DTOs, validators, service interface, Result errors, and scoring service.
4. Implement transactional availability, submission, persistence, and score calculation.
5. Register DI and add thin ownership-protected API endpoints.
6. Add App API client, availability card, quiz form, result, and history UI.
7. Add logging and automated/manual tests.
8. Build solution and verify with active/archived content and RBAC cases.

## 17. Verification Checklist

- Incomplete enrolled student cannot open/submit final quiz.
- Completing every active lesson unlocks available quiz.
- Student-safe quiz response has no answer key/correctness metadata.
- Tampered question/option/attempt IDs are rejected.
- Server score matches stored valid answers; client-supplied score is ignored.
- Attempt and answer rows persist atomically.
- Student can view only own results/history.
- Archived content cannot create a new attempt.
- Duplicate/concurrent submit behavior follows configured policy.
- Build/tests pass without database schema changes.

## 18. Definition of Done

Eligible enrolled students can take a course-final quiz, submit answers once according to verified rules, receive an immediate server-calculated result, and access only their own history. The feature preserves N-layer boundaries, uses existing EF Core/Result Pattern/JWT/RBAC infrastructure, and exposes no answer key or client-trusted score.
