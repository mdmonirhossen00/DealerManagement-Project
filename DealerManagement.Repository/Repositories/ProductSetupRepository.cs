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
    public class ProductSetupRepository : BaseRepository, IProductSetupRepository
    {
        public ProductSetupRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<ProductSetup>> GetAllAsync()
        {
            const string sql = @"
                SELECT 
                    p.ProductID, p.ProductCode, p.ProductNameID, p.ProductSizeID, p.ProductColorID, p.ProductGradeID,
                    p.Barcode, p.UnitName, p.ReorderLevel, p.IsActive, p.CreatedAt,
                    pn.ProductName AS ProductNameText, pc.CategoryName,
                    ps.SizeName, pcol.ColorName, pg.GradeName
                FROM dbo.ProductSetup p
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                INNER JOIN dbo.ProductCategory pc ON pn.ProductCategoryID = pc.ProductCategoryID
                LEFT JOIN dbo.ProductSize ps ON p.ProductSizeID = ps.ProductSizeID
                LEFT JOIN dbo.ProductColor pcol ON p.ProductColorID = pcol.ProductColorID
                LEFT JOIN dbo.ProductGrade pg ON p.ProductGradeID = pg.ProductGradeID
                ORDER BY pn.ProductName, p.ProductCode";

            var list = new List<ProductSetup>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapProductSetup(reader));
            }
            return list;
        }

        public async Task<ProductSetup?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT TOP 1
                    p.ProductID, p.ProductCode, p.ProductNameID, p.ProductSizeID, p.ProductColorID, p.ProductGradeID,
                    p.Barcode, p.UnitName, p.ReorderLevel, p.IsActive, p.CreatedAt,
                    pn.ProductName AS ProductNameText, pc.CategoryName,
                    ps.SizeName, pcol.ColorName, pg.GradeName
                FROM dbo.ProductSetup p
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                INNER JOIN dbo.ProductCategory pc ON pn.ProductCategoryID = pc.ProductCategoryID
                LEFT JOIN dbo.ProductSize ps ON p.ProductSizeID = ps.ProductSizeID
                LEFT JOIN dbo.ProductColor pcol ON p.ProductColorID = pcol.ProductColorID
                LEFT JOIN dbo.ProductGrade pg ON p.ProductGradeID = pg.ProductGradeID
                WHERE p.ProductID = @ID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapProductSetup(reader);
            }
            return null;
        }

        public async Task<(List<ProductSetup> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    p.ProductID, p.ProductCode, p.ProductNameID, p.ProductSizeID, p.ProductColorID, p.ProductGradeID,
                    p.Barcode, p.UnitName, p.ReorderLevel, p.IsActive, p.CreatedAt,
                    pn.ProductName AS ProductNameText, pc.CategoryName,
                    ps.SizeName, pcol.ColorName, pg.GradeName
                FROM dbo.ProductSetup p
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                INNER JOIN dbo.ProductCategory pc ON pn.ProductCategoryID = pc.ProductCategoryID
                LEFT JOIN dbo.ProductSize ps ON p.ProductSizeID = ps.ProductSizeID
                LEFT JOIN dbo.ProductColor pcol ON p.ProductColorID = pcol.ProductColorID
                LEFT JOIN dbo.ProductGrade pg ON p.ProductGradeID = pg.ProductGradeID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.ProductSetup p
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                INNER JOIN dbo.ProductCategory pc ON pn.ProductCategoryID = pc.ProductCategoryID
                LEFT JOIN dbo.ProductSize ps ON p.ProductSizeID = ps.ProductSizeID
                LEFT JOIN dbo.ProductColor pcol ON p.ProductColorID = pcol.ProductColorID
                LEFT JOIN dbo.ProductGrade pg ON p.ProductGradeID = pg.ProductGradeID";

            string filterCondition = options.filter != null ? GridQueryBuilder<ProductSetup>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<ProductSetup>.Query(options, baseQuery, "p.ProductID DESC");

            var list = new List<ProductSetup>();
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
                    list.Add(MapProductSetup(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<int> InsertAsync(ProductSetup model)
        {
            const string sql = @"
                INSERT INTO dbo.ProductSetup 
                    (ProductCode, ProductNameID, ProductSizeID, ProductColorID, ProductGradeID, Barcode, UnitName, ReorderLevel, IsActive, CreatedAt)
                OUTPUT INSERTED.ProductID
                VALUES 
                    (@ProductCode, @ProductNameID, @ProductSizeID, @ProductColorID, @ProductGradeID, @Barcode, @UnitName, @ReorderLevel, @IsActive, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ProductCode", SqlDbType.NVarChar, 80).Value = model.ProductCode.Trim();
            cmd.Parameters.Add("@ProductNameID", SqlDbType.Int).Value = model.ProductNameID;
            cmd.Parameters.Add("@ProductSizeID", SqlDbType.Int).Value = (object?)model.ProductSizeID ?? DBNull.Value;
            cmd.Parameters.Add("@ProductColorID", SqlDbType.Int).Value = (object?)model.ProductColorID ?? DBNull.Value;
            cmd.Parameters.Add("@ProductGradeID", SqlDbType.Int).Value = (object?)model.ProductGradeID ?? DBNull.Value;
            cmd.Parameters.Add("@Barcode", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(model.Barcode) ? DBNull.Value : model.Barcode.Trim();
            cmd.Parameters.Add("@UnitName", SqlDbType.NVarChar, 30).Value = model.UnitName.Trim();
            cmd.Parameters.Add("@ReorderLevel", SqlDbType.Decimal).Value = model.ReorderLevel;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<bool> UpdateAsync(ProductSetup model)
        {
            const string sql = @"
                UPDATE dbo.ProductSetup
                SET ProductCode = @ProductCode,
                    ProductNameID = @ProductNameID,
                    ProductSizeID = @ProductSizeID,
                    ProductColorID = @ProductColorID,
                    ProductGradeID = @ProductGradeID,
                    Barcode = @Barcode,
                    UnitName = @UnitName,
                    ReorderLevel = @ReorderLevel,
                    IsActive = @IsActive
                WHERE ProductID = @ProductID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ProductCode", SqlDbType.NVarChar, 80).Value = model.ProductCode.Trim();
            cmd.Parameters.Add("@ProductNameID", SqlDbType.Int).Value = model.ProductNameID;
            cmd.Parameters.Add("@ProductSizeID", SqlDbType.Int).Value = (object?)model.ProductSizeID ?? DBNull.Value;
            cmd.Parameters.Add("@ProductColorID", SqlDbType.Int).Value = (object?)model.ProductColorID ?? DBNull.Value;
            cmd.Parameters.Add("@ProductGradeID", SqlDbType.Int).Value = (object?)model.ProductGradeID ?? DBNull.Value;
            cmd.Parameters.Add("@Barcode", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(model.Barcode) ? DBNull.Value : model.Barcode.Trim();
            cmd.Parameters.Add("@UnitName", SqlDbType.NVarChar, 30).Value = model.UnitName.Trim();
            cmd.Parameters.Add("@ReorderLevel", SqlDbType.Decimal).Value = model.ReorderLevel;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
            cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = model.ProductID;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM dbo.ProductSetup WHERE ProductID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> ExistsByCodeAsync(string code, int excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.ProductSetup WHERE ProductCode = @Code AND ProductID <> @ExcludeID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 80).Value = code.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.Int).Value = excludeId;

            await conn.OpenAsync();
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        public async Task<bool> ExistsByBarcodeAsync(string barcode, int excludeId = 0)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return false;
            const string sql = "SELECT COUNT(1) FROM dbo.ProductSetup WHERE Barcode = @Barcode AND ProductID <> @ExcludeID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@Barcode", SqlDbType.NVarChar, 100).Value = barcode.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.Int).Value = excludeId;

            await conn.OpenAsync();
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        private static ProductSetup MapProductSetup(SqlDataReader reader)
        {
            return new ProductSetup
            {
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                ProductNameID = reader.GetInt32(reader.GetOrdinal("ProductNameID")),
                ProductSizeID = reader.IsDBNull(reader.GetOrdinal("ProductSizeID")) ? null : reader.GetInt32(reader.GetOrdinal("ProductSizeID")),
                ProductColorID = reader.IsDBNull(reader.GetOrdinal("ProductColorID")) ? null : reader.GetInt32(reader.GetOrdinal("ProductColorID")),
                ProductGradeID = reader.IsDBNull(reader.GetOrdinal("ProductGradeID")) ? null : reader.GetInt32(reader.GetOrdinal("ProductGradeID")),
                Barcode = reader.IsDBNull(reader.GetOrdinal("Barcode")) ? null : reader.GetString(reader.GetOrdinal("Barcode")),
                UnitName = reader.GetString(reader.GetOrdinal("UnitName")),
                ReorderLevel = reader.GetDecimal(reader.GetOrdinal("ReorderLevel")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                ProductNameText = reader.GetString(reader.GetOrdinal("ProductNameText")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                SizeName = reader.IsDBNull(reader.GetOrdinal("SizeName")) ? null : reader.GetString(reader.GetOrdinal("SizeName")),
                ColorName = reader.IsDBNull(reader.GetOrdinal("ColorName")) ? null : reader.GetString(reader.GetOrdinal("ColorName")),
                GradeName = reader.IsDBNull(reader.GetOrdinal("GradeName")) ? null : reader.GetString(reader.GetOrdinal("GradeName"))
            };
        }
    }
}

