-- ============================================================================
-- Seed a new "React Certification Examination" for the React course
-- (CourseId 7, "React Tutorial: Build UIs with React") with a pool of 20
-- real React / JSX questions, and set QuestionCount = 20.
-- Database: ELearningManagementSystem
-- Idempotent: safe to run multiple times.
-- ============================================================================

USE [ELearningManagementSystem];
GO

SET NOCOUNT ON;

PRINT 'Seeding React Certification Examination...';

DECLARE @CourseId INT;
SELECT TOP 1 @CourseId = [CourseId]
FROM [dbo].[Courses]
WHERE [Title] LIKE N'%React%' AND [DeleteFlag] = 0
ORDER BY [CourseId] ASC;

IF @CourseId IS NULL
BEGIN
    RAISERROR (N'React course not found. Aborting.', 16, 1);
    RETURN;
END

DECLARE @AdminUserId INT;
SELECT TOP 1 @AdminUserId = [UserId]
FROM [dbo].[Users]
WHERE [DeleteFlag] = 0
ORDER BY [UserId] ASC;
IF @AdminUserId IS NULL SET @AdminUserId = 1;

-- 1. Create the exam for the React course
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
        (@CourseId, N'React Certification Examination',
         N'Official certification exam testing React fundamentals: components, JSX, props, state, hooks, lists, events and rendering behaviour.',
         13000.00, 20, 60, 65, 3, 1, @AdminUserId, GETUTCDATE(), 0);

    SET @ExamId = SCOPE_IDENTITY();
    PRINT 'Created new exam: ' + CAST(@ExamId AS NVARCHAR(10));
END
ELSE
    PRINT 'Exam already exists: ' + CAST(@ExamId AS NVARCHAR(10));

-- 2. Question pool (20 real React / JSX questions)
-- Question 1 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 1 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is React?', 1, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q1 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q1, N'A JavaScript library for building user interfaces', 1, 0),
    (@Q1, N'A programming language', 0, 0),
    (@Q1, N'A CSS framework', 0, 0),
    (@Q1, N'A relational database', 0, 0);
END

-- Question 2 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 2 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which command-line tool is commonly used to scaffold a new React application?', 2, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q2 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q2, N'create-react-app', 1, 0),
    (@Q2, N'dotnet new web', 0, 0),
    (@Q2, N'react init', 0, 0),
    (@Q2, N'npm uninstall', 0, 0);
END

-- Question 3 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 3 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which hook is used to add state to a functional component?', 3, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q3 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q3, N'useState', 1, 0),
    (@Q3, N'useProps', 0, 0),
    (@Q3, N'setState', 0, 0),
    (@Q3, N'useRender', 0, 0);
END

-- Question 4 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 4 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Which JSX attribute is used to attach an event handler that fires when an element is clicked?', 4, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q4 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q4, N'onClick', 1, 0),
    (@Q4, N'onMouseClick', 0, 0),
    (@Q4, N'click', 0, 0),
    (@Q4, N'onclick', 0, 0);
END

-- Question 5 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 5 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is JSX?', 5, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q5 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q5, N'A syntax extension for JavaScript that allows writing HTML-like markup in the same file', 1, 0),
    (@Q5, N'A command used to compile a React app', 0, 0),
    (@Q5, N'A global state management tool', 0, 0),
    (@Q5, N'A special type of component that renders only once', 0, 0);
END

-- Question 6 (Easy)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 6 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'How do you pass data from a parent component to a child component in React?', 6, 'Easy', GETUTCDATE(), 0);
    DECLARE @Q6 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q6, N'By passing them as props (attributes) on the child component', 1, 0),
    (@Q6, N'By storing them in localStorage', 0, 0),
    (@Q6, N'By writing them directly inside the child component', 0, 0),
    (@Q6, N'By using window.globalThis', 0, 0);
END

-- Question 7 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 7 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does ReactDOM.createRoot(...).render(...) do?', 7, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q7 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q7, N'Creates the root container and renders the React tree into the DOM', 1, 0),
    (@Q7, N'Starts the React development server', 0, 0),
    (@Q7, N'Compiles JSX into plain JavaScript', 0, 0),
    (@Q7, N'Installs the React packages into the project', 0, 0);
END

-- Question 8 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 8 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'Why does each item in a React list need a unique key prop?', 8, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q8 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q8, N'So React can identify which items changed, were added or removed', 1, 0),
    (@Q8, N'To set the CSS class of the item', 0, 0),
    (@Q8, N'To make the item clickable', 0, 0),
    (@Q8, N'To improve network performance', 0, 0);
END

-- Question 9 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 9 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the useState hook return?', 9, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q9 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q9, N'A pair: the current state value and a function to update it', 1, 0),
    (@Q9, N'Only the current state value', 0, 0),
    (@Q9, N'Only the function used to update state', 0, 0),
    (@Q9, N'An object containing the full component', 0, 0);
END

-- Question 10 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 10 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the virtual DOM in React?', 10, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q10 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q10, N'An in-memory lightweight representation of the real DOM used to apply updates efficiently', 1, 0),
    (@Q10, N'A browser API used to query HTML elements', 0, 0),
    (@Q10, N'A database schema used to store components', 0, 0),
    (@Q10, N'A server-side cache for static pages', 0, 0);
END

-- Question 11 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 11 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'When does useEffect(() => { ... }, []) run?', 11, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q11 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q11, N'Once, after the initial render', 1, 0),
    (@Q11, N'On every single render', 0, 0),
    (@Q11, N'Only when the component unmounts', 0, 0),
    (@Q11, N'Only when a prop value changes', 0, 0);
END

-- Question 12 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 12 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'When does an effect with the dependency array [a, b] re-run?', 12, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q12 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q12, N'Whenever the value of a or b changes', 1, 0),
    (@Q12, N'Once right after mounting', 0, 0),
    (@Q12, N'On every render of the component', 0, 0),
    (@Q12, N'Only when the component unmounts', 0, 0);
END

-- Question 13 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 13 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is conditional rendering in React?', 13, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q13 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q13, N'Rendering different JSX depending on a condition', 1, 0),
    (@Q13, N'Rendering a component only on slow connections', 0, 0),
    (@Q13, N'Rendering UI only during the night', 0, 0),
    (@Q13, N'Compiling only the components that are used', 0, 0);
END

-- Question 14 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 14 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the typical way to render a list of items in JSX?', 14, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q14 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q14, N'Calling .map() over the array and returning JSX for each item', 1, 0),
    (@Q14, N'Using a for loop directly inside JSX', 0, 0),
    (@Q14, N'Writing each element manually', 0, 0),
    (@Q14, N'Using document.write()', 0, 0);
END

-- Question 15 (Medium)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 15 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What causes a React component to re-render?', 15, 'Medium', GETUTCDATE(), 0);
    DECLARE @Q15 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q15, N'A change in its state or props, or its parent re-rendering', 1, 0),
    (@Q15, N'Editing the component source file', 0, 0),
    (@Q15, N'Refreshing the browser tab', 0, 0),
    (@Q15, N'Changing the file name of the component', 0, 0);
END

-- Question 16 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 16 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the difference between a controlled and an uncontrolled form component?', 16, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q16 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q16, N'Controlled: value is stored in React state; uncontrolled: value is handled by the DOM itself', 1, 0),
    (@Q16, N'Controlled only works with numbers; uncontrolled with text', 0, 0),
    (@Q16, N'Controlled values are read-only; uncontrolled values are editable', 0, 0),
    (@Q16, N'There is no difference between them', 0, 0);
END

-- Question 17 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 17 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is React StrictMode?', 17, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q17 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q17, N'A development-only wrapper that double-invokes components and effects to surface side-effect bugs', 1, 0),
    (@Q17, N'A production feature that blocks unsafe network requests', 0, 0),
    (@Q17, N'A CSS reset included by default', 0, 0),
    (@Q17, N'A tool that minifies the final bundle', 0, 0);
END

-- Question 18 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 18 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'When is the useCallback hook beneficial?', 18, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q18 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q18, N'When you want to prevent a function from being recreated on every render', 1, 0),
    (@Q18, N'When you need to add CSS to a component', 0, 0),
    (@Q18, N'When you need to fetch data from an API', 0, 0),
    (@Q18, N'To persist state across browser sessions', 0, 0);
END

-- Question 19 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 19 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What does the cleanup function returned from useEffect do?', 19, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q19 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q19, N'Runs before the effect re-runs and before the component unmounts to clean up timers and subscriptions', 1, 0),
    (@Q19, N'Runs instead of the effect body', 0, 0),
    (@Q19, N'Stops the render loop permanently', 0, 0),
    (@Q19, N'Deletes the component from the DOM', 0, 0);
END

-- Question 20 (Hard)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ExamQuestions] WHERE [ExamId] = @ExamId AND [DisplayOrder] = 20 AND [DeleteFlag] = 0)
BEGIN
    INSERT INTO [dbo].[ExamQuestions] ([ExamId], [QuestionText], [DisplayOrder], [DifficultyLevel], [CreatedAt], [DeleteFlag])
    VALUES (@ExamId, N'What is the main difference between state and props in React?', 20, 'Hard', GETUTCDATE(), 0);
    DECLARE @Q20 INT = SCOPE_IDENTITY();
    INSERT INTO [dbo].[ExamQuestionOptions] ([ExamQuestionId], [OptionText], [IsCorrect], [DeleteFlag]) VALUES
    (@Q20, N'State is mutable and owned by the component; props are read-only and passed from a parent', 1, 0),
    (@Q20, N'Props are mutable and owned by the child; state is read-only', 0, 0),
    (@Q20, N'Props persist after a browser reload; state does not', 0, 0),
    (@Q20, N'State is global; props are local', 0, 0);
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

PRINT 'React Certification Examination seeded successfully.';
GO