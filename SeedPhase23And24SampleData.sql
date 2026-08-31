-- ============================================================================
-- Seed Sample Data for Phase 23 (Course Exams & Pool) & Phase 24 (Exam Payments)
-- Database: ELearningManagementSystem
-- ============================================================================

USE [ELearningManagementSystem];
GO

SET NOCOUNT ON;

PRINT 'Ensuring Tables Exist...';

-- 1. Create CourseExams Table if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseExams')
BEGIN
    CREATE TABLE [dbo].[CourseExams] (
        [ExamId] INT IDENTITY(1,1) NOT NULL,
        [CourseId] INT NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [ExamFee] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [QuestionCount] INT NOT NULL DEFAULT 50,
        [DurationMinutes] INT NOT NULL DEFAULT 60,
        [PassingScore] INT NOT NULL DEFAULT 70,
        [MaxAttempts] INT NOT NULL DEFAULT 3,
        [Status] BIT NOT NULL DEFAULT 1,
        [CreatedBy] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [DeleteFlag] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_CourseExams] PRIMARY KEY CLUSTERED ([ExamId] ASC),
        CONSTRAINT [FK_CourseExams_Courses] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]),
        CONSTRAINT [FK_CourseExams_Users] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users] ([UserId])
    );
END
GO

-- 2. Create ExamQuestions Table if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExamQuestions')
BEGIN
    CREATE TABLE [dbo].[ExamQuestions] (
        [ExamQuestionId] INT IDENTITY(1,1) NOT NULL,
        [ExamId] INT NOT NULL,
        [QuestionText] NVARCHAR(MAX) NOT NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 1,
        [DifficultyLevel] NVARCHAR(50) NOT NULL DEFAULT 'Medium',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [DeleteFlag] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ExamQuestions] PRIMARY KEY CLUSTERED ([ExamQuestionId] ASC),
        CONSTRAINT [FK_ExamQuestions_CourseExams] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[CourseExams] ([ExamId])
    );
END
GO

-- 3. Create ExamQuestionOptions Table if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExamQuestionOptions')
BEGIN
    CREATE TABLE [dbo].[ExamQuestionOptions] (
        [OptionId] INT IDENTITY(1,1) NOT NULL,
        [ExamQuestionId] INT NOT NULL,
        [OptionText] NVARCHAR(MAX) NOT NULL,
        [IsCorrect] BIT NOT NULL DEFAULT 0,
        [DeleteFlag] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ExamQuestionOptions] PRIMARY KEY CLUSTERED ([OptionId] ASC),
        CONSTRAINT [FK_ExamQuestionOptions_ExamQuestions] FOREIGN KEY ([ExamQuestionId]) REFERENCES [dbo].[ExamQuestions] ([ExamQuestionId])
    );
END
GO

-- 4. Create ExamPayments Table if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExamPayments')
BEGIN
    CREATE TABLE [dbo].[ExamPayments] (
        [ExamPaymentId] INT IDENTITY(1,1) NOT NULL,
        [ExamId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [PaymentMethod] NVARCHAR(50) NOT NULL,
        [TransactionId] NVARCHAR(100) NOT NULL,
        [ScreenshotUrl] NVARCHAR(500) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [IsUsed] BIT NOT NULL DEFAULT 0,
        [RejectionReason] NVARCHAR(500) NULL,
        [ReviewedBy] INT NULL,
        [ReviewedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [DeleteFlag] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ExamPayments] PRIMARY KEY CLUSTERED ([ExamPaymentId] ASC),
        CONSTRAINT [FK_ExamPayments_CourseExams] FOREIGN KEY ([ExamId]) REFERENCES [dbo].[CourseExams] ([ExamId]),
        CONSTRAINT [FK_ExamPayments_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]),
        CONSTRAINT [FK_ExamPayments_ReviewedBy] FOREIGN KEY ([ReviewedBy]) REFERENCES [dbo].[Users] ([UserId])
    );
END
GO

PRINT 'Starting Sample Data Insertion...';

-- Get existing Category and Admin User
DECLARE @CategoryId INT;
SELECT TOP 1 @CategoryId = [CategoryId] FROM [dbo].[Categories] ORDER BY [CategoryId] ASC;
IF @CategoryId IS NULL SET @CategoryId = 1;

DECLARE @AdminUserId INT;
SELECT TOP 1 @AdminUserId = [UserId] FROM [dbo].[Users] ORDER BY [UserId] ASC;
IF @AdminUserId IS NULL SET @AdminUserId = 1;

-- 5. Ensure Course exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[Courses] WHERE [Title] LIKE '%C#%')
BEGIN
    INSERT INTO [dbo].[Courses] ([Title], [Description], [Status], [CategoryId], [CreatedBy], [CreatedAt], [DeleteFlag])
    VALUES ('Complete C# & .NET Masterclass', 'Master C# programming, OOP, EF Core, and Blazor WASM from scratch.', 1, @CategoryId, @AdminUserId, GETUTCDATE(), 0);
END

DECLARE @CourseId INT;
SELECT TOP 1 @CourseId = [CourseId] FROM [dbo].[Courses] WHERE [Title] LIKE '%C#%' ORDER BY [CourseId] ASC;

-- 6. Create Course Exam
IF NOT EXISTS (SELECT 1 FROM [dbo].[CourseExams] WHERE [CourseId] = @CourseId)
BEGIN
    INSERT INTO [dbo].[CourseExams] 
        ([CourseId], [Title], [Description], [ExamFee], [QuestionCount], [DurationMinutes], [PassingScore], [MaxAttempts], [Status], [CreatedBy], [CreatedAt], [DeleteFlag])
    VALUES 
        (@CourseId, 'C# Professional Certification Exam', 'Official certification exam testing core C# syntax, OOP principles, collections, and async programming.', 15000.00, 5, 30, 70, 1, 1, @AdminUserId, GETUTCDATE(), 0);
END

DECLARE @ExamId INT;
SELECT TOP 1 @ExamId = [ExamId] FROM [dbo].[CourseExams] WHERE [CourseId] = @CourseId ORDER BY [ExamId] ASC;

-- 7. Seed Question Pool (6 Multiple Choice Questions)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId)
BEGIN
    -- Question 1
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, 'What does CLR stand for in the .NET framework?', 1, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q1 INT = SCOPE_IDENTITY();

    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q1, 'Common Language Runtime', 1, 0),
    (@Q1, 'Central Logic Reader', 0, 0),
    (@Q1, 'Code Layer Routine', 0, 0),
    (@Q1, 'Common Library Resource', 0, 0);

    -- Question 2
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, 'Which keyword is used to create an instance of a class in C#?', 2, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q2 INT = SCOPE_IDENTITY();

    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q2, 'new', 1, 0),
    (@Q2, 'create', 0, 0),
    (@Q2, 'instance', 0, 0),
    (@Q2, 'alloc', 0, 0);

    -- Question 3
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, 'What is the default value of a boolean variable in C#?', 3, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q3 INT = SCOPE_IDENTITY();

    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q3, 'false', 1, 0),
    (@Q3, 'true', 0, 0),
    (@Q3, 'null', 0, 0),
    (@Q3, '0', 0, 0);

    -- Question 4
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, 'Which generic collection type enforces unique elements in C#?', 4, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q4 INT = SCOPE_IDENTITY();

    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q4, 'HashSet<T>', 1, 0),
    (@Q4, 'List<T>', 0, 0),
    (@Q4, 'ArrayList', 0, 0),
    (@Q4, 'LinkedList<T>', 0, 0);

    -- Question 5
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, 'What is the return type of a method that does not return any value in C#?', 5, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q5 INT = SCOPE_IDENTITY();

    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q5, 'void', 1, 0),
    (@Q5, 'null', 0, 0),
    (@Q5, 'empty', 0, 0),
    (@Q5, 'int', 0, 0);

    -- Question 6
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, 'Which operator is known as the Null-Coalescing Operator in C#?', 6, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q6 INT = SCOPE_IDENTITY();

    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q6, '??', 1, 0),
    (@Q6, '?:', 0, 0),
    (@Q6, '?.', 0, 0),
    (@Q6, '!!', 0, 0);
END

-- 8. Ensure Student has Completed Enrollment for this course
DECLARE @StudentUserId INT;
SELECT TOP 1 @StudentUserId = [UserId] FROM [dbo].[Users] ORDER BY [UserId] ASC;

IF @StudentUserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Enrollments] WHERE [UserId] = @StudentUserId AND [CourseId] = @CourseId)
BEGIN
    INSERT INTO [dbo].[Enrollments] ([UserId], [CourseId], [EnrollDate], [Completed])
    VALUES (@StudentUserId, @CourseId, GETUTCDATE(), 1);
END
ELSE IF @StudentUserId IS NOT NULL
BEGIN
    UPDATE [dbo].[Enrollments] SET [Completed] = 1 WHERE [UserId] = @StudentUserId AND [CourseId] = @CourseId;
END

-- 9. Insert Sample Exam Payment
IF @StudentUserId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[ExamPayments] WHERE [ExamId] = @ExamId AND [UserId] = @StudentUserId)
BEGIN
    INSERT INTO [dbo].[ExamPayments]
        ([ExamId], [UserId], [Amount], [PaymentMethod], [TransactionId], [ScreenshotUrl], [Status], [IsUsed], [RejectionReason], [CreatedAt], [DeleteFlag])
    VALUES
        (@ExamId, @StudentUserId, 15000.00, 'KPay', '2026082900123456', '/uploads/exam-payments/sample_receipt.png', 'Pending', 0, NULL, GETUTCDATE(), 0);
END

PRINT 'Sample Data Seeding Completed Successfully! ✅';
GO
