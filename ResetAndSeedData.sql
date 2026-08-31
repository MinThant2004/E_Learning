/*=============================================================================================
 ResetAndSeedData.sql
 ---------------------------------------------------------------------------------------------
 Project : E-Learning Management System (W3Schools-like)
 Purpose : Complete RESET + RICH SEED of the ELearningManagementSystem database for local dev,
           demos and UI pagination testing.

 WHAT IT DOES
   1. Disables foreign keys ONLY on the e-learning schema tables (see scope below).
   2. Deletes every existing row from those tables.
   3. Reseed all identity columns so the next generated id starts at 1.
   4. Inserts rich, realistic seed data in strict foreign-key dependency order
      (identities inserted explicitly, so every id is deterministic).
   5. Re-enables all foreign keys WITH CHECK.

 SCOPE / SAFETY
   - This database is SHARED with another application (TblAdminUser, TblBranch, TblOutward*,
     sysdiagrams, ...). Those tables are NEVER touched by this script.
   - The script refuses to run if the current database is not [ELearningManagementSystem].
   - Runs in ONE transaction; any error rolls EVERYTHING back.
   - Phase 23+ certification-exam tables (CertificationExams, ExamPayments, ExamAttempts,
     snapshots, pools, Certificates) are NOT referenced - they were removed by
     DropPhase23Tables.sql because the feature was never implemented.

 SEEDED DATA OVERVIEW
   - 28 permissions (canonical codes of ACTIVE modules only - reports included,
     Phase 23+ certification codes removed)
   - 3 roles: Student, Administrator, System Administrator
   - 12 users: 1 System Administrator, 3 Administrators, 8 students
   - 8 categories: HTML, CSS, JavaScript, SQL, C#, Python, React, Git & Tools
   - 12 courses (course 12 soft-deleted/archived; courses 9-12 without thumbnails for fallback UI)
   - 42 lessons with full W3Schools-style tutorial content (plain text; the lesson reader renders
     it with white-space: pre-wrap, so structure and code indentation are preserved)
   - 12 final quizzes (one per course - business rule), 40 questions, 160 options
   - 24 enrollments: 9 Completed, 10 In Progress, 5 Not Started
   - 59 lesson-progress rows consistent with each enrollment state
   - 11 quiz attempts (incl. a FAILED first attempt followed by a PASSING retake),
     43 answer rows, all consistent with server-side scoring rules
     (score = correct/total*100, passed = score >= PassingScore, pass => enrollment completed)
   - 31 audit-log entries using the app's real conventions:
       Action in {Create, Update, Archive, Restore, ChangePassword, ResetPassword}
       TableName in {Users, Roles, Categories, Courses, Lessons, Quizzes}
       Changes = JSON array [{"field":"...","oldValue":"...","newValue":"..."}] (camelCase)

 TEST ACCOUNT PASSWORDS (BCrypt work factor 11, verified against BCrypt.Net-Next 4.0.3)
   System Administrator  superadmin@elearning.com          Admin@12345
   Administrator         admin@elearning.com               Admin@12345
   Administrator         content.admin@elearning.com       Admin@12345
   Administrator         auditor@elearning.com             Admin@12345
   Students          emily.johnson@student.com         Student@12345
                     liam.smith@student.com            Student@12345
                     olivia.brown@student.com          Student@12345
                     noah.davis@student.com            Student@12345
                     ava.wilson@student.com            Student@12345
                     ethan.moore@student.com           Student@12345
                     sofia.garcia@student.com          Student@12345
                     daniel.kim@student.com            Student@12345

 USAGE
   Open in SSMS (or sqlcmd) while connected to the ELearningManagementSystem database and run.
=============================================================================================
*/

USE [ELearningManagementSystem];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* Safety gate: never run against the wrong database */
IF DB_NAME() <> N'ELearningManagementSystem'
BEGIN
    RAISERROR (N'ResetAndSeedData.sql must be executed while connected to the [ELearningManagementSystem] database.', 16, 1);
    RETURN;
END;

PRINT N'=================================================';
PRINT N' Resetting and reseeding e-learning data...';
PRINT N'=================================================';

BEGIN TRY
BEGIN TRANSACTION;

/*---------------------------------------------------------------------------------------------
 1) DISABLE FOREIGN KEY CONSTRAINTS (e-learning tables only)
---------------------------------------------------------------------------------------------*/
ALTER TABLE dbo.AuditLogs                        NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.QuizAnswers                      NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.QuizAttempts                     NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.LessonProgress                   NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.Enrollments                      NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.QuestionOptions                  NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.Questions                        NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.Quizzes                          NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.Lessons                          NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.Courses                          NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.Categories                       NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.RefreshTokens                    NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.PasswordResetTokens              NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.UserRoles                        NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.RolePermissions                  NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.Permissions                      NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.Roles                            NOCHECK CONSTRAINT ALL;
ALTER TABLE dbo.Users                            NOCHECK CONSTRAINT ALL;

/*---------------------------------------------------------------------------------------------
 2) DELETE EXISTING ROWS (children first, parents last)
---------------------------------------------------------------------------------------------*/
DELETE FROM dbo.AuditLogs;
DELETE FROM dbo.QuizAnswers;
DELETE FROM dbo.QuizAttempts;
DELETE FROM dbo.LessonProgress;
DELETE FROM dbo.Enrollments;
DELETE FROM dbo.QuestionOptions;
DELETE FROM dbo.Questions;
DELETE FROM dbo.Quizzes;
DELETE FROM dbo.Lessons;
DELETE FROM dbo.Courses;
DELETE FROM dbo.Categories;
DELETE FROM dbo.RefreshTokens;
DELETE FROM dbo.PasswordResetTokens;
DELETE FROM dbo.UserRoles;
DELETE FROM dbo.RolePermissions;
DELETE FROM dbo.Permissions;
DELETE FROM dbo.Roles;
DELETE FROM dbo.Users;

/*---------------------------------------------------------------------------------------------
 3) RESEED IDENTITY COLUMNS (next value = 1)
---------------------------------------------------------------------------------------------*/
DBCC CHECKIDENT ('dbo.AuditLogs',                    RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.QuizAnswers',                  RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.QuizAttempts',                 RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.LessonProgress',               RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Enrollments',                  RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.QuestionOptions',              RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Questions',                    RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Quizzes',                      RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Lessons',                      RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Courses',                      RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Categories',                   RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.RefreshTokens',                RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.PasswordResetTokens',          RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.UserRoles',                    RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.RolePermissions',              RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Permissions',                  RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Roles',                        RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Users',                        RESEED, 0) WITH NO_INFOMSGS;

PRINT N'Schema cleared and identities reset.';
PRINT N'Seeding data...';

/*---------------------------------------------------------------------------------------------
 4) SEED DATA (parents first, children last)
---------------------------------------------------------------------------------------------*/

/*------------------------------ PERMISSIONS (canonical API permission codes) */
SET IDENTITY_INSERT dbo.Permissions ON;
INSERT INTO dbo.Permissions (PermissionId, PermissionCode, PermissionName, Module) VALUES
 (1,  N'Course.Read',             N'Read',                                    N'Courses'),
 (2,  N'Course.Create',           N'Create',                                  N'Courses'),
 (3,  N'Course.Update',           N'Update',                                  N'Courses'),
 (4,  N'Course.Delete',           N'Delete',                                  N'Courses'),
 (5,  N'Lesson.Read',             N'Read',                                    N'Lessons'),
 (6,  N'Lesson.Create',           N'Create',                                  N'Lessons'),
 (7,  N'Lesson.Update',           N'Update',                                  N'Lessons'),
 (8,  N'Lesson.Delete',           N'Delete',                                  N'Lessons'),
 (9,  N'Quiz.Read',               N'Read',                                    N'Quizzes'),
 (10, N'Quiz.Create',             N'Create',                                  N'Quizzes'),
 (11, N'Quiz.Update',             N'Update',                                  N'Quizzes'),
 (12, N'Quiz.Delete',             N'Delete',                                  N'Quizzes'),
 (13, N'User.Read',               N'Read',                                    N'Users'),
 (14, N'User.Update',             N'Update',                                  N'Users'),
 (15, N'Role.Read',               N'Read',                                    N'Roles'),
 (16, N'Role.Create',             N'Create',                                  N'Roles'),
 (17, N'Role.Update',             N'Update',                                  N'Roles'),
 (18, N'Role.Delete',             N'Delete',                                  N'Roles'),
 (19, N'Permission.Read',         N'Read',                                    N'Permissions'),
 (20, N'Permission.Assign',       N'Assign',                                  N'Permissions'),
 (21, N'Category.Read',           N'Read',                                    N'Categories'),
 (22, N'Category.Create',         N'Create',                                  N'Categories'),
 (23, N'Category.Update',         N'Update',                                  N'Categories'),
 (24, N'Category.Delete',         N'Delete',                                  N'Categories'),
 (25, N'AuditLog.Read',           N'Read',                                    N'Audit Logs'),
 (26, N'EnrollmentHistory.Read',  N'Read Enrollment History',                 N'Reports'),
 (27, N'CoursePerformance.Read',  N'Read Course Performance Reports',         N'Reports'),
 (28, N'QuizPerformance.Read',    N'Read Quiz Performance Reports',           N'Reports');
SET IDENTITY_INSERT dbo.Permissions OFF;

/*------------------------------ ROLES */
SET IDENTITY_INSERT dbo.Roles ON;
INSERT INTO dbo.Roles (RoleId, RoleName, Description, CreatedAt) VALUES
 (1, N'Student',              N'Built-in role. Students browse courses, enroll, complete lessons and take final quizzes.',                                        '2026-01-15T08:00:00'),
 (2, N'Administrator',        N'Built-in role. Manages day-to-day content and users. Cannot create/delete roles or assign permissions.',                          '2026-01-15T08:00:00'),
 (3, N'System Administrator', N'Built-in role. Full platform control including roles and permission assignment.',                                                '2026-01-15T08:00:00');
SET IDENTITY_INSERT dbo.Roles OFF;

/*------------------------------ ROLE -> PERMISSION MAPPINGS */
SET IDENTITY_INSERT dbo.RolePermissions ON;
INSERT INTO dbo.RolePermissions (RolePermissionId, RoleId, PermissionId) VALUES
 /* System Administrator (3): all 28 permissions */
 (1,  3, 1 ), (2,  3, 2 ), (3,  3, 3 ), (4,  3, 4 ),
 (5,  3, 5 ), (6,  3, 6 ), (7,  3, 7 ), (8,  3, 8 ),
 (9,  3, 9 ), (10, 3, 10), (11, 3, 11), (12, 3, 12),
 (13, 3, 13), (14, 3, 14), (15, 3, 15), (16, 3, 16),
 (17, 3, 17), (18, 3, 18), (19, 3, 19), (20, 3, 20),
 (21, 3, 21), (22, 3, 22), (23, 3, 23), (24, 3, 24),
 (25, 3, 25), (26, 3, 26), (27, 3, 27), (28, 3, 28),
 /* Administrator (2): everything except Role.Create/Role.Update/Role.Delete and Permission.Assign */
 (29, 2, 1 ), (30, 2, 2 ), (31, 2, 3 ), (32, 2, 4 ),
 (33, 2, 5 ), (34, 2, 6 ), (35, 2, 7 ), (36, 2, 8 ),
 (37, 2, 9 ), (38, 2, 10), (39, 2, 11), (40, 2, 12),
 (41, 2, 13), (42, 2, 14), (43, 2, 15), (44, 2, 19),
 (45, 2, 21), (46, 2, 22), (47, 2, 23), (48, 2, 24),
 (49, 2, 25), (50, 2, 26), (51, 2, 27), (52, 2, 28),
 /* Student (1): read-only access needed for browsing and learning flows */
 (53, 1, 1), (54, 1, 5), (55, 1, 9);
SET IDENTITY_INSERT dbo.RolePermissions OFF;

/*------------------------------ USERS
  Password hashes are genuine BCrypt (work factor 11) values:
    Admin@12345   => $2a$11$5zDvlmTRdqtNxqKfoMYrT.WsfFNT/5uIY3K8i2ngNH.pUYjl44br6
    Student@12345 => $2a$11$FxWj7K1wxqt/VUNaNXefDeaGZOhVNcYD/lmyz01N0MVptjFvbEejm
*/
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (UserId, FullName, Email, PasswordHash, Status, MustChangePassword, CreatedAt, UpdatedAt, DeleteFlag) VALUES
 (1,  N'Sarah Mitchell', N'superadmin@elearning.com',    N'$2a$11$5zDvlmTRdqtNxqKfoMYrT.WsfFNT/5uIY3K8i2ngNH.pUYjl44br6', 1, 0, '2026-02-01T08:00:00', NULL, 0),
 (2,  N'David Chen',     N'admin@elearning.com',         N'$2a$11$5zDvlmTRdqtNxqKfoMYrT.WsfFNT/5uIY3K8i2ngNH.pUYjl44br6', 1, 0, '2026-02-03T09:15:00', NULL, 0),
 (3,  N'Amina Yusuf',    N'content.admin@elearning.com', N'$2a$11$5zDvlmTRdqtNxqKfoMYrT.WsfFNT/5uIY3K8i2ngNH.pUYjl44br6', 1, 0, '2026-02-06T11:30:00', NULL, 0),
 (4,  N'Omar Farouk',    N'auditor@elearning.com',       N'$2a$11$5zDvlmTRdqtNxqKfoMYrT.WsfFNT/5uIY3K8i2ngNH.pUYjl44br6', 1, 0, '2026-02-06T11:45:00', NULL, 0),
 (5,  N'Emily Johnson',  N'emily.johnson@student.com',   N'$2a$11$FxWj7K1wxqt/VUNaNXefDeaGZOhVNcYD/lmyz01N0MVptjFvbEejm', 1, 0, '2026-02-10T10:20:00', NULL, 0),
 (6,  N'Liam Smith',     N'liam.smith@student.com',      N'$2a$11$FxWj7K1wxqt/VUNaNXefDeaGZOhVNcYD/lmyz01N0MVptjFvbEejm', 1, 0, '2026-02-12T14:05:00', NULL, 0),
 (7,  N'Olivia Brown',   N'olivia.brown@student.com',    N'$2a$11$FxWj7K1wxqt/VUNaNXefDeaGZOhVNcYD/lmyz01N0MVptjFvbEejm', 1, 0, '2026-02-14T16:40:00', NULL, 0),
 (8,  N'Noah Davis',     N'noah.davis@student.com',      N'$2a$11$FxWj7K1wxqt/VUNaNXefDeaGZOhVNcYD/lmyz01N0MVptjFvbEejm', 1, 0, '2026-02-18T09:30:00', NULL, 0),
 (9,  N'Ava Wilson',     N'ava.wilson@student.com',      N'$2a$11$FxWj7K1wxqt/VUNaNXefDeaGZOhVNcYD/lmyz01N0MVptjFvbEejm', 1, 0, '2026-02-21T11:55:00', NULL, 0),
 (10, N'Ethan Moore',    N'ethan.moore@student.com',     N'$2a$11$FxWj7K1wxqt/VUNaNXefDeaGZOhVNcYD/lmyz01N0MVptjFvbEejm', 1, 0, '2026-02-25T13:10:00', NULL, 0),
 (11, N'Sofia Garcia',   N'sofia.garcia@student.com',    N'$2a$11$FxWj7K1wxqt/VUNaNXefDeaGZOhVNcYD/lmyz01N0MVptjFvbEejm', 1, 0, '2026-03-01T15:25:00', NULL, 0),
 (12, N'Daniel H. Kim',  N'daniel.kim@student.com',      N'$2a$11$FxWj7K1wxqt/VUNaNXefDeaGZOhVNcYD/lmyz01N0MVptjFvbEejm', 1, 0, '2026-03-05T10:45:00', NULL, 0);
SET IDENTITY_INSERT dbo.Users OFF;

/*------------------------------ USER <-> ROLE ASSIGNMENTS */
SET IDENTITY_INSERT dbo.UserRoles ON;
INSERT INTO dbo.UserRoles (UserRoleId, UserId, RoleId, AssignedBy, AssignedAt) VALUES
 (1,  1,  3, NULL, '2026-02-01T08:00:00'), /* Sarah  -> System Administrator (bootstrap) */
 (2,  2,  2, 1,    '2026-02-03T09:15:00'), /* David  -> Administrator                    */
 (3,  3,  2, 1,    '2026-02-06T11:30:00'), /* Amina  -> Administrator                    */
 (4,  4,  2, 1,    '2026-02-06T11:45:00'), /* Omar   -> Administrator                    */
 (5,  5,  1, 2,    '2026-02-10T10:20:00'), /* Emily  -> Student                 */
 (6,  6,  1, 2,    '2026-02-12T14:05:00'), /* Liam   -> Student                 */
 (7,  7,  1, 2,    '2026-02-14T16:40:00'), /* Olivia -> Student                 */
 (8,  8,  1, 2,    '2026-02-18T09:30:00'), /* Noah   -> Student                 */
 (9,  9,  1, 2,    '2026-02-21T11:55:00'), /* Ava    -> Student                 */
 (10, 10, 1, 2,    '2026-02-25T13:10:00'), /* Ethan  -> Student                 */
 (11, 11, 1, 2,    '2026-03-01T15:25:00'), /* Sofia  -> Student                 */
 (12, 12, 1, 2,    '2026-03-05T10:45:00'); /* Daniel -> Student                 */
SET IDENTITY_INSERT dbo.UserRoles OFF;

/*------------------------------ CATEGORIES */
SET IDENTITY_INSERT dbo.Categories ON;
INSERT INTO dbo.Categories (CategoryId, CategoryName, Description, CreatedAt, UpdatedAt, DeleteFlag) VALUES
 (1, N'HTML',        N'The standard markup language for structuring web pages.',                             '2026-03-01T09:00:00', NULL,                  0),
 (2, N'CSS',         N'Cascading Style Sheets - styling, layout and responsive design.',                     '2026-03-01T09:05:00', NULL,                  0),
 (3, N'JavaScript',  N'The programming language of the web - interactivity, logic and the DOM.',             '2026-03-01T09:10:00', NULL,                  0),
 (4, N'SQL',         N'Structured Query Language for storing, retrieving and managing relational data.',     '2026-03-01T09:15:00', NULL,                  0),
 (5, N'C#',          N'Modern, object-oriented language for web, desktop and game development.',             '2026-03-01T09:20:00', NULL,                  0),
 (6, N'Python',      N'Versatile, beginner-friendly language popular for scripting, data and automation.',   '2026-03-01T09:25:00', NULL,                  0),
 (7, N'React',       N'A JavaScript library for building fast, component-driven user interfaces.',           '2026-03-01T09:30:00', NULL,                  0),
 (8, N'Git & Tools', N'Version control and essential developer tooling for modern web teams.',               '2026-03-01T09:35:00', '2026-04-10T08:30:00', 0);
SET IDENTITY_INSERT dbo.Categories OFF;

/*------------------------------ COURSES
  ThumbnailUrl values point at files that already exist under
  src/ELearningManagementSystem.Api/wwwroot/uploads/courses/.
  Courses 9-12 deliberately have NULL thumbnails (thumbnail-fallback UI testing).
  Course 12 is soft-deleted (DeleteFlag = 1) for admin archive-view testing.
*/
SET IDENTITY_INSERT dbo.Courses ON;
INSERT INTO dbo.Courses (CourseId, CategoryId, Title, Description, ThumbnailUrl, Status, CreatedBy, CreatedAt, UpdatedAt, DeleteFlag) VALUES
 (1, 1, N'HTML Tutorial: The Complete Guide',
    N'Learn HTML step by step - the standard markup language for creating web pages. Covers documents, headings, paragraphs, links, images, tables and lists with hands-on examples you can try yourself.',
    N'/uploads/courses/e56fb30e-d944-4169-82e2-6ada7d5a7eb0.jpg', 1, 2, '2026-03-02T10:20:00', NULL, 0),
 (2, 2, N'CSS Tutorial: Styling the Web',
    N'CSS describes how HTML elements should be displayed on screen, paper or media. This tutorial teaches selectors, colors, backgrounds, fonts and the box model through practical, copy-paste examples.',
    N'/uploads/courses/ff36d711-4336-4f8d-87c1-e3d2a47f374a.png', 1, 2, '2026-03-05T11:00:00', NULL, 0),
 (3, 3, N'JavaScript Tutorial: From Zero to Hero',
    N'JavaScript is the programming language of the web. Start from variables and functions and work your way up to arrays, objects and events - the foundations every web developer needs.',
    N'/uploads/courses/e1180aaf-f3e9-4c04-a8fe-34545c8ec29c.png', 1, 3, '2026-03-09T13:25:00', NULL, 0),
 (4, 4, N'SQL Tutorial: Querying Databases',
    N'SQL lets you access and manipulate relational databases. Learn the essential statements - SELECT, WHERE, INSERT, UPDATE and DELETE - with runnable examples on sample tables.',
    N'/uploads/courses/ac6ebf18-d8e8-4768-ac61-3edcf85744f8.png', 1, 2, '2026-03-14T09:40:00', NULL, 0),
 (5, 5, N'C# Tutorial: Learn C# Programming',
    N'C# (pronounced C sharp) is a modern, object-oriented programming language built on the .NET platform. Learn syntax, types, conditions and loops with simple console examples.',
    N'/uploads/courses/a4438463-855e-45b3-b8ff-bacf78d4590a.png', 1, 3, '2026-03-20T10:10:00', NULL, 0),
 (6, 6, N'Python Tutorial: Basics for Beginners',
    N'Python is an easy-to-learn, powerful programming language with clean, readable syntax. Perfect for your first steps into programming, scripting, automation and data work.',
    N'/uploads/courses/80fb0d64-c4cc-46e5-b435-2a681a622a99.png', 1, 2, '2026-03-25T14:30:00', NULL, 0),
 (7, 7, N'React Tutorial: Build UIs with React',
    N'React is a free and open-source front-end JavaScript library for building user interfaces based on components. Learn JSX, components, props and state by building small UI pieces.',
    N'/uploads/courses/7a4f66fa-88b6-4642-a3e1-4c8086902a0f.png', 1, 3, '2026-04-01T09:30:00', NULL, 0),
 (8, 3, N'JavaScript DOM Manipulation',
    N'The DOM is a programming interface for web documents. Learn how to select, create, modify and remove HTML elements dynamically with modern vanilla JavaScript APIs.',
    N'/uploads/courses/600e5ef8-d21a-41df-860b-757b1ec207d5.png', 1, 2, '2026-04-10T10:45:00', NULL, 0),
 (9, 2, N'CSS Flexbox and Grid Layouts',
    N'Modern CSS layout without hacks. Flexbox handles one-dimensional layouts, CSS Grid handles two-dimensional designs. Build navbars, card grids and full page layouts that just work.',
    NULL, 1, 3, '2026-04-18T11:15:00', NULL, 0),
 (10, 4, N'SQL Joins and Advanced Queries',
    N'Real data lives in many tables. Master INNER JOIN, LEFT JOIN, RIGHT JOIN, FULL OUTER JOIN and self joins, plus UNION combinations, to answer complex questions across tables.',
    NULL, 1, 2, '2026-04-26T13:00:00', NULL, 0),
 (11, 5, N'C# Object-Oriented Programming',
    N'Go beyond basics with classes, objects, constructors, properties, inheritance and polymorphism. Understand how real-world .NET applications model the domain with OOP principles.',
    NULL, 1, 3, '2026-05-05T08:50:00', NULL, 0),
 (12, 6, N'Python Data Structures (Legacy)',
    N'Legacy edition of our Python data structures course covering tuples, sets, stacks and queues. Superseded by the updated Python track - kept online for reference only.',
    NULL, 0, 2, '2026-05-12T15:20:00', '2026-05-20T16:00:00', 1);
SET IDENTITY_INSERT dbo.Courses OFF;


/*------------------------------ LESSONS
  W3Schools-style tutorial content. The Blazor lesson reader renders lesson content as plain
  text with white-space: pre-wrap, so blank lines, structure and code indentation are preserved.
*/
SET IDENTITY_INSERT dbo.Lessons ON;
INSERT INTO dbo.Lessons (LessonId, CourseId, Title, Content, DisplayOrder, CreatedAt, UpdatedAt, DeleteFlag) VALUES

/* ===================== COURSE 1 - HTML Tutorial ===================== */
(1, 1, N'HTML Home and Introduction',
N'HTML is the standard markup language for creating Web pages.

- HTML stands for Hyper Text Markup Language
- HTML describes the structure of a Web page
- HTML consists of a series of elements
- HTML elements tell the browser how to display the content

A Simple HTML Document

Example

<!DOCTYPE html>
<html>
<head>
  <title>Page Title</title>
</head>
<body>

  <h1>My First Heading</h1>
  <p>My first paragraph.</p>

</body>
</html>

Try It Yourself >>

Example Explained

- The <!DOCTYPE html> declaration defines that this document is an HTML5 document
- The <html> element is the root element of an HTML page
- The <head> element contains meta information about the page
- The <title> element specifies a title for the page
- The <body> element defines the document body

Note: Only the content inside the <body> section is displayed in a browser.',
1, '2026-03-02T11:00:00', NULL, 0),

(2, 1, N'HTML Headings and Paragraphs',
N'HTML headings are defined with the <h1> to <h6> tags. <h1> defines the most important heading and <h6> defines the least important heading.

HTML paragraphs are defined with the <p> tag. Browsers automatically add some white space (a margin) before and after each paragraph.

Example

<h1>Heading 1</h1>
<h2>Heading 2</h2>
<h3>Heading 3</h3>

<p>This is a paragraph.</p>
<p>This is another paragraph.</p>

Try It Yourself >>

Line Breaks

The HTML <br> element defines a line break without starting a new paragraph:

<p>This is<br>a paragraph<br>with line breaks.</p>

The Horizontal Rule

The <hr> tag defines a thematic break in an HTML page and is most often displayed as a horizontal rule:

<h2>Section One</h2>
<p>Some text about section one.</p>
<hr>
<h2>Section Two</h2>
<p>Some text about section two.</p>

Important: Use HTML headings for headings only. Do not use headings to make text BIG or bold - search engines use headings to index the structure of your pages.',
2, '2026-03-02T11:05:00', NULL, 0),

(3, 1, N'HTML Links and Images',
N'HTML links are defined with the <a> tag. The link destination is specified in the href attribute.

Example

<a href="https://www.w3schools.com">This is a link</a>

Try It Yourself >>

The target Attribute

The target attribute specifies where to open the linked document:

<a href="https://www.w3schools.com" target="_blank">Visit W3Schools</a>

- _self  : Default. Opens the document in the same window/tab
- _blank : Opens the document in a new window or tab

HTML Images

HTML images are defined with the <img> tag. The source file (src), alternative text (alt), width and height are provided as attributes:

<img src="pic_trulli.jpg" alt="Italian Trulli" width="500" height="333">

Note: The alt attribute provides text for screen readers and is shown if the image cannot be loaded. Always specify width and height to avoid flickering while the page loads.

Absolute vs Relative URLs

- Absolute URL: <img src="https://www.mysite.com/images/cat.jpg">
- Relative URL: <img src="images/cat.jpg">

Relative URLs are recommended because they keep working when you move your project to another host.',
3, '2026-03-02T11:10:00', NULL, 0),

(4, 1, N'HTML Tables and Lists',
N'HTML tables allow web developers to arrange data into rows and columns. A table is defined with the <table> tag; each row with <tr>; a header cell with <th>; and a data cell with <td>.

Example

<table>
  <tr>
    <th>Company</th>
    <th>Contact</th>
  </tr>
  <tr>
    <td>Alfreds Futterkiste</td>
    <td>Maria Anders</td>
  </tr>
</table>

Try It Yourself >>

HTML Lists

Unordered lists start with the <ul> tag; list items start with the <li> tag and are marked with bullets by default:

<ul>
  <li>Coffee</li>
  <li>Tea</li>
  <li>Milk</li>
</ul>

Ordered lists use the <ol> tag; items are numbered by default:

<ol>
  <li>Coffee</li>
  <li>Tea</li>
  <li>Milk</li>
</ol>

Tip: Use tables for tabular data only - never for page layout. Modern layouts belong to CSS (see the CSS courses in this library).',
4, '2026-03-02T11:15:00', NULL, 0),


/* ===================== COURSE 2 - CSS Tutorial ===================== */
(5, 2, N'CSS Syntax and Selectors',
N'A CSS rule consists of a selector and a declaration block. The selector points to the HTML element you want to style; the declaration block contains one or more declarations separated by semicolons.

Example

p {
  color: red;
  text-align: center;
}

Try It Yourself >>

Example Explained

- p is the selector (all <p> elements)
- color: red is one property/value pair
- text-align: center is another property/value pair

Three Ways to Insert CSS

1. External stylesheet - <link rel="stylesheet" href="styles.css">
2. Internal stylesheet - a <style> block inside the <head>
3. Inline styles       - the style attribute on a single element

CSS Selectors

- Element selector: p { }        selects all <p> elements
- Id selector:      #intro { }   selects the element with id="intro"
- Class selector:   .center { }  selects all elements with class="center"
- Universal:        * { }        selects all elements
- Grouping:         h1, h2, p { } groups several selectors

Tip: External stylesheets are recommended - one file can control the look of an entire website.',
5, '2026-03-05T11:30:00', NULL, 0),

(6, 2, N'CSS Colors and Backgrounds',
N'Colors in CSS can be specified by name, RGB value, HEX value, HSL value or RGBA/HSLA values that include transparency.

Example

h1 {
  background-color: lightblue;
  color: #ff0000;
}

p {
  background-color: rgb(240, 248, 255);
  color: hsl(120, 60%, 70%);
}

Try It Yourself >>

Background Properties

body {
  background-color: #ffffff;
  background-image: url("paper.gif");
  background-repeat: no-repeat;
  background-position: right top;
  background-attachment: fixed;
}

- background-color    : solid fill color
- background-image    : image used as the background
- background-repeat   : repeat | repeat-x | repeat-y | no-repeat
- background-position : where the image sits
- background-size     : e.g. cover scales the image over the whole area

Tip: background-size: cover together with background-position: center is the classic recipe for hero banners.',
6, '2026-03-05T11:35:00', NULL, 0),

(7, 2, N'The CSS Box Model',
N'All HTML elements can be considered boxes. The CSS box model wraps around every element and consists of: margins, borders, padding and the actual content.

+---------------------------+
|         MARGIN            |
|  +---------------------+  |
|  |      BORDER         |  |
|  |  +---------------+  |  |
|  |  |   PADDING     |  |  |
|  |  |  +---------+  |  |  |
|  |  |  | CONTENT |  |  |  |
|  |  |  +---------+  |  |  |
|  |  +---------------+  |  |
|  +---------------------+  |
+---------------------------+

Explanation of the different parts:

- Content : the text and images of the box
- Padding : transparent area around the content, INSIDE the border
- Border  : a border that goes around the padding and content
- Margin  : transparent area OUTSIDE the border

Example

div {
  width: 300px;
  border: 15px solid green;
  padding: 50px;
  margin: 20px;
}

Try It Yourself >>

Important: By default, width and height set the size of the CONTENT box only - adding padding and border makes the visible box bigger. Use box-sizing: border-box so width includes padding and border. Most projects apply this globally.',
7, '2026-03-05T11:40:00', NULL, 0),

(8, 2, N'CSS Text Styling and Fonts',
N'CSS has many properties for formatting text: color, text-align, text-decoration, text-transform, letter-spacing, line-height and more.

Text Formatting Example

h1 {
  color: navy;
  text-align: center;
  text-transform: uppercase;
  letter-spacing: 3px;
}

p {
  line-height: 1.6;
}

a {
  text-decoration: underline;
}

Try It Yourself >>

Font Families

In CSS there are five generic font families: serif, sans-serif, monospace, cursive and fantasy. Sans-serif fonts are typically easier to read on screens.

p {
  font-family: "Segoe UI", Arial, sans-serif;
  font-size: 16px;
  font-weight: normal;
  font-style: italic;
}

Web-Safe Font Stacks

Always end a font-family declaration with a generic family so the browser can fall back gracefully:

- "Times New Roman", Times, serif
- Arial, Helvetica, sans-serif
- "Courier New", Courier, monospace

Note: font-size accepts px, em, rem or %. rem scales with the root element and is preferred for accessible, responsive typography.',
8, '2026-03-05T11:45:00', NULL, 0),


/* ===================== COURSE 3 - JavaScript Tutorial ===================== */
(9, 3, N'JavaScript Introduction and Output',
N'JavaScript is the programming language of the web. It can calculate, validate input, change HTML content and react to what the user does - all inside the browser.

Adding JavaScript to a Page

<script>
  // Your code goes here
</script>

Display Output - Four Ways

// 1. Into an HTML element
document.getElementById("demo").innerHTML = "Hello JavaScript";

// 2. Into the browser console
console.log("Hello console");

// 3. Into an alert box
window.alert("Hello alert");

// 4. Directly into the HTML output (testing only!)
document.write("Hello document");

Try It Yourself >>

Changing Content Dynamically

JavaScript accepts both double and single quotes:

document.getElementById("demo").innerHTML = ''Hello JavaScript'';

Note: document.write should only be used for testing - calling it after the page loads overwrites the whole document.',
9, '2026-03-09T14:00:00', NULL, 0),

(10, 3, N'JavaScript Variables and Data Types',
N'Variables are containers for storing values. Modern JavaScript gives you three keywords:

let   - block-scoped, value CAN change
const - block-scoped, value CANNOT be reassigned
var   - function-scoped (old style - avoid in new code)

Example

let price = 5;
const taxRate = 0.2;
let name = "John";
let active = true;

price = price + 10;
console.log(price);        // 15

Try It Yourself >>

JavaScript Data Types

String    let text = "Hello";
Number    let quantity = 42;
Boolean   let isActive = true;
Undefined let empty;
Null      let nothing = null;
Object    let person = { firstName: "John", lastName: "Doe" };
Array     let cars = ["Saab", "Volvo", "BMW"];

The typeof Operator

typeof "John"      // returns "string"
typeof 3.14        // returns "number"
typeof true        // returns "boolean"

Important: const person = { name: "John" }; does NOT make the object immutable - it only prevents reassignment of the variable itself. person.name = "Jane"; still works.',
10, '2026-03-09T14:05:00', NULL, 0),

(11, 3, N'JavaScript Functions and Events',
N'A JavaScript function is a block of code designed to perform a particular task. It is executed when something invokes it (calls it).

Defining and Calling a Function

function greet(name) {
  return "Hello, " + name + "!";
}

let message = greet("World");
console.log(message);   // Hello, World!

Try It Yourself >>

Function Syntax Explained

- function : the keyword that declares the function
- greet    : the function name
- (name)   : parameters (inputs)
- return   : the value sent back out of the function

Arrow Functions (ES6)

const square = (n) => n * n;
console.log(square(5));   // 25

Events - Code That Reacts

HTML events are things that happen to HTML elements, such as clicks or key presses. You can attach handlers directly in the markup:

<button onclick="alert(''Button clicked!'')">Click Me</button>

Common events: onclick, onchange, onmouseover, onmouseout, onkeydown, onload.

Tip: For larger apps prefer addEventListener() - it keeps JavaScript out of your HTML and allows multiple listeners on one element.',
11, '2026-03-09T14:10:00', NULL, 0),

(12, 3, N'JavaScript Arrays and Objects',
N'Arrays are ordered lists of values. Objects store collections of keyed properties. Together they model almost everything in JavaScript.

Arrays

const fruits = ["Apple", "Banana", "Orange"];

fruits.push("Mango");     // add to the end
fruits.pop();             // remove from the end
fruits.length;            // number of items
fruits[0];                // "Apple" (indexes start at 0)

Useful Array Methods

fruits.forEach(f => console.log(f));           // loop over items
const upper  = fruits.map(f => f.toUpperCase());
const citrus = fruits.filter(f => f.includes("a"));

Try It Yourself >>

Objects

const person = {
  firstName: "John",
  lastName:  "Doe",
  age: 50,
  fullName: function () {
    return this.firstName + " " + this.lastName;
  }
};

console.log(person.fullName());   // John Doe

Note: Arrays are actually special objects - typeof [] returns "object". Use Array.isArray(value) to test for arrays reliably.',
12, '2026-03-09T14:15:00', NULL, 0),

/* ===================== COURSE 4 - SQL Tutorial ===================== */
(13, 4, N'The SQL SELECT Statement',
N'SELECT statements are used to fetch data from a database. The data returned is stored in a result table called the result set.

Sample table: Customers

CustomerID | CustomerName        | City
-----------|---------------------|----------
1          | Alfreds Futterkiste | Berlin
2          | Ana Trujillo        | Mexico D.F.
3          | Antonio Moreno      | Mexico D.F.

Syntax

SELECT column1, column2, ...
FROM table_name;

SELECT * FROM table_name;    -- all columns

Examples

SELECT CustomerName, City FROM Customers;

SELECT * FROM Customers;

Try It Yourself >>

Select Only What You Need

SELECT DISTINCT returns only different (unique) values:

SELECT DISTINCT Country FROM Customers;

Tip: Avoid SELECT * in production code - listing explicit columns makes queries faster to read, safer against schema changes and easier for the database to optimize.',
13, '2026-03-14T10:20:00', NULL, 0),

(14, 4, N'The SQL WHERE Clause',
N'The WHERE clause filters records so you get only the rows that satisfy a condition. It is used not only in SELECT statements but also in UPDATE and DELETE.

Syntax

SELECT column1, column2, ...
FROM table_name
WHERE condition;

Operators You Can Use

=         equal
<> or !=  not equal
> and <   greater than / less than
BETWEEN   between an inclusive range
LIKE      search for a pattern
IN        specify multiple possible values

Examples

SELECT * FROM Customers
WHERE Country = ''Mexico'';

SELECT * FROM Customers
WHERE CustomerID > 80;

SELECT * FROM Customers
WHERE CustomerName LIKE ''A%'';

SELECT * FROM Customers
WHERE Country IN (''Germany'', ''France'', ''UK'');

Try It Yourself >>

Text vs Numeric Values

SQL requires single quotes around TEXT values, but no quotes around numeric values.

Tip: Always preview your condition inside a SELECT before using it with UPDATE or DELETE - an unconditional statement changes every row in the table!',
14, '2026-03-14T10:25:00', NULL, 0),

(15, 4, N'The SQL INSERT INTO Statement',
N'The INSERT INTO statement inserts new records into a table. There are two ways to write it.

1) Specify both column names and values:

INSERT INTO table_name (column1, column2, column3, ...)
VALUES (value1, value2, value3, ...);

2) Add values for ALL columns (order must match the table):

INSERT INTO table_name
VALUES (value1, value2, value3, ...);

Example

INSERT INTO Customers (CustomerName, ContactName, City, Country)
VALUES (''Cardinal'', ''Tom B. Erichsen'', ''Stavanger'', ''Norway'');

Try It Yourself >>

Insert Multiple Rows

INSERT INTO Customers (CustomerName, City, Country)
VALUES
  (''Chop-suey Chinese'', ''Bern'', ''Switzerland''),
  (''Wolski'', ''Warsaw'', ''Poland''),
  (''Rattlesnake Canyon Grocery'', ''Albuquerque'', ''USA'');

Insert Only Specific Columns

Columns you skip will be filled with NULL (or their DEFAULT value):

INSERT INTO Customers (CustomerName, Country)
VALUES (''Cardinal'', ''Norway'');

Note: If a column is NOT NULL and has no default, skipping it raises an error - always check the table definition first.',
15, '2026-03-14T10:30:00', NULL, 0),

(16, 4, N'SQL UPDATE and DELETE Statements',
N'UPDATE modifies existing rows; DELETE removes them. Both support a WHERE clause - and both deserve great respect!

UPDATE Syntax

UPDATE table_name
SET column1 = value1, column2 = value2, ...
WHERE condition;

Example: update one customer

UPDATE Customers
SET ContactName = ''Alfred Schmidt'', City = ''Frankfurt''
WHERE CustomerID = 1;

DELETE Syntax

DELETE FROM table_name
WHERE condition;

Example: delete one customer

DELETE FROM Customers
WHERE CustomerID = 1;

Delete ALL rows (keep the table structure):

DELETE FROM Customers;

Try It Yourself >>

Safety Checklist Before Running UPDATE / DELETE

1. Run the same WHERE condition inside a SELECT first and inspect the rows.
2. Remember: no WHERE means every row is affected!
3. Consider wrapping risky operations in a transaction:

BEGIN TRANSACTION;
UPDATE Customers SET City = ''Oslo'' WHERE Country = ''Norway'';
-- check results...
COMMIT;   -- or ROLLBACK;

Tip: In production systems most teams prefer soft deletes (an IsDeleted flag) so history is never lost - which is exactly how this platform archives courses.',
16, '2026-03-14T10:35:00', NULL, 0),


/* ===================== COURSE 5 - C# Tutorial ===================== */
(17, 5, N'C# Introduction and Your First Program',
N'C# is a modern, object-oriented programming language developed by Microsoft. It runs on the .NET platform and powers web apps, desktop apps, mobile apps, games and much more.

Why Learn C#?

- One of the most popular programming languages in the world
- Easy to learn and read, similar syntax to Java and C++
- Huge ecosystem: ASP.NET Core web APIs, Unity games, desktop WPF/WinForms
- Cross platform thanks to modern .NET

Your First Program

using System;

namespace HelloWorld
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Hello World!");
    }
  }
}

Try It Yourself >>

Example Explained

- using System        : lets us use classes from System, like Console
- namespace           : a container that groups your code
- class Program       : a class - C# code always lives inside classes
- static void Main    : the entry point where the program starts
- Console.WriteLine() : prints a line of text to the console

Note: In modern .NET you can also write the whole thing as one line using top-level statements:
Console.WriteLine("Hello World!");',
17, '2026-03-20T10:40:00', NULL, 0),

(18, 5, N'C# Variables and Data Types',
N'Variables are containers for storing data values. In C#, every variable has a specific type, and the compiler checks that you use it correctly.

Declaring Variables

int score = 100;
double price = 19.99;
char grade = ''A'';
string name = "Emily";
bool isActive = true;

const int MaxUsers = 500;   // cannot be changed later

Try It Yourself >>

Common Built-in Types

int     whole numbers            42
long    big whole numbers        9000000000L
double  decimal numbers          3.99
decimal precise money values      19.99m
char    single character         ''B''
string  text                     "Hello"
bool    true / false             true

Type Conversion

Implicit conversion happens automatically when it is safe:

int small = 9;
double bigger = small;      // automatic

Explicit conversion (a cast) is needed when information could be lost:

double pi = 3.14;
int rounded = (int)pi;      // 3

Strings to numbers:

string ageText = "25";
int age = Convert.ToInt32(ageText);
int age2 = int.Parse(ageText);

Important: If parsing can fail, prefer int.TryParse which returns false instead of throwing an exception.',
18, '2026-03-20T10:45:00', NULL, 0),

(19, 5, N'C# Conditionals: if, else and switch',
N'Conditional statements let your program make decisions and run different code depending on the situation.

if / else if / else

int temperature = 30;

if (temperature > 25)
{
  Console.WriteLine("It is hot.");
}
else if (temperature > 15)
{
  Console.WriteLine("It is warm.");
}
else
{
  Console.WriteLine("It is cold.");
}

Try It Yourself >>

Ternary Expression (short if)

string result = (temperature > 25) ? "Hot" : "Not hot";

switch Statement

int day = 4;
switch (day)
{
  case 1:
    Console.WriteLine("Monday");
    break;
  case 6:
  case 7:
    Console.WriteLine("Weekend!");
    break;
  default:
    Console.WriteLine("Looking forward to the Weekend");
    break;
}

Switch Expressions (C# 8+)

string label = day switch
{
  6 or 7 => "Weekend",
  < 6    => "Weekday",
  _      => "Unknown"
};

Note: Every case section must end with break (or another jump statement) unless the section is empty.',
19, '2026-03-20T10:50:00', NULL, 0),

(20, 5, N'C# Loops: for, while and foreach',
N'Loops execute a block of code as long as a specified condition is reached. They are handy when you want to run the same code repeatedly, each time with a different value.

The for Loop

for (int i = 0; i < 5; i++)
{
  Console.WriteLine("Iteration number: " + i);
}

- Statement 1 (int i = 0)       runs once before the loop starts
- Statement 2 (i < 5)           the condition checked before every iteration
- Statement 3 (i++)             runs after every iteration

The while Loop

int counter = 0;
while (counter < 5)
{
  Console.WriteLine(counter);
  counter++;
}

The do...while Loop runs at least ONCE:

int n = 100;
do
{
  Console.WriteLine(n);   // prints even though the condition is already false
} while (n < 10);

foreach - perfect for collections

string[] cars = { "Volvo", "BMW", "Ford" };
foreach (string car in cars)
{
  Console.WriteLine(car);
}

Try It Yourself >>

break exits the loop immediately; continue skips to the next iteration.

Tip: Use foreach whenever you just need every element - it is cleaner and less error-prone than manual index juggling.',
20, '2026-03-20T10:55:00', NULL, 0),

/* ===================== COURSE 6 - Python Tutorial ===================== */
(21, 6, N'Python Introduction and Syntax',
N'Python is a popular programming language created by Guido van Rossum and released in 1991. It is loved for its clean, readable syntax that almost looks like English.

What Can Python Do?

- Web development (Django, Flask) and API backends
- Automation scripts and task scheduling
- Data science, machine learning and AI
- Rapid prototyping and glue code between systems

Python Syntax Compared to Other Languages

Python was designed for readability and uses NEW LINES to complete a command instead of semicolons, and INDENTATION to define blocks instead of curly braces:

if 5 > 2:
  print("Five is greater than two!")

Comments start with a hash character:

# This is a comment
print("Hello, World!")   # inline comment works too

Multi-line strings double as multi-line comments:

"""
This program demonstrates
basic Python syntax.
"""

Try It Yourself >>

Execute Python

Save the code in a file named demo.py and run it from the command line:

python demo.py

Or experiment interactively in the Python REPL by typing python in your terminal.',
21, '2026-03-25T15:00:00', NULL, 0),

(22, 6, N'Python Variables and Data Types',
N'Variables are created the moment you first assign a value to them. No type declaration is needed - Python figures out the type automatically.

x = 5                  # int
y = "John"             # str
price = 19.99          # float
is_ready = True        # bool
nothing = None         # NoneType

You can specify the type explicitly with casting:

x = int(3)        # x will be 3
y = float(3)      # y will be 3.0
z = str(3)        # z will be ''3''

Getting the Type

print(type(x))      # <class ''int''>
print(type(y))      # <class ''str''>

String Basics

name = "Alice"
greeting = f"Hello, {name}!"     # f-strings format variables
print(greeting)                  # Hello, Alice!

Multiple Assignment and Unpacking

a, b, c = "red", "green", "blue"
first, *rest = [1, 2, 3, 4]      # first=1, rest=[2, 3, 4]

Global vs Local Scope

x = "awesome"

def myfunc():
  x = "fantastic"     # local variable
  print("Python is " + x)

myfunc()                       # fantastic
print("Python is " + x)        # awesome

Important: Variable names are case-sensitive (Age and age are different variables).',
22, '2026-03-25T15:05:00', NULL, 0),

(23, 6, N'Python Lists and Dictionaries',
N'Lists store ordered collections of items; dictionaries store key/value pairs. Together they cover most data-modeling needs of everyday scripts.

Lists

mylist = ["apple", "banana", "cherry"]

mylist.append("orange")      # add item
mylist.remove("banana")      # remove item
mylist[0]                    # "apple"
mylist[-1]                   # "orange" (negative index = from end)
len(mylist)                  # number of items

Slicing

numbers = [0, 1, 2, 3, 4, 5]
numbers[1:4]                 # [1, 2, 3]
numbers[:3]                  # [0, 1, 2]

List Comprehension - concise transformation

squares = [n * n for n in range(6)]
# [0, 1, 4, 9, 16, 25]

Dictionaries

thisdict = {
  "brand": "Ford",
  "model": "Mustang",
  "year": 1964
}

thisdict["color"] = "red"    # add or update a key
thisdict.get("model")        # safe access
thisdict.keys()              # dict_keys([...])
thisdict.values()            # dict_values([...])

Looping

for key, value in thisdict.items():
  print(key, "->", value)

Try It Yourself >>

Note: Dictionaries keep insertion order since Python 3.7, and keys must be immutable types such as strings or numbers.',
23, '2026-03-25T15:10:00', NULL, 0),

(24, 6, N'Python Functions',
N'A function is a block of code that only runs when it is called. You can pass data (parameters) into a function and it can return data back.

Defining and Calling

def greet(name):
  return "Hello, " + name

print(greet("Alice"))    # Hello, Alice

Default Parameter Values

def greet(name="World"):
  return f"Hello, {name}!"

print(greet())           # Hello, World!
print(greet("Bob"))      # Hello, Bob!

Arbitrary Arguments

def total(*numbers):          # receives a tuple
  return sum(numbers)

print(total(1, 2, 3))         # 6

Keyword Arguments

def child_info(name, age):
  print(f"{name} is {age}")

child_info(age=7, name="Emil")   # order does not matter

Return Multiple Values

def min_max(values):
  return min(values), max(values)

low, high = min_max([3, 8, 1, 9])

Recursion - a function calling itself

def factorial(n):
  if n <= 1:
    return 1
  return n * factorial(n - 1)

Try It Yourself >>

Tip: Keep functions short and focused on ONE job - descriptive names like calculate_invoice_total beat comments any day.',
24, '2026-03-25T15:15:00', NULL, 0),


/* ===================== COURSE 7 - React Tutorial ===================== */
(25, 7, N'React Getting Started',
N'React is a JavaScript library for building user interfaces. Instead of manipulating the DOM directly, you describe what the UI should look like for a given piece of data, and React updates the page efficiently when that data changes.

Create a React Application

The easiest way to start learning React is with a toolchain:

npx create-react-app my-app
cd my-app
npm start

This creates a project folder, starts a development server and opens http://localhost:3000.

Your First Component

Open src/App.js and replace its contents:

function App() {
  return (
    <div className="App">
      <h1>Hello React!</h1>
    </div>
  );
}

export default App;

Try It Yourself >>

What Just Happened?

- App is a plain JavaScript function
- It returns JSX - an HTML-like syntax (more in the next lesson)
- index.js renders <App /> into the page:

import ReactDOM from "react-dom/client";

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(<App />);

Note: React components must start with an uppercase letter. <app /> would not render your component.',
25, '2026-04-01T10:00:00', NULL, 0),

(26, 7, N'JSX Introduction',
N'JSX stands for JavaScript XML. It lets you write HTML-like markup inside JavaScript, and it gets compiled to regular function calls behind the scenes.

Rules of JSX

1. Return ONE root element (wrap siblings in a fragment <>...</>)
2. Use className instead of class
3. Curly braces { } embed any JavaScript expression
4. All tags must be closed (<img />, <br />, <input />)

Example

const name = "Emily";
const age = 21;

function Profile() {
  return (
    <>
      <h1 className="title">Hello {name}!</h1>
      <p>Next year you will be {age + 1}.</p>
      <img src="avatar.png" alt="Avatar" />
    </>
  );
}

Try It Yourself >>

Conditional Rendering

{age >= 18 ? <p>Adult content</p> : <p>Kid friendly</p>}

{isLoggedIn && <LogoutButton />}

Rendering Lists

const fruits = ["Apple", "Banana", "Orange"];

<ul>
  {fruits.map((fruit, index) => (
    <li key={index}>{fruit}</li>
  ))}
</ul>

Important: Always provide a stable key when rendering lists so React can track items efficiently between renders.',
26, '2026-04-01T10:05:00', NULL, 0),

(27, 7, N'React Components and Props',
N'Components are independent, reusable pieces of UI. Props (short for properties) pass data FROM parent TO child components - they are read-only.

Passing Props

Parent component:

function Parent() {
  return <Car brand="Ford" model="Mustang" />;
}

Child component receives them as one props object:

function Car(props) {
  return <h2>I am a {props.brand} {props.model}!</h2>;
}

Try It Yourself >>

Destructuring for Readability

function Car({ brand, model }) {
  return <h2>I am a {brand} {model}!</h2>;
}

Props Are Immutable

Never modify props inside a child component:

// WRONG:
props.brand = "BMW";   // props are read-only!

Children via Nested Content

Content placed between opening and closing tags arrives as props.children:

<Card>
  <h2>Card Title</h2>
  <p>Any markup can go here.</p>
</Card>

Note: Components can be defined as functions (modern style) or ES6 classes (older codebases). Prefer function components plus hooks for new code.',
27, '2026-04-01T10:10:00', NULL, 0),

(28, 7, N'React State and useState',
N'State lets components remember data between renders and re-render the UI whenever that data changes. The useState hook adds state to function components.

Counter Example

import { useState } from "react";

function Counter() {
  const [count, setCount] = useState(0);

  return (
    <>
      <p>Count: {count}</p>
      <button onClick={() => setCount(count + 1)}>
        Increment
      </button>
    </>
  );
}

Try It Yourself >>

Anatomy Explained

- useState(0)          declares state, initial value 0
- count                current value for this render
- setCount             updater function; call it to change state
- Changing state       triggers a re-render automatically

Updating Based on Previous State

Always use the callback form when the new value depends on the old one:

<button onClick={() => setCount(prev => prev + 5)}>+5</button>

State Is Per Component Instance

Two <Counter /> rendered side by side keep completely independent counts.

Tip: State changes should be immutable - create new arrays/objects instead of mutating:

setItems([...items, newItem]);   // correct
items.push(newItem);             // wrong - no re-render',
28, '2026-04-01T10:15:00', NULL, 0),

/* ===================== COURSE 8 - JavaScript DOM Manipulation ===================== */
(29, 8, N'What is the DOM?',
N'The Document Object Model (DOM) is a programming interface for web documents. When the browser loads a page it builds a tree of objects - every element, attribute and piece of text becomes a node your code can reach and change.

The Page as a Tree

document
â””â”€â”€ html
    â”œâ”€â”€ head
    â”‚   â””â”€â”€ title -> "My page"
    â””â”€â”€ body
        â”œâ”€â”€ h1 -> "Welcome!"
        â””â”€â”€ p -> "Some text"

Accessing the Entry Point

Everything starts from the global document object:

console.log(document.title);
console.log(document.body);

Changing Things Live

You can experiment right now - open DevTools (F12) and type:

document.body.style.background = "lightblue";
document.querySelector("h1").textContent = "Changed by JavaScript!";

Try It Yourself >>

Why the DOM Matters

- Form validation feedback without reloading
- Dynamic lists (todos, carts, search results)
- Single-page applications are built on top of DOM APIs

Note: Frameworks like React manipulate the DOM for you - but understanding what happens underneath makes you a dramatically better developer and debugger.',
29, '2026-04-10T11:30:00', NULL, 0),

(30, 8, N'Selecting Elements',
N'Before you can change an element you need to find it. Modern JavaScript gives you two powerful query methods on document (or any element).

querySelector - first match only

const heading = document.querySelector("h1");              // first <h1>
const byClass = document.querySelector(".menu-item");      // first .menu-item
const byId    = document.querySelector("#main-nav");       // #main-nav

querySelectorAll - ALL matches (a NodeList)

const buttons = document.querySelectorAll("button");

buttons.forEach(btn => console.log(btn.textContent));

Classic Methods Still Around

document.getElementById("demo");
document.getElementsByClassName("note");
document.getElementsByTagName("li");

Try It Yourself >>

Walking the Tree

element.parentElement;
element.children;
element.nextElementSibling;
element.previousElementSibling;

Checking and Filtering

const link = document.querySelector("a");

link.matches(".external");   // does it match this selector?
link.closest("nav");         // nearest ancestor matching selector

Tip: querySelector/querySelectorAll accept ANY valid CSS selector - including attribute selectors like input[type="email"] - which makes them the most flexible choice.',
30, '2026-04-10T11:35:00', NULL, 0),

(31, 8, N'Creating, Updating and Removing Elements',
N'Once you can find elements, the next step is changing them: update text, tweak styles, build new elements from scratch and remove what is no longer needed.

Reading and Writing Content

const el = document.querySelector("#status");

el.textContent;                 // get text (safe)
el.innerHTML;                   // get/set HTML (beware XSS!)
el.setAttribute("href", "/new");
el.classList.add("active");
el.classList.toggle("open");
el.style.backgroundColor = "#eee";

Creating New Elements

const li = document.createElement("li");
li.textContent = "New task";
li.className = "task";

document.querySelector("#todo-list").appendChild(li);

Insert With Precision

parent.prepend(node);            // first child
parent.append(node);             // last child
node2.before(node);              // before a specific node
node2.after(node);               // after a specific node
parent.insertAdjacentHTML("beforeend", "<li>Fast path</li>");

Removing Elements

const item = document.querySelector(".old");
item.remove();                   // modern way
// older browsers:
item.parentNode.removeChild(item);

Try It Yourself >>

A Tiny Practical Example

document.querySelectorAll(".task.done").forEach(task => {
  task.style.opacity = "0.5";
});

Security Note: Never inject untrusted user input with innerHTML - use textContent instead to avoid cross-site scripting (XSS).',
31, '2026-04-10T11:40:00', NULL, 0),


/* ===================== COURSE 9 - CSS Flexbox and Grid ===================== */
(32, 9, N'Flexbox Fundamentals',
N'Flexible Box Layout (Flexbox) makes it easy to align and distribute space among items in a container, even when their size is unknown or dynamic.

Enabling Flexbox

Flexbox is one-dimensional: items flow along a main axis (a row OR a column).

.flex-container {
  display: flex;
}

Main Properties on the Container

.flex-container {
  display: flex;
  flex-direction: row;            /* row | row-reverse | column | column-reverse */
  justify-content: center;        /* main axis alignment */
  align-items: stretch;           /* cross axis alignment */
  gap: 12px;                      /* spacing between items */
}

justify-content Values

- flex-start : packed toward the start
- center     : centered
- space-between : first/last at edges, even gaps between
- space-around / space-evenly : distribute leftover space

A Perfect Navbar

.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

Try It Yourself >>

Growing and Shrinking Items

.item {
  flex-grow: 1;    /* how much it stretches when there is extra space */
  flex-shrink: 1;  /* how much it shrinks when space runs out */
  flex-basis: 200px; /* starting size */
}

Shorthand: flex: 1 1 200px; or simply flex: 1 to make items share space equally.

Note: Flexbox replaced float hacks for most layouts. Use it whenever elements should sit along ONE line.',
32, '2026-04-18T11:45:00', NULL, 0),

(33, 9, N'Flex Alignment and Wrapping',
N'Real interfaces rarely fit everything on one line. Wrapping plus advanced alignment turn Flexbox into a workhorse for cards, tags and toolbars.

Wrapping to New Lines

By default flex items shrink instead of wrapping. Enable wrapping:

.flex-container {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
}

A Responsive Card Grid Without Media Queries

.card-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
}

.card {
  flex: 1 1 260px;   /* grow, shrink, ideal width */
}

Cards keep a comfortable size and reflow automatically as the viewport changes.

Try It Yourself >>

align-content vs align-items

- align-items   : aligns each item within its line
- align-content : distributes ALL lines when there are several

.flex-container {
  display: flex;
  flex-wrap: wrap;
  height: 300px;
  align-items: center;      /* items centered in their row */
  align-content: space-around;  /* rows distributed vertically */
}

Centering Anything - the Famous Trick

.parent {
  display: flex;
  justify-content: center;   /* horizontal */
  align-items: center;       /* vertical   */
  min-height: 100vh;
}

Ordering and Individual Overrides

.first-item { order: -1; }        /* move to front */
.special    { align-self: flex-end; }  /* override align-items */

Important: order only changes VISUAL position, not screen-reader order - use sparingly for accessibility.',
33, '2026-04-18T11:50:00', NULL, 0),

(34, 9, N'CSS Grid Layout',
N'CSS Grid Layout divides a page into major regions with rows AND columns - true two-dimensional layout.

Defining a Grid

.grid-container {
  display: grid;
  grid-template-columns: auto auto auto;  /* three columns */
  grid-template-rows: 80px auto;
  gap: 10px;
}

Fractional Units (fr)

The fr unit distributes free space:

.grid-container {
  display: grid;
  grid-template-columns: 1fr 2fr 1fr;   /* middle column twice as wide */
}

Repeat Notation

grid-template-columns: repeat(3, 1fr);

Placing Items

.item1 {
  grid-column-start: 1;
  grid-column-end: 3;      /* spans columns 1-2 */
}

/* shorthand: */
.item1 { grid-column: 1 / 3; }
.header { grid-row: 1; }

Named Areas - Whole Page Layouts

.page {
  display: grid;
  grid-template-areas:
    "header header header"
    "menu   main   main"
    "footer footer footer";
  grid-template-rows: 60px 1fr 40px;
  grid-template-columns: 200px 1fr 200px;
  height: 100vh;
}

.area-header { grid-area: header; }
.area-menu   { grid-area: menu; }
.area-main   { grid-area: main; }
.area-footer { grid-area: footer; }

Try It Yourself >>

Auto-Fitting Galleries

.gallery {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
}

Tip: Choose Grid for two-dimensional structure (page shells, galleries) and Flexbox for content flowing in one direction. They combine beautifully.',
34, '2026-04-18T11:55:00', NULL, 0),

/* ===================== COURSE 10 - SQL Joins ===================== */
(35, 10, N'INNER JOIN',
N'A JOIN clause combines rows from two or more tables based on a related column between them. INNER JOIN returns records that have matching values in BOTH tables.

Sample Tables

Customers                 Orders
CustomerID | Name         OrderID | CustomerID | Product
-----------|------        --------|-----------|-------
1          | Alfred       1       | 3         | Keyboard
2          | Ana          2       | 1         | Monitor
3          | Antonio      3       | 1         | Mouse

Syntax

SELECT column_name(s)
FROM table1
INNER JOIN table2
ON table1.column_name = table2.column_name;

Example

SELECT Orders.OrderID, Customers.CustomerName, Orders.Product
FROM Orders
INNER JOIN Customers ON Orders.CustomerID = Customers.CustomerID;

Result

OrderID | CustomerName | Product
--------|--------------|--------
1       | Antonio      | Keyboard
2       | Alfred       | Monitor
3       | Alfred       | Mouse

Notice: customer 2 (Ana) has no orders, so she does NOT appear in the result.

Joining Three Tables

SELECT o.OrderID, c.CustomerName, p.ProductName
FROM ((Orders o
INNER JOIN Customers c ON o.CustomerID = c.CustomerID)
INNER JOIN Products p ON o.ProductID = p.ProductID);

Try It Yourself >>

Table Aliases make joins readable - once you alias a table you must use the alias everywhere.

Note: JOIN alone means INNER JOIN; the word INNER is optional.',
35, '2026-04-26T13:30:00', NULL, 0),

(36, 10, N'LEFT JOIN, RIGHT JOIN and FULL OUTER JOIN',
N'Outer joins keep unmatched rows from one or both tables. Choosing the right one depends on which side must survive even without a match.

LEFT JOIN

Returns ALL rows from the left table, with NULLs where no match exists on the right:

SELECT Customers.CustomerName, Orders.OrderID
FROM Customers
LEFT JOIN Orders ON Customers.CustomerID = Orders.CustomerID;

Result: every customer appears; customers without orders get NULL OrderID.

Finding Customers WITHOUT Orders - the Classic Pattern

SELECT c.CustomerName
FROM Customers c
LEFT JOIN Orders o ON c.CustomerID = o.CustomerID
WHERE o.OrderID IS NULL;

RIGHT JOIN

Same idea mirrored - all rows from the right table:

SELECT Customers.CustomerName, Orders.OrderID
FROM Customers
RIGHT JOIN Orders ON Customers.CustomerID = Orders.CustomerID;

FULL OUTER JOIN

All rows from BOTH sides; missing matches filled with NULL:

SELECT Customers.CustomerName, Orders.OrderID
FROM Customers
FULL OUTER JOIN Orders ON Customers.CustomerID = Orders.CustomerID;

Try It Yourself >>

Cheat Sheet

- INNER JOIN     : only matching rows
- LEFT JOIN      : all left + matched right
- RIGHT JOIN     : all right + matched left
- FULL OUTER JOIN: everything from both sides

Tip: In practice LEFT JOIN covers 90% of outer-join needs because we usually think "all customers... even those without orders". RIGHT JOIN reads awkwardly and is often rewritten as a LEFT JOIN with the tables swapped.',
36, '2026-04-26T13:35:00', NULL, 0),

(37, 10, N'Self Joins, UNION and Set Operators',
N'Tables can join to themselves, and separate result sets can be combined with set operators.

SELF JOIN - joining a table with itself

Classic example: an Employees table where ManagerID points at another employee:

SELECT e1.Name AS Employee, e2.Name AS Manager
FROM Employees e1
JOIN Employees e2 ON e1.ManagerID = e2.EmployeeID;

Result shows each employee next to their manager - both come from the SAME physical table, just under two aliases.

UNION vs UNION ALL

Both stack results of two queries vertically. Rules: same number of columns, compatible types.

SELECT City FROM Customers
UNION
SELECT City FROM Suppliers;        -- duplicates removed

SELECT City FROM Customers
UNION ALL
SELECT City FROM Suppliers;        -- keeps duplicates (faster)

INTERSECT and EXCEPT

SELECT City FROM Customers
INTERSECT                          -- cities present in both
SELECT City FROM Suppliers;

SELECT City FROM Customers
EXCEPT                             -- cities only customers have
SELECT City FROM Suppliers;

Try It Yourself >>

Performance Note: UNION performs an implicit DISTINCT sort. When you genuinely want every row, prefer UNION ALL - it skips that expensive step.',
37, '2026-04-26T13:40:00', NULL, 0),


/* ===================== COURSE 11 - C# Object-Oriented Programming ===================== */
(38, 11, N'Classes and Objects',
N'Classes and objects are the two main aspects of object-oriented programming. A class is a template/blueprint; an object is an instance created from that template.

A Class Contains

- Fields      : variables that hold state
- Methods     : functions that define behavior
- Properties  : controlled access to state (next lesson)

Defining and Using a Class

class Car
{
  public string brand;         // field
  public string color;
  public int    maxSpeed;

  public void FullThrottle()   // method
  {
    Console.WriteLine("The " + brand + " is going fast!");
  }
}

Creating Objects

Car myCar = new Car();        // instantiate with new
myCar.brand = "Ford";
myCar.color = "red";
myCar.maxSpeed = 200;
myCar.FullThrottle();

Try It Yourself >>

Multiple Objects - Independent State

Car car1 = new Car();
car1.brand = "BMW";
Car car2 = new Car();
car2.brand = "Tesla";

// car1 and car2 each hold their OWN copy of the fields

Note: The public keyword means the member can be reached from outside the class. Fields are usually kept private instead, exposing them through properties.',
38, '2026-05-05T09:20:00', NULL, 0),

(39, 11, N'Constructors and Properties',
N'A constructor is a special method that runs automatically when an object is created. It has the same name as the class and no return type.

Constructor Basics

class Car
{
  public string brand;

  public Car(string brandName)     // constructor
  {
    brand = brandName;
    Console.WriteLine("A " + brand + " was created!");
  }
}

Car myCar = new Car("Ford");       // constructor runs here

Try It Yourself >>

Default and Overloaded Constructors

class Point
{
  public int X, Y;

  public Point()                : this(0, 0) { }   // chains to the next one
  public Point(int x, int y)    { X = x; Y = y; }
}

Properties - Smart Fields

Properties encapsulate data with get/set accessors:

class Person
{
  private string name;                 // backing field

  public string Name
  {
    get { return name; }
    set { name = value.Trim(); }       // validate on the way in
  }

  // Auto-implemented property (compiler creates the hidden field):
  public int Age { get; set; }

  // Read-only computed property:
  public string Greeting => "Hi, I am " + Name;
}

var p = new Person { Name = "  Anna ", Age = 30 };
Console.WriteLine(p.Name);             // "Anna" - trimmed!

Important: Properties let you start simple and add validation or logging later WITHOUT breaking callers - unlike public fields.',
39, '2026-05-05T09:25:00', NULL, 0),

(40, 11, N'Inheritance and Polymorphism',
N'Inheritance lets a class derive members from another class. Polymorphism lets those derived classes provide their own behavior while sharing a common interface.

Inheritance - the : Symbol

class Animal                      // base class
{
  public virtual void MakeSound()
  {
    Console.WriteLine("Some generic sound");
  }
}

class Pig : Animal                // derived class
{
  public override void MakeSound()
  {
    Console.WriteLine("Oink oink");
  }
}

class Dog : Animal
{
  public override void MakeSound()
  {
    Console.WriteLine("Woof woof");
  }
}

Polymorphism in Action

Animal[] animals = { new Pig(), new Dog(), new Animal() };

foreach (Animal a in animals)
{
  a.MakeSound();     // calls the RIGHT override at runtime
}

Output:
Oink oink
Woof woof
Some generic sound

Try It Yourself >>

Keywords Cheat Sheet

- virtual    : marks a base method as overridable
- override   : replaces the base implementation
- abstract   : method/class WITHOUT implementation; must be overridden
- sealed     : blocks further inheritance
- base.Member: call the parent version explicitly

Note: C# supports single class inheritance (one base class) but a class can implement MANY interfaces - e.g. class Repository : IRepository, IDisposable.',
40, '2026-05-05T09:30:00', NULL, 0),

/* ===================== COURSE 12 - Python Data Structures (Legacy, archived) ===================== */
(41, 12, N'Python Tuples and Sets',
N'Tuples are ordered collections that CANNOT be changed after creation. Sets are unordered collections of UNIQUE elements.

Tuples - Immutable Sequences

thistuple = ("apple", "banana", "cherry")

thistuple[0]          # "apple"
len(thistuple)        # 3
# thistuple[0] = "kiwi"   -> TypeError! tuples are immutable

One-element tuples need a trailing comma:

single = ("apple",)

Tuple Unpacking

fruits = ("apple", "banana", "cherry")
green, yellow, red = fruits

Sets - Unique Items Only

myset = {"apple", "banana", "cherry"}
myset.add("orange")
myset.discard("banana")
"apple" in myset            # True - fast membership test

Duplicates vanish automatically:

{1, 2, 2, 3}                # {1, 2, 3}

Set Algebra

a = {1, 2, 3}
b = {3, 4, 5}

a | b        # union            {1, 2, 3, 4, 5}
a & b        # intersection     {3}
a - b        # difference       {1, 2}

Try It Yourself >>

Tip: Reach for a tuple when the shape of the data is fixed (coordinates, RGB colors) and for a set when you need membership tests without duplicates.',
41, '2026-05-12T16:00:00', '2026-05-20T15:30:00', 1),

(42, 12, N'Stacks and Queues in Python',
N'Stacks and queues are abstract data types that control HOW items are added and removed. Python lists make both easy to build.

Stack - Last In, First Out (LIFO)

Think of a pile of plates: the last plate placed is the first one taken.

stack = []

stack.append("a")     # push
stack.append("b")
stack.append("c")

stack.pop()           # returns "c"
stack[-1]             # peek -> "b"
len(stack)            # size

Classic uses: undo/redo history, browser back button, expression evaluation, depth-first search.

Queue - First In, First Out (FIFO)

Think of a checkout line: first person in line is served first.

from collections import deque

queue = deque()

queue.append("first")    # enqueue at the end
queue.append("second")
queue.popleft()          # dequeue from the front -> "first"

Important: list.pop(0) works but is O(n) because every element shifts. deque gives O(1) operations at BOTH ends - always prefer it for queues.

Priority Queue - serve by importance, not order

import heapq

heap = []
heapq.heappush(heap, (2, "write code"))
heapq.heappush(heap, (1, "fix bug"))
heapq.heappop(heap)      # (1, "fix bug") - lowest value first

Try It Yourself >>

Note: Choosing the right structure is about the OPERATIONS you need most - random access favors lists, uniqueness favors sets, ordering guarantees favor stacks/queues/heaps.',
42, '2026-05-12T16:05:00', '2026-05-20T15:30:00', 1);
SET IDENTITY_INSERT dbo.Lessons OFF;


/*------------------------------ QUIZZES
  Business rule: exactly ONE final quiz per course. QuizId mirrors CourseId 1:1.
  Course 12 is archived, so its quiz carries DeleteFlag = 1 as well.
*/
SET IDENTITY_INSERT dbo.Quizzes ON;
INSERT INTO dbo.Quizzes (QuizId, CourseId, Title, PassingScore, CreatedAt, UpdatedAt, DeleteFlag) VALUES
 (1,  1,  N'HTML Fundamentals Final Quiz',        70, '2026-03-04T15:30:00', NULL,                  0),
 (2,  2,  N'CSS Fundamentals Final Quiz',         65, '2026-03-07T09:00:00', NULL,                  0),
 (3,  3,  N'JavaScript Fundamentals Final Quiz',  60, '2026-03-11T10:40:00', NULL,                  0),
 (4,  4,  N'SQL Fundamentals Final Quiz',         65, '2026-03-16T08:30:00', NULL,                  0),
 (5,  5,  N'C# Fundamentals Final Quiz',          60, '2026-03-22T11:20:00', NULL,                  0),
 (6,  6,  N'Python Fundamentals Final Quiz',      60, '2026-03-27T16:05:00', NULL,                  0),
 (7,  7,  N'React Fundamentals Final Quiz',       70, '2026-04-03T09:45:00', NULL,                  0),
 (8,  8,  N'DOM Manipulation Final Quiz',         65, '2026-04-12T10:30:00', NULL,                  0),
 (9,  9,  N'Modern CSS Layout Final Quiz',        60, '2026-04-20T14:10:00', NULL,                  0),
 (10, 10, N'Advanced SQL Final Quiz',             65, '2026-04-28T09:25:00', NULL,                  0),
 (11, 11, N'OOP in C# Final Quiz',                60, '2026-05-07T10:50:00', NULL,                  0),
 (12, 12, N'Python Data Structures Final Quiz',   60, '2026-05-14T11:35:00', '2026-05-20T15:30:00', 1);
SET IDENTITY_INSERT dbo.Quizzes OFF;

/*------------------------------ QUESTIONS (40 questions across 12 quizzes) */
SET IDENTITY_INSERT dbo.Questions ON;
INSERT INTO dbo.Questions (QuestionId, QuizId, QuestionText, DisplayOrder) VALUES
 /* Quiz 1 - HTML */
 (1,  1,  N'Which HTML tag defines the largest heading?', 1),
 (2,  1,  N'Which attribute specifies a unique identifier for an HTML element?', 2),
 (3,  1,  N'Which tag creates a hyperlink to another page?', 3),
 (4,  1,  N'Which tag defines a row in an HTML table?', 4),
 /* Quiz 2 - CSS */
 (5,  2,  N'Which of the following is the correct CSS syntax?', 1),
 (6,  2,  N'Which CSS property changes the text color of an element?', 2),
 (7,  2,  N'Which property adds space INSIDE the border of a box?', 3),
 (8,  2,  N'Which selector targets elements with class="intro"?', 4),
 /* Quiz 3 - JavaScript */
 (9,  3,  N'Which keyword declares a block-scoped variable that can be reassigned?', 1),
 (10, 3,  N'Which method writes output into the browser console?', 2),
 (11, 3,  N'What is the correct way to CALL a function named myFunction?', 3),
 (12, 3,  N'Which array method adds a new item to the END of an array?', 4),
 /* Quiz 4 - SQL */
 (13, 4,  N'Which SQL statement is used to extract data from a database?', 1),
 (14, 4,  N'Which clause filters rows returned by a SELECT statement?', 2),
 (15, 4,  N'Which keyword sorts the result set of a query?', 3),
 (16, 4,  N'Which statement removes rows from a table but keeps the table structure?', 4),
 /* Quiz 5 - C# */
 (17, 5,  N'Which method prints text to the console in C#?', 1),
 (18, 5,  N'Which C# type stores a true or false value?', 2),
 (19, 5,  N'What is the correct single-line comment syntax in C#?', 3),
 /* Quiz 6 - Python */
 (20, 6,  N'What is the standard file extension for Python files?', 1),
 (21, 6,  N'Which built-in function displays output in Python?', 2),
 (22, 6,  N'Which collection is mutable, ordered and allows duplicate items?', 3),
 /* Quiz 7 - React */
 (23, 7,  N'Which command creates a new React application?', 1),
 (24, 7,  N'What does JSX stand for?', 2),
 (25, 7,  N'How do you pass data from a parent component to a child component?', 3),
 /* Quiz 8 - DOM */
 (26, 8,  N'Which method returns the FIRST element matching a CSS selector?', 1),
 (27, 8,  N'Which property sets the text content of an element safely?', 2),
 (28, 8,  N'Which event fires when the user clicks on an element?', 3),
 /* Quiz 9 - Flexbox & Grid */
 (29, 9,  N'Which CSS declaration enables flexbox on a container?', 1),
 (30, 9,  N'Which property aligns flex items along the MAIN axis?', 2),
 (31, 9,  N'In CSS Grid, which property defines the columns of the grid?', 3),
 /* Quiz 10 - Advanced SQL */
 (32, 10, N'Which join returns ONLY rows with matching values in both tables?', 1),
 (33, 10, N'Which join returns ALL rows from the left table plus matches from the right?', 2),
 (34, 10, N'Which operator combines two result sets and removes duplicate rows?', 3),
 /* Quiz 11 - OOP C# */
 (35, 11, N'Which keyword is used to define a class in C#?', 1),
 (36, 11, N'Which special method runs automatically when an object is created?', 2),
 (37, 11, N'Which symbol separates a derived class from its base class in C#?', 3),
 /* Quiz 12 - Python Data Structures */
 (38, 12, N'Which Python collection is ordered but immutable?', 1),
 (39, 12, N'What does a Python set guarantee about its elements?', 2),
 (40, 12, N'Which standard module provides deque for efficient queues?', 3);
SET IDENTITY_INSERT dbo.Questions OFF;

/*------------------------------ ANSWER OPTIONS (4 per question, exactly one correct) */
SET IDENTITY_INSERT dbo.QuestionOptions ON;
INSERT INTO dbo.QuestionOptions (OptionId, QuestionId, OptionText, IsCorrect) VALUES
 /* Q1 - correct: <h1> (option 2) */
 (1,  1,  N'<h6>', 0), (2, 1, N'<h1>', 1), (3, 1, N'<heading>', 0), (4, 1, N'<head>', 0),
 /* Q2 - correct: id (option 1) */
 (5,  2,  N'id', 1), (6, 2, N'class', 0), (7, 2, N'name', 0), (8, 2, N'ref', 0),
 /* Q3 - correct: <a> (option 3) */
 (9,  3,  N'<link>', 0), (10, 3, N'<href>', 0), (11, 3, N'<a>', 1), (12, 3, N'<anchor>', 0),
 /* Q4 - correct: <tr> (option 4) */
 (13, 4,  N'<td>', 0), (14, 4, N'<th>', 0), (15, 4, N'<table>', 0), (16, 4, N'<tr>', 1),
 /* Q5 - correct option 1 */
 (17, 5,  N'body {color: black;}', 1), (18, 5, N'body:color=black;', 0),
 (19, 5,  N'{body;color:black;}', 0), (20, 5, N'body{color=black;}', 0),
 /* Q6 - correct: color (option 2) */
 (21, 6,  N'background-color', 0), (22, 6, N'color', 1), (23, 6, N'font-color', 0), (24, 6, N'text-style', 0),
 /* Q7 - correct: padding (option 4) */
 (25, 7,  N'margin', 0), (26, 7, N'outline', 0), (27, 7, N'gap', 0), (28, 7, N'padding', 1),
 /* Q8 - correct: .intro (option 3) */
 (29, 8,  N'#intro', 0), (30, 8, N'intro', 0), (31, 8, N'.intro', 1), (32, 8, N'*', 0),
 /* Q9 - correct: let (option 2) */
 (33, 9,  N'var', 0), (34, 9, N'let', 1), (35, 9, N'function', 0), (36, 9, N'def', 0),
 /* Q10 - correct: console.log() (option 4) */
 (37, 10, N'document.write()', 0), (38, 10, N'print()', 0), (39, 10, N'alert()', 0), (40, 10, N'console.log()', 1),
 /* Q11 - correct option 1 */
 (41, 11, N'myFunction();', 1), (42, 11, N'call myFunction();', 0),
 (43, 11, N'call function myFunction();', 0), (44, 11, N'function myFunction();', 0),
 /* Q12 - correct: push() (option 3) */
 (45, 12, N'shift()', 0), (46, 12, N'unshift()', 0), (47, 12, N'push()', 1), (48, 12, N'pop()', 0),
 /* Q13 - correct: SELECT (option 3) */
 (49, 13, N'UPDATE', 0), (50, 13, N'EXTRACT', 0), (51, 13, N'SELECT', 1), (52, 13, N'GET', 0),
 /* Q14 - correct: WHERE (option 1) */
 (53, 14, N'WHERE', 1), (54, 14, N'HAVING', 0), (55, 14, N'FILTER', 0), (56, 14, N'GROUP BY', 0),
 /* Q15 - correct: ORDER BY (option 2) */
 (57, 15, N'GROUP BY', 0), (58, 15, N'ORDER BY', 1), (59, 15, N'SORT BY', 0), (60, 15, N'ALIGN BY', 0),
 /* Q16 - correct: DELETE (option 4) */
 (61, 16, N'REMOVE', 0), (62, 16, N'DROP TABLE', 0), (63, 16, N'CLEAR TABLE', 0), (64, 16, N'DELETE', 1),
 /* Q17 - correct: Console.WriteLine() (option 2) */
 (65, 17, N'echo()', 0), (66, 17, N'Console.WriteLine()', 1), (67, 17, N'print()', 0), (68, 17, N'System.out.println()', 0),
 /* Q18 - correct: bool (option 3) */
 (69, 18, N'int', 0), (70, 18, N'string', 0), (71, 18, N'bool', 1), (72, 18, N'char', 0),
 /* Q19 - correct option 1 */
 (73, 19, N'// comment', 1), (74, 19, N'<!-- comment -->', 0), (75, 19, N'# comment', 0), (76, 19, N'* comment *', 0),
 /* Q20 - correct: .py (option 4) */
 (77, 20, N'.pyt', 0), (78, 20, N'.pt', 0), (79, 20, N'.pn', 0), (80, 20, N'.py', 1),
 /* Q21 - correct: print() (option 1) */
 (81, 21, N'print()', 1), (82, 21, N'echo()', 0), (83, 21, N'Console.WriteLine()', 0), (84, 21, N'printf()', 0),
 /* Q22 - correct: list (option 2) */
 (85, 22, N'set', 0), (86, 22, N'list', 1), (87, 22, N'dict', 0), (88, 22, N'tuple', 0),
 /* Q23 - correct option 3 */
 (89, 23, N'npm install react', 0), (90, 23, N'react create app', 0),
 (91, 23, N'npx create-react-app my-app', 1), (92, 23, N'node start react', 0),
 /* Q24 - correct: JavaScript XML (option 2) */
 (93, 24, N'Java Syntax Extension', 0), (94, 24, N'JavaScript XML', 1),
 (95, 24, N'JavaScript Executable', 0), (96, 24, N'JSON Syntax Exchange', 0),
 /* Q25 - correct: Props (option 1) */
 (97, 25, N'Props', 1), (98, 25, N'State', 0), (99, 25, N'useEffect', 0), (100, 25, N'Refs', 0),
 /* Q26 - correct: querySelector() (option 4) */
 (101, 26, N'getElementById()', 0), (102, 26, N'getElementsByClassName()', 0),
 (103, 26, N'querySelectorAll()', 0), (104, 26, N'querySelector()', 1),
 /* Q27 - correct: textContent (option 1) */
 (105, 27, N'textContent', 1), (106, 27, N'innerTextValue', 0), (107, 27, N'value', 0), (108, 27, N'setText()', 0),
 /* Q28 - correct: onclick (option 3) */
 (109, 28, N'onchange', 0), (110, 28, N'onmouseover', 0), (111, 28, N'onclick', 1), (112, 28, N'onload', 0),
 /* Q29 - correct: display:flex (option 2) */
 (113, 29, N'flex-box: true', 0), (114, 29, N'display: flex', 1), (115, 29, N'float: left', 0), (116, 29, N'position: flex', 0),
 /* Q30 - correct: justify-content (option 4) */
 (117, 30, N'align-items', 0), (118, 30, N'place-content', 0), (119, 30, N'cross-align', 0), (120, 30, N'justify-content', 1),
 /* Q31 - correct: grid-template-columns (option 1) */
 (121, 31, N'grid-template-columns', 1), (122, 31, N'grid-columns', 0), (123, 31, N'column-count', 0), (124, 31, N'grid-auto-flow', 0),
 /* Q32 - correct: INNER JOIN (option 3) */
 (125, 32, N'LEFT JOIN', 0), (126, 32, N'FULL OUTER JOIN', 0), (127, 32, N'INNER JOIN', 1), (128, 32, N'CROSS JOIN', 0),
 /* Q33 - correct: LEFT JOIN (option 2) */
 (129, 33, N'INNER JOIN', 0), (130, 33, N'LEFT JOIN', 1), (131, 33, N'RIGHT JOIN', 0), (132, 33, N'SELF JOIN', 0),
 /* Q34 - correct: UNION (option 4) */
 (133, 34, N'JOIN ALL', 0), (134, 34, N'MERGE', 0), (135, 34, N'UNION ALL', 0), (136, 34, N'UNION', 1),
 /* Q35 - correct: class (option 1) */
 (137, 35, N'class', 1), (138, 35, N'object', 0), (139, 35, N'new', 0), (140, 35, N'type', 0),
 /* Q36 - correct: Constructor (option 3) */
 (141, 36, N'Main()', 0), (142, 36, N'Destructor', 0), (143, 36, N'Constructor', 1), (144, 36, N'Factory', 0),
 /* Q37 - correct: colon (option 2) */
 (145, 37, N'extends', 0), (146, 37, N': (colon)', 1), (147, 37, N'implements', 0), (148, 37, N'inherits', 0),
 /* Q38 - correct: tuple (option 4) */
 (149, 38, N'list', 0), (150, 38, N'dict', 0), (151, 38, N'set', 0), (152, 38, N'tuple', 1),
 /* Q39 - correct: unique elements (option 2) */
 (153, 39, N'Keeps insertion order and duplicates', 0), (154, 39, N'Stores unique elements', 1),
 (155, 39, N'Stores key-value pairs', 0), (156, 39, N'Automatically sorts all items', 0),
 /* Q40 - correct: collections (option 1) */
 (157, 40, N'collections', 1), (158, 40, N'threading', 0), (159, 40, N'itertools', 0), (160, 40, N'array', 0);
SET IDENTITY_INSERT dbo.QuestionOptions OFF;


/*------------------------------ ENROLLMENTS PART 1 (24: 10 completed / 10 in-progress / 4 not-started)
  Completed enrollments pair 1:1 with the quiz attempts seeded further below.
*/
SET IDENTITY_INSERT dbo.Enrollments ON;
INSERT INTO dbo.Enrollments (EnrollmentId, UserId, CourseId, EnrollDate, Completed, CompletedDate) VALUES
 /* Completed */
 (1,  5,  1, '2026-03-02T10:00:00', 1, '2026-03-05T16:30:00'),
 (2,  6,  3, '2026-03-08T09:15:00', 1, '2026-03-12T15:40:00'),
 (3,  7,  6, '2026-03-24T13:20:00', 1, '2026-03-29T19:10:00'),
 (4,  8,  1, '2026-03-03T14:00:00', 1, '2026-03-06T19:25:00'),
 (5,  9,  2, '2026-03-06T09:40:00', 1, '2026-03-09T20:15:00'),
 (6,  10, 6, '2026-03-27T10:05:00', 1, '2026-04-02T18:05:00'),
 (7,  11, 7, '2026-04-05T09:30:00', 1, '2026-04-08T21:10:00'),
 (8,  12, 1, '2026-03-04T13:45:00', 1, '2026-03-08T15:20:00'),
 (9,  8,  6, '2026-03-28T08:50:00', 1, '2026-04-03T16:40:00'),
 (10, 10, 4, '2026-03-17T09:00:00', 1, '2026-04-15T14:30:00'),
 /* In progress */
 (11, 9,  3, '2026-03-15T10:30:00', 0, NULL),
 (12, 12, 4, '2026-03-19T15:10:00', 0, NULL),
 (13, 5,  2, '2026-03-20T11:30:00', 0, NULL),
 (14, 6,  4, '2026-03-22T08:25:00', 0, NULL),
 (15, 7,  5, '2026-04-10T10:15:00', 0, NULL),
 (16, 9,  6, '2026-04-06T14:35:00', 0, NULL),
 (17, 11, 3, '2026-04-12T11:05:00', 0, NULL),
 (18, 12, 2, '2026-04-14T09:50:00', 0, NULL),
 (19, 5,  6, '2026-04-20T13:40:00', 0, NULL),
 (20, 6,  1, '2026-04-22T10:20:00', 0, NULL),
 /* Not started - dated into Jun/Jul so the dashboard growth chart has no dead months */
 (21, 10, 5, '2026-06-11T09:20:00', 0, NULL),
 (22, 11, 1, '2026-06-24T15:40:00', 0, NULL),
 (23, 9,  4, '2026-07-08T10:25:00', 0, NULL),
 (24, 12, 6, '2026-07-15T13:55:00', 0, NULL);
SET IDENTITY_INSERT dbo.Enrollments OFF;

/*------------------------------ ENROLLMENTS PART 2 (45 more for chart history)
  Growth curve across Feb-Aug 2026 (3/17/9/6/7/11/16 per month incl. part 1).
  Recent admin/student test enrollments spread over the last ~4 weeks instead of
  clustering on one day. All unique (UserId, CourseId), active courses only,
  Completed = 0 (the app sets it when a final quiz is passed).
*/
SET IDENTITY_INSERT dbo.Enrollments ON;
INSERT INTO dbo.Enrollments (EnrollmentId, UserId, CourseId, EnrollDate, Completed, CompletedDate) VALUES
 /* Staff/self-testing enrollments, un-bunched across recent weeks */
 (25, 2,  9, '2026-08-20T10:15:00', 0, NULL),
 (26, 5,  5, '2026-08-12T09:40:00', 0, NULL),
 (27, 5,  4, '2026-08-05T14:20:00', 0, NULL),
 (28, 5,  7, '2026-08-17T11:05:00', 0, NULL),
 (29, 5, 11, '2026-07-30T16:35:00', 0, NULL),
 (30, 5,  9, '2026-08-21T10:50:00', 0, NULL),
 (31, 5,  3, '2026-08-14T15:25:00', 0, NULL),
 (32, 6, 11, '2026-07-27T09:55:00', 0, NULL),
 (33, 6, 10, '2026-08-10T13:40:00', 0, NULL),
 (34, 6,  9, '2026-08-19T16:10:00', 0, NULL),
 (35, 6,  7, '2026-08-06T10:30:00', 0, NULL),
 (36, 6,  6, '2026-07-24T14:45:00', 0, NULL),
 /* February 2026 - first adopters */
 (37, 7,  1, '2026-02-09T09:20:00', 0, NULL),
 (38, 9,  1, '2026-02-16T14:05:00', 0, NULL),
 (39, 10, 1, '2026-02-23T10:40:00', 0, NULL),
 /* March 2026 */
 (40, 11, 2, '2026-03-06T11:15:00', 0, NULL),
 (41, 12, 3, '2026-03-13T15:50:00', 0, NULL),
 (42, 8,  2, '2026-03-19T09:35:00', 0, NULL),
 (43, 7,  2, '2026-03-26T13:20:00', 0, NULL),
 /* April 2026 */
 (44, 9,  5, '2026-04-03T10:10:00', 0, NULL),
 (45, 10, 2, '2026-04-09T16:25:00', 0, NULL),
 /* May 2026 */
 (46, 11, 4, '2026-05-05T09:45:00', 0, NULL),
 (47, 12, 5, '2026-05-12T14:30:00', 0, NULL),
 (48, 7,  3, '2026-05-01T11:55:00', 0, NULL),
 (49, 8,  3, '2026-05-08T10:20:00', 0, NULL),
 (50, 9,  7, '2026-05-15T15:40:00', 0, NULL),
 (51, 10, 3, '2026-05-22T09:05:00', 0, NULL),
 /* June 2026 */
 (52, 11, 5, '2026-06-02T13:45:00', 0, NULL),
 (53, 12, 7, '2026-06-08T10:30:00', 0, NULL),
 (54, 7,  4, '2026-06-15T14:15:00', 0, NULL),
 (55, 8,  4, '2026-06-19T09:50:00', 0, NULL),
 (56, 9,  8, '2026-06-26T16:00:00', 0, NULL),
 /* July 2026 */
 (57, 10, 7, '2026-07-01T11:25:00', 0, NULL),
 (58, 11, 6, '2026-07-06T15:10:00', 0, NULL),
 (59, 12, 8, '2026-07-10T09:40:00', 0, NULL),
 (60, 7,  7, '2026-07-16T13:05:00', 0, NULL),
 (61, 8,  5, '2026-07-22T10:50:00', 0, NULL),
 (62, 9,  9, '2026-07-28T14:35:00', 0, NULL),
 /* August 2026 - recent momentum incl. this week */
 (63, 10, 8,  '2026-08-23T08:50:00', 0, NULL),
 (64, 11, 8,  '2026-08-07T15:45:00', 0, NULL),
 (65, 12, 9,  '2026-08-11T09:15:00', 0, NULL),
 (66, 7,  8,  '2026-08-14T13:30:00', 0, NULL),
 (67, 8,  7,  '2026-08-18T10:05:00', 0, NULL),
 (68, 9,  10, '2026-08-21T14:50:00', 0, NULL),
 (69, 12, 10, '2026-08-22T09:30:00', 0, NULL);
SET IDENTITY_INSERT dbo.Enrollments OFF;

/*------------------------------ LESSON PROGRESS (57)
  Rule honored: an enrollment may only be Completed / have an attempt AFTER
  ALL of its course lessons are marked complete (timestamps prove ordering).
*/
SET IDENTITY_INSERT dbo.LessonProgress ON;
INSERT INTO dbo.LessonProgress (LessonProgressId, EnrollmentId, LessonId, Completed, CompletedDate) VALUES
 /* E1 - Emily, HTML course (lessons 1-4) */
 (1,  1, 1, 1, '2026-03-02T11:05:00'), (2,  1, 2, 1, '2026-03-03T10:15:00'),
 (3,  1, 3, 1, '2026-03-04T09:40:00'), (4,  1, 4, 1, '2026-03-05T16:20:00'),
 /* E2 - Liam, JavaScript course (lessons 9-12) */
 (5,  2, 9, 1, '2026-03-08T10:20:00'), (6,  2, 10, 1, '2026-03-09T11:35:00'),
 (7,  2, 11, 1, '2026-03-11T14:50:00'), (8,  2, 12, 1, '2026-03-12T15:30:00'),
 /* E3 - Olivia, Python course (lessons 21-24) */
 (9,  3, 21, 1, '2026-03-24T14:30:00'), (10, 3, 22, 1, '2026-03-26T10:05:00'),
 (11, 3, 23, 1, '2026-03-28T09:15:00'), (12, 3, 24, 1, '2026-03-29T18:55:00'),
 /* E4 - Noah, HTML course (lessons 1-4) */
 (13, 4, 1, 1, '2026-03-03T15:10:00'), (14, 4, 2, 1, '2026-03-04T13:25:00'),
 (15, 4, 3, 1, '2026-03-05T18:40:00'), (16, 4, 4, 1, '2026-03-06T19:10:00'),
 /* E5 - Ava, CSS course (lessons 5-8) */
 (17, 5, 5, 1, '2026-03-06T10:50:00'), (18, 5, 6, 1, '2026-03-07T15:05:00'),
 (19, 5, 7, 1, '2026-03-08T19:30:00'), (20, 5, 8, 1, '2026-03-09T20:00:00'),
 /* E6 - Ethan, Python course (lessons 21-24) - finished BEFORE failed attempt 04-01 */
 (21, 6, 21, 1, '2026-03-27T11:15:00'), (22, 6, 22, 1, '2026-03-29T09:40:00'),
 (23, 6, 23, 1, '2026-03-30T16:25:00'), (24, 6, 24, 1, '2026-03-31T09:55:00'),
 /* E7 - Sofia, React course (lessons 25-28) */
 (25, 7, 25, 1, '2026-04-05T10:40:00'), (26, 7, 26, 1, '2026-04-06T19:15:00'),
 (27, 7, 27, 1, '2026-04-08T10:30:00'), (28, 7, 28, 1, '2026-04-08T20:50:00'),
 /* E8 - Daniel, HTML course (lessons 1-4) */
 (29, 8, 1, 1, '2026-03-04T14:55:00'), (30, 8, 2, 1, '2026-03-05T16:30:00'),
 (31, 8, 3, 1, '2026-03-07T11:20:00'), (32, 8, 4, 1, '2026-03-08T15:05:00'),
 /* E9 - Noah, Python course (lessons 21-24) */
 (33, 9, 21, 1, '2026-03-28T10:05:00'), (34, 9, 22, 1, '2026-03-30T14:45:00'),
 (35, 9, 23, 1, '2026-04-02T10:10:00'), (36, 9, 24, 1, '2026-04-03T16:25:00'),
 /* E10 - Ethan, SQL course (lessons 13-16) */
 (37, 10, 13, 1, '2026-03-17T10:15:00'), (38, 10, 14, 1, '2026-03-19T13:50:00'),
 (39, 10, 15, 1, '2026-04-14T16:05:00'), (40, 10, 16, 1, '2026-04-15T14:15:00'),
 /* Partial progress - in-progress enrollments */
 (41, 11, 9,  1, '2026-03-15T11:40:00'), (42, 11, 10, 1, '2026-03-16T09:25:00'),
 (43, 12, 13, 1, '2026-03-19T16:20:00'), (44, 12, 14, 1, '2026-03-20T10:45:00'),
 (45, 13, 5,  1, '2026-03-20T12:35:00'), (46, 13, 6, 1, '2026-03-21T15:50:00'),
 (47, 13, 7,  1, '2026-03-23T09:05:00'),
 (48, 14, 13, 1, '2026-03-22T09:30:00'),
 (49, 15, 17, 1, '2026-04-10T11:20:00'), (50, 15, 18, 1, '2026-04-12T14:05:00'),
 (51, 16, 21, 1, '2026-04-06T15:40:00'), (52, 16, 22, 1, '2026-04-07T10:55:00'),
 (53, 17, 9,  1, '2026-04-12T12:10:00'),
 (54, 18, 5,  1, '2026-04-14T10:55:00'), (55, 18, 6, 1, '2026-04-15T13:30:00'),
 (56, 19, 21, 1, '2026-04-20T14:45:00'),
 (57, 20, 1,  1, '2026-04-22T11:25:00');
SET IDENTITY_INSERT dbo.LessonProgress OFF;


/*------------------------------ QUIZ ATTEMPTS (11)
  Server-side scoring reproduced exactly: score = correct/total*100 (decimal(5,2)),
  passed = score >= PassingScore. Quizzes 5-12 have THREE questions each, so their
  possible scores are 0 / 33.33 / 66.67 / 100.
*/
SET IDENTITY_INSERT dbo.QuizAttempts ON;
INSERT INTO dbo.QuizAttempts (AttemptId, QuizId, UserId, Score, CorrectAnswers, TotalQuestions, Passed, SubmittedAt) VALUES
 (1,  1, 5,  100.00, 4, 4, 1, '2026-03-05T16:45:00'),  /* Emily aces HTML */
 (2,  3, 6,   75.00, 3, 4, 1, '2026-03-12T16:00:00'),  /* Liam passes JavaScript */
 (3,  6, 7,  100.00, 3, 3, 1, '2026-03-29T19:25:00'),  /* Olivia perfect on Python */
 (4,  4, 10,  25.00, 1, 4, 0, '2026-04-15T14:35:00'),  /* Ethan FAILS SQL first try (needs >= 65) */
 (5,  2, 9,  100.00, 4, 4, 1, '2026-03-09T20:30:00'),  /* Ava perfect on CSS */
 (6,  6, 10,  33.33, 1, 3, 0, '2026-04-01T10:20:00'),  /* Ethan FAILS first try (needs >= 60) */
 (7,  6, 10, 100.00, 3, 3, 1, '2026-04-02T18:05:00'),  /* Ethan passes on the retake */
 (8,  7, 11, 100.00, 3, 3, 1, '2026-04-08T21:25:00'),  /* Sofia perfect on React */
 (9,  1, 12,  75.00, 3, 4, 1, '2026-03-08T15:35:00'),  /* Daniel passes HTML */
 (10, 6, 8,   66.67, 2, 3, 1, '2026-04-03T17:00:00'),  /* Noah scrapes past Python */
 (11, 4, 10, 100.00, 4, 4, 1, '2026-04-15T14:50:00');  /* Ethan perfect on SQL */
SET IDENTITY_INSERT dbo.QuizAttempts OFF;

/*------------------------------ QUIZ ANSWERS (39 - one row per answered question)
  SelectedOptionId values match the option table seeded in part 10; wrong picks
  are deliberately adjacent distractors.
*/
SET IDENTITY_INSERT dbo.QuizAnswers ON;
INSERT INTO dbo.QuizAnswers (QuizAnswerId, AttemptId, QuestionId, SelectedOptionId) VALUES
 /* A1 - Emily, all correct */
 (1,  1, 1,  2), (2,  1, 2,  5), (3,  1, 3, 11), (4,  1, 4, 16),
 /* A2 - Liam, misses Q11 (picked 42 instead of 41) */
 (5,  2, 9, 34), (6,  2, 10, 40), (7,  2, 11, 42), (8,  2, 12, 47),
 /* A3 - Olivia, all correct */
 (9,  3, 20, 80), (10, 3, 21, 81), (11, 3, 22, 86),
 /* A4 - Ethan FAILED SQL: only Q13 correct */
 (12, 4, 13, 51), (13, 4, 14, 54), (14, 4, 15, 60), (15, 4, 16, 62),
 /* A5 - Ava, all correct */
 (16, 5, 5, 17), (17, 5, 6, 22), (18, 5, 7, 28), (19, 5, 8, 31),
 /* A6 - Ethan FAILED: only Q21 correct */
 (20, 6, 20, 77), (21, 6, 21, 86), (22, 6, 22, 90),
 /* A7 - Ethan retake, all correct */
 (23, 7, 20, 80), (24, 7, 21, 81), (25, 7, 22, 86),
 /* A8 - Sofia, all correct */
 (26, 8, 23, 91), (27, 8, 24, 94), (28, 8, 25, 97),
 /* A9 - Daniel, misses Q1 (picked 1 <h6> instead of 2 <h1>) */
 (29, 9, 1, 1), (30, 9, 2, 5), (31, 9, 3, 11), (32, 9, 4, 16),
 /* A10 - Noah, misses Q21 (82 print() vs 81) */
 (33, 10, 20, 80), (34, 10, 21, 82), (35, 10, 22, 86),
 /* A11 - Ethan, all correct */
 (36, 11, 13, 51), (37, 11, 14, 53), (38, 11, 15, 58), (39, 11, 16, 64);
SET IDENTITY_INSERT dbo.QuizAnswers OFF;


/*------------------------------ AUDIT LOGS (31)
  Conventions match AuditLogService exactly:
    Action in {Create, Update, Archive, Restore, Delete, ChangePassword, ResetPassword}
    TableName in {Users, Roles, Categories, Courses, Lessons, Quizzes, Questions}
    Changes = camelCase JSON [{field, oldValue, newValue}] for updates/archives, else NULL.
*/
SET IDENTITY_INSERT dbo.AuditLogs ON;
INSERT INTO dbo.AuditLogs (AuditLogId, UserId, Action, TableName, RecordId, CreatedAt, Changes) VALUES
 /* Staff onboarding */
 (1,  1, N'Create',        N'Users',      1, '2026-02-10T09:00:00', NULL),
 (2,  1, N'Create',        N'Users',      2, '2026-02-10T09:05:00', NULL),
 (3,  1, N'Create',        N'Users',      3, '2026-02-10T09:10:00', NULL),
 (4,  1, N'Create',        N'Users',      4, '2026-02-10T09:15:00', NULL),
 /* Category catalog built */
 (5,  2, N'Create',        N'Categories', 1, '2026-02-20T10:00:00', NULL),
 (6,  2, N'Create',        N'Categories', 2, '2026-02-20T10:02:00', NULL),
 (7,  2, N'Create',        N'Categories', 3, '2026-02-20T10:04:00', NULL),
 (8,  2, N'Create',        N'Categories', 4, '2026-02-20T10:06:00', NULL),
 (9,  3, N'Create',        N'Categories', 5, '2026-02-20T10:10:00', NULL),
 (10, 3, N'Create',        N'Categories', 6, '2026-02-20T10:12:00', NULL),
 (11, 3, N'Create',        N'Categories', 7, '2026-02-20T10:14:00', NULL),
 (12, 3, N'Create',        N'Categories', 8, '2026-02-20T10:16:00', NULL),
 /* Course production begins */
 (13, 2, N'Create',        N'Courses',    1, '2026-02-25T09:30:00', NULL),
 (14, 2, N'Create',        N'Courses',    2, '2026-02-26T14:15:00', NULL),
 (15, 2, N'Create',        N'Courses',    3, '2026-03-10T11:20:00', NULL),
 (16, 2, N'Create',        N'Courses',    4, '2026-03-13T16:40:00', NULL),
 (17, 3, N'Create',        N'Courses',    5, '2026-03-19T10:05:00', NULL),
 (18, 3, N'Create',        N'Courses',    6, '2026-03-24T13:50:00', NULL),
 /* Lesson authoring */
 (19, 2, N'Create',        N'Lessons',    1, '2026-03-02T08:45:00', NULL),
 (20, 2, N'Create',        N'Lessons',    5, '2026-03-05T13:10:00', NULL),
 (21, 2, N'Update',        N'Lessons',    3, '2026-03-03T15:20:00',
     N'[{"field":"title","oldValue":"HTML Attributes","newValue":"HTML Text Formatting"}]'),
 /* Quiz authoring */
 (22, 2, N'Create',        N'Quizzes',    1, '2026-03-04T15:30:00', NULL),
 (23, 2, N'Create',        N'Questions',  1, '2026-03-04T15:35:00', NULL),
 (24, 2, N'Create',        N'Quizzes',    3, '2026-03-11T10:40:00', NULL),
 (25, 3, N'Create',        N'Questions',  17, '2026-03-22T11:20:00', NULL),
 /* Account maintenance */
 (26, 2, N'Update',        N'Users',      5, '2026-04-10T09:15:00',
     N'[{"field":"fullName","oldValue":"Emilly Johnson","newValue":"Emily Johnson"}]'),
 (27, 2, N'ResetPassword', N'Users',      8, '2026-04-18T10:30:00', NULL),
 (28, 2, N'ChangePassword',N'Users',      2, '2026-05-01T08:20:00', NULL),
 /* Legacy course 12 retired with its content */
 (29, 3, N'Archive',       N'Lessons',    41, '2026-05-20T15:30:00',
     N'[{"field":"deleteFlag","oldValue":"false","newValue":"true"}]'),
 (30, 3, N'Archive',       N'Lessons',    42, '2026-05-20T15:30:00',
     N'[{"field":"deleteFlag","oldValue":"false","newValue":"true"}]'),
 (31, 1, N'Archive',       N'Courses',    12, '2026-05-20T15:30:00',
     N'[{"field":"deleteFlag","oldValue":"false","newValue":"true"}]'),
 (32, 1, N'Archive',       N'Quizzes',    12, '2026-05-20T15:31:00',
     N'[{"field":"deleteFlag","oldValue":"false","newValue":"true"}]');
SET IDENTITY_INSERT dbo.AuditLogs OFF;

/*=============================================================================================
  RE-ENABLE ALL FOREIGN KEY CONSTRAINTS (WITH CHECK - validates the data we just inserted)
=============================================================================================*/
ALTER TABLE dbo.Users               WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.Roles               WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.Permissions         WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.UserRoles           WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.RolePermissions     WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.Categories          WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.RefreshTokens       WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.PasswordResetTokens WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.AuditLogs           WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.Courses             WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.Enrollments         WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.LessonProgress      WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.Lessons             WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.Quizzes             WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.Questions           WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.QuestionOptions     WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.QuizAttempts        WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE dbo.QuizAnswers         WITH CHECK CHECK CONSTRAINT ALL;

COMMIT TRANSACTION;

PRINT N'ResetAndSeedData completed successfully.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT N'ERROR - full rollback performed, database untouched.';
    THROW;
END CATCH;
GO

/*---------------------------------------------------------------------------------------------
  VERIFICATION SUMMARY - run automatically after the script finishes
---------------------------------------------------------------------------------------------*/
PRINT N'==================== SEED VERIFICATION ====================';
SELECT N'Permissions'      AS [Table], COUNT(*) AS [Rows] FROM dbo.Permissions
UNION ALL SELECT N'Roles',                COUNT(*) FROM dbo.Roles
UNION ALL SELECT N'RolePermissions',      COUNT(*) FROM dbo.RolePermissions
UNION ALL SELECT N'Users',                COUNT(*) FROM dbo.Users
UNION ALL SELECT N'UserRoles',            COUNT(*) FROM dbo.UserRoles
UNION ALL SELECT N'Categories',           COUNT(*) FROM dbo.Categories WHERE DeleteFlag = 0
UNION ALL SELECT N'Courses (active)',     COUNT(*) FROM dbo.Courses    WHERE DeleteFlag = 0
UNION ALL SELECT N'Lessons (active)',     COUNT(*) FROM dbo.Lessons    WHERE DeleteFlag = 0
UNION ALL SELECT N'Quizzes (active)',     COUNT(*) FROM dbo.Quizzes    WHERE DeleteFlag = 0
UNION ALL SELECT N'Questions',            COUNT(*) FROM dbo.Questions
UNION ALL SELECT N'QuestionOptions',      COUNT(*) FROM dbo.QuestionOptions
UNION ALL SELECT N'Enrollments',          COUNT(*) FROM dbo.Enrollments
UNION ALL SELECT N'LessonProgress',       COUNT(*) FROM dbo.LessonProgress
UNION ALL SELECT N'QuizAttempts',         COUNT(*) FROM dbo.QuizAttempts
UNION ALL SELECT N'QuizAnswers',          COUNT(*) FROM dbo.QuizAnswers
UNION ALL SELECT N'AuditLogs',            COUNT(*) FROM dbo.AuditLogs;

/* Expected: 28 permissions | 3 roles | 55 role-permissions | 12 users | 12 user-roles |
   8 categories | 11 active courses (+1 archived) | 40 active lessons (+2 archived) |
   11 active quizzes (+1 archived) | 40 questions | 160 options | 69 enrollments |
   57 progress rows | 11 attempts | 39 answers | 32 audit logs. */

/* Identity seeds should now start right after the highest seeded id */
SELECT CAST(IDENT_SEED(N'dbo.Users') AS int)                       AS SeedValue,
       CAST(IDENT_CURRENT(N'dbo.Users') AS int)                    AS CurrentValue,
       N'Users'                                                    AS [Table]
UNION ALL SELECT CAST(IDENT_CURRENT(N'dbo.Lessons') AS int), NULL, N'Lessons'
UNION ALL SELECT CAST(IDENT_CURRENT(N'dbo.QuizAttempts') AS int), NULL, N'QuizAttempts';

PRINT N'==========================================================';

