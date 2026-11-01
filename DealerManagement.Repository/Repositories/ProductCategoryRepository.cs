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
    public class ProductCategoryRepository : BaseRepository, IProductCategoryRepository
    {
        public ProductCategoryRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<ProductCategory>> GetAllAsync()
        {
            const string sql = "SELECT ProductCategoryID, CategoryName, Description, IsActive, CreatedAt FROM dbo.ProductCategory ORDER BY CategoryName";
            var list = new List<ProductCategory>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapCategory(reader));
            }
            return list;
        }

        public async Task<ProductCategory?> GetByIdAsync(int id)
        {
            const string sql = "SELECT TOP 1 ProductCategoryID, CategoryName, Description, IsActive, CreatedAt FROM dbo.ProductCategory WHERE ProductCategoryID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapCategory(reader);
            }
            return null;
        }

        public async Task<(List<ProductCategory> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = "SELECT ProductCategoryID, CategoryName, Description, IsActive, CreatedAt FROM dbo.ProductCategory";
            string countQuery = "SELECT COUNT(1) FROM dbo.ProductCategory";

            string filterCondition = options.filter != null ? GridQueryBuilder<ProductCategory>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<ProductCategory>.Query(options, baseQuery, "ProductCategoryID DESC");

            var list = new List<ProductCategory>();
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
                    list.Add(MapCategory(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<int> InsertAsync(ProductCategory model)
        {
            const string sql = @"
                INSERT INTO dbo.ProductCategory (CategoryName, Description, IsActive, CreatedAt)
                OUTPUT INSERTED.ProductCategoryID
                VALUES (@CategoryName, @Description, @IsActive, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@CategoryName", SqlDbType.NVarChar, 100).Value = model.CategoryName.Trim();
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object?)model.Description ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<bool> UpdateAsync(ProductCategory model)
        {
            const string sql = @"
                UPDATE dbo.ProductCategory
                SET CategoryName = @CategoryName,
                    Description = @Description,
                    IsActive = @IsActive
                WHERE ProductCategoryID = @ProductCategoryID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@CategoryName", SqlDbType.NVarChar, 100).Value = model.CategoryName.Trim();
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object?)model.Description ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
            cmd.Parameters.Add("@ProductCategoryID", SqlDbType.Int).Value = model.ProductCategoryID;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM dbo.ProductCategory WHERE ProductCategoryID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> ExistsByNameAsync(string name, int excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.ProductCategory WHERE CategoryName = @Name AND ProductCategoryID <> @ExcludeID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.Int).Value = excludeId;

            await conn.OpenAsync();
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        private static ProductCategory MapCategory(SqlDataReader reader)
        {
            return new ProductCategory
            {
                ProductCategoryID = reader.GetInt32(reader.GetOrdinal("ProductCategoryID")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}

