-- Creates the PasswordResetTokens table used for the forgot-password flow.
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'PasswordResetTokens'
)
BEGIN
    CREATE TABLE dbo.PasswordResetTokens (
        PasswordResetTokenId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
        UserId int NOT NULL,
        TokenHash nvarchar(500) NOT NULL,
        ExpiresAt datetime2 NOT NULL,
        CreatedAt datetime2 NOT NULL,
        VerifiedAt datetime2 NULL,
        UsedAt datetime2 NULL,
        CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users (UserId)
    );

    CREATE INDEX IX_PasswordResetTokens_TokenHash ON dbo.PasswordResetTokens (TokenHash);
    CREATE INDEX IX_PasswordResetTokens_UserId ON dbo.PasswordResetTokens (UserId);
END