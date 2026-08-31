-- =========================================================
-- Phase 23: Course Exams & Question Pool Tables
-- Run this script in SQL Server Management Studio on ELearningManagementSystem DB
-- =========================================================

USE [ELearningManagementSystem];
GO

-- 1. Create CourseExams Table
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

    CREATE NONCLUSTERED INDEX [IX_CourseExams_CourseId] ON [dbo].[CourseExams] ([CourseId]);
    PRINT 'Created CourseExams table.';
END
GO

-- 2. Create ExamQuestions Table (Question Pool)
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

    CREATE NONCLUSTERED INDEX [IX_ExamQuestions_ExamId] ON [dbo].[ExamQuestions] ([ExamId]);
    PRINT 'Created ExamQuestions table.';
END
GO

-- 3. Create ExamQuestionOptions Table
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

    CREATE NONCLUSTERED INDEX [IX_ExamQuestionOptions_ExamQuestionId] ON [dbo].[ExamQuestionOptions] ([ExamQuestionId]);
    PRINT 'Created ExamQuestionOptions table.';
END
GO
