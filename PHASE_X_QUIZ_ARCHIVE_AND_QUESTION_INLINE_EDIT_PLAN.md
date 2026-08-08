# Quiz Archive/Restore and Question Inline Editing Plan

## 1. Objective

Refine Quiz Management so admins manage quiz lifecycle with Active/Archived terminology and edit each question inline inside its own question card, keeping its existing options visibly grouped and intact.

## 2. Current Implementation Analysis

- `QuizService` already supports `GetQuizzesByCourseAsync(courseId, bool? isArchived)`, `ArchiveQuizAsync`, and `RestoreQuizAsync` using `Quizzes.DeleteFlag`.
- Restore already blocks a quiz whose parent course is archived and prevents multiple active quizzes for the same course.
- `QuizApiClient` already exposes archive/restore operations and nullable archive filtering.
- `Question`/`QuestionOption` are an existing aggregate. `UpdateQuestionAsync` updates question text/order without recreating options.
- Options do not support `DeleteFlag`; the current service explicitly returns `ArchiveNotSupported` for option archive/restore. Therefore this plan does not add option soft archive.
- Current question-edit UI must be inspected in `ManageQuizzes.razor`; its current top-level edit state is the likely cause of the form moving away from the selected card.

## 3. Required Changes

1. Add/complete Manage Quizzes status filter with `All` = `null`, `Active` = `false`, `Archived` = `true`; default is `All` as requested.
2. Render a professional table: `No. | Quiz Title | Course | Status | Actions`, using `index + 1`, never `QuizId`.
3. Active row: Edit and Archive; Archived row: Restore only.
4. Use shared `ConfirmationModal` for both lifecycle actions; preserve current filter after refresh.
5. Move question edit state from a page-level form to a question-card-level state keyed by `QuestionId`.
6. Render the edit input and Save/Cancel controls inside only the selected question card, above its existing Options section.
7. Keep options rendered from the existing question response during edit; update only the selected question on Save.

## 4. Database Impact

No schema change. Use `Quizzes.DeleteFlag` only. Existing attempts, questions, and options remain stored when quiz is archived. Do not physically delete quizzes/questions/options as part of quiz archive/restore.

## 5. Domain Changes

None. Reuse `Quiz`, `Question`, and `QuestionOption`. Do not add question soft delete because the existing schema does not support it.

## 6. Application Changes

- Retain existing `ArchiveQuizAsync`/`RestoreQuizAsync` service methods and dependency checks.
- Ensure `GetQuizzesByCourseAsync` maps nullable status exactly: null returns both, false Active only, true Archived only.
- Retain `UpdateQuestionAsync` as a text/order update only; it must not remove/recreate options.
- Result errors: `QuizNotFound`, `CannotRestoreArchivedCourse`, active-quiz conflict, `QuestionNotFound`, `InvalidQuestionText`, and `PermissionDenied`.

## 7. API Changes

Reuse existing endpoints where available:

- `GET /api/courses/{courseId}/quizzes?isArchived={true|false}`; omit parameter for All.
- `POST /api/quizzes/{quizId}/archive`.
- `POST /api/quizzes/{quizId}/restore`.
- Existing `PUT /api/questions/{questionId}` endpoint for inline Save.

Do not add duplicate archive/restore or question endpoints. Confirm controllers map errors consistently through Result Pattern.

## 8. Blazor/UI Changes

- Add status dropdown bound to nullable `StatusFilter`; changing it reloads quizzes.
- Use Active/Archived badges; no Delete/Deleted wording.
- Keep one `PendingQuiz`/action state for shared archive/restore modal.
- Maintain `EditingQuestionId` and an edit draft keyed to the selected question. On Edit, initialize draft from that question; on Cancel, clear only that draft.
- In the question-card `@if (EditingQuestionId == question.QuestionId)` branch, render input + Save/Cancel before Options; otherwise render static text + Edit action.
- Keep Options component/markup outside the edit branch so it remains visible.
- On Save success, refresh the quiz detail and clear edit state; preserve options and scroll position where practical.
- Provide loading, empty, Result error, successful Archive/Restore, 401, and 403 states using existing Tailwind/shared patterns.

## 9. RBAC / Permission Changes

Use live existing permissions. Verify the actual database catalog before changes:

- `Quiz.Read` for list/detail.
- `Quiz.Update` for quiz/question editing and restore according to existing conventions.
- Existing `Quiz.Delete` may currently protect archive; use it rather than adding a duplicate code.

If `Quiz.Archive`/`Quiz.Restore` already exist, use them; otherwise do not create duplicate permissions merely for naming. Students receive no archive/restore access.

## 10. Validation

- Valid course and quiz IDs.
- Status filter accepts only null/true/false.
- Archive only active; restore only archived.
- Restore requires active parent course and no conflicting active final quiz.
- Question text required, trimmed, schema-length safe.
- Edited question must belong to the current quiz before client state/API update.
- No option IDs/text/correct flags are changed by question-text Save.

## 11. Error Handling

Parse existing Result/API error response in `QuizApiClient`/page. Display user-friendly messages, including: course must be restored first, another active course quiz exists, question no longer exists, and update failed. Do not suppress failed responses or use `window.alert`/`window.confirm`.

## 12. Loading and Empty States

- Disable archive/restore/save while request is in progress.
- Show loading skeleton/spinner for quiz list/detail.
- Show “No active/archived quizzes found” based on selected filter.
- Keep selected filter after mutation; if Archive empties current Active list, show correct empty state.

## 13. Ordered Implementation Steps

1. Inspect `ManageQuizzes.razor`, `QuizzesController`, DTOs, and current permission annotations.
2. Confirm current filter behavior/API route mapping and correct it if All is coerced to Active.
3. Implement table/filter/status-badge UI using existing client service.
4. Wire shared confirmation modal for Archive and Restore with action-specific copy.
5. Replace page-level question editing state with `QuestionId`-scoped inline state.
6. Keep Options rendering in selected card during editing; call existing update endpoint on Save.
7. Add Result/error/success/loading behavior and preserve filter/UI state after reload.
8. Build and test API/UI/RBAC cases.

## 14. Verification Checklist

- All shows active and archived quizzes; Active and Archived filters return correct records.
- Row number is 1-based display order, not QuizId.
- Archive opens custom modal, sets `DeleteFlag = 1`, and hides quiz from Active/student availability.
- Restore opens custom modal, sets `DeleteFlag = 0`, and fails safely if parent course is archived/conflicting active quiz exists.
- Existing QuizAttempts, questions, and options remain unchanged across archive/restore.
- Editing Question 1 appears only in Question 1 card; Question 2 stays unchanged.
- Options remain visible while Question 1 is edited; Save changes only question text/order; Cancel restores display.
- Student cannot invoke archive/restore/question edit endpoints; restricted admin RBAC works.
- No browser dialogs, physical deletes, schema changes, or option soft-delete behavior are introduced.

## 15. Definition of Done

Admins can clearly filter and manage Active/Archived quizzes with reusable modal confirmation, while question editing occurs inline within the selected question and retains its option relationship and visible context. Quiz archive remains a safe `DeleteFlag` lifecycle operation with no student access to archived quizzes and no loss of historical attempts.
