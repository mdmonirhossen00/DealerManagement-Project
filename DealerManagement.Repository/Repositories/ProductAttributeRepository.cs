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
    public class ProductAttributeRepository : BaseRepository, IProductAttributeRepository
    {
        public ProductAttributeRepository(IConfiguration configuration) : base(configuration)
        {
        }

        #region Product Color
        public async Task<ProductColor?> GetColorByIdAsync(int id)
        {
            const string sql = "SELECT TOP 1 ProductColorID, ColorName, ColorCode, IsActive FROM dbo.ProductColor WHERE ProductColorID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ProductColor
                {
                    ProductColorID = reader.GetInt32(reader.GetOrdinal("ProductColorID")),
                    ColorName = reader.GetString(reader.GetOrdinal("ColorName")),
                    ColorCode = reader.IsDBNull(reader.GetOrdinal("ColorCode")) ? null : reader.GetString(reader.GetOrdinal("ColorCode")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                };
            }
            return null;
        }

        public async Task<(List<ProductColor> Items, int TotalCount)> GetColorGridDataAsync(GridOptions options)
        {
            const string baseQuery = "SELECT ProductColorID, ColorName, ColorCode, IsActive FROM dbo.ProductColor";
            string countQuery = "SELECT COUNT(1) FROM dbo.ProductColor";

            string filterCondition = options.filter != null ? GridQueryBuilder<ProductColor>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<ProductColor>.Query(options, baseQuery, "ProductColorID DESC");

            var list = new List<ProductColor>();
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
                    list.Add(new ProductColor
                    {
                        ProductColorID = reader.GetInt32(reader.GetOrdinal("ProductColorID")),
                        ColorName = reader.GetString(reader.GetOrdinal("ColorName")),
                        ColorCode = reader.IsDBNull(reader.GetOrdinal("ColorCode")) ? null : reader.GetString(reader.GetOrdinal("ColorCode")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    });
                }
            }

            return (list, totalCount);
        }

        public async Task<int> InsertColorAsync(ProductColor model)
        {
            const string sql = @"
                INSERT INTO dbo.ProductColor (ColorName, ColorCode, IsActive)
                OUTPUT INSERTED.ProductColorID
                VALUES (@ColorName, @ColorCode, @IsActive)";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ColorName", SqlDbType.NVarChar, 50).Value = model.ColorName.Trim();
            cmd.Parameters.Add("@ColorCode", SqlDbType.NVarChar, 20).Value = (object?)model.ColorCode ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<bool> UpdateColorAsync(ProductColor model)
        {
            const string sql = @"
                UPDATE dbo.ProductColor 
                SET ColorName = @ColorName,
                    ColorCode = @ColorCode,
                    IsActive = @IsActive
                WHERE ProductColorID = @ProductColorID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ColorName", SqlDbType.NVarChar, 50).Value = model.ColorName.Trim();
            cmd.Parameters.Add("@ColorCode", SqlDbType.NVarChar, 20).Value = (object?)model.ColorCode ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
            cmd.Parameters.Add("@ProductColorID", SqlDbType.Int).Value = model.ProductColorID;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteColorAsync(int id)
        {
            const string sql = "DELETE FROM dbo.ProductColor WHERE ProductColorID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        #endregion

        #region Product Size
        public async Task<ProductSize?> GetSizeByIdAsync(int id)
        {
            const string sql = "SELECT TOP 1 ProductSizeID, SizeName, SortOrder, IsActive FROM dbo.ProductSize WHERE ProductSizeID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ProductSize
                {
                    ProductSizeID = reader.GetInt32(reader.GetOrdinal("ProductSizeID")),
                    SizeName = reader.GetString(reader.GetOrdinal("SizeName")),
                    SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                };
            }
            return null;
        }

        public async Task<(List<ProductSize> Items, int TotalCount)> GetSizeGridDataAsync(GridOptions options)
        {
            const string baseQuery = "SELECT ProductSizeID, SizeName, SortOrder, IsActive FROM dbo.ProductSize";
            string countQuery = "SELECT COUNT(1) FROM dbo.ProductSize";

            string filterCondition = options.filter != null ? GridQueryBuilder<ProductSize>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<ProductSize>.Query(options, baseQuery, "SortOrder ASC, ProductSizeID DESC");

            var list = new List<ProductSize>();
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
                    list.Add(new ProductSize
                    {
                        ProductSizeID = reader.GetInt32(reader.GetOrdinal("ProductSizeID")),
                        SizeName = reader.GetString(reader.GetOrdinal("SizeName")),
                        SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    });
                }
            }

            return (list, totalCount);
        }

        public async Task<int> InsertSizeAsync(ProductSize model)
        {
            const string sql = @"
                INSERT INTO dbo.ProductSize (SizeName, SortOrder, IsActive)
                OUTPUT INSERTED.ProductSizeID
                VALUES (@SizeName, @SortOrder, @IsActive)";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@SizeName", SqlDbType.NVarChar, 50).Value = model.SizeName.Trim();
            cmd.Parameters.Add("@SortOrder", SqlDbType.Int).Value = model.SortOrder;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<bool> UpdateSizeAsync(ProductSize model)
        {
            const string sql = @"
                UPDATE dbo.ProductSize 
                SET SizeName = @SizeName,
                    SortOrder = @SortOrder,
                    IsActive = @IsActive
                WHERE ProductSizeID = @ProductSizeID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@SizeName", SqlDbType.NVarChar, 50).Value = model.SizeName.Trim();
            cmd.Parameters.Add("@SortOrder", SqlDbType.Int).Value = model.SortOrder;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
            cmd.Parameters.Add("@ProductSizeID", SqlDbType.Int).Value = model.ProductSizeID;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteSizeAsync(int id)
        {
            const string sql = "DELETE FROM dbo.ProductSize WHERE ProductSizeID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        #endregion

        #region Product Grade
        public async Task<ProductGrade?> GetGradeByIdAsync(int id)
        {
            const string sql = "SELECT TOP 1 ProductGradeID, GradeName, Description, IsActive FROM dbo.ProductGrade WHERE ProductGradeID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ProductGrade
                {
                    ProductGradeID = reader.GetInt32(reader.GetOrdinal("ProductGradeID")),
                    GradeName = reader.GetString(reader.GetOrdinal("GradeName")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                };
            }
            return null;
        }

        public async Task<(List<ProductGrade> Items, int TotalCount)> GetGradeGridDataAsync(GridOptions options)
        {
            const string baseQuery = "SELECT ProductGradeID, GradeName, Description, IsActive FROM dbo.ProductGrade";
            string countQuery = "SELECT COUNT(1) FROM dbo.ProductGrade";

            string filterCondition = options.filter != null ? GridQueryBuilder<ProductGrade>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<ProductGrade>.Query(options, baseQuery, "ProductGradeID DESC");

            var list = new List<ProductGrade>();
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
                    list.Add(new ProductGrade
                    {
                        ProductGradeID = reader.GetInt32(reader.GetOrdinal("ProductGradeID")),
                        GradeName = reader.GetString(reader.GetOrdinal("GradeName")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    });
                }
            }

            return (list, totalCount);
        }

        public async Task<int> InsertGradeAsync(ProductGrade model)
        {
            const string sql = @"
                INSERT INTO dbo.ProductGrade (GradeName, Description, IsActive)
                OUTPUT INSERTED.ProductGradeID
                VALUES (@GradeName, @Description, @IsActive)";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@GradeName", SqlDbType.NVarChar, 50).Value = model.GradeName.Trim();
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 250).Value = (object?)model.Description ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<bool> UpdateGradeAsync(ProductGrade model)
        {
            const string sql = @"
                UPDATE dbo.ProductGrade 
                SET GradeName = @GradeName,
                    Description = @Description,
                    IsActive = @IsActive
                WHERE ProductGradeID = @ProductGradeID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@GradeName", SqlDbType.NVarChar, 50).Value = model.GradeName.Trim();
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 250).Value = (object?)model.Description ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
            cmd.Parameters.Add("@ProductGradeID", SqlDbType.Int).Value = model.ProductGradeID;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteGradeAsync(int id)
        {
            const string sql = "DELETE FROM dbo.ProductGrade WHERE ProductGradeID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        #endregion
    }
}

