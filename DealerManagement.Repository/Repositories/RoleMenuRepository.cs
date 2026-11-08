using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using DealerManagement.Model.Entities;
using DealerManagement.Model.ViewModels;
using DealerManagement.Repository.Base;
using DealerManagement.Repository.Interfaces;

namespace DealerManagement.Repository.Repositories
{
    public class RoleMenuRepository : BaseRepository, IRoleMenuRepository
    {
        public RoleMenuRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<RoleMenuItemVM>> GetRoleMenuAccessDataAsync(int roleId)
        {
            const string sql = @"
                WITH MenuHierarchy AS (
                    SELECT 
                        Id, 
                        [Name], 
                        ParentId, 
                        CAST([Name] AS NVARCHAR(MAX)) AS MenuName
                    FROM dbo.Menu
                    WHERE IsActive = 1 AND ParentId = 0
                    UNION ALL
                    SELECT 
                        m.Id, 
                        m.[Name], 
                        m.ParentId, 
                        CAST(mc.MenuName + ' > ' + m.[Name] AS NVARCHAR(MAX)) AS MenuName
                    FROM dbo.Menu m    
                    INNER JOIN MenuHierarchy mc ON m.ParentId = mc.Id
                    WHERE m.IsActive = 1
                )
                SELECT 
                    m.Id AS MenuId,
                    mh.MenuName,
                    m.ParentId,
                    ISNULL(pm.[Name], 'Top Level') AS ParentMenuName,
                    m.Controller,
                    m.[Action],
                    m.[Url],
                    m.IconClass,
                    m.DisplayOrder,
                    CASE WHEN rm.MenuId IS NOT NULL THEN 1 ELSE 0 END AS IsChecked
                FROM dbo.Menu m
                INNER JOIN MenuHierarchy mh ON m.Id = mh.Id
                LEFT JOIN dbo.Menu pm ON m.ParentId = pm.Id
                LEFT JOIN dbo.RoleMenu rm ON rm.MenuId = m.Id AND rm.RoleId = @RoleId
                WHERE m.IsActive = 1
                ORDER BY 
                    CASE WHEN m.ParentId = 0 THEN m.Id ELSE m.ParentId END ASC,
                    CASE WHEN m.ParentId = 0 THEN 0 ELSE 1 END ASC,
                    m.DisplayOrder ASC";

            var list = new List<RoleMenuItemVM>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new RoleMenuItemVM
                {
                    MenuId = reader.GetInt32(reader.GetOrdinal("MenuId")),
                    MenuName = reader.GetString(reader.GetOrdinal("MenuName")),
                    ParentId = reader.GetInt32(reader.GetOrdinal("ParentId")),
                    ParentMenuName = reader.IsDBNull(reader.GetOrdinal("ParentMenuName")) ? null : reader.GetString(reader.GetOrdinal("ParentMenuName")),
                    Controller = reader.IsDBNull(reader.GetOrdinal("Controller")) ? null : reader.GetString(reader.GetOrdinal("Controller")),
                    Action = reader.IsDBNull(reader.GetOrdinal("Action")) ? null : reader.GetString(reader.GetOrdinal("Action")),
                    Url = reader.IsDBNull(reader.GetOrdinal("Url")) ? null : reader.GetString(reader.GetOrdinal("Url")),
                    IconClass = reader.IsDBNull(reader.GetOrdinal("IconClass")) ? null : reader.GetString(reader.GetOrdinal("IconClass")),
                    DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                    IsChecked = reader.GetInt32(reader.GetOrdinal("IsChecked")) == 1
                });
            }

            return list;
        }

        public async Task<List<Menu>> GetMenusByRoleIdAsync(int roleId)
        {
            const string sql = @"
                SELECT 
                    m.Id, m.Name, m.Controller, m.Action, m.Url, m.IconClass,
                    m.ParentId, m.SubParentId, m.SubChildId, m.DisplayOrder, m.Module, m.IsActive, m.CreatedAt,
                    (SELECT COUNT(1) FROM dbo.Menu c 
                     INNER JOIN dbo.RoleMenu crm ON crm.MenuId = c.Id
                     WHERE c.ParentId = m.Id AND c.IsActive = 1 AND crm.RoleId = @RoleId) AS TotalChild,
                    0 AS TotalSubChild
                FROM dbo.Menu m
                INNER JOIN dbo.RoleMenu rm ON m.Id = rm.MenuId
                WHERE m.IsActive = 1 AND rm.RoleId = @RoleId
                ORDER BY m.ParentId ASC, m.DisplayOrder ASC";

            var list = new List<Menu>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapMenu(reader));
            }
            return list;
        }

        public async Task<List<Menu>> GetMenusByUserIdAsync(int userId)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            // Check if user is Admin or has active role
            const string userRoleSql = @"
                SELECT TOP 1 u.RoleID, r.Name AS RoleName
                FROM dbo.Users u
                LEFT JOIN dbo.Role r ON u.RoleID = r.Id
                WHERE u.UserID = @UserID AND u.IsActive = 1";

            int? roleId = null;
            string? roleName = null;

            using (var userCmd = CreateCommand(userRoleSql, conn))
            {
                userCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                using var reader = await userCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("RoleID")))
                    {
                        roleId = reader.GetInt32(reader.GetOrdinal("RoleID"));
                    }
                    if (!reader.IsDBNull(reader.GetOrdinal("RoleName")))
                    {
                        roleName = reader.GetString(reader.GetOrdinal("RoleName"));
                    }
                }
            }

            if (!roleId.HasValue)
            {
                // Fallback to checking UserRole table
                const string fallbackSql = @"
                    SELECT TOP 1 ur.RoleID, r.Name AS RoleName
                    FROM dbo.UserRole ur
                    INNER JOIN dbo.Role r ON ur.RoleID = r.Id
                    WHERE ur.UserID = @UserID AND r.IsActive = 1";

                using var fbCmd = CreateCommand(fallbackSql, conn);
                fbCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                using var reader = await fbCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    roleId = reader.GetInt32(reader.GetOrdinal("RoleID"));
                    roleName = reader.GetString(reader.GetOrdinal("RoleName"));
                }
            }

                // If user is Admin, return all active menus
                if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    const string allMenusSql = @"
                        SELECT 
                            m.Id, m.Name, m.Controller, m.Action, m.Url, m.IconClass,
                            m.ParentId, m.SubParentId, m.SubChildId, m.DisplayOrder, m.Module, m.IsActive, m.CreatedAt,
                            (SELECT COUNT(1) FROM dbo.Menu c WHERE c.ParentId = m.Id AND c.IsActive = 1) AS TotalChild,
                            0 AS TotalSubChild
                        FROM dbo.Menu m
                        WHERE m.IsActive = 1
                        ORDER BY 
                            CASE WHEN m.ParentId = 0 THEN m.Id ELSE m.ParentId END ASC,
                            CASE WHEN m.ParentId = 0 THEN 0 ELSE 1 END ASC,
                            m.DisplayOrder ASC";

                    var allMenus = new List<Menu>();
                    using var allCmd = CreateCommand(allMenusSql, conn);
                    using var reader = await allCmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        allMenus.Add(MapMenu(reader));
                    }
                    return allMenus;
                }

                if (!roleId.HasValue)
                {
                    // No role assigned, return empty list
                    return new List<Menu>();
                }

                // Get authorized menus for user via RoleMenu
                const string authMenusSql = @"
                    SELECT 
                        m.Id, m.Name, m.Controller, m.Action, m.Url, m.IconClass,
                        m.ParentId, m.SubParentId, m.SubChildId, m.DisplayOrder, m.Module, m.IsActive, m.CreatedAt,
                        (SELECT COUNT(1) FROM dbo.Menu c 
                         WHERE c.ParentId = m.Id AND c.IsActive = 1 
                           AND EXISTS (SELECT 1 FROM dbo.RoleMenu crm WHERE crm.MenuId = c.Id AND crm.RoleId = @RoleId)) AS TotalChild,
                        0 AS TotalSubChild
                    FROM dbo.Menu m
                    WHERE m.IsActive = 1 
                      AND EXISTS (SELECT 1 FROM dbo.RoleMenu rm WHERE rm.MenuId = m.Id AND rm.RoleId = @RoleId)
                    ORDER BY 
                        CASE WHEN m.ParentId = 0 THEN m.Id ELSE m.ParentId END ASC,
                        CASE WHEN m.ParentId = 0 THEN 0 ELSE 1 END ASC,
                        m.DisplayOrder ASC";

                var list = new List<Menu>();
                using (var cmd = CreateCommand(authMenusSql, conn))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId.Value;
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        list.Add(MapMenu(reader));
                    }
                }

                return list;
        }

        public async Task<bool> SaveRoleMenusAsync(int roleId, List<int> menuIds, string? createdBy)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Delete existing RoleMenu records for this role
                const string deleteSql = "DELETE FROM dbo.RoleMenu WHERE RoleId = @RoleId";
                using (var delCmd = conn.CreateCommand())
                {
                    delCmd.Transaction = transaction;
                    delCmd.CommandText = deleteSql;
                    delCmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
                    await delCmd.ExecuteNonQueryAsync();
                }

                if (menuIds == null || menuIds.Count == 0)
                {
                    await transaction.CommitAsync();
                    return true;
                }

                // 2. Expand selected menuIds to include their parent menus (so sidebar hierarchy is preserved)
                var allMenus = new List<Menu>();
                const string getMenuHierarchySql = "SELECT Id, ParentId FROM dbo.Menu WHERE IsActive = 1";
                using (var hierarchyCmd = conn.CreateCommand())
                {
                    hierarchyCmd.Transaction = transaction;
                    hierarchyCmd.CommandText = getMenuHierarchySql;
                    using var reader = await hierarchyCmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        allMenus.Add(new Menu
                        {
                            Id = reader.GetInt32(0),
                            ParentId = reader.GetInt32(1)
                        });
                    }
                }

                var finalMenuIds = new HashSet<int>(menuIds);
                foreach (var id in menuIds)
                {
                    var current = allMenus.FirstOrDefault(m => m.Id == id);
                    while (current != null && current.ParentId > 0)
                    {
                        finalMenuIds.Add(current.ParentId);
                        current = allMenus.FirstOrDefault(m => m.Id == current.ParentId);
                    }
                }

                // 3. Insert each distinct menu into dbo.RoleMenu
                const string insertSql = @"
                    INSERT INTO dbo.RoleMenu (RoleId, MenuId, CreatedAt, CreatedBy)
                    VALUES (@RoleId, @MenuId, GETDATE(), @CreatedBy)";

                foreach (var mId in finalMenuIds)
                {
                    using var insCmd = conn.CreateCommand();
                    insCmd.Transaction = transaction;
                    insCmd.CommandText = insertSql;
                    insCmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
                    insCmd.Parameters.Add("@MenuId", SqlDbType.Int).Value = mId;
                    insCmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 100).Value = (object?)createdBy ?? DBNull.Value;
                    await insCmd.ExecuteNonQueryAsync();
                }

                // 4. Synchronize dbo.UserMenu for all users having this Role if table exists
                const string syncUserMenuSql = @"
                    IF OBJECT_ID('dbo.UserMenu', 'U') IS NOT NULL
                    BEGIN
                        DELETE FROM dbo.UserMenu WHERE RoleId = @RoleId;
                        INSERT INTO dbo.UserMenu (UserId, RoleId, MenuId, List, [Insert], [Delete], Post, CreatedAt, CreatedBy)
                        SELECT u.UserID, @RoleId, rm.MenuId, 1, 1, 1, 1, GETDATE(), @CreatedBy
                        FROM dbo.Users u
                        CROSS JOIN dbo.RoleMenu rm
                        WHERE u.RoleID = @RoleId AND rm.RoleId = @RoleId AND u.IsActive = 1;
                    END";

                using (var syncCmd = conn.CreateCommand())
                {
                    syncCmd.Transaction = transaction;
                    syncCmd.CommandText = syncUserMenuSql;
                    syncCmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
                    syncCmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 100).Value = (object?)createdBy ?? DBNull.Value;
                    await syncCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> IsUserAuthorizedAsync(int userId, string controller, string action)
        {
            if (string.IsNullOrWhiteSpace(controller))
            {
                return true;
            }

            // Controllers that are accessible to all authenticated users
            if (controller.Equals("Home", StringComparison.OrdinalIgnoreCase) ||
                controller.Equals("Login", StringComparison.OrdinalIgnoreCase) ||
                controller.Equals("Common", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            using var conn = GetConnection();
            await conn.OpenAsync();

            // Check if user is active and whether they have Admin role
            const string checkUserSql = @"
                SELECT TOP 1 
                    u.UserName,
                    ISNULL(u.RoleID, ur.RoleID) AS RoleID, 
                    ISNULL(r.Name, r2.Name) AS RoleName, 
                    u.IsActive
                FROM dbo.Users u
                LEFT JOIN dbo.Role r ON u.RoleID = r.Id
                LEFT JOIN dbo.UserRole ur ON u.UserID = ur.UserID
                LEFT JOIN dbo.Role r2 ON ur.RoleID = r2.Id
                WHERE u.UserID = @UserID";

            int? roleId = null;
            using (var userCmd = CreateCommand(checkUserSql, conn))
            {
                userCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                using var reader = await userCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    bool isActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
                    if (!isActive) return false;

                    string? userName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : reader.GetString(reader.GetOrdinal("UserName"));
                    string? roleName = reader.IsDBNull(reader.GetOrdinal("RoleName")) ? null : reader.GetString(reader.GetOrdinal("RoleName"));
                    if (!reader.IsDBNull(reader.GetOrdinal("RoleID")))
                    {
                        roleId = reader.GetInt32(reader.GetOrdinal("RoleID"));
                    }

                    if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(userName, "admin", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (!roleId.HasValue)
            {
                return false;
            }

            // Check if user has permission to this controller via RoleMenu
            const string authCheckSql = @"
                SELECT COUNT(1)
                FROM dbo.Menu m
                WHERE m.IsActive = 1 
                  AND m.Controller = @Controller
                  AND EXISTS (
                      SELECT 1 FROM dbo.RoleMenu rm 
                      WHERE rm.MenuId = m.Id AND rm.RoleId = @RoleId
                  )";

            using (var authCmd = CreateCommand(authCheckSql, conn))
            {
                authCmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId.Value;
                authCmd.Parameters.Add("@Controller", SqlDbType.NVarChar, 100).Value = controller.Trim();

                int count = Convert.ToInt32(await authCmd.ExecuteScalarAsync() ?? 0);
                return count > 0;
            }
        }

        private static Menu MapMenu(SqlDataReader reader)
        {
            return new Menu
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Controller = reader.IsDBNull(reader.GetOrdinal("Controller")) ? null : reader.GetString(reader.GetOrdinal("Controller")),
                Action = reader.IsDBNull(reader.GetOrdinal("Action")) ? null : reader.GetString(reader.GetOrdinal("Action")),
                Url = reader.IsDBNull(reader.GetOrdinal("Url")) ? null : reader.GetString(reader.GetOrdinal("Url")),
                IconClass = reader.IsDBNull(reader.GetOrdinal("IconClass")) ? null : reader.GetString(reader.GetOrdinal("IconClass")),
                ParentId = reader.GetInt32(reader.GetOrdinal("ParentId")),
                SubParentId = reader.GetInt32(reader.GetOrdinal("SubParentId")),
                SubChildId = reader.GetInt32(reader.GetOrdinal("SubChildId")),
                DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                Module = reader.IsDBNull(reader.GetOrdinal("Module")) ? null : reader.GetString(reader.GetOrdinal("Module")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                TotalChild = reader.IsDBNull(reader.GetOrdinal("TotalChild")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalChild")),
                TotalSubChild = reader.IsDBNull(reader.GetOrdinal("TotalSubChild")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalSubChild"))
            };
        }
    }
}

