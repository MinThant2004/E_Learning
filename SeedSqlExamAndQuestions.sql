-- ============================================================================
-- Seed a new "SQL Certification Examination" for the SQL course
-- (CourseId 4, "SQL Tutorial: Querying Databases") with a pool of 20
-- real SQL questions, and set QuestionCount = 20.
-- Database: ELearningManagementSystem
-- Idempotent: safe to run multiple times.
-- ============================================================================

USE [ELearningManagementSystem];
GO

SET NOCOUNT ON;

PRINT 'Seeding SQL Certification Examination...';

DECLARE @CourseId INT;
SELECT TOP 1 @CourseId = [CourseId]
FROM [dbo].[Courses]
WHERE [Title] LIKE N'%SQL Tutorial%' AND [DeleteFlag] = 0
ORDER BY [CourseId] ASC;

IF @CourseId IS NULL
BEGIN
    RAISERROR (N'SQL course not found. Aborting.', 16, 1);
    RETURN;
END

DECLARE @AdminUserId INT;
SELECT TOP 1 @AdminUserId = [UserId]
FROM [dbo].[Users]
WHERE [DeleteFlag] = 0
ORDER BY [UserId] ASC;
IF @AdminUserId IS NULL SET @AdminUserId = 1;

-- 1. Create the exam for the SQL course
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
        (@CourseId, N'SQL Certification Examination',
         N'Official certification exam testing core SQL: SELECT, filtering, joins, grouping, functions, constraints and data modification statements.',
         11000.00, 20, 60, 65, 3, 1, @AdminUserId, GETUTCDATE(), 0);

    SET @ExamId = SCOPE_IDENTITY();
    PRINT 'Created new exam: ' + CAST(@ExamId AS NVARCHAR(10));
END
ELSE
    PRINT 'Exam already exists: ' + CAST(@ExamId AS NVARCHAR(10));

-- 2. Question pool (20 real SQL questions)
-- Question 1 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 1 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which SQL statement is used to retrieve data from a database?', 1, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q1 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q1, N'SELECT', 1, 0),
    (@Q1, N'GET', 0, 0),
    (@Q1, N'OPEN', 0, 0),
    (@Q1, N'EXTRACT', 0, 0);
END

-- Question 2 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 2 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which SQL clause is used to filter which records are returned?', 2, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q2 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q2, N'WHERE', 1, 0),
    (@Q2, N'HAVING', 0, 0),
    (@Q2, N'FILTER', 0, 0),
    (@Q2, N'LET', 0, 0);
END

-- Question 3 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 3 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which SQL statement is used to insert new records into a table?', 3, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q3 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q3, N'INSERT INTO', 1, 0),
    (@Q3, N'ADD INTO', 0, 0),
    (@Q3, N'INSERT ROW', 0, 0),
    (@Q3, N'NEW INTO', 0, 0);
END

-- Question 4 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 4 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which SQL keyword is used to sort the result set?', 4, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q4 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q4, N'ORDER BY', 1, 0),
    (@Q4, N'SORT BY', 0, 0),
    (@Q4, N'GROUP BY', 0, 0),
    (@Q4, N'SORT', 0, 0);
END

-- Question 5 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 5 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which SQL operator is used to search for a specified pattern in a column?', 5, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q5 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q5, N'LIKE', 1, 0),
    (@Q5, N'CONTAINS', 0, 0),
    (@Q5, N'MATCH', 0, 0),
    (@Q5, N'IN', 0, 0);
END

-- Question 6 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 6 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which SQL statement is used to delete records from a table?', 6, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q6 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q6, N'DELETE', 1, 0),
    (@Q6, N'DROP', 0, 0),
    (@Q6, N'REMOVE', 0, 0),
    (@Q6, N'ERASE', 0, 0);
END

-- Question 7 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 7 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which SQL statement is used to modify existing records in a table?', 7, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q7 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q7, N'UPDATE', 1, 0),
    (@Q7, N'MODIFY', 0, 0),
    (@Q7, N'ALTER', 0, 0),
    (@Q7, N'CHANGE', 0, 0);
END

-- Question 8 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 8 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the COUNT() function do in SQL?', 8, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q8 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q8, N'Returns the number of rows (or non-NULL values) that match a query', 1, 0),
    (@Q8, N'Adds up all numeric values in a column', 0, 0),
    (@Q8, N'Returns the average value of a column', 0, 0),
    (@Q8, N'Counts the number of tables in the database', 0, 0);
END

-- Question 9 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 9 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which clause is used together with GROUP BY to filter groups?', 9, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q9 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q9, N'HAVING', 1, 0),
    (@Q9, N'WHERE', 0, 0),
    (@Q9, N'LIMIT', 0, 0),
    (@Q9, N'FILTER GROUP', 0, 0);
END

-- Question 10 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 10 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is a PRIMARY KEY in a database table?', 10, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q10 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q10, N'A column (or set of columns) that uniquely identifies each row and cannot be NULL', 1, 0),
    (@Q10, N'A column that stores only numeric values', 0, 0),
    (@Q10, N'A column that can never be updated', 0, 0),
    (@Q10, N'The first column defined in the table', 0, 0);
END

-- Question 11 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 11 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does a LEFT JOIN return?', 11, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q11 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q11, N'All rows from the left table plus matching rows from the right table (NULLs where no match)', 1, 0),
    (@Q11, N'Only rows that exist in both tables', 0, 0),
    (@Q11, N'All rows from the right table only', 0, 0),
    (@Q11, N'A cartesian product of both tables', 0, 0);
END

-- Question 12 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 12 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which SQL function returns the largest value of a selected column?', 12, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q12 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q12, N'MAX()', 1, 0),
    (@Q12, N'TOP()', 0, 0),
    (@Q12, N'LARGE()', 0, 0),
    (@Q12, N'BIGGEST()', 0, 0);
END

-- Question 13 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 13 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the difference between WHERE and HAVING in SQL?', 13, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q13 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q13, N'WHERE filters rows before grouping; HAVING filters groups after aggregation', 1, 0),
    (@Q13, N'WHERE is used only with SELECT; HAVING only with DELETE', 0, 0),
    (@Q13, N'There is no difference between them', 0, 0),
    (@Q13, N'HAVING filters rows; WHERE filters groups', 0, 0);
END

-- Question 14 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 14 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the DISTINCT keyword do in a SELECT query?', 14, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q14 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q14, N'Removes duplicate values from the result set', 1, 0),
    (@Q14, N'Sorts the result set alphabetically', 0, 0),
    (@Q14, N'Returns only the first row', 0, 0),
    (@Q14, N'Limits the number of columns returned', 0, 0);
END

-- Question 15 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 15 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which SQL statement is used to create a new table?', 15, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q15 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q15, N'CREATE TABLE', 1, 0),
    (@Q15, N'NEW TABLE', 0, 0),
    (@Q15, N'MAKE TABLE', 0, 0),
    (@Q15, N'ADD TABLE', 0, 0);
END

-- Question 16 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 16 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is a self join in SQL?', 16, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q16 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q16, N'Joining a table with itself using table aliases', 1, 0),
    (@Q16, N'Joining a table with its own copy in another database', 0, 0),
    (@Q16, N'A join that requires no ON condition', 0, 0),
    (@Q16, N'Combining a table with all other tables at once', 0, 0);
END

-- Question 17 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 17 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is database normalization?', 17, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q17 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q17, N'Organizing tables to reduce data redundancy and improve integrity', 1, 0),
    (@Q17, N'Encrypting the database to protect sensitive data', 0, 0),
    (@Q17, N'Combining all tables into one large table', 0, 0),
    (@Q17, N'Converting text columns into numeric values', 0, 0);
END

-- Question 18 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 18 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the UNION operator do in SQL?', 18, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q18 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q18, N'Combines the result sets of two or more SELECT queries and removes duplicate rows', 1, 0),
    (@Q18, N'Combines two tables by matching common columns', 0, 0),
    (@Q18, N'Adds the counts returned by two queries', 0, 0),
    (@Q18, N'Returns the intersection of two result sets', 0, 0);
END

-- Question 19 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 19 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is an index in a database?', 19, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q19 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q19, N'A database structure that speeds up data retrieval on a table column', 1, 0),
    (@Q19, N'A backup copy of the whole database', 0, 0),
    (@Q19, N'A special row that stores the table name', 0, 0),
    (@Q19, N'A constraint that prevents inserting NULL values', 0, 0);
END

-- Question 20 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 20 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the difference between TRUNCATE and DELETE in SQL?', 20, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q20 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q20, N'TRUNCATE removes all rows in one operation and resets identity; DELETE removes rows selectively and supports a WHERE clause', 1, 0),
    (@Q20, N'TRUNCATE supports a WHERE clause; DELETE removes all rows', 0, 0),
    (@Q20, N'Both are exactly the same', 0, 0),
    (@Q20, N'DELETE drops the whole table; TRUNCATE only deletes columns', 0, 0);
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

PRINT 'SQL Certification Examination seeded successfully.';
GO