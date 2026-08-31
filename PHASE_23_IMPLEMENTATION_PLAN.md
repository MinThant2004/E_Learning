# Phase 23 — Course Exam & Question Pool Management: Detailed Implementation Plan

## 1. Objective

Phase 23 establishes the foundational data model, backend API, business logic, and admin management UI for **Course Exams** and the **Random Question Pool**. It allows Administrators to define exams for courses (fee, question count, duration, passing score) and populate question pools with multiple-choice options.

This phase lays the foundation for Phase 24 (Payments), Phase 25 (Exam Taking), and Phase 26 (Certificates).

---

## 2. Current State & Dependencies

- Phases 1–22 are completed (Authentication, Dynamic RBAC, Courses, Lessons, Quizzes, Enrollments, Progress, Scoring, Dashboard, Audit Logs, Reports).
- Existing `Quiz` tables and logic remain 100% untouched.
- `CourseExam` and `ExamQuestion` are new entities separate from `Quiz`.

---

## 3. Scope of Phase 23

### In Scope:
1. **Domain Layer**: New entities `CourseExam`, `ExamQuestion`, `ExamQuestionOption`.
2. **Database Layer**: Update `AppDbContext` & `IAppDbContext` + SQL table creation script.
3. **Application Layer**:
   - DTOs for exams, question pool, and options.
   - `ICourseExamService` and `CourseExamService` (CRUD for exams and question pool).
   - FluentValidation validators for exam and question creation/updates.
   - Service registration in `Application/DependencyInjection.cs`.
4. **Security / Infrastructure Layer**:
   - 6 new permission codes: `CourseExam.Read`, `CourseExam.Create`, `CourseExam.Update`, `CourseExam.Delete`, `ExamQuestion.Create`, `ExamQuestion.Update`.
   - Update `PermissionPolicyProvider.cs`.
5. **Api Layer**:
   - `CourseExamsController` (`/api/course-exams`)
   - `ExamQuestionsController` (`/api/course-exams/{examId}/questions`)
6. **Blazor App Layer**:
   - `CourseExamApiClient.cs` registered in `Program.cs`.
   - Admin Page `Pages/Admin/Exams/AdminExams.razor` (`/admin/exams`) — Exam management grid & Create/Edit modal.
   - Admin Page `Pages/Admin/Exams/QuestionPool.razor` (`/admin/exams/{ExamId:int}/questions`) — Question pool manager & Add/Edit Question modal.
   - Navbar update in `Components/Layout/NavMenu.razor` under "Administration".

### Out of Scope (Saved for subsequent phases):
- Payments & Payment Verification (Phase 24)
- Exam Taking Engine & Randomization (Phase 25)
- Certificate Generation (Phase 26)

---

## 4. Proposed Changes File-by-File

### 4.1 Domain Project (`ELearningManagementSystem.Domain`)

#### [NEW] [CourseExam.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Domain/Entities/CourseExam.cs)
```csharp
namespace ELearningManagementSystem.Domain.Entities;

public partial class CourseExam
{
    public int ExamId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal ExamFee { get; set; }
    public int QuestionCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public bool Status { get; set; } = true;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }

    public virtual Course Course { get; set; } = null!;
    public virtual User CreatedByNavigation { get; set; } = null!;
    public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
}
```

#### [NEW] [ExamQuestion.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Domain/Entities/ExamQuestion.cs)
```csharp
namespace ELearningManagementSystem.Domain.Entities;

public partial class ExamQuestion
{
    public int ExamQuestionId { get; set; }
    public int ExamId { get; set; }
    public string QuestionText { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public string DifficultyLevel { get; set; } = "Medium";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }

    public virtual CourseExam Exam { get; set; } = null!;
    public virtual ICollection<ExamQuestionOption> ExamQuestionOptions { get; set; } = new List<ExamQuestionOption>();
}
```

#### [NEW] [ExamQuestionOption.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Domain/Entities/ExamQuestionOption.cs)
```csharp
namespace ELearningManagementSystem.Domain.Entities;

public partial class ExamQuestionOption
{
    public int OptionId { get; set; }
    public int ExamQuestionId { get; set; }
    public string OptionText { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public bool DeleteFlag { get; set; }

    public virtual ExamQuestion Question { get; set; } = null!;
}
```

---

### 4.2 Database Project (`ELearningManagementSystem.Database`)

#### [MODIFY] [IAppDbContext.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Application/Interfaces/IAppDbContext.cs)
- Add `DbSet<CourseExam> CourseExams { get; }`
- Add `DbSet<ExamQuestion> ExamQuestions { get; }`
- Add `DbSet<ExamQuestionOption> ExamQuestionOptions { get; }`

#### [MODIFY] [AppDbContext.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Database/AppDbContext.cs)
- Register `DbSet<CourseExam>`, `DbSet<ExamQuestion>`, `DbSet<ExamQuestionOption>`.
- Configure EF Core relationships, indexes, primary keys, foreign keys, and column types (`decimal(18,2)` for `ExamFee`).

---

### 4.3 Application Project (`ELearningManagementSystem.Application`)

#### [NEW] Feature Folder structure under `Features/CourseExams/`:
```
Features/CourseExams/
├── DTOs/
│   ├── CourseExamDTOs.cs
│   └── ExamQuestionDTOs.cs
├── Services/
│   ├── ICourseExamService.cs
│   └── CourseExamService.cs
└── Validators/
    ├── CreateCourseExamRequestValidator.cs
    └── CreateExamQuestionRequestValidator.cs
```

#### Key DTO Definitions:
- `CourseExamResponse`: ExamId, CourseId, CourseTitle, Title, Description, ExamFee, QuestionCount, DurationMinutes, PassingScore, MaxAttempts, PoolQuestionCount, Status, CreatedAt.
- `CreateCourseExamRequest`: CourseId, Title, Description, ExamFee, QuestionCount, DurationMinutes, PassingScore, MaxAttempts.
- `UpdateCourseExamRequest`: Title, Description, ExamFee, QuestionCount, DurationMinutes, PassingScore, MaxAttempts, Status.
- `ExamQuestionResponse`: ExamQuestionId, ExamId, QuestionText, DisplayOrder, DifficultyLevel, Options (list of `ExamQuestionOptionResponse`).
- `CreateExamQuestionRequest`: QuestionText, DifficultyLevel, Options (list of `CreateExamOptionRequest`).

#### Service Methods in `ICourseExamService`:
- `Task<Result<PagedResult<CourseExamResponse>>> GetPagedExamsAsync(ExamListQuery query, CancellationToken cancellationToken);`
- `Task<Result<CourseExamResponse>> GetExamByIdAsync(int examId, CancellationToken cancellationToken);`
- `Task<Result<CourseExamResponse>> CreateExamAsync(CreateCourseExamRequest request, int userId, CancellationToken cancellationToken);`
- `Task<Result<CourseExamResponse>> UpdateExamAsync(int examId, UpdateCourseExamRequest request, CancellationToken cancellationToken);`
- `Task<Result> ArchiveExamAsync(int examId, CancellationToken cancellationToken);`
- `Task<Result> RestoreExamAsync(int examId, CancellationToken cancellationToken);`
- `Task<Result<List<ExamQuestionResponse>>> GetQuestionPoolAsync(int examId, CancellationToken cancellationToken);`
- `Task<Result<ExamQuestionResponse>> AddQuestionToPoolAsync(int examId, CreateExamQuestionRequest request, CancellationToken cancellationToken);`
- `Task<Result<ExamQuestionResponse>> UpdateQuestionInPoolAsync(int questionId, UpdateExamQuestionRequest request, CancellationToken cancellationToken);`
- `Task<Result> DeleteQuestionFromPoolAsync(int questionId, CancellationToken cancellationToken);`

#### Service Registrations in [DependencyInjection.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Application/DependencyInjection.cs)
```csharp
services.AddScoped<ICourseExamService, CourseExamService>();
```

---

### 4.4 Infrastructure Project (`ELearningManagementSystem.Infrastructure`)

#### [MODIFY] [PermissionPolicyProvider.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Infrastructure/Security/PermissionPolicyProvider.cs)
- Register permission codes: `CourseExam.Read`, `CourseExam.Create`, `CourseExam.Update`, `CourseExam.Delete`, `ExamQuestion.Create`, `ExamQuestion.Update`.

---

### 4.5 API Project (`ELearningManagementSystem.Api`)

#### [NEW] [CourseExamsController.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Api/Controllers/CourseExamsController.cs)
- `GET /api/course-exams` (`Permission:CourseExam.Read`) — Paged list of exams with search & filter.
- `GET /api/course-exams/{id}` (`Permission:CourseExam.Read`) — Get exam detail.
- `POST /api/course-exams` (`Permission:CourseExam.Create`) — Create course exam.
- `PUT /api/course-exams/{id}` (`Permission:CourseExam.Update`) — Update course exam.
- `POST /api/course-exams/{id}/archive` (`Permission:CourseExam.Delete`) — Soft delete exam.
- `POST /api/course-exams/{id}/restore` (`Permission:CourseExam.Update`) — Restore archived exam.

#### [NEW] [ExamQuestionsController.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Api/Controllers/ExamQuestionsController.cs)
- `GET /api/course-exams/{examId}/questions` (`Permission:CourseExam.Read`) — Get full question pool for exam.
- `POST /api/course-exams/{examId}/questions` (`Permission:ExamQuestion.Create`) — Add new question + options.
- `PUT /api/exam-questions/{questionId}` (`Permission:ExamQuestion.Update`) — Update question + options.
- `DELETE /api/exam-questions/{questionId}` (`Permission:ExamQuestion.Update`) — Soft delete pool question.

---

### 4.6 Blazor WASM App Project (`ELearningManagementSystem.App`)

#### [NEW] [CourseExamApiClient.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.App/Services/CourseExamApiClient.cs)
- Implement HTTP methods using `HttpClient` for all `/api/course-exams` and `/api/course-exams/{examId}/questions` endpoints.
- Register `CourseExamApiClient` in `Program.cs`.

#### [NEW] [AdminExams.razor](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.App/Pages/Admin/Exams/AdminExams.razor) (`/admin/exams`)
- Table displaying course exams (Course Name, Exam Title, Fee in MMK, Question Count, Duration, Passing Score, Pool Question Count, Actions).
- Search bar, page-size selector, active/archived toggle.
- Create / Edit Exam Modal with course selector, title, fee, question count, duration, passing score, and max attempts.
- Archive & Restore confirmation modal.

#### [NEW] [QuestionPool.razor](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.App/Pages/Admin/Exams/QuestionPool.razor) (`/admin/exams/{ExamId:int}/questions`)
- Header displaying Exam Title, Course Name, and Pool Health Badge (e.g. "Pool size: 120 / Required: 50").
- Question list accordion with difficulty badge (Easy/Medium/Hard) and option list.
- Add / Edit Question Modal with:
  - Question text input
  - Difficulty level dropdown
  - 4 Option text inputs
  - Radio button to select the single correct option.
- Delete Question confirmation modal.

#### [MODIFY] [NavMenu.razor](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.App/Components/Layout/NavMenu.razor)
- Add "Manage Course Exams" under Administration section gated by `Permission:CourseExam.Create`.

---

## 5. Verification Plan

### Automated Build Verification:
- Execute `dotnet build` on solution root to verify clean compilation across all 7 projects.

### Manual & Functional Verification:
1. **Admin Exam Creation**:
   - Navigate to `/admin/exams`.
   - Click "Create Exam", select a course, set title, fee (e.g., 15000 MMK), question count (50), duration (60 mins), passing score (70%).
   - Verify exam appears in table and pool count shows 0.
2. **Question Pool Population**:
   - Click "Manage Pool" for the created exam.
   - Add multiple questions with 4 options each, selecting one correct option per question.
   - Verify pool count updates dynamically on the header badge.
3. **RBAC & Authorization**:
   - Verify non-admin users cannot access `/admin/exams` or call the API endpoints directly.
