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
    public class PriceSetupRepository : BaseRepository, IPriceSetupRepository
    {
        public PriceSetupRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<PriceSetup>> GetAllAsync()
        {
            const string sql = @"
                SELECT 
                    pr.PriceSetupID, pr.ProductID, pr.PriceType, pr.UnitPrice, pr.EffectiveFrom, pr.EffectiveTo, pr.IsActive, pr.CreatedAt,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.PriceSetup pr
                INNER JOIN dbo.ProductSetup p ON pr.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                ORDER BY pr.EffectiveFrom DESC";

            var list = new List<PriceSetup>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapPrice(reader));
            }
            return list;
        }

        public async Task<PriceSetup?> GetByIdAsync(long id)
        {
            const string sql = @"
                SELECT TOP 1
                    pr.PriceSetupID, pr.ProductID, pr.PriceType, pr.UnitPrice, pr.EffectiveFrom, pr.EffectiveTo, pr.IsActive, pr.CreatedAt,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.PriceSetup pr
                INNER JOIN dbo.ProductSetup p ON pr.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                WHERE pr.PriceSetupID = @ID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapPrice(reader);
            }
            return null;
        }

        public async Task<(List<PriceSetup> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    pr.PriceSetupID, pr.ProductID, pr.PriceType, pr.UnitPrice, pr.EffectiveFrom, pr.EffectiveTo, pr.IsActive, pr.CreatedAt,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.PriceSetup pr
                INNER JOIN dbo.ProductSetup p ON pr.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.PriceSetup pr
                INNER JOIN dbo.ProductSetup p ON pr.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID";

            string filterCondition = options.filter != null ? GridQueryBuilder<PriceSetup>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<PriceSetup>.Query(options, baseQuery, "pr.PriceSetupID DESC");

            var list = new List<PriceSetup>();
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
                    list.Add(MapPrice(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<long> InsertAsync(PriceSetup model)
        {
            const string sql = @"
                INSERT INTO dbo.PriceSetup 
                    (ProductID, PriceType, UnitPrice, EffectiveFrom, EffectiveTo, IsActive, CreatedAt)
                OUTPUT INSERTED.PriceSetupID
                VALUES 
                    (@ProductID, @PriceType, @UnitPrice, @EffectiveFrom, @EffectiveTo, @IsActive, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = model.ProductID;
            cmd.Parameters.Add("@PriceType", SqlDbType.NVarChar, 30).Value = model.PriceType.Trim();
            cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = model.UnitPrice;
            cmd.Parameters.Add("@EffectiveFrom", SqlDbType.Date).Value = model.EffectiveFrom;
            cmd.Parameters.Add("@EffectiveTo", SqlDbType.Date).Value = (object?)model.EffectiveTo ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(id);
        }

        public async Task<bool> UpdateAsync(PriceSetup model)
        {
            const string sql = @"
                UPDATE dbo.PriceSetup
                SET ProductID = @ProductID,
                    PriceType = @PriceType,
                    UnitPrice = @UnitPrice,
                    EffectiveFrom = @EffectiveFrom,
                    EffectiveTo = @EffectiveTo,
                    IsActive = @IsActive
                WHERE PriceSetupID = @PriceSetupID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = model.ProductID;
            cmd.Parameters.Add("@PriceType", SqlDbType.NVarChar, 30).Value = model.PriceType.Trim();
            cmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = model.UnitPrice;
            cmd.Parameters.Add("@EffectiveFrom", SqlDbType.Date).Value = model.EffectiveFrom;
            cmd.Parameters.Add("@EffectiveTo", SqlDbType.Date).Value = (object?)model.EffectiveTo ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
            cmd.Parameters.Add("@PriceSetupID", SqlDbType.BigInt).Value = model.PriceSetupID;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            const string sql = "DELETE FROM dbo.PriceSetup WHERE PriceSetupID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        private static PriceSetup MapPrice(SqlDataReader reader)
        {
            return new PriceSetup
            {
                PriceSetupID = reader.GetInt64(reader.GetOrdinal("PriceSetupID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                PriceType = reader.GetString(reader.GetOrdinal("PriceType")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                EffectiveFrom = reader.GetDateTime(reader.GetOrdinal("EffectiveFrom")),
                EffectiveTo = reader.IsDBNull(reader.GetOrdinal("EffectiveTo")) ? null : reader.GetDateTime(reader.GetOrdinal("EffectiveTo")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                ProductNameText = reader.GetString(reader.GetOrdinal("ProductNameText")),
                UnitName = reader.GetString(reader.GetOrdinal("UnitName"))
            };
        }
    }
}

