/*=============================================================================================
 DropForeignAppTables.sql
 ---------------------------------------------------------------------------------------------
 Project : E-Learning Management System
 Purpose : Removes 19 leftover tables belonging to a DIFFERENT application (banking /
           outward-cheque-clearing system) that were accidentally created inside the
           [ELearningManagementSystem] database on 2026-08-20 14:01.

 TABLES REMOVED
   TblAdminUser, TblBranch, TblPermission, TblRole, TblRolePermission, TblScanner,
   TblRejectReason (7 cheque rejection-code rows - owned by that app's seeder),
   TblOutwardAuditLog, TblOutwardBatch, TblOutwardCheque,
   TblOutwardCreditPostingFile, TblOutwardCreditPostingFileCheque,
   TblOutwardIclFile, TblOutwardIclFileCheque,
   TblOutwardReturnFile, TblOutwardReturnFileCheque,
   TblOutwardUploadResponse, TblOutwardUploadResponseItem, TblOutwardWorkstepHistory

 SAFETY
   - Verified beforehand: ZERO stored procedures / views / functions reference them.
   - No e-learning table starts with 'Tbl', so the whitelist below cannot match ours.
   - Drops happen child-first via a dependency-aware loop; a fallback breaks any
     circular FK pair by dropping the blocking constraint.
   - Transactional + idempotent (IF EXISTS everywhere).
   - Refuses to run against any database other than [ELearningManagementSystem].
=============================================================================================
*/

USE [ELearningManagementSystem];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'ELearningManagementSystem'
BEGIN
    RAISERROR (N'DropForeignAppTables.sql must be executed while connected to the [ELearningManagementSystem] database.', 16, 1);
    RETURN;
END;

PRINT N'=================================================';
PRINT N' Dropping foreign application tables (Tbl*)...';
PRINT N'=================================================';

BEGIN TRY
BEGIN TRANSACTION;

/*---------------------------------------------------------------------------------------------
 1) Whitelist the exact foreign tables into a work queue
---------------------------------------------------------------------------------------------*/
IF OBJECT_ID(N'tempdb..#ForeignTables') IS NOT NULL DROP TABLE #ForeignTables;
CREATE TABLE #ForeignTables (TableName sysname PRIMARY KEY);

INSERT INTO #ForeignTables (TableName) VALUES
 (N'TblAdminUser'), (N'TblBranch'), (N'TblPermission'), (N'TblRole'),
 (N'TblRolePermission'), (N'TblScanner'), (N'TblRejectReason'),
 (N'TblOutwardAuditLog'), (N'TblOutwardBatch'), (N'TblOutwardCheque'),
 (N'TblOutwardCreditPostingFile'), (N'TblOutwardCreditPostingFileCheque'),
 (N'TblOutwardIclFile'), (N'TblOutwardIclFileCheque'),
 (N'TblOutwardReturnFile'), (N'TblOutwardReturnFileCheque'),
 (N'TblOutwardUploadResponse'), (N'TblOutwardUploadResponseItem'),
 (N'TblOutwardWorkstepHistory');

/* Keep only the ones that actually exist right now (idempotency) */
DELETE ft FROM #ForeignTables ft
WHERE OBJECT_ID(N'dbo.' + ft.TableName) IS NULL;

/*---------------------------------------------------------------------------------------------
 2) Drop child-first; if everything remaining is locked in an FK cycle,
    break the cycle by removing one inbound foreign key and continue.
---------------------------------------------------------------------------------------------*/
DECLARE @table sysname, @fk sysname, @parent sysname, @sql nvarchar(400);

WHILE EXISTS (SELECT 1 FROM #ForeignTables)
BEGIN
    /* Pick a table whose only inbound references come from itself */
    SELECT TOP (1) @table = ft.TableName
    FROM #ForeignTables ft
    JOIN sys.tables t ON t.name = ft.TableName
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys fk
        WHERE fk.referenced_object_id = t.object_id
          AND fk.parent_object_id <> t.object_id                 -- ignore self-reference
          AND fk.parent_object_id IN (SELECT object_id FROM sys.tables)
          AND EXISTS (SELECT 1 FROM #ForeignTables q2
                      JOIN sys.tables t2 ON t2.name = q2.TableName
                      WHERE t2.object_id = fk.parent_object_id)  -- referrer still queued
    )
    ORDER BY ft.TableName;

    IF @table IS NULL
    BEGIN
        /* Circular FK chain: force-drop one inbound foreign key of the first queued table */
        SELECT TOP (1) @table = ft.TableName
        FROM #ForeignTables ft ORDER BY ft.TableName;

        SELECT TOP (1) @fk = fk.name
        FROM sys.foreign_keys fk
        WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.' + @table)
          AND fk.parent_object_id <> OBJECT_ID(N'dbo.' + @table);

        IF @fk IS NULL THROW 50001, N'Could not resolve foreign-key dependencies.', 1;

        SET @parent =
            (SELECT QUOTENAME(t.name)
             FROM sys.tables t
             WHERE t.object_id = (SELECT fk2.parent_object_id
                                  FROM sys.foreign_keys fk2
                                  WHERE fk2.name = @fk));

        SET @sql =
            N'ALTER TABLE dbo.' + @parent + N' DROP CONSTRAINT ' + QUOTENAME(@fk) + N';';
        EXEC (@sql);
        PRINT N'  Broke circular FK: ' + @fk;
        SET @table = NULL;   /* retry the normal pick next iteration */
    END
    ELSE
    BEGIN
        SET @sql = N'DROP TABLE dbo.' + QUOTENAME(@table) + N';';
        EXEC (@sql);
        DELETE FROM #ForeignTables WHERE TableName = @table;
        PRINT N'  Dropped: ' + @table;
    END;

    SET @table = NULL;
END;

COMMIT TRANSACTION;
PRINT N'Foreign application tables dropped successfully.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT N'ERROR - transaction rolled back.';
    THROW;
END CATCH;
GO

/*---------------------------------------------------------------------------------------------
 3) VERIFICATION - expect zero rows (nothing matching the whitelist remains)
---------------------------------------------------------------------------------------------*/
SELECT t.name AS RemainingForeignTable
FROM sys.tables t
WHERE t.name IN (
    N'TblAdminUser', N'TblBranch', N'TblPermission', N'TblRole', N'TblRolePermission',
    N'TblScanner', N'TblRejectReason', N'TblOutwardAuditLog', N'TblOutwardBatch',
    N'TblOutwardCheque', N'TblOutwardCreditPostingFile', N'TblOutwardCreditPostingFileCheque',
    N'TblOutwardIclFile', N'TblOutwardIclFileCheque', N'TblOutwardReturnFile',
    N'TblOutwardReturnFileCheque', N'TblOutwardUploadResponse',
    N'TblOutwardUploadResponseItem', N'TblOutwardWorkstepHistory');

PRINT N'Done.';
