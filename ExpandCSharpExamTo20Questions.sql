-- ============================================================================
-- Expand the existing "C# Examination" (CourseExams.ExamId = 1) from 5 to 20
-- real C# / .NET questions, and set CourseExams.QuestionCount = 20 so the
-- exam draws from the full 20-question pool.
-- Database: ELearningManagementSystem
-- Idempotent: safe to run multiple times.
-- ============================================================================

USE [ELearningManagementSystem];
GO

SET NOCOUNT ON;

PRINT 'Expanding C# Examination question pool to 20...';

DECLARE @ExamId INT;
SELECT TOP 1 @ExamId = [ExamId]
FROM [dbo].[CourseExams]
WHERE [Title] = N'C# Examination' AND [DeleteFlag] = 0
ORDER BY [ExamId] ASC;

IF @ExamId IS NULL
BEGIN
    PRINT 'C# Examination not found; defaulting to ExamId 1.';
    SET @ExamId = 1;
END

-- Question 6 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 6 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the default value of an int variable in C#?', 6, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q6 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q6, N'0', 1, 0),
    (@Q6, N'null', 0, 0),
    (@Q6, N'-1', 0, 0),
    (@Q6, N'undefined', 0, 0);
END

-- Question 7 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 7 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which keyword is used to declare a compile-time constant whose value can never change?', 7, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q7 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q7, N'const', 1, 0),
    (@Q7, N'static', 0, 0),
    (@Q7, N'final', 0, 0),
    (@Q7, N'readonly', 0, 0);
END

-- Question 8 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 8 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which loop in C# is guaranteed to execute its body at least once?', 8, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q8 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q8, N'do-while', 1, 0),
    (@Q8, N'while', 0, 0),
    (@Q8, N'for', 0, 0),
    (@Q8, N'foreach', 0, 0);
END

-- Question 9 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 9 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What character terminates every statement in C#?', 9, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q9 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q9, N'Semicolon (;)', 1, 0),
    (@Q9, N'Colon (:)', 0, 0),
    (@Q9, N'Period (.)', 0, 0),
    (@Q9, N'Comma (,)', 0, 0);
END

-- Question 10 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 10 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the correct way to declare an array that holds five integers in C#?', 10, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q10 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q10, N'int[] numbers = new int[5];', 1, 0),
    (@Q10, N'int numbers = new int[5];', 0, 0),
    (@Q10, N'new int[5] numbers;', 0, 0),
    (@Q10, N'array<int> numbers = new array<int>(5);', 0, 0);
END

-- Question 11 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 11 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which of the following C# types is a value type?', 11, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q11 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q11, N'decimal', 1, 0),
    (@Q11, N'string', 0, 0),
    (@Q11, N'Array', 0, 0),
    (@Q11, N'Exception', 0, 0);
END

-- Question 12 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 12 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the output of Console.WriteLine("3" + 4);?', 12, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q12 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q12, N'34', 1, 0),
    (@Q12, N'7', 0, 0),
    (@Q12, N'"3 4"', 0, 0),
    (@Q12, N'Compile error', 0, 0);
END

-- Question 13 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 13 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which keyword allows a method to accept a variable number of arguments?', 13, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q13 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q13, N'params', 1, 0),
    (@Q13, N'variable', 0, 0),
    (@Q13, N'args', 0, 0),
    (@Q13, N'array', 0, 0);
END

-- Question 14 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 14 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the ??= operator do in C# 8 and later?', 14, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q14 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q14, N'Assigns the right-hand value to the left-hand variable only if the left-hand side is null', 1, 0),
    (@Q14, N'Always assigns the right-hand value', 0, 0),
    (@Q14, N'Compares two values for equality', 0, 0),
    (@Q14, N'Logically ORs a value with itself', 0, 0);
END

-- Question 15 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 15 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which keyword prevents a class from being inherited by other classes?', 15, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q15 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q15, N'sealed', 1, 0),
    (@Q15, N'static', 0, 0),
    (@Q15, N'abstract', 0, 0),
    (@Q15, N'final', 0, 0);
END

-- Question 16 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 16 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the name of the special member invoked automatically when a new instance of a class is created?', 16, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q16 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q16, N'Constructor', 1, 0),
    (@Q16, N'Destructor', 0, 0),
    (@Q16, N'Finalizer', 0, 0),
    (@Q16, N'Initializer', 0, 0);
END

-- Question 17 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 17 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which LINQ method is used to filter a sequence based on a condition?', 17, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q17 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q17, N'Where', 1, 0),
    (@Q17, N'Select', 0, 0),
    (@Q17, N'OrderBy', 0, 0),
    (@Q17, N'GroupBy', 0, 0);
END

-- Question 18 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 18 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the output of Console.WriteLine(null ?? "Default");?', 18, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q18 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q18, N'Default', 1, 0),
    (@Q18, N'null', 0, 0),
    (@Q18, N'An empty string', 0, 0),
    (@Q18, N'NullReferenceException', 0, 0);
END

-- Question 19 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 19 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the behavior of a method parameter declared with the out keyword?', 19, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q19 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q19, N'It is passed by reference and the method must assign it a value before returning', 1, 0),
    (@Q19, N'It is passed by value and cannot be modified', 0, 0),
    (@Q19, N'It must be initialized before the method is called', 0, 0),
    (@Q19, N'It makes the parameter optional', 0, 0);
END

-- Question 20 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 20 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Given class A { public virtual int F() => 1; }, class B : A { public override int F() => 2; } and class C : B { public new int F() => 3; }, what does new A[] { new C() }[0].F() output?', 20, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q20 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q20, N'2', 1, 0),
    (@Q20, N'1', 0, 0),
    (@Q20, N'3', 0, 0),
    (@Q20, N'Compile error', 0, 0);
END

-- Make the exam draw from the full 20-question pool
UPDATE [dbo].[CourseExams]
SET [QuestionCount] = 20,
    [UpdatedAt] = GETUTCDATE()
WHERE [ExamId] = @ExamId AND [QuestionCount] < 20 AND [DeleteFlag] = 0;

-- Verification
SELECT
    (SELECT COUNT(*) FROM [dbo].[ExamQuestions]  WHERE [ExamId] = @ExamId AND [DeleteFlag] = 0) AS ExamQuestions,
    (SELECT COUNT(*) FROM [dbo].[ExamQuestionOptions]
       WHERE [DeleteFlag] = 0
         AND [ExamQuestionId] IN (SELECT [ExamQuestionId] FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DeleteFlag] = 0)) AS ExamOptions,
    (SELECT [QuestionCount] FROM [dbo].[CourseExams] WHERE [ExamId] = @ExamId) AS QuestionCount;

PRINT 'C# Examination question pool expanded successfully.';
GO