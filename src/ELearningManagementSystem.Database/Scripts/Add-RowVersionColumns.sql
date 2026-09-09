-- Adds a RowVersion column to the core curriculum tables.
-- Used by EF Core (byte[] RowVersion + IsRowVersion()) for optimistic concurrency:
-- UPDATE/DELETE statements are guarded by (PK = @p AND RowVersion = @original) and
-- throw DbUpdateConcurrencyException when another change has occurred since the row was read.
-- SQL Server fills the column automatically for existing rows and each subsequent change.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Categories' AND COLUMN_NAME = 'RowVersion')
BEGIN
    ALTER TABLE dbo.[Categories] ADD RowVersion rowversion NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Courses' AND COLUMN_NAME = 'RowVersion')
BEGIN
    ALTER TABLE dbo.[Courses] ADD RowVersion rowversion NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Lessons' AND COLUMN_NAME = 'RowVersion')
BEGIN
    ALTER TABLE dbo.[Lessons] ADD RowVersion rowversion NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Quizzes' AND COLUMN_NAME = 'RowVersion')
BEGIN
    ALTER TABLE dbo.[Quizzes] ADD RowVersion rowversion NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Questions' AND COLUMN_NAME = 'RowVersion')
BEGIN
    ALTER TABLE dbo.[Questions] ADD RowVersion rowversion NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'QuestionOptions' AND COLUMN_NAME = 'RowVersion')
BEGIN
    ALTER TABLE dbo.[QuestionOptions] ADD RowVersion rowversion NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CourseExams' AND COLUMN_NAME = 'RowVersion')
BEGIN
    ALTER TABLE dbo.[CourseExams] ADD RowVersion rowversion NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExamQuestions' AND COLUMN_NAME = 'RowVersion')
BEGIN
    ALTER TABLE dbo.[ExamQuestions] ADD RowVersion rowversion NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ExamQuestionOptions' AND COLUMN_NAME = 'RowVersion')
BEGIN
    ALTER TABLE dbo.[ExamQuestionOptions] ADD RowVersion rowversion NOT NULL;
END
GO