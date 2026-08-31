-- Adds the MustChangePassword flag to Users.
-- Admin-created accounts are provisioned with a temporary password and
-- MustChangePassword = 1; it is cleared after the user changes their password.
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'MustChangePassword'
)
BEGIN
    ALTER TABLE dbo.Users ADD MustChangePassword bit NOT NULL DEFAULT (0);
END
