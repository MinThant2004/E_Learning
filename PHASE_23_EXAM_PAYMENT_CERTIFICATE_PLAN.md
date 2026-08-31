# Phase 23 — Course Exam, Question Pool, Payment & Certificate System: Detailed Implementation Plan

## 1. Objective

Integrate a complete, production-grade **Course Exam, Payment Verification, Random Question Pool, and Certificate System** into the E-Learning Management System without modifying or breaking any existing Course Quiz, Enrollment, or Authentication flows.

---

## 2. System Architecture & Flow

```
Student Flow:
Register → Login → Enroll → Complete Lessons → Complete Course Quiz (Free) → Course Completed ✅
  → Click "Course Exams" (Navbar)
  → Select Completed Course Exam
  → Fill Payment Form (KPay / AyaPay / CB Pay / Wave Pay) + Upload Screenshot
  → Status: Pending Admin Verification
  → Admin Approves Payment in "Pending Payments"
  → Student Unlocks Exam
  → Take Exam (Randomized N questions from Question Pool, Shuffled Options, Timer)
  → Server-Side Scoring
  → Pass → Auto-generate Certificate PDF + Verification Link 🏆

Admin Flow:
Sidebar → "Exam Management" (Expandable)
  ├── 📝 Course Exams       → CRUD Exams per Course (Fee, QuestionCount, PassingScore, Duration)
  ├── 📚 Question Pool       → CRUD Questions & Options per Exam (Difficulty, Correct Answer)
  ├── 💰 Pending Payments    → Review payment screenshots & TxnIDs → Approve / Reject with Reason
  ├── 📊 Exam Results        → Overview of student scores, attempts, and pass rates
  └── 📜 Certificates        → Manage & Revoke issued certificates
```

---

## 3. Detailed Data Model (Database Layer)

Add 8 new entities to `ELearningManagementSystem.Domain/Entities` and register them in `AppDbContext.cs`:

### 3.1 Entities

1. **`CourseExam.cs`**
   - `ExamId` (PK, int, Identity)
   - `CourseId` (FK → `Courses`, int, Unique index - 1 exam per course)
   - `Title` (nvarchar(200), Required)
   - `Description` (nvarchar(max), Nullable)
   - `ExamFee` (decimal(18,2), Required)
   - `QuestionCount` (int, Required) — How many questions to pull from pool (e.g. 50 out of 200)
   - `DurationMinutes` (int, Required) — Time limit in minutes
   - `PassingScore` (int, Required) — Passing percentage (e.g. 70%)
   - `MaxAttempts` (int, Required, Default = 3)
   - `Status` (bool, Default = true)
   - `CreatedBy` (FK → `Users`, int)
   - `CreatedAt` (DateTime, Required)
   - `UpdatedAt` (DateTime, Nullable)
   - `DeleteFlag` (bool, Default = false)

2. **`ExamQuestion.cs`** (Question Pool)
   - `ExamQuestionId` (PK, int, Identity)
   - `ExamId` (FK → `CourseExams`, int)
   - `QuestionText` (nvarchar(max), Required)
   - `DisplayOrder` (int, Required)
   - `DifficultyLevel` (nvarchar(50), Default = 'Medium') — Easy / Medium / Hard
   - `CreatedAt` (DateTime, Required)
   - `UpdatedAt` (DateTime, Nullable)
   - `DeleteFlag` (bool, Default = false)

3. **`ExamQuestionOption.cs`**
   - `OptionId` (PK, int, Identity)
   - `ExamQuestionId` (FK → `ExamQuestions`, int)
   - `OptionText` (nvarchar(max), Required)
   - `IsCorrect` (bool, Required)
   - `DeleteFlag` (bool, Default = false)

4. **`ExamPayment.cs`**
   - `ExamPaymentId` (PK, int, Identity)
   - `ExamId` (FK → `CourseExams`, int)
   - `UserId` (FK → `Users`, int)
   - `Amount` (decimal(18,2), Required)
   - `PaymentMethod` (nvarchar(50), Required) — KPay, AyaPay, CBPay, WavePay
   - `TransactionId` (nvarchar(100), Required)
   - `ScreenshotUrl` (nvarchar(500), Required)
   - `Status` (nvarchar(50), Required, Default = 'Pending') — Pending, Approved, Rejected
   - `RejectionReason` (nvarchar(500), Nullable)
   - `ReviewedBy` (FK → `Users`, int, Nullable)
   - `ReviewedAt` (DateTime, Nullable)
   - `CreatedAt` (DateTime, Required)
   - `DeleteFlag` (bool, Default = false)

5. **`ExamAttempt.cs`**
   - `ExamAttemptId` (PK, int, Identity)
   - `ExamId` (FK → `CourseExams`, int)
   - `UserId` (FK → `Users`, int)
   - `ExamPaymentId` (FK → `ExamPayments`, int)
   - `Score` (decimal(5,2), Required)
   - `CorrectAnswers` (int, Required)
   - `TotalQuestions` (int, Required)
   - `Passed` (bool, Required)
   - `StartedAt` (DateTime, Required)
   - `SubmittedAt` (DateTime, Required)
   - `DurationSeconds` (int, Required)
   - `DeleteFlag` (bool, Default = false)

6. **`ExamAttemptQuestion.cs`** (Locks in the assigned random questions)
   - `ExamAttemptQuestionId` (PK, int, Identity)
   - `ExamAttemptId` (FK → `ExamAttempts`, int)
   - `ExamQuestionId` (FK → `ExamQuestions`, int)
   - `DisplayOrder` (int, Required)

7. **`ExamAttemptAnswer.cs`** (Student selected answers)
   - `ExamAttemptAnswerId` (PK, int, Identity)
   - `ExamAttemptId` (FK → `ExamAttempts`, int)
   - `ExamQuestionId` (FK → `ExamQuestions`, int)
   - `SelectedOptionId` (FK → `ExamQuestionOptions`, int)

8. **`Certificate.cs`**
   - `CertificateId` (PK, int, Identity)
   - `ExamAttemptId` (FK → `ExamAttempts`, int, Unique index)
   - `UserId` (FK → `Users`, int)
   - `ExamId` (FK → `CourseExams`, int)
   - `CertificateNumber` (nvarchar(100), Required, Unique index) e.g., "CERT-2026-00001"
   - `IssuedAt` (DateTime, Required)
   - `ExpiresAt` (DateTime, Nullable)
   - `RevokedAt` (DateTime, Nullable)
   - `RevokedReason` (nvarchar(500), Nullable)
   - `PdfUrl` (nvarchar(500), Nullable)
   - `DeleteFlag` (bool, Default = false)

---

## 4. Application Layer Architecture (`ELearningManagementSystem.Application`)

Organized into feature subfolders under `Features/`:

```
Features/
├── CourseExams/
│   ├── DTOs/
│   │   ├── CourseExamDTOs.cs (CourseExamResponse, CreateCourseExamRequest, UpdateCourseExamRequest, ExamListQuery)
│   │   └── ExamQuestionDTOs.cs (ExamQuestionResponse, CreateExamQuestionRequest, UpdateExamQuestionRequest)
│   ├── Services/
│   │   ├── ICourseExamService.cs
│   │   └── CourseExamService.cs
│   └── Validators/
│       ├── CreateCourseExamRequestValidator.cs
│       └── CreateExamQuestionRequestValidator.cs
│
├── ExamPayments/
│   ├── DTOs/
│   │   └── ExamPaymentDTOs.cs (SubmitPaymentRequest, ReviewPaymentRequest, ExamPaymentResponse, PaymentListQuery)
│   ├── Services/
│   │   ├── IExamPaymentService.cs
│   │   └── ExamPaymentService.cs
│   └── Validators/
│       └── SubmitPaymentRequestValidator.cs
│
├── ExamAttempts/
│   ├── DTOs/
│   │   └── ExamAttemptDTOs.cs (StartExamResponse, SubmitExamAttemptRequest, ExamAttemptResultResponse, ExamAttemptHistoryResponse)
│   ├── Services/
│   │   ├── IExamAttemptService.cs
│   │   └── ExamAttemptService.cs
│   └── Validators/
│       └── SubmitExamAttemptRequestValidator.cs
│
└── Certificates/
    ├── DTOs/
    │   └── CertificateDTOs.cs (CertificateResponse, VerifyCertificateResponse)
    ├── Services/
    │   ├── ICertificateService.cs
    │   └── CertificateService.cs
    └── Validators/
```

### Key Business Rules & Result Codes:
- `CourseNotCompleted`: Must complete all lessons and course quiz before paying/taking exam.
- `PaymentRequired`: Exam locked until payment is approved.
- `PaymentPending`: Payment submitted, waiting for admin approval.
- `PaymentRejected`: Payment rejected by admin.
- `MaxAttemptsExceeded`: Maximum retakes reached.
- `InsufficientPoolQuestions`: Question pool size is smaller than `Exam.QuestionCount`.
- `RandomQuestionSelection`: Server shuffles questions using `Guid.NewGuid()` / `Random` and shuffles options within each question. Option `IsCorrect` flag is **omitted** from student responses.
- `ServerSideScoring`: Score calculated strictly server-side. Auto-creates `Certificate` upon passing.

---

## 5. Security & Permission Codes

Add 10 new permission codes to `Permissions` table and `PermissionPolicyProvider`:

```csharp
"CourseExam.Read"       // View exam details
"CourseExam.Create"     // Admin: Create course exam
"CourseExam.Update"     // Admin: Update course exam
"CourseExam.Delete"     // Admin: Soft delete/archive exam

"ExamQuestion.Create"   // Admin: Add questions to pool
"ExamQuestion.Update"   // Admin: Edit question pool

"ExamPayment.Read"      // Admin: View pending/all payments
"ExamPayment.Approve"   // Admin: Approve or reject payments

"ExamResult.Read"       // Admin: View all student exam results

"Certificate.Read"      // View/Download certificates
"Certificate.Revoke"    // Admin: Revoke certificate
```

---

## 6. API Layer Endpoints (`ELearningManagementSystem.Api`)

### Controllers:

1. **`CourseExamsController.cs`** (`/api/course-exams`)
   - `GET /api/course-exams` — List available exams for completed courses (Student) or all exams (Admin)
   - `GET /api/course-exams/{id}` — Get exam details + payment status for current user
   - `POST /api/course-exams` — Create exam (`CourseExam.Create`)
   - `PUT /api/course-exams/{id}` — Update exam (`CourseExam.Update`)
   - `POST /api/course-exams/{id}/archive` — Archive exam (`CourseExam.Delete`)

2. **`ExamQuestionsController.cs`** (`/api/course-exams/{examId}/questions`)
   - `GET /api/course-exams/{examId}/questions` — Get question pool (`ExamQuestion.Create`)
   - `POST /api/course-exams/{examId}/questions` — Add question + options to pool (`ExamQuestion.Create`)
   - `PUT /api/exam-questions/{questionId}` — Edit question + options (`ExamQuestion.Update`)
   - `DELETE /api/exam-questions/{questionId}` — Archive question (`ExamQuestion.Update`)

3. **`ExamPaymentsController.cs`** (`/api/exam-payments`)
   - `POST /api/course-exams/{examId}/payments` — Submit payment with screenshot upload (`[FromForm]`)
   - `GET /api/course-exams/{examId}/payment-status` — Get current student payment status
   - `GET /api/admin/exam-payments` — List payments with status filter (`ExamPayment.Read`)
   - `POST /api/admin/exam-payments/{paymentId}/approve` — Approve payment (`ExamPayment.Approve`)
   - `POST /api/admin/exam-payments/{paymentId}/reject` — Reject payment with reason (`ExamPayment.Approve`)

4. **`ExamAttemptsController.cs`** (`/api/exam-attempts`)
   - `POST /api/course-exams/{examId}/start` — Start exam & generate random question set
   - `POST /api/course-exams/{examId}/submit` — Submit exam attempt & calculate score
   - `GET /api/course-exams/{examId}/attempts` — Student attempt history
   - `GET /api/exam-attempts/{attemptId}` — Attempt result detail

5. **`CertificatesController.cs`** (`/api/certificates`)
   - `GET /api/certificates/my` — Get authenticated student's certificates
   - `GET /api/certificates/{id}` — Get certificate detail
   - `GET /api/certificates/verify/{certificateNumber}` — **Public** verification endpoint (AllowAnonymous)
   - `POST /api/admin/certificates/{id}/revoke` — Admin revoke certificate (`Certificate.Revoke`)

---

## 7. Frontend Layer (`ELearningManagementSystem.App`)

### 7.1 API Clients (`Services/`)
- `CourseExamApiClient.cs`
- `ExamPaymentApiClient.cs`
- `ExamAttemptApiClient.cs`
- `CertificateApiClient.cs`

Register all 4 clients as `Scoped` in `Program.cs` attached to named HttpClient `"ELearningApi"`.

### 7.2 UI Components & Pages

#### Student Pages:
- **`Pages/Student/Exams/CourseExams.razor`** (`/course-exams`) — Grid of available exams for completed courses, showing status badges (*Not Paid*, *Pending Approval*, *Unlocked*, *Passed*).
- **`Pages/Student/Exams/ExamDetail.razor`** (`/course-exams/{ExamId:int}`) — Exam info, instructions, fee breakdown, and dynamic action button (*Pay Now*, *Pending Verification*, *Start Exam*).
- **`Pages/Student/Exams/PaymentModal.razor`** — Modal dialog for choosing payment provider (KPay, AyaPay, CB Pay, Wave Pay), viewing admin account details, entering Transaction ID, and uploading screenshot file.
- **`Pages/Student/Exams/TakeExam.razor`** (`/course-exams/{ExamId:int}/take`) — Exam page with live countdown timer, pagination/grid for questions, auto-submit on time expiry, and clean submit modal.
- **`Pages/Student/Exams/ExamResult.razor`** (`/course-exams/{ExamId:int}/result/{AttemptId:int}`) — Pass/Fail score breakdown, summary statistics, retake prompt, and "View Certificate" button.
- **`Pages/Student/Certificates/MyCertificates.razor`** (`/my-certificates`) — List of earned certificates with badge preview, download button, and public verification link.
- **`Pages/Certificates/VerifyCertificate.razor`** (`/verify-certificate/{CertificateNumber}`) — Publicly accessible verification page.

#### Admin Pages (`Pages/Admin/`):
- **`Pages/Admin/Exams/AdminExams.razor`** (`/admin/exams`) — Course exam list & modal to create/edit exam parameters.
- **`Pages/Admin/Exams/QuestionPool.razor`** (`/admin/exams/{ExamId:int}/questions`) — Question pool manager with search, filter by difficulty, question modal with 4 option inputs & correct answer toggle.
- **`Pages/Admin/Payments/PendingPayments.razor`** (`/admin/payments`) — Table of payments with image preview drawer, transaction ID check, and Approve / Reject modal with reason.
- **`Pages/Admin/Certificates/AdminCertificates.razor`** (`/admin/certificates`) — Searchable list of issued certificates with Revoke action modal.

#### Navigation Updates (`Components/Layout/NavMenu.razor`):
- Add **`Course Exams`** (icon: clipboard-document-check) to Student section.
- Add **`My Certificates`** (icon: academic-cap) to Student section.
- Add expandable **`Exam Management`** parent menu to Admin section containing:
  - `Course Exams` (`/admin/exams`)
  - `Pending Payments` (`/admin/payments`)
  - `Certificates` (`/admin/certificates`)

---

## 8. Implementation Steps & Milestones

1. **Step 1: Database Entities & Scaffolding**
   - Add new domain entities and update `AppDbContext.cs`.
   - Add database script/migration for SQL Server.

2. **Step 2: Backend Core Services & API Controllers**
   - Implement `CourseExamService`, `ExamPaymentService`, `ExamAttemptService`, `CertificateService`.
   - Implement Validators and DTOs.
   - Register Services in `Application/DependencyInjection.cs`.
   - Create Controllers (`CourseExamsController`, `ExamQuestionsController`, `ExamPaymentsController`, `ExamAttemptsController`, `CertificatesController`).
   - Add permission policies to `PermissionPolicyProvider`.

3. **Step 3: Blazor WASM API Clients & Navigation**
   - Create `CourseExamApiClient`, `ExamPaymentApiClient`, `ExamAttemptApiClient`, `CertificateApiClient`.
   - Register clients in `App/Program.cs`.
   - Update `NavMenu.razor` with new links.

4. **Step 4: Student Exam & Payment UI**
   - Build `CourseExams.razor`, `ExamDetail.razor`, `PaymentModal.razor`.
   - Build `TakeExam.razor` with countdown timer and `ExamResult.razor`.
   - Build `MyCertificates.razor` and `VerifyCertificate.razor`.

5. **Step 5: Admin Exam & Payment Management UI**
   - Build `AdminExams.razor` and `QuestionPool.razor`.
   - Build `PendingPayments.razor` with image preview & approval workflow.
   - Build `AdminCertificates.razor`.

6. **Step 6: End-to-End Verification & Fixes**
   - Build solution and verify all compilation & runtime flows.

---

## 9. Verification & Quality Plan

- **Compilation Check**: Run `dotnet build` across all 7 projects.
- **Payment Verification Check**: Verify student screenshot upload, pending state, admin approval, and instant exam unlock.
- **Random Pool Check**: Confirm two consecutive exam starts generate different subsets/order of questions.
- **Timer Check**: Confirm countdown timer auto-submits exam when time expires.
- **Certificate Verification Check**: Verify public verification URL renders correct certificate metadata without requiring authentication.
