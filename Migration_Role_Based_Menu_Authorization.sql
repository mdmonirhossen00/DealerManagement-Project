USE [DealerManagementDB];
GO

-- =========================================================================
-- Step 1: Create Role Table
-- =========================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Role')
BEGIN
    CREATE TABLE dbo.Role (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(250) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NULL,
        UpdatedAt DATETIME NULL,
        UpdatedBy NVARCHAR(100) NULL
    );
END
GO

-- =========================================================================
-- Step 2: Create RoleMenu Table
-- =========================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RoleMenu')
BEGIN
    CREATE TABLE dbo.RoleMenu (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoleId INT NOT NULL,
        MenuId INT NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NULL,
        CONSTRAINT FK_RoleMenu_Role FOREIGN KEY (RoleId) REFERENCES dbo.Role(Id) ON DELETE CASCADE,
        CONSTRAINT FK_RoleMenu_Menu FOREIGN KEY (MenuId) REFERENCES dbo.Menu(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_RoleMenu_Role_Menu UNIQUE (RoleId, MenuId)
    );
END
GO

-- =========================================================================
-- Step 3: Add RoleID to Users Table
-- =========================================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'RoleID')
BEGIN
    ALTER TABLE dbo.Users ADD RoleID INT NULL;
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Role FOREIGN KEY (RoleID) REFERENCES dbo.Role(Id);
END
GO

-- =========================================================================
-- Step 4: Create UserRole Mapping Table
-- =========================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserRole')
BEGIN
    CREATE TABLE dbo.UserRole (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NOT NULL,
        RoleID INT NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NULL,
        CONSTRAINT FK_UserRole_Users FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID) ON DELETE CASCADE,
        CONSTRAINT FK_UserRole_Role FOREIGN KEY (RoleID) REFERENCES dbo.Role(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_UserRole_User_Role UNIQUE (UserID, RoleID)
    );
END
GO

-- =========================================================================
-- Step 5: Seed Default Roles
-- =========================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Role WHERE Name = 'Admin')
BEGIN
    INSERT INTO dbo.Role (Name, Description, IsActive, CreatedAt, CreatedBy)
    VALUES ('Admin', 'System Administrator with full permissions', 1, GETDATE(), 'System');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Role WHERE Name = 'Dealer')
BEGIN
    INSERT INTO dbo.Role (Name, Description, IsActive, CreatedAt, CreatedBy)
    VALUES ('Dealer', 'Dealer / Sales portal user', 1, GETDATE(), 'System');
END
GO

-- =========================================================================
-- Step 6: Add Menu Entries for Role and RoleMenu under Administration (ParentId = 8)
-- =========================================================================
DECLARE @AdminParentId INT = 8;
SELECT TOP 1 @AdminParentId = Id FROM dbo.Menu WHERE [Name] = 'Administration' AND ParentId = 0;

IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Controller = 'Role' AND [Action] = 'Index')
BEGIN
    INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
    VALUES ('Role Management', 'Role', 'Index', '/Role/Index', 'fas fa-user-shield', @AdminParentId, 0, 0, 2, 'Admin', 1, SYSDATETIME());
END

IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE Controller = 'RoleMenu' AND [Action] = 'Index')
BEGIN
    INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
    VALUES ('Role Menu Permission', 'RoleMenu', 'Index', '/RoleMenu/Index', 'fas fa-shield-alt', @AdminParentId, 0, 0, 3, 'Admin', 1, SYSDATETIME());
END
GO

-- =========================================================================
-- Step 7: Assign All Active Menus to Admin Role
-- =========================================================================
DECLARE @AdminRoleId INT;
SELECT TOP 1 @AdminRoleId = Id FROM dbo.Role WHERE Name = 'Admin';

IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO dbo.RoleMenu (RoleId, MenuId, CreatedAt, CreatedBy)
    SELECT @AdminRoleId, m.Id, GETDATE(), 'System'
    FROM dbo.Menu m
    WHERE m.IsActive = 1
      AND NOT EXISTS (
          SELECT 1 FROM dbo.RoleMenu rm 
          WHERE rm.RoleId = @AdminRoleId AND rm.MenuId = m.Id
      );
END
GO

-- =========================================================================
-- Step 8: Assign Admin Role to UserID = 1 (and any users without role)
-- =========================================================================
DECLARE @AdminRoleId2 INT;
SELECT TOP 1 @AdminRoleId2 = Id FROM dbo.Role WHERE Name = 'Admin';

IF @AdminRoleId2 IS NOT NULL
BEGIN
    -- Update Users table
    UPDATE dbo.Users 
    SET RoleID = @AdminRoleId2 
    WHERE RoleID IS NULL;

    -- Ensure UserRole table has the entry
    INSERT INTO dbo.UserRole (UserID, RoleID, CreatedAt, CreatedBy)
    SELECT u.UserID, @AdminRoleId2, GETDATE(), 'System'
    FROM dbo.Users u
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.UserRole ur 
        WHERE ur.UserID = u.UserID AND ur.RoleID = @AdminRoleId2
    );
END
GO

