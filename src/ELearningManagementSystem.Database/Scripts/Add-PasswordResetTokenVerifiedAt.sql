-- Adds the VerifiedAt column to PasswordResetTokens.
-- Records the moment an OTP passed verification so the Reset Password step can
-- proceed without the 1-minute OTP lifetime continuing to apply to it.
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'PasswordResetTokens' AND COLUMN_NAME = 'VerifiedAt'
)
BEGIN
    ALTER TABLE dbo.PasswordResetTokens ADD VerifiedAt datetime2 NULL;
END