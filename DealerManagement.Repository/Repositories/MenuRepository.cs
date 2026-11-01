using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using DealerManagement.Model.Entities;
using DealerManagement.Repository.Base;
using DealerManagement.Repository.Interfaces;

namespace DealerManagement.Repository.Repositories
{
    public class MenuRepository : BaseRepository, IMenuRepository
    {
        public MenuRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<Menu>> GetActiveMenusAsync()
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            // Check if full menus need to be seeded into dbo.Menu
            const string checkSql = "SELECT COUNT(1) FROM dbo.Menu WHERE Controller IS NOT NULL AND Controller NOT IN ('Home', 'Login')";
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = checkSql;
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync() ?? 0);
                if (count < 20)
                {
                    await SeedAllMenusInternalAsync(conn);
                }
            }

            const string sql = @"
                SELECT 
                    M.Id,
                    M.[Name],
                    M.Controller,
                    M.[Action],
                    M.[Url],
                    M.IconClass,
                    M.ParentId,
                    M.SubParentId,
                    M.SubChildId,
                    M.DisplayOrder,
                    M.Module,
                    M.IsActive,
                    M.CreatedAt,
                    (SELECT COUNT(1) FROM dbo.Menu C WHERE C.ParentId = M.Id AND C.IsActive = 1) AS TotalChild,
                    (SELECT COUNT(1) FROM dbo.Menu SC WHERE SC.SubChildId = M.Id AND SC.IsActive = 1) AS TotalSubChild
                FROM dbo.Menu M
                WHERE M.IsActive = 1
                ORDER BY M.ParentId ASC, M.DisplayOrder ASC";

            var list = new List<Menu>();
            using (var cmd = CreateCommand(sql, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(MapMenu(reader));
                }
            }

            return list;
        }

        public async Task<List<Menu>> GetAllAsync()
        {
            const string sql = @"
                SELECT 
                    M.Id,
                    M.[Name],
                    M.Controller,
                    M.[Action],
                    M.[Url],
                    M.IconClass,
                    M.ParentId,
                    M.SubParentId,
                    M.SubChildId,
                    M.DisplayOrder,
                    M.Module,
                    M.IsActive,
                    M.CreatedAt,
                    (SELECT COUNT(1) FROM dbo.Menu C WHERE C.ParentId = M.Id) AS TotalChild,
                    (SELECT COUNT(1) FROM dbo.Menu SC WHERE SC.SubChildId = M.Id) AS TotalSubChild
                FROM dbo.Menu M
                ORDER BY M.ParentId ASC, M.DisplayOrder ASC";

            var list = new List<Menu>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapMenu(reader));
            }
            return list;
        }

        public async Task<Menu?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT TOP 1
                    M.Id,
                    M.[Name],
                    M.Controller,
                    M.[Action],
                    M.[Url],
                    M.IconClass,
                    M.ParentId,
                    M.SubParentId,
                    M.SubChildId,
                    M.DisplayOrder,
                    M.Module,
                    M.IsActive,
                    M.CreatedAt,
                    (SELECT COUNT(1) FROM dbo.Menu C WHERE C.ParentId = M.Id) AS TotalChild,
                    (SELECT COUNT(1) FROM dbo.Menu SC WHERE SC.SubChildId = M.Id) AS TotalSubChild
                FROM dbo.Menu M
                WHERE M.Id = @Id";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapMenu(reader);
            }
            return null;
        }

        private async Task SeedAllMenusInternalAsync(SqlConnection conn)
        {
            using var transaction = conn.BeginTransaction();
            try
            {
                const string clearSql = @"
                    DELETE FROM dbo.Menu;
                    DBCC CHECKIDENT ('dbo.Menu', RESEED, 0);";

                using (var clearCmd = conn.CreateCommand())
                {
                    clearCmd.Transaction = transaction;
                    clearCmd.CommandText = clearSql;
                    await clearCmd.ExecuteNonQueryAsync();
                }

                // Insert Level 1 Parents
                const string insertParentSql = @"
                    INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
                    OUTPUT INSERTED.Id
                    VALUES (@Name, @Controller, @Action, @Url, @IconClass, @ParentId, 0, 0, @DisplayOrder, @Module, 1, SYSDATETIME());";

                int dashboardId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertParentSql;
                    cmd.Parameters.AddWithValue("@Name", "Dashboard");
                    cmd.Parameters.AddWithValue("@Controller", "Home");
                    cmd.Parameters.AddWithValue("@Action", "Index");
                    cmd.Parameters.AddWithValue("@Url", "/Home/Index");
                    cmd.Parameters.AddWithValue("@IconClass", "fas fa-tachometer-alt");
                    cmd.Parameters.AddWithValue("@ParentId", 0);
                    cmd.Parameters.AddWithValue("@DisplayOrder", 1);
                    cmd.Parameters.AddWithValue("@Module", "Core");
                    dashboardId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                int masterSetupId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertParentSql;
                    cmd.Parameters.AddWithValue("@Name", "Master Setup");
                    cmd.Parameters.AddWithValue("@Controller", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Url", "#");
                    cmd.Parameters.AddWithValue("@IconClass", "fas fa-boxes");
                    cmd.Parameters.AddWithValue("@ParentId", 0);
                    cmd.Parameters.AddWithValue("@DisplayOrder", 2);
                    cmd.Parameters.AddWithValue("@Module", "Setup");
                    masterSetupId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                int stakeholdersId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertParentSql;
                    cmd.Parameters.AddWithValue("@Name", "Stakeholders");
                    cmd.Parameters.AddWithValue("@Controller", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Url", "#");
                    cmd.Parameters.AddWithValue("@IconClass", "fas fa-users");
                    cmd.Parameters.AddWithValue("@ParentId", 0);
                    cmd.Parameters.AddWithValue("@DisplayOrder", 3);
                    cmd.Parameters.AddWithValue("@Module", "Stakeholders");
                    stakeholdersId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                int salesId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertParentSql;
                    cmd.Parameters.AddWithValue("@Name", "Sales & Distribution");
                    cmd.Parameters.AddWithValue("@Controller", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Url", "#");
                    cmd.Parameters.AddWithValue("@IconClass", "fas fa-shopping-cart");
                    cmd.Parameters.AddWithValue("@ParentId", 0);
                    cmd.Parameters.AddWithValue("@DisplayOrder", 4);
                    cmd.Parameters.AddWithValue("@Module", "Sales");
                    salesId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                int purchaseId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertParentSql;
                    cmd.Parameters.AddWithValue("@Name", "Procurement & Purchase");
                    cmd.Parameters.AddWithValue("@Controller", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Url", "#");
                    cmd.Parameters.AddWithValue("@IconClass", "fas fa-truck-loading");
                    cmd.Parameters.AddWithValue("@ParentId", 0);
                    cmd.Parameters.AddWithValue("@DisplayOrder", 5);
                    cmd.Parameters.AddWithValue("@Module", "Purchase");
                    purchaseId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                int productionId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertParentSql;
                    cmd.Parameters.AddWithValue("@Name", "Production & Loss");
                    cmd.Parameters.AddWithValue("@Controller", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Url", "#");
                    cmd.Parameters.AddWithValue("@IconClass", "fas fa-industry");
                    cmd.Parameters.AddWithValue("@ParentId", 0);
                    cmd.Parameters.AddWithValue("@DisplayOrder", 6);
                    cmd.Parameters.AddWithValue("@Module", "Production");
                    productionId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                int financialsId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertParentSql;
                    cmd.Parameters.AddWithValue("@Name", "Financials & Collections");
                    cmd.Parameters.AddWithValue("@Controller", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Url", "#");
                    cmd.Parameters.AddWithValue("@IconClass", "fas fa-wallet");
                    cmd.Parameters.AddWithValue("@ParentId", 0);
                    cmd.Parameters.AddWithValue("@DisplayOrder", 7);
                    cmd.Parameters.AddWithValue("@Module", "Financials");
                    financialsId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                int adminId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertParentSql;
                    cmd.Parameters.AddWithValue("@Name", "Administration");
                    cmd.Parameters.AddWithValue("@Controller", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Url", "#");
                    cmd.Parameters.AddWithValue("@IconClass", "fas fa-cogs");
                    cmd.Parameters.AddWithValue("@ParentId", 0);
                    cmd.Parameters.AddWithValue("@DisplayOrder", 8);
                    cmd.Parameters.AddWithValue("@Module", "Admin");
                    adminId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                // Insert Children
                const string insertChildSql = @"
                    INSERT INTO dbo.Menu (Name, Controller, Action, Url, IconClass, ParentId, SubParentId, SubChildId, DisplayOrder, Module, IsActive, CreatedAt)
                    VALUES (@Name, @Controller, @Action, @Url, @IconClass, @ParentId, 0, 0, @DisplayOrder, @Module, 1, SYSDATETIME());";

                var children = new List<(string Name, string Controller, string Action, string Url, string IconClass, int ParentId, int DisplayOrder, string Module)>
                {
                    // 1. Master Setup
                    ("Product Category", "ProductCategory", "Index", "/ProductCategory/Index", "fas fa-tags", masterSetupId, 1, "Setup"),
                    ("Product Name", "ProductName", "Index", "/ProductName/Index", "fas fa-cube", masterSetupId, 2, "Setup"),
                    ("Product Definition", "ProductSetup", "Index", "/ProductSetup/Index", "fas fa-box-open", masterSetupId, 3, "Setup"),
                    ("Price Setup", "PriceSetup", "Index", "/PriceSetup/Index", "fas fa-money-bill-wave", masterSetupId, 4, "Setup"),
                    ("Product Color", "ProductColor", "Index", "/ProductColor/Index", "fas fa-palette", masterSetupId, 5, "Setup"),
                    ("Product Size", "ProductSize", "Index", "/ProductSize/Index", "fas fa-ruler-combined", masterSetupId, 6, "Setup"),
                    ("Product Grade", "ProductGrade", "Index", "/ProductGrade/Index", "fas fa-star", masterSetupId, 7, "Setup"),

                    // 2. Stakeholders
                    ("Customer / Dealer", "Customer", "Index", "/Customer/Index", "fas fa-user-tie", stakeholdersId, 1, "Stakeholders"),
                    ("Supplier Master", "Supplier", "Index", "/Supplier/Index", "fas fa-building", stakeholdersId, 2, "Stakeholders"),

                    // 3. Sales & Distribution
                    ("Customer Order", "CustomerOrder", "Index", "/CustomerOrder/Index", "fas fa-file-invoice", salesId, 1, "Sales"),
                    ("Delivery Challan", "CustomerDelivery", "Index", "/CustomerDelivery/Index", "fas fa-truck", salesId, 2, "Sales"),
                    ("Sales Return", "CustomerReturn", "Index", "/CustomerReturn/Index", "fas fa-undo-alt", salesId, 3, "Sales"),
                    ("Sample Delivery", "SampleDelivery", "Index", "/SampleDelivery/Index", "fas fa-gift", salesId, 4, "Sales"),

                    // 4. Procurement & Purchase
                    ("Purchase Order", "PurchaseOrder", "Index", "/PurchaseOrder/Index", "fas fa-clipboard-list", purchaseId, 1, "Purchase"),
                    ("Purchase Receipt (MRR)", "PurchaseReceipt", "Index", "/PurchaseReceipt/Index", "fas fa-warehouse", purchaseId, 2, "Purchase"),
                    ("Purchase Return", "PurchaseReturn", "Index", "/PurchaseReturn/Index", "fas fa-truck-loading", purchaseId, 3, "Purchase"),

                    // 5. Production & Loss
                    ("Production Entry", "Production", "Index", "/Production/Index", "fas fa-cogs", productionId, 1, "Production"),
                    ("Damage & Missing", "ProductDamage", "Index", "/ProductDamage/Index", "fas fa-exclamation-triangle", productionId, 2, "Production"),

                    // 6. Financials & Collections
                    ("Dealer Collection", "CustomerDeposit", "Index", "/CustomerDeposit/Index", "fas fa-money-check-alt", financialsId, 1, "Financials"),
                    ("Dealer Commission", "CustomerCommission", "Index", "/CustomerCommission/Index", "fas fa-percentage", financialsId, 2, "Financials"),
                    ("Dealer Opening Balance", "CustomerOpeningBalance", "Index", "/CustomerOpeningBalance/Index", "fas fa-book-open", financialsId, 3, "Financials"),
                    ("Supplier Payment", "SupplierDeposit", "Index", "/SupplierDeposit/Index", "fas fa-hand-holding-usd", financialsId, 4, "Financials"),
                    ("Supplier Rebate", "SupplierCommission", "Index", "/SupplierCommission/Index", "fas fa-tag", financialsId, 5, "Financials"),
                    ("Supplier Opening Balance", "SupplierOpeningBalance", "Index", "/SupplierOpeningBalance/Index", "fas fa-book", financialsId, 6, "Financials"),

                    // 7. Administration
                    ("User Management", "User", "Index", "/User/Index", "fas fa-users-cog", adminId, 1, "Admin")
                };

                foreach (var child in children)
                {
                    using var cCmd = conn.CreateCommand();
                    cCmd.Transaction = transaction;
                    cCmd.CommandText = insertChildSql;
                    cCmd.Parameters.AddWithValue("@Name", child.Name);
                    cCmd.Parameters.AddWithValue("@Controller", child.Controller);
                    cCmd.Parameters.AddWithValue("@Action", child.Action);
                    cCmd.Parameters.AddWithValue("@Url", child.Url);
                    cCmd.Parameters.AddWithValue("@IconClass", child.IconClass);
                    cCmd.Parameters.AddWithValue("@ParentId", child.ParentId);
                    cCmd.Parameters.AddWithValue("@DisplayOrder", child.DisplayOrder);
                    cCmd.Parameters.AddWithValue("@Module", child.Module);
                    await cCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
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
                TotalChild = reader.GetInt32(reader.GetOrdinal("TotalChild")),
                TotalSubChild = reader.GetInt32(reader.GetOrdinal("TotalSubChild"))
            };
        }
    }
}
