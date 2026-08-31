/*=============================================================================================
 DropPhase23Tables.sql
 ---------------------------------------------------------------------------------------------
 Project : E-Learning Management System (W3Schools-like)
 Purpose : Safely DROP the Phase 23+ paid-certification tables that were created directly in
           SQL Server even though the feature was never implemented. This restores the database
           to a clean state containing only active modules.

 TABLES REMOVED (per PHASE_23_IMPLEMENTATION_PLAN.md)
   dbo.Certificates                      (verifiable certificates)
   dbo.CertificationExams                (per-course exam configuration)
   dbo.ExamQuestionPool                  (exam question bank)
   dbo.ExamOptionPool                    (exam answer options pool)
   dbo.ExamPayments                      (mock exam payments)
   dbo.ExamAttempts                      (student exam attempts)
   dbo.ExamAttemptQuestionSnapshots      (question snapshots per attempt)
   dbo.ExamAttemptOptionSnapshots        (option snapshots per question snapshot)
   dbo.ExamAnswers                       (selected answers per attempt)

 DEPENDENCIES HANDLED
   - Circular foreign keys between ExamAttempts <-> ExamPayments
       FK_ExamAttempts_ExamPayments   (ExamAttempts.PaymentId -> ExamPayments)
       FK_ExamPayments_ExamAttempts   (ExamPayments.UsedForAttemptId -> ExamAttempts)
     One side of this cycle MUST be dropped before either table can go.
   - All other FKs vanish together with their child tables (children dropped first).
   - Idempotent: uses IF EXISTS guards, so it can be run multiple times safely.
   - Transactional: any failure rolls everything back.
   - Refuses to run against any database other than [ELearningManagementSystem].

 NOT TOUCHED
   - Shared-database tables from the other application (Tbl*, sysdiagrams, ...).
   - Core e-learning tables (Users, Roles, Permissions, Categories, Courses, Lessons,
     Enrollments, LessonProgress, Quizzes, Questions, QuestionOptions, QuizAttempts,
     QuizAnswers, RefreshTokens, PasswordResetTokens, UserRoles, RolePermissions, AuditLogs).
   - Rows in dbo.Permissions such as 'Certification.Configure' / 'QuestionBank.Manage' /
     'Certificate.Revoke' remain but become inert; ResetAndSeedData.sql reseeds a clean,
     focused permission set without them.

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
    RAISERROR (N'DropPhase23Tables.sql must be executed while connected to the [ELearningManagementSystem] database.', 16, 1);
    RETURN;
END;

PRINT N'=================================================';
PRINT N' Dropping Phase 23+ certification tables...';
PRINT N'=================================================';

BEGIN TRY
BEGIN TRANSACTION;

/*---------------------------------------------------------------------------------------------
 1) BREAK THE CIRCULAR FK PAIR FIRST (ExamAttempts <-> ExamPayments reference each other)
---------------------------------------------------------------------------------------------*/
ALTER TABLE dbo.ExamPayments DROP CONSTRAINT IF EXISTS [FK_ExamPayments_ExamAttempts];
ALTER TABLE dbo.ExamAttempts DROP CONSTRAINT IF EXISTS [FK_ExamAttempts_ExamPayments];

/*---------------------------------------------------------------------------------------------
 2) DROP CHILD TABLES FIRST, THEN PARENTS (each DROP removes its own outbound FKs)
---------------------------------------------------------------------------------------------*/
/* Leaf children of the attempt chain */
DROP TABLE IF EXISTS dbo.ExamAnswers;
DROP TABLE IF EXISTS dbo.ExamAttemptOptionSnapshots;
DROP TABLE IF EXISTS dbo.ExamAttemptQuestionSnapshots;

/* Certificates point at Users, Courses and ExamAttempts */
DROP TABLE IF EXISTS dbo.Certificates;

/* Attempt/payment layer */
DROP TABLE IF EXISTS dbo.ExamAttempts;
DROP TABLE IF EXISTS dbo.ExamPayments;

/* Question pools hang off CertificationExams */
DROP TABLE IF EXISTS dbo.ExamOptionPool;
DROP TABLE IF EXISTS dbo.ExamQuestionPool;

/* Root configuration table (references Courses) */
DROP TABLE IF EXISTS dbo.CertificationExams;

COMMIT TRANSACTION;

PRINT N'Phase 23+ tables dropped successfully.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT N'ERROR - transaction rolled back.';
    THROW;
END CATCH;
GO

/*---------------------------------------------------------------------------------------------
 3) VERIFICATION - expect zero rows below (no Phase 23 objects left behind)
---------------------------------------------------------------------------------------------*/
SELECT t.name AS RemainingPhase23Object
FROM sys.tables t
WHERE t.name IN (
    N'Certificates', N'CertificationExams', N'ExamQuestionPool', N'ExamOptionPool',
    N'ExamPayments', N'ExamAttempts', N'ExamAttemptQuestionSnapshots',
    N'ExamAttemptOptionSnapshots', N'ExamAnswers'
);

SELECT fk.name AS OrphanedForeignKey
FROM sys.foreign_keys fk
WHERE fk.name IN (
    N'FK_CertificationExams_Courses', N'FK_ExamQuestionPool_CertificationExams',
    N'FK_ExamOptionPool_ExamQuestionPool', N'FK_ExamPayments_Users',
    N'FK_ExamPayments_CertificationExams', N'FK_ExamPayments_ExamAttempts',
    N'FK_ExamAttempts_CertificationExams', N'FK_ExamAttempts_Users',
    N'FK_ExamAttempts_ExamPayments', N'FK_ExamAttemptQuestionSnapshots_ExamAttempts',
    N'FK_ExamAttemptOptionSnapshots_ExamAttemptQuestionSnapshots',
    N'FK_ExamAnswers_ExamAttempts', N'FK_ExamAnswers_ExamAttemptQuestionSnapshots',
    N'FK_ExamAnswers_ExamAttemptOptionSnapshots', N'FK_Certificates_Users',
    N'FK_Certificates_Courses', N'FK_Certificates_ExamAttempts'
);

PRINT N'Done. If both queries returned no rows, the cleanup is complete.';
