USE [DealerManagementDB];
GO

-- =========================================================================
-- Migration: Add ProfilePicture Column to dbo.Users
-- =========================================================================
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'ProfilePicture'
)
BEGIN
    ALTER TABLE dbo.Users ADD ProfilePicture NVARCHAR(500) NULL;
    PRINT 'Column ProfilePicture added to dbo.Users successfully.';
END
ELSE
BEGIN
    PRINT 'Column ProfilePicture already exists in dbo.Users.';
END
GO

