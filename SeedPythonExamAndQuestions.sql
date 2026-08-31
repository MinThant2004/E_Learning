-- ============================================================================
-- Seed a new "Python Certification Examination" for the Python course
-- (CourseId 6, "Python Tutorial: Basics for Beginners") with a pool of 20
-- real Python questions, and set QuestionCount = 20.
-- Database: ELearningManagementSystem
-- Idempotent: safe to run multiple times.
-- ============================================================================

USE [ELearningManagementSystem];
GO

SET NOCOUNT ON;

PRINT 'Seeding Python Certification Examination...';

DECLARE @CourseId INT;
SELECT TOP 1 @CourseId = [CourseId]
FROM [dbo].[Courses]
WHERE [Title] LIKE N'%Python Tutorial%' AND [DeleteFlag] = 0
ORDER BY [CourseId] ASC;

IF @CourseId IS NULL
BEGIN
    RAISERROR (N'Python course not found. Aborting.', 16, 1);
    RETURN;
END

DECLARE @AdminUserId INT;
SELECT TOP 1 @AdminUserId = [UserId]
FROM [dbo].[Users]
WHERE [DeleteFlag] = 0
ORDER BY [UserId] ASC;
IF @AdminUserId IS NULL SET @AdminUserId = 1;

-- 1. Create the exam for the Python course
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
        (@CourseId, N'Python Certification Examination',
         N'Official certification exam testing core Python: syntax, built-in types, collections, functions, control flow, exceptions, comprehensions and advanced language concepts.',
         10000.00, 20, 60, 65, 3, 1, @AdminUserId, GETUTCDATE(), 0);

    SET @ExamId = SCOPE_IDENTITY();
    PRINT 'Created new exam: ' + CAST(@ExamId AS NVARCHAR(10));
END
ELSE
    PRINT 'Exam already exists: ' + CAST(@ExamId AS NVARCHAR(10));

-- 2. Question pool (20 real Python questions)
-- Question 1 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 1 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which Python keyword is used to define a function?', 1, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q1 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q1, N'def', 1, 0),
    (@Q1, N'function', 0, 0),
    (@Q1, N'func', 0, 0),
    (@Q1, N'void', 0, 0);
END

-- Question 2 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 2 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the output of print(2 ** 3)?', 2, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q2 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q2, N'8', 1, 0),
    (@Q2, N'6', 0, 0),
    (@Q2, N'23', 0, 0),
    (@Q2, N'9', 0, 0);
END

-- Question 3 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 3 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which Python data type is used to store whole numbers such as 42?', 3, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q3 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q3, N'int', 1, 0),
    (@Q3, N'float', 0, 0),
    (@Q3, N'string', 0, 0),
    (@Q3, N'decimal', 0, 0);
END

-- Question 4 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 4 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the correct way to create a list containing 1, 2 and 3 in Python?', 4, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q4 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q4, N'nums = [1, 2, 3]', 1, 0),
    (@Q4, N'nums = (1, 2, 3)', 0, 0),
    (@Q4, N'nums = {1, 2, 3}', 0, 0),
    (@Q4, N'nums = 1, 2, 3;', 0, 0);
END

-- Question 5 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 5 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which list method adds an item to the end of a list in Python?', 5, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q5 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q5, N'append()', 1, 0),
    (@Q5, N'add()', 0, 0),
    (@Q5, N'push()', 0, 0),
    (@Q5, N'insert_end()', 0, 0);
END

-- Question 6 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 6 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the print() function do in Python?', 6, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q6 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q6, N'Outputs text and values to the console', 1, 0),
    (@Q6, N'Sends a document to the printer', 0, 0),
    (@Q6, N'Creates a new Python file', 0, 0),
    (@Q6, N'Reads input from the keyboard', 0, 0);
END

-- Question 7 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 7 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the output of print(type([]))?', 7, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q7 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q7, N'<class "list">', 1, 0),
    (@Q7, N'<class "array">', 0, 0),
    (@Q7, N'list', 0, 0),
    (@Q7, N'<class "tuple">', 0, 0);
END

-- Question 8 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 8 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which keyword is used with the try statement to catch and handle an exception in Python?', 8, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q8 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q8, N'except', 1, 0),
    (@Q8, N'catch', 0, 0),
    (@Q8, N'finally', 0, 0),
    (@Q8, N'rescue', 0, 0);
END

-- Question 9 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 9 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is a dictionary in Python?', 9, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q9 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q9, N'An unordered collection of key-value pairs', 1, 0),
    (@Q9, N'A list of duplicate values', 0, 0),
    (@Q9, N'An ordered collection of numbers only', 0, 0),
    (@Q9, N'A read-only tuple of strings', 0, 0);
END

-- Question 10 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 10 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the len() function return in Python?', 10, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q10 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q10, N'The number of items in a collection or characters in a string', 1, 0),
    (@Q10, N'The memory size of an object in bytes', 0, 0),
    (@Q10, N'The length of a function name', 0, 0),
    (@Q10, N'The index of the last element', 0, 0);
END

-- Question 11 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 11 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the output of print(10 // 3)?', 11, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q11 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q11, N'3', 1, 0),
    (@Q11, N'3.333', 0, 0),
    (@Q11, N'1', 0, 0),
    (@Q11, N'4', 0, 0);
END

-- Question 12 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 12 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which Python loop continues to run only while a given condition is True?', 12, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q12 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q12, N'while', 1, 0),
    (@Q12, N'for', 0, 0),
    (@Q12, N'foreach', 0, 0),
    (@Q12, N'repeat', 0, 0);
END

-- Question 13 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 13 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the upper() string method do in Python?', 13, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q13 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q13, N'Returns a copy of the string converted to uppercase', 1, 0),
    (@Q13, N'Removes whitespace from the string', 0, 0),
    (@Q13, N'Splits the string into words', 0, 0),
    (@Q13, N'Checks whether the string is empty', 0, 0);
END

-- Question 14 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 14 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the difference between a tuple and a list in Python?', 14, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q14 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q14, N'A tuple is immutable; a list is mutable', 1, 0),
    (@Q14, N'A list is immutable; a tuple is mutable', 0, 0),
    (@Q14, N'Tuples can only store numbers; lists can store anything', 0, 0),
    (@Q14, N'There is no difference between them', 0, 0);
END

-- Question 15 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 15 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the range() function return?', 15, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q15 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q15, N'A sequence of numbers (used in loops and iteration)', 1, 0),
    (@Q15, N'A random number generator', 0, 0),
    (@Q15, N'The length of a string', 0, 0),
    (@Q15, N'A fixed list of letters', 0, 0);
END

-- Question 16 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 16 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is a lambda function in Python?', 16, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q16 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q16, N'An anonymous function defined as a single expression', 1, 0),
    (@Q16, N'A recursive function that calls itself', 0, 0),
    (@Q16, N'A function that returns multiple values', 0, 0),
    (@Q16, N'A built-in function for sorting', 0, 0);
END

-- Question 17 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 17 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is a list comprehension in Python?', 17, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q17 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q17, N'A compact one-line syntax for creating a list from an iterable', 1, 0),
    (@Q17, N'A function that removes duplicates from a list', 0, 0),
    (@Q17, N'A loop that runs while a list is not empty', 0, 0),
    (@Q17, N'A special type of nested list', 0, 0);
END

-- Question 18 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 18 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the difference between the == operator and the is operator in Python?', 18, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q18 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q18, N'== compares values; is checks whether two variables refer to the same object', 1, 0),
    (@Q18, N'== checks identity; is compares values', 0, 0),
    (@Q18, N'== is used only for strings; is only for numbers', 0, 0),
    (@Q18, N'There is no difference between them', 0, 0);
END

-- Question 19 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 19 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the with statement do in Python?', 19, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q19 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q19, N'Manages a context (such as opening a file) and guarantees cleanup is performed', 1, 0),
    (@Q19, N'Repeats a block of code until a condition is True', 0, 0),
    (@Q19, N'Imports a module at runtime', 0, 0),
    (@Q19, N'Creates a backup of a variable', 0, 0);
END

-- Question 20 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 20 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the Global Interpreter Lock (GIL) in CPython?', 20, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q20 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q20, N'A mutex that allows only one thread to execute Python bytecode at a time', 1, 0),
    (@Q20, N'A firewall rule that blocks network threads', 0, 0),
    (@Q20, N'A cache that speeds up imports', 0, 0),
    (@Q20, N'A security mechanism that locks down memory', 0, 0);
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

PRINT 'Python Certification Examination seeded successfully.';
GO