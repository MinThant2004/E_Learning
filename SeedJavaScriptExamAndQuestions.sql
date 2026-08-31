-- ============================================================================
-- Seed a new "JavaScript Certification Examination" for the JavaScript course
-- (CourseId 3, "JavaScript Tutorial: From Zero to Hero") with a pool of 20
-- real JavaScript questions, and set QuestionCount = 20.
-- Database: ELearningManagementSystem
-- Idempotent: safe to run multiple times.
-- ============================================================================

USE [ELearningManagementSystem];
GO

SET NOCOUNT ON;

PRINT 'Seeding JavaScript Certification Examination...';

DECLARE @CourseId INT;
SELECT TOP 1 @CourseId = [CourseId]
FROM [dbo].[Courses]
WHERE [Title] LIKE N'%JavaScript%' AND [DeleteFlag] = 0
ORDER BY [CourseId] ASC;

IF @CourseId IS NULL
BEGIN
    RAISERROR (N'JavaScript course not found. Aborting.', 16, 1);
    RETURN;
END

DECLARE @AdminUserId INT;
SELECT TOP 1 @AdminUserId = [UserId]
FROM [dbo].[Users]
WHERE [DeleteFlag] = 0
ORDER BY [UserId] ASC;
IF @AdminUserId IS NULL SET @AdminUserId = 1;

-- 1. Create the exam for the JavaScript course
DECLARE @ExamId INT;
SELECT TOP 1 @ExamId = [ExamId]
FROM [dbo].[CourseExams]
WHERE [CourseId] = @CourseId AND [DeleteFlag] = 0
ORDER BY [ExamId] ASC;

IF @ExamId IS NULL
BEGIN
    INSERT INTO [dbo].[CourseExams]
        ([CourseId], [Title], [Description], [ExamFee], [QuestionCount], [DurationMinutes], [PassingScore], [MaxAttempts], [Status], [CreatedBy], [CreatedAt], [DeleteFlag])
    VALUES
        (@CourseId, N'JavaScript Certification Examination',
         N'Official certification exam testing core JavaScript syntax, variables, data types, arrays, functions, scope and asynchronous programming.',
         12000.00, 20, 60, 65, 3, 1, @AdminUserId, GETUTCDATE(), 0);

    SET @ExamId = SCOPE_IDENTITY();
    PRINT 'Created new exam: ' + CAST(@ExamId AS NVARCHAR(10));
END
ELSE
    PRINT 'Exam already exists: ' + CAST(@ExamId AS NVARCHAR(10));

-- 2. Question pool (20 real JavaScript questions)
-- Question 1 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 1 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which keyword is used to declare a variable that cannot be reassigned in JavaScript?', 1, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q1 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q1, N'const', 1, 0),
    (@Q1, N'let', 0, 0),
    (@Q1, N'var', 0, 0),
    (@Q1, N'static', 0, 0);
END

-- Question 2 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 2 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which method writes output to the browser console?', 2, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q2 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q2, N'console.log()', 1, 0),
    (@Q2, N'print()', 0, 0),
    (@Q2, N'document.write()', 0, 0),
    (@Q2, N'System.out.println()', 0, 0);
END

-- Question 3 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 3 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which keyword is used to declare a function in JavaScript?', 3, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q3 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q3, N'function', 1, 0),
    (@Q3, N'def', 0, 0),
    (@Q3, N'func', 0, 0),
    (@Q3, N'method', 0, 0);
END

-- Question 4 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 4 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the correct way to create an array containing the numbers 1, 2 and 3 in JavaScript?', 4, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q4 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q4, N'let arr = [1, 2, 3];', 1, 0),
    (@Q4, N'let arr = (1, 2, 3);', 0, 0),
    (@Q4, N'let arr = {1, 2, 3};', 0, 0),
    (@Q4, N'let arr = 1, 2, 3;', 0, 0);
END

-- Question 5 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 5 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which operator compares both the value and the type of two operands in JavaScript?', 5, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q5 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q5, N'===', 1, 0),
    (@Q5, N'==', 0, 0),
    (@Q5, N'=', 0, 0),
    (@Q5, N'=>', 0, 0);
END

-- Question 6 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 6 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the length property of an array return in JavaScript?', 6, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q6 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q6, N'The number of elements in the array', 1, 0),
    (@Q6, N'The last element in the array', 0, 0),
    (@Q6, N'The index of the last element', 0, 0),
    (@Q6, N'The memory size of the array', 0, 0);
END

-- Question 7 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 7 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which array method adds one or more elements to the end of an array in JavaScript?', 7, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q7 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q7, N'push()', 1, 0),
    (@Q7, N'pop()', 0, 0),
    (@Q7, N'unshift()', 0, 0),
    (@Q7, N'shift()', 0, 0);
END

-- Question 8 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 8 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which array method returns a NEW array containing the results of calling a function on every element?', 8, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q8 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q8, N'map()', 1, 0),
    (@Q8, N'forEach()', 0, 0),
    (@Q8, N'reduce()', 0, 0),
    (@Q8, N'filter()', 0, 0);
END

-- Question 9 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 9 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the output of console.log(typeof null);?', 9, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q9 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q9, N'"object"', 1, 0),
    (@Q9, N'"null"', 0, 0),
    (@Q9, N'"undefined"', 0, 0),
    (@Q9, N'"number"', 0, 0);
END

-- Question 10 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 10 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the result of the expression "5" + 3 in JavaScript?', 10, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q10 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q10, N'"53"', 1, 0),
    (@Q10, N'8', 0, 0),
    (@Q10, N'"8"', 0, 0),
    (@Q10, N'NaN', 0, 0);
END

-- Question 11 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 11 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which statement is used to test a block of code for errors in JavaScript?', 11, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q11 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q11, N'try', 1, 0),
    (@Q11, N'catch', 0, 0),
    (@Q11, N'throw', 0, 0),
    (@Q11, N'finally', 0, 0);
END

-- Question 12 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 12 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is hoisting in JavaScript?', 12, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q12 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q12, N'Declarations of var variables and functions are moved to the top of their scope', 1, 0),
    (@Q12, N'Elements are moved to the top of the DOM', 0, 0),
    (@Q12, N'CSS rules are applied before HTML finishes loading', 0, 0),
    (@Q12, N'Variables can only be used after they are assigned a value', 0, 0);
END

-- Question 13 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 13 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which keyword lets a class inherit behavior from another class in JavaScript?', 13, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q13 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q13, N'extends', 1, 0),
    (@Q13, N'inherits', 0, 0),
    (@Q13, N'super', 0, 0),
    (@Q13, N'prototype', 0, 0);
END

-- Question 14 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 14 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does JSON.stringify() do in JavaScript?', 14, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q14 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q14, N'Turns a JavaScript object into a JSON string', 1, 0),
    (@Q14, N'Turns a JSON string into a JavaScript object', 0, 0),
    (@Q14, N'Validates whether a string is valid JSON', 0, 0),
    (@Q14, N'Minifies a JSON document', 0, 0);
END

-- Question 15 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 15 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which loop in JavaScript always executes its code block at least once?', 15, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q15 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q15, N'do-while', 1, 0),
    (@Q15, N'while', 0, 0),
    (@Q15, N'for', 0, 0),
    (@Q15, N'for-of', 0, 0);
END

-- Question 16 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 16 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is a closure in JavaScript?', 16, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q16 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q16, N'A function that remembers and accesses variables from its outer scope even after that scope has closed', 1, 0),
    (@Q16, N'An error that occurs when a function finishes executing', 0, 0),
    (@Q16, N'A network request that cannot be cancelled', 0, 0),
    (@Q16, N'A function that does not contain a return statement', 0, 0);
END

-- Question 17 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 17 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the fetch() API return in JavaScript?', 17, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q17 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q17, N'A Promise', 1, 0),
    (@Q17, N'An array of the requested data', 0, 0),
    (@Q17, N'A string containing the response body', 0, 0),
    (@Q17, N'The response object immediately', 0, 0);
END

-- Question 18 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 18 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the output of console.log(typeof NaN);?', 18, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q18 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q18, N'"number"', 1, 0),
    (@Q18, N'"NaN"', 0, 0),
    (@Q18, N'"undefined"', 0, 0),
    (@Q18, N'"object"', 0, 0);
END

-- Question 19 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 19 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the main difference between the var and let keywords in JavaScript?', 19, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q19 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q19, N'let is block-scoped while var is function-scoped', 1, 0),
    (@Q19, N'var is block-scoped while let is function-scoped', 0, 0),
    (@Q19, N'let can only store numbers while var stores any type', 0, 0),
    (@Q19, N'There is no difference between them', 0, 0);
END

-- Question 20 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 20 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'When a regular function is called without any context (a plain call such as myFunction()) in strict mode, what is the value of this inside the function?', 20, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q20 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q20, N'undefined', 1, 0),
    (@Q20, N'The global object', 0, 0),
    (@Q20, N'null', 0, 0),
    (@Q20, N'An empty object', 0, 0);
END

-- 3. Make sure the exam draws from the full 20-question pool
UPDATE [dbo].[CourseExams]
SET [QuestionCount] = 20,
    [UpdatedAt] = GETUTCDATE()
WHERE [ExamId] = @ExamId AND [QuestionCount] < 20 AND [Status] = 1 AND [DeleteFlag] = 0;

-- Verification
SELECT
    [ExamId],
    [CourseId],
    [Title],
    [ExamFee],
    [QuestionCount],
    [PassingScore],
    (SELECT COUNT(*) FROM [dbo].[ExamQuestions]
       WHERE [ExamId] = ce.[ExamId] AND [DeleteFlag] = 0) AS QuestionRows,
    (SELECT COUNT(*) FROM [dbo].[ExamQuestionOptions]
       WHERE [DeleteFlag] = 0
         AND [ExamQuestionId] IN (SELECT [ExamQuestionId] FROM [dbo].[ExamQuestions]
                                    WHERE [ExamId] = ce.[ExamId] AND [DeleteFlag] = 0)) AS OptionRows
FROM [dbo].[CourseExams] ce
WHERE [ExamId] = @ExamId;

PRINT 'JavaScript Certification Examination seeded successfully.';
GO