USE [DealerManagementDB];
GO

-- Clear existing menus and re-populate full hierarchy
DELETE FROM dbo.Menu;
DBCC CHECKIDENT ('dbo.Menu', RESEED, 0);
GO

-- Level 1: Main Headers
-- 1: Dashboard
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES ('Dashboard', 'Home', 'Index', '/Home/Index', 'fas fa-tachometer-alt', 0, 0, 0, 1, 'Core', 1, SYSDATETIME());

-- 2: Master Setup
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES ('Master Setup', NULL, NULL, '#', 'fas fa-boxes', 0, 0, 0, 2, 'Setup', 1, SYSDATETIME());

-- 3: Stakeholders
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES ('Stakeholders', NULL, NULL, '#', 'fas fa-users', 0, 0, 0, 3, 'Stakeholders', 1, SYSDATETIME());

-- 4: Sales & Distribution
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES ('Sales & Distribution', NULL, NULL, '#', 'fas fa-shopping-cart', 0, 0, 0, 4, 'Sales', 1, SYSDATETIME());

-- 5: Procurement & Purchase
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES ('Procurement & Purchase', NULL, NULL, '#', 'fas fa-truck-loading', 0, 0, 0, 5, 'Purchase', 1, SYSDATETIME());

-- 6: Production & Inventory
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES ('Production & Loss', NULL, NULL, '#', 'fas fa-industry', 0, 0, 0, 6, 'Production', 1, SYSDATETIME());

-- 7: Financials & Accounts
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES ('Financials & Collections', NULL, NULL, '#', 'fas fa-wallet', 0, 0, 0, 7, 'Financials', 1, SYSDATETIME());

-- 8: Administration
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES ('Administration', NULL, NULL, '#', 'fas fa-cogs', 0, 0, 0, 8, 'Admin', 1, SYSDATETIME());

-- Level 2: Children of Master Setup (ParentId = 2)
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES 
('Product Category', 'ProductCategory', 'Index', '/ProductCategory/Index', 'fas fa-tags', 2, 0, 0, 1, 'Setup', 1, SYSDATETIME()),
('Product Name', 'ProductName', 'Index', '/ProductName/Index', 'fas fa-cube', 2, 0, 0, 2, 'Setup', 1, SYSDATETIME()),
('Product Definition', 'ProductSetup', 'Index', '/ProductSetup/Index', 'fas fa-box-open', 2, 0, 0, 3, 'Setup', 1, SYSDATETIME()),
('Price Setup', 'PriceSetup', 'Index', '/PriceSetup/Index', 'fas fa-money-bill-wave', 2, 0, 0, 4, 'Setup', 1, SYSDATETIME()),
('Color Master', 'ProductColor', 'Index', '/ProductColor/Index', 'fas fa-palette', 2, 0, 0, 5, 'Setup', 1, SYSDATETIME()),
('Size Master', 'ProductSize', 'Index', '/ProductSize/Index', 'fas fa-ruler-combined', 2, 0, 0, 6, 'Setup', 1, SYSDATETIME()),
('Grade Master', 'ProductGrade', 'Index', '/ProductGrade/Index', 'fas fa-star', 2, 0, 0, 7, 'Setup', 1, SYSDATETIME());

-- Level 2: Children of Stakeholders (ParentId = 3)
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES 
('Customer / Dealer', 'Customer', 'Index', '/Customer/Index', 'fas fa-user-tie', 3, 0, 0, 1, 'Stakeholders', 1, SYSDATETIME()),
('Supplier Master', 'Supplier', 'Index', '/Supplier/Index', 'fas fa-building', 3, 0, 0, 2, 'Stakeholders', 1, SYSDATETIME());

-- Level 2: Children of Sales & Distribution (ParentId = 4)
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES 
('Customer Order', 'CustomerOrder', 'Index', '/CustomerOrder/Index', 'fas fa-file-invoice', 4, 0, 0, 1, 'Sales', 1, SYSDATETIME()),
('Delivery Challan', 'CustomerDelivery', 'Index', '/CustomerDelivery/Index', 'fas fa-truck', 4, 0, 0, 2, 'Sales', 1, SYSDATETIME()),
('Sales Return', 'CustomerReturn', 'Index', '/CustomerReturn/Index', 'fas fa-undo-alt', 4, 0, 0, 3, 'Sales', 1, SYSDATETIME()),
('Sample Delivery', 'SampleDelivery', 'Index', '/SampleDelivery/Index', 'fas fa-gift', 4, 0, 0, 4, 'Sales', 1, SYSDATETIME());

-- Level 2: Children of Procurement & Purchase (ParentId = 5)
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES 
('Purchase Order', 'PurchaseOrder', 'Index', '/PurchaseOrder/Index', 'fas fa-clipboard-list', 5, 0, 0, 1, 'Purchase', 1, SYSDATETIME()),
('Purchase Receipt (MRR)', 'PurchaseReceipt', 'Index', '/PurchaseReceipt/Index', 'fas fa-warehouse', 5, 0, 0, 2, 'Purchase', 1, SYSDATETIME()),
('Purchase Return', 'PurchaseReturn', 'Index', '/PurchaseReturn/Index', 'fas fa-truck-loading', 5, 0, 0, 3, 'Purchase', 1, SYSDATETIME());

-- Level 2: Children of Production & Loss (ParentId = 6)
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES 
('Production Entry', 'Production', 'Index', '/Production/Index', 'fas fa-cogs', 6, 0, 0, 1, 'Production', 1, SYSDATETIME()),
('Damage & Missing Report', 'ProductDamage', 'Index', '/ProductDamage/Index', 'fas fa-exclamation-triangle', 6, 0, 0, 2, 'Production', 1, SYSDATETIME());

-- Level 2: Children of Financials & Collections (ParentId = 7)
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES 
('Dealer Deposit / Collection', 'CustomerDeposit', 'Index', '/CustomerDeposit/Index', 'fas fa-money-check-alt', 7, 0, 0, 1, 'Financials', 1, SYSDATETIME()),
('Dealer Commission', 'CustomerCommission', 'Index', '/CustomerCommission/Index', 'fas fa-percentage', 7, 0, 0, 2, 'Financials', 1, SYSDATETIME()),
('Dealer Opening Balance', 'CustomerOpeningBalance', 'Index', '/CustomerOpeningBalance/Index', 'fas fa-book-open', 7, 0, 0, 3, 'Financials', 1, SYSDATETIME()),
('Supplier Payment', 'SupplierDeposit', 'Index', '/SupplierDeposit/Index', 'fas fa-hand-holding-usd', 7, 0, 0, 4, 'Financials', 1, SYSDATETIME()),
('Supplier Rebate', 'SupplierCommission', 'Index', '/SupplierCommission/Index', 'fas fa-tag', 7, 0, 0, 5, 'Financials', 1, SYSDATETIME()),
('Supplier Opening Balance', 'SupplierOpeningBalance', 'Index', '/SupplierOpeningBalance/Index', 'fas fa-book', 7, 0, 0, 6, 'Financials', 1, SYSDATETIME());

-- Level 2: Children of Administration (ParentId = 8)
INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
VALUES 
('User Management', 'User', 'Index', '/User/Index', 'fas fa-users-cog', 8, 0, 0, 1, 'Admin', 1, SYSDATETIME());
GO

