using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Repository.Base;
using DealerManagement.Repository.Interfaces;

namespace DealerManagement.Repository.Repositories
{
    public class ProductNameRepository : BaseRepository, IProductNameRepository
    {
        public ProductNameRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<ProductName>> GetAllAsync()
        {
            const string sql = @"
                SELECT pn.ProductNameID, pn.ProductCategoryID, pn.ProductCode, pn.ProductName AS [Name], pn.Description, pn.IsActive, pn.CreatedAt, pc.CategoryName
                FROM dbo.ProductName pn
                INNER JOIN dbo.ProductCategory pc ON pn.ProductCategoryID = pc.ProductCategoryID
                ORDER BY pn.ProductName";

            var list = new List<ProductName>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapProductName(reader));
            }
            return list;
        }

        public async Task<ProductName?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT TOP 1 pn.ProductNameID, pn.ProductCategoryID, pn.ProductCode, pn.ProductName AS [Name], pn.Description, pn.IsActive, pn.CreatedAt, pc.CategoryName
                FROM dbo.ProductName pn
                INNER JOIN dbo.ProductCategory pc ON pn.ProductCategoryID = pc.ProductCategoryID
                WHERE pn.ProductNameID = @ID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapProductName(reader);
            }
            return null;
        }

        public async Task<(List<ProductName> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT pn.ProductNameID, pn.ProductCategoryID, pn.ProductCode, pn.ProductName AS [Name], pn.Description, pn.IsActive, pn.CreatedAt, pc.CategoryName
                FROM dbo.ProductName pn
                INNER JOIN dbo.ProductCategory pc ON pn.ProductCategoryID = pc.ProductCategoryID";

            string countQuery = "SELECT COUNT(1) FROM dbo.ProductName pn INNER JOIN dbo.ProductCategory pc ON pn.ProductCategoryID = pc.ProductCategoryID";

            string filterCondition = options.filter != null ? GridQueryBuilder<ProductName>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<ProductName>.Query(options, baseQuery, "pn.ProductNameID DESC");

            var list = new List<ProductName>();
            int totalCount = 0;

            using var conn = GetConnection();
            await conn.OpenAsync();

            using (var countCmd = CreateCommand(countQuery, conn))
            {
                var result = await countCmd.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(result ?? 0);
            }

            using (var dataCmd = CreateCommand(pagedSql, conn))
            {
                using var reader = await dataCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(MapProductName(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<int> InsertAsync(ProductName model)
        {
            const string sql = @"
                INSERT INTO dbo.ProductName (ProductCategoryID, ProductCode, ProductName, Description, IsActive, CreatedAt)
                OUTPUT INSERTED.ProductNameID
                VALUES (@ProductCategoryID, @ProductCode, @ProductName, @Description, @IsActive, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ProductCategoryID", SqlDbType.Int).Value = model.ProductCategoryID;
            cmd.Parameters.Add("@ProductCode", SqlDbType.NVarChar, 50).Value = model.ProductCode.Trim();
            cmd.Parameters.Add("@ProductName", SqlDbType.NVarChar, 150).Value = model.Name.Trim();
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object?)model.Description ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<bool> UpdateAsync(ProductName model)
        {
            const string sql = @"
                UPDATE dbo.ProductName
                SET ProductCategoryID = @ProductCategoryID,
                    ProductCode = @ProductCode,
                    ProductName = @ProductName,
                    Description = @Description,
                    IsActive = @IsActive
                WHERE ProductNameID = @ProductNameID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ProductCategoryID", SqlDbType.Int).Value = model.ProductCategoryID;
            cmd.Parameters.Add("@ProductCode", SqlDbType.NVarChar, 50).Value = model.ProductCode.Trim();
            cmd.Parameters.Add("@ProductName", SqlDbType.NVarChar, 150).Value = model.Name.Trim();
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object?)model.Description ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
            cmd.Parameters.Add("@ProductNameID", SqlDbType.Int).Value = model.ProductNameID;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM dbo.ProductName WHERE ProductNameID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> ExistsByCodeAsync(string code, int excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.ProductName WHERE ProductCode = @Code AND ProductNameID <> @ExcludeID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 50).Value = code.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.Int).Value = excludeId;

            await conn.OpenAsync();
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        private static ProductName MapProductName(SqlDataReader reader)
        {
            return new ProductName
            {
                ProductNameID = reader.GetInt32(reader.GetOrdinal("ProductNameID")),
                ProductCategoryID = reader.GetInt32(reader.GetOrdinal("ProductCategoryID")),
                ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
            };
        }
    }
}

