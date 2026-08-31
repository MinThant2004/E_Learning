/*=============================================================================================
 SpreadEnrollmentHistory.sql
 ---------------------------------------------------------------------------------------------
 Purpose : Give the Admin Dashboard "Enrollment Growth" chart realistic history.
           Before this script the EnrollDates lived in three clusters:
             - 24 seeded rows in Mar-May 2026
             - a dead gap through Jun-Jul
             - 12 test rows all stamped today within one hour
           Chart impact: 7d/30d = single-day spike; 6m = two empty months.

 WHAT IT DOES (safe directions only - EnrollDate never moves AFTER existing activity)
   1) Moves the four "not started" enrollments (no LessonProgress rows) into the Jun-Jul gap.
   2) Spreads today's testing burst across the last ~4 weeks (all their lesson/quiz activity
     happened today, so any earlier EnrollDate stays chronological).
   3) Inserts 33 historical enrollments (Feb -> Aug 2026) forming a growth curve:
        Feb 3 | Mar 17 | Apr 9 | May 6 | Jun 7 | Jul 10 | Aug 17
     All pairs are unique (student, course), reference active courses only, and are seeded
     as Completed = 0 (the app itself flips that when a final quiz is passed).

=============================================================================================
*/

USE [ELearningManagementSystem];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'ELearningManagementSystem'
BEGIN
    RAISERROR (N'SpreadEnrollmentHistory.sql must be executed while connected to the [ELearningManagementSystem] database.', 16, 1);
    RETURN;
END;

BEGIN TRY
BEGIN TRANSACTION;

PRINT N'-- Monthly distribution BEFORE --';
SELECT CONVERT(varchar(7), EnrollDate, 126) AS [Month], COUNT(*) AS [Enrollments]
FROM dbo.Enrollments GROUP BY CONVERT(varchar(7), EnrollDate, 126) ORDER BY [Month];

/*---------------------------------------------------------------------------------------------
 1) Fill the Jun-Jul hole with the four "not started" enrollments (zero progress => movable)
---------------------------------------------------------------------------------------------*/
UPDATE dbo.Enrollments SET EnrollDate = '2026-06-11T09:20:00' WHERE EnrollmentId = 21 AND CourseId = 5;   /* Ethan  -> C# course   */
UPDATE dbo.Enrollments SET EnrollDate = '2026-06-24T15:40:00' WHERE EnrollmentId = 22 AND CourseId = 1;   /* Sofia  -> HTML course */
UPDATE dbo.Enrollments SET EnrollDate = '2026-07-08T10:25:00' WHERE EnrollmentId = 23 AND CourseId = 4;   /* Ava    -> SQL course  */
UPDATE dbo.Enrollments SET EnrollDate = '2026-07-15T13:55:00' WHERE EnrollmentId = 24 AND CourseId = 6;   /* Daniel -> Python      */

/*---------------------------------------------------------------------------------------------
 2) Un-bunch today's testing burst (ids 25-36) across recent weeks
---------------------------------------------------------------------------------------------*/
UPDATE dbo.Enrollments SET EnrollDate = '2026-07-24T14:45:00' WHERE EnrollmentId = 36;   /* David admin -> React          */
UPDATE dbo.Enrollments SET EnrollDate = '2026-07-27T09:55:00' WHERE EnrollmentId = 32;   /* Liam -> OOP C#                */
UPDATE dbo.Enrollments SET EnrollDate = '2026-07-30T16:35:00' WHERE EnrollmentId = 29;   /* Emily -> OOP C#               */
UPDATE dbo.Enrollments SET EnrollDate = '2026-08-05T14:20:00' WHERE EnrollmentId = 27;   /* Emily -> SQL                  */
UPDATE dbo.Enrollments SET EnrollDate = '2026-08-06T10:30:00' WHERE EnrollmentId = 35;   /* Liam -> React                 */
UPDATE dbo.Enrollments SET EnrollDate = '2026-08-10T13:40:00' WHERE EnrollmentId = 33;   /* Liam -> Advanced SQL          */
UPDATE dbo.Enrollments SET EnrollDate = '2026-08-12T09:40:00' WHERE EnrollmentId = 26;   /* Emily -> C# Fundamentals      */
UPDATE dbo.Enrollments SET EnrollDate = '2026-08-14T15:25:00' WHERE EnrollmentId = 31;   /* Emily -> JavaScript           */
UPDATE dbo.Enrollments SET EnrollDate = '2026-08-17T11:05:00' WHERE EnrollmentId = 28;   /* Emily -> React                */
UPDATE dbo.Enrollments SET EnrollDate = '2026-08-19T16:10:00' WHERE EnrollmentId = 34;   /* Liam -> Modern CSS Layout     */
UPDATE dbo.Enrollments SET EnrollDate = '2026-08-20T10:15:00' WHERE EnrollmentId = 25;   /* David admin -> Flexbox/Grid   */
UPDATE dbo.Enrollments SET EnrollDate = '2026-08-21T10:50:00' WHERE EnrollmentId = 30;   /* Emily -> Flexbox/Grid         */

/*---------------------------------------------------------------------------------------------
 3) Historical enrollments - early adoption through steady growth
---------------------------------------------------------------------------------------------*/
INSERT INTO dbo.Enrollments (UserId, CourseId, EnrollDate, Completed, CompletedDate) VALUES
 /* February 2026 - first adopters (3) */
 (7,  1, '2026-02-09T09:20:00', 0, NULL),
 (9,  1, '2026-02-16T14:05:00', 0, NULL),
 (10, 1, '2026-02-23T10:40:00', 0, NULL),
 /* March 2026 (4) */
 (11, 2, '2026-03-06T11:15:00', 0, NULL),
 (12, 3, '2026-03-13T15:50:00', 0, NULL),
 (8,  2, '2026-03-19T09:35:00', 0, NULL),
 (7,  2, '2026-03-26T13:20:00', 0, NULL),
 /* April 2026 (2) */
 (9,  5, '2026-04-03T10:10:00', 0, NULL),
 (10, 2, '2026-04-09T16:25:00', 0, NULL),
 /* May 2026 (6) */
 (11, 4, '2026-05-05T09:45:00', 0, NULL),
 (12, 5, '2026-05-12T14:30:00', 0, NULL),
 (7,  3, '2026-05-01T11:55:00', 0, NULL),
 (8,  3, '2026-05-08T10:20:00', 0, NULL),
 (9,  7, '2026-05-15T15:40:00', 0, NULL),
 (10, 3, '2026-05-22T09:05:00', 0, NULL),
 /* June 2026 (5) */
 (11, 5, '2026-06-02T13:45:00', 0, NULL),
 (12, 7, '2026-06-08T10:30:00', 0, NULL),
 (7,  4, '2026-06-15T14:15:00', 0, NULL),
 (8,  4, '2026-06-19T09:50:00', 0, NULL),
 (9,  8, '2026-06-26T16:00:00', 0, NULL),
 /* July 2026 (6) */
 (10, 7, '2026-07-01T11:25:00', 0, NULL),
 (11, 6, '2026-07-06T15:10:00', 0, NULL),
 (12, 8, '2026-07-10T09:40:00', 0, NULL),
 (7,  7, '2026-07-16T13:05:00', 0, NULL),
 (8,  5, '2026-07-22T10:50:00', 0, NULL),
 (9,  9, '2026-07-28T14:35:00', 0, NULL),
 /* August 2026 - recent momentum incl. this week (7) */
 (10, 8, '2026-08-23T08:50:00', 0, NULL),
 (11, 8, '2026-08-07T15:45:00', 0, NULL),
 (12, 9, '2026-08-11T09:15:00', 0, NULL),
 (7,  8, '2026-08-14T13:30:00', 0, NULL),
 (8,  7, '2026-08-18T10:05:00', 0, NULL),
 (9,  10, '2026-08-21T14:50:00', 0, NULL),
 (12, 10, '2026-08-22T09:30:00', 0, NULL);

COMMIT TRANSACTION;

PRINT N'Enrollment history spread successfully.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT N'ERROR - rolled back.';
    THROW;
END CATCH;
GO

/* Verification: monthly growth curve + last-7-day activity */
SELECT CONVERT(varchar(7), EnrollDate, 126) AS [Month], COUNT(*) AS [Enrollments]
FROM dbo.Enrollments GROUP BY CONVERT(varchar(7), EnrollDate, 126) ORDER BY [Month];

SELECT CONVERT(varchar(10), EnrollDate, 126) AS [Day], COUNT(*) AS [Enrollments]
FROM dbo.Enrollments
WHERE EnrollDate >= DATEADD(DAY, -7, CAST(GETUTCDATE() AS date))
GROUP BY CONVERT(varchar(10), EnrollDate, 126) ORDER BY [Day];

SELECT COUNT(*) AS DuplicatePairCheck FROM
(
    SELECT UserId, CourseId FROM dbo.Enrollments GROUP BY UserId, CourseId HAVING COUNT(*) > 1
) d;
