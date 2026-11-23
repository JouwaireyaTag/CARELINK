-- CARELINK V2 – Authentication & Access patch (Member 3)
-- Run AFTER CARELINK_Version2.sql (and after the seed data, if any).
-- Safe to run more than once.
-- In SSMS just run it. With sqlcmd add -I (QUOTED_IDENTIFIER ON), e.g.:
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -I -i CARELINK_V2_Auth_Patch.sql
--
-- 1) dbo.ApplicationUsers: Identity columns
-- 2) dbo.AccessGrants: AccessExpiresAt
--
-- 1) ApplicationUsers
-- ASP.NET Core Identity is mapped onto dbo.ApplicationUsers instead of its own
-- AspNetUsers table. Identity needs these extra columns to look users up,
-- invalidate cookies after a password change, and lock accounts after
-- too many failed logins. The original columns (User_Id, Email, Role,
-- PasswordHash) are unchanged, so every existing foreign key still works.
--
-- Not added on purpose (not used in CARELINK): PhoneNumber,
-- PhoneNumberConfirmed, TwoFactorEnabled, and the AspNetUserClaims /
-- AspNetUserLogins / AspNetUserTokens / AspNetRoles tables. Roles come from
-- the existing Role column.

USE [CARELINK_V2];
GO

IF COL_LENGTH(N'dbo.ApplicationUsers', N'UserName') IS NULL
BEGIN
    ALTER TABLE dbo.ApplicationUsers ADD
        UserName            NVARCHAR(256)  NULL,
        NormalizedUserName  NVARCHAR(256)  NULL,
        NormalizedEmail     NVARCHAR(256)  NULL,
        EmailConfirmed      BIT            NOT NULL
            CONSTRAINT DF_ApplicationUsers_EmailConfirmed DEFAULT (0),
        SecurityStamp       NVARCHAR(256)  NULL,
        ConcurrencyStamp    NVARCHAR(256)  NULL,
        LockoutEnd          DATETIMEOFFSET NULL,
        LockoutEnabled      BIT            NOT NULL
            CONSTRAINT DF_ApplicationUsers_LockoutEnabled DEFAULT (1),
        AccessFailedCount   INT            NOT NULL
            CONSTRAINT DF_ApplicationUsers_AccessFailedCount DEFAULT (0);
END;
GO

-- Back-fill rows that already exist (seed data). Their PasswordHash is a
-- placeholder, so they still cannot log in; that is expected.
UPDATE dbo.ApplicationUsers
SET UserName           = COALESCE(UserName, Email),
    NormalizedUserName = COALESCE(NormalizedUserName, UPPER(Email)),
    NormalizedEmail    = COALESCE(NormalizedEmail, UPPER(Email)),
    SecurityStamp      = COALESCE(SecurityStamp, CONVERT(NVARCHAR(36), NEWID())),
    ConcurrencyStamp   = COALESCE(ConcurrencyStamp, CONVERT(NVARCHAR(36), NEWID()));
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_ApplicationUsers_NormalizedUserName')
    CREATE UNIQUE INDEX UX_ApplicationUsers_NormalizedUserName
    ON dbo.ApplicationUsers(NormalizedUserName)
    WHERE NormalizedUserName IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ApplicationUsers_NormalizedEmail')
    CREATE INDEX IX_ApplicationUsers_NormalizedEmail
    ON dbo.ApplicationUsers(NormalizedEmail);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = N'CK_ApplicationUsers_Role')
    ALTER TABLE dbo.ApplicationUsers ADD
        CONSTRAINT CK_ApplicationUsers_Role
        CHECK (Role IN ('Patient', 'Doctor', 'Admin'));
GO

-- 2) AccessGrants
-- ExpiredAt (existing) = when the one-time CODE stops working if not used.
-- AccessExpiresAt (new) = when the doctor's ACCESS to the file ends. It is set when
-- the doctor enters the right code, and moved to "now" if the patient revokes access.
IF COL_LENGTH(N'dbo.AccessGrants', N'AccessExpiresAt') IS NULL
    ALTER TABLE dbo.AccessGrants ADD AccessExpiresAt DATETIME2 NULL;
GO
