-- Adds the Changes column to AuditLogs.
-- Stores a JSON array of field-level changes ([{"field":"...","oldValue":"...","newValue":"..."}])
-- captured for Update actions so the Audit Log detail modal can show a before/after comparison.
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AuditLogs' AND COLUMN_NAME = 'Changes'
)
BEGIN
    ALTER TABLE dbo.AuditLogs ADD Changes nvarchar(max) NULL;
END
