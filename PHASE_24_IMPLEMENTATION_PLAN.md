# Phase 24 — Exam Payment Verification System: Detailed Implementation Plan

## 1. Objective

Phase 24 implements the **Payment Submission & Admin Verification Engine** for Course Exams. It connects completed course students to paid exams using Myanmar's widely used payment methods (KPay, AyaPay, CB Pay, Wave Pay).

### Core Rule:
**1 Approved Payment = Exactly 1 Exam Attempt.**
- Student completes all lessons & course quiz $\to$ Unlocks Course Exam payment option.
- Student submits KPay / AyaPay transaction ID + payment screenshot.
- Payment Status: `Pending` $\to$ Admin verifies screenshot & TxnID in `PendingPayments.razor`.
- Admin Approves $\to$ Payment Status: `Approved` $\to$ Student unlocks 1 exam attempt.
- Admin Rejects $\to$ Payment Status: `Rejected` with reason $\to$ Student can re-submit screenshot.

### Notification & UX Alert System (In-App & Toast):
- **On Payment Approval**: Student receives a notification (`"Payment Approved! 🎉 You can now start taking your 'C# Certification Exam'."`).
- **On Payment Rejection**: Student receives a notification (`"Payment Needed Attention ⚠️ Reason: {Reason}. Click to re-submit."`).
- **On Payment Submission**: Admin receives a notification (`"New Exam Payment Received 💰 From Mg Mg for 'C# Certification Exam'."`).

---

## 2. Scope of Phase 24

### In Scope:
1. **Domain Layer**: `ExamPayment.cs` entity.
2. **Database Layer**: Update `IAppDbContext` & `AppDbContext` + SQL script & auto-creation logic for `ExamPayments` table.
3. **Application Layer**:
   - `Features/ExamPayments/DTOs/ExamPaymentDTOs.cs`
   - `Features/ExamPayments/Validators/SubmitExamPaymentRequestValidator.cs`
   - `Features/ExamPayments/Services/IExamPaymentService.cs` & `ExamPaymentService.cs`
   - Service registration in `Application/DependencyInjection.cs`.
4. **Api Layer**:
   - File upload handling (`wwwroot/uploads/exam-payments/`).
   - `ExamPaymentsController.cs` endpoints for student submission and admin review drawer.
5. **Blazor App Layer**:
   - `ExamPaymentApiClient.cs` registered in `Program.cs`.
   - **Student UI**: `Pages/Student/Exams/CourseExams.razor` (`/course-exams`) & `Components/Exams/PaymentModal.razor`.
   - **Admin UI**: `Pages/Admin/Payments/PendingPayments.razor` (`/admin/payments`) with screenshot preview drawer & verification workflow.
   - **NavMenu**: Add "Course Exams" link for Students & "Pending Payments" link for Admins.

### Out of Scope (Saved for subsequent phases):
- Exam Taking Engine & Randomization (Phase 25)
- Certificate Generation (Phase 26)

---

## 3. Proposed Changes File-by-File

### 3.1 Domain Project (`ELearningManagementSystem.Domain`)

#### [NEW] [ExamPayment.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Domain/Entities/ExamPayment.cs)
```csharp
namespace ELearningManagementSystem.Domain.Entities;

public partial class ExamPayment
{
    public int ExamPaymentId { get; set; }
    public int ExamId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!; // KPay, AyaPay, CBPay, WavePay
    public string TransactionId { get; set; } = null!;
    public string ScreenshotUrl { get; set; } = null!;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public bool IsUsed { get; set; } = false; // Set to true when exam attempt is started
    public string? RejectionReason { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool DeleteFlag { get; set; }

    public virtual CourseExam Exam { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual User? ReviewedByNavigation { get; set; }
}
```

---

### 3.2 Database Project (`ELearningManagementSystem.Database`)

#### [MODIFY] [IAppDbContext.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Application/Interfaces/IAppDbContext.cs)
- Add `DbSet<ExamPayment> ExamPayments { get; }`

#### [MODIFY] [AppDbContext.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Database/AppDbContext.cs)
- Register `DbSet<ExamPayment>` and configure Fluent API keys, column types (`decimal(18,2)` for `Amount`), foreign keys, and default values.

#### [MODIFY] [DependencyInjection.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Database/DependencyInjection.cs)
- Update auto-table creation to automatically create `ExamPayments` table in SQL Server on API startup.

---

### 3.3 Application Project (`ELearningManagementSystem.Application`)

#### [NEW] Feature Folder structure under `Features/ExamPayments/`:
```
Features/ExamPayments/
├── DTOs/
│   └── ExamPaymentDTOs.cs
├── Services/
│   ├── IExamPaymentService.cs
│   └── ExamPaymentService.cs
└── Validators/
    └── SubmitExamPaymentRequestValidator.cs
```

#### Key DTO Definitions:
- `SubmitExamPaymentRequest`: PaymentMethod (KPay/AyaPay/CBPay/WavePay), TransactionId.
- `ReviewExamPaymentRequest`: PaymentId, Approve (bool), RejectionReason (string?).
- `ExamPaymentResponse`: PaymentId, ExamId, ExamTitle, CourseTitle, UserId, StudentName, StudentEmail, Amount, PaymentMethod, TransactionId, ScreenshotUrl, Status, IsUsed, RejectionReason, ReviewedByName, ReviewedAt, CreatedAt.
- `StudentCourseExamStatusResponse`: ExamId, CourseId, CourseTitle, ExamTitle, Description, ExamFee, QuestionCount, DurationMinutes, PassingScore, PoolQuestionCount, PaymentStatus (NotPaid, Pending, Approved, Rejected), PaymentId, RejectionReason, CanTakeExam.
- `PaymentListQuery`: Page, PageSize, Status (Pending, Approved, Rejected, All), SearchTerm.

#### Service Methods in `IExamPaymentService`:
- `Task<Result<List<StudentCourseExamStatusResponse>>> GetStudentAvailableExamsAsync(int userId, CancellationToken cancellationToken);`
- `Task<Result<StudentCourseExamStatusResponse>> GetStudentExamStatusAsync(int examId, int userId, CancellationToken cancellationToken);`
- `Task<Result<ExamPaymentResponse>> SubmitPaymentAsync(int examId, int userId, SubmitExamPaymentRequest request, string screenshotUrl, CancellationToken cancellationToken);`
- `Task<Result<PagedResult<ExamPaymentResponse>>> GetPagedPaymentsAsync(PaymentListQuery query, CancellationToken cancellationToken);`
- `Task<Result<ExamPaymentResponse>> ReviewPaymentAsync(int paymentId, ReviewExamPaymentRequest request, int adminId, CancellationToken cancellationToken);`

#### Service Registration in [DependencyInjection.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Application/DependencyInjection.cs)
```csharp
services.AddScoped<IExamPaymentService, ExamPaymentService>();
```

---

### 3.4 API Project (`ELearningManagementSystem.Api`)

#### [NEW] [ExamPaymentsController.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.Api/Controllers/ExamPaymentsController.cs)
- `GET /api/course-exams/my-available` — List exams for completed courses + current payment status for student.
- `GET /api/course-exams/{examId}/my-payment` — Get student's payment status for an exam.
- `POST /api/course-exams/{examId}/payments` — Submit payment with screenshot file (`[FromForm]`, saves image to `wwwroot/uploads/exam-payments/{GUID}{ext}`).
- `GET /api/admin/exam-payments` (`Permission:Course.Update`) — Admin paged list of payments with status filters.
- `POST /api/admin/exam-payments/{paymentId}/review` (`Permission:Course.Update`) — Admin Approve or Reject payment with reason.

---

### 3.5 Blazor WASM App Project (`ELearningManagementSystem.App`)

#### [NEW] [ExamPaymentApiClient.cs](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.App/Services/ExamPaymentApiClient.cs)
- Injected into Blazor WASM via `Program.cs`. Handles GET, POST, and multipart form uploads for screenshots.

#### [NEW] [CourseExams.razor](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.App/Pages/Student/Exams/CourseExams.razor) (`/course-exams`)
- Student page displaying available course exams for completed courses.
- Exam cards showing: Course Title, Exam Title, Fee (`15,000 MMK`), Question Count, Duration, and dynamic status badge (*Not Paid*, *Pending Verification*, *Approved*, *Rejected*).
- Action buttons: "Pay Exam Fee", "View Status", "Start Exam".

#### [NEW] [PaymentModal.razor](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.App/Components/Exams/PaymentModal.razor)
- Interactive student modal:
  1. Select Payment Method: KPay, AyaPay, CB Pay, Wave Pay (displays Admin account numbers & QR guidance).
  2. Enter Transaction ID input.
  3. Upload Screenshot file picker (with live image thumbnail preview).
  4. Submit Payment button.

#### [NEW] [PendingPayments.razor](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.App/Pages/Admin/Payments/PendingPayments.razor) (`/admin/payments`)
- Admin verification dashboard:
  - Status tabs: `Pending` (default), `Approved`, `Rejected`, `All`.
  - Search bar by Student Name, Email, Exam Title, or Transaction ID.
  - Verification Table: Student, Exam, Fee, Provider, Transaction ID, Date, Status, Actions.
  - **Verification Drawer / Modal**: Shows full-resolution payment screenshot, student profile info, transaction ID, and action buttons: **Approve Payment** or **Reject Payment (with rejection note input)**.

#### [MODIFY] [NavMenu.razor](file:///d:/VS%20Folder/ELearningManagementSystem%286%29v-3/ELearningManagementSystem%286%29v-2/ELearningManagementSystem/src/ELearningManagementSystem.App/Components/Layout/NavMenu.razor)
- Add **`Course Exams`** link under Student general section.
- Add **`Pending Payments`** link under Admin Administration section.

---

## 4. Verification Plan

### Automated Build Verification:
- Execute `dotnet build` on solution root to verify clean compilation across all 7 projects.

### Manual & Functional Verification:
1. **Student Course Completion & Gate Check**:
   - Verify non-enrolled or incomplete course students cannot submit payments.
2. **Student Payment Submission**:
   - Student selects KPay, enters Transaction ID e.g. `KP-987654321`, uploads screenshot image.
   - Status changes to `Pending Verification`.
3. **Admin Payment Review Drawer**:
   - Admin opens `/admin/payments`, sees pending payment, opens drawer previewing screenshot.
   - Admin clicks **Approve** $\to$ Payment status becomes `Approved`.
   - Student page immediately updates status to `Approved` with **Start Exam** unlocked!
4. **Rejection & Re-submission**:
   - Admin clicks **Reject** with note "Invalid Transaction ID".
   - Student sees rejection banner with reason and can submit a new payment.
