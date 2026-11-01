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
    public class ProductDamageRepository : BaseRepository, IProductDamageRepository
    {
        public ProductDamageRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<ProductDamageMaster?> GetByIdWithDetailsAsync(long id)
        {
            const string masterSql = @"
                SELECT 
                    pdm.ProductDamageMasterID, pdm.DamageReportNo, pdm.ReportDate, pdm.ReportType,
                    pdm.LocationName, pdm.ReportedBy, pdm.Remarks, pdm.Status, pdm.CreatedAt,
                    ISNULL((SELECT SUM(pdd.Quantity) FROM dbo.ProductDamageOrBrokenOrMissingDetails pdd WHERE pdd.ProductDamageMasterID = pdm.ProductDamageMasterID), 0) AS TotalQuantity,
                    ISNULL((SELECT SUM(pdd.EstimatedLoss) FROM dbo.ProductDamageOrBrokenOrMissingDetails pdd WHERE pdd.ProductDamageMasterID = pdm.ProductDamageMasterID), 0) AS TotalLoss
                FROM dbo.ProductDamageOrBrokenOrMisdingMaster pdm
                WHERE pdm.ProductDamageMasterID = @ID";

            const string detailsSql = @"
                SELECT 
                    pdd.ProductDamageDetailID, pdd.ProductDamageMasterID, pdd.ProductID,
                    pdd.Quantity, pdd.EstimatedLoss, pdd.Reason,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.ProductDamageOrBrokenOrMissingDetails pdd
                INNER JOIN dbo.ProductSetup p ON pdd.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                WHERE pdd.ProductDamageMasterID = @ID";

            ProductDamageMaster? master = null;

            using var conn = GetConnection();
            await conn.OpenAsync();

            using (var cmd = CreateCommand(masterSql, conn))
            {
                cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    master = MapMaster(reader);
                }
            }

            if (master == null) return null;

            using (var dCmd = CreateCommand(detailsSql, conn))
            {
                dCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                using var reader = await dCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    master.DetailsList.Add(MapDetails(reader));
                }
            }

            return master;
        }

        public async Task<(List<ProductDamageMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    pdm.ProductDamageMasterID, pdm.DamageReportNo, pdm.ReportDate, pdm.ReportType,
                    pdm.LocationName, pdm.ReportedBy, pdm.Remarks, pdm.Status, pdm.CreatedAt,
                    ISNULL((SELECT SUM(pdd.Quantity) FROM dbo.ProductDamageOrBrokenOrMissingDetails pdd WHERE pdd.ProductDamageMasterID = pdm.ProductDamageMasterID), 0) AS TotalQuantity,
                    ISNULL((SELECT SUM(pdd.EstimatedLoss) FROM dbo.ProductDamageOrBrokenOrMissingDetails pdd WHERE pdd.ProductDamageMasterID = pdm.ProductDamageMasterID), 0) AS TotalLoss
                FROM dbo.ProductDamageOrBrokenOrMisdingMaster pdm";

            string countQuery = @"SELECT COUNT(1) FROM dbo.ProductDamageOrBrokenOrMisdingMaster pdm";

            string filterCondition = options.filter != null ? GridQueryBuilder<ProductDamageMaster>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<ProductDamageMaster>.Query(options, baseQuery, "pdm.ProductDamageMasterID DESC");

            var list = new List<ProductDamageMaster>();
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
                    list.Add(MapMaster(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<long> InsertAsync(ProductDamageMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertMasterSql = @"
                    INSERT INTO dbo.ProductDamageOrBrokenOrMisdingMaster 
                        (DamageReportNo, ReportDate, ReportType, LocationName, ReportedBy, Remarks, Status, CreatedAt)
                    OUTPUT INSERTED.ProductDamageMasterID
                    VALUES 
                        (@DamageReportNo, @ReportDate, @ReportType, @LocationName, @ReportedBy, @Remarks, @Status, SYSDATETIME())";

                long newMasterId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertMasterSql;
                    cmd.Parameters.Add("@DamageReportNo", SqlDbType.NVarChar, 30).Value = master.DamageReportNo.Trim();
                    cmd.Parameters.Add("@ReportDate", SqlDbType.Date).Value = master.ReportDate;
                    cmd.Parameters.Add("@ReportType", SqlDbType.NVarChar, 20).Value = master.ReportType;
                    cmd.Parameters.Add("@LocationName", SqlDbType.NVarChar, 100).Value = (object?)master.LocationName ?? DBNull.Value;
                    cmd.Parameters.Add("@ReportedBy", SqlDbType.NVarChar, 100).Value = (object?)master.ReportedBy ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;

                    newMasterId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.ProductDamageOrBrokenOrMissingDetails 
                            (ProductDamageMasterID, ProductID, Quantity, EstimatedLoss, Reason)
                        VALUES 
                            (@ProductDamageMasterID, @ProductID, @Quantity, @EstimatedLoss, @Reason)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@ProductDamageMasterID", SqlDbType.BigInt).Value = newMasterId;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@EstimatedLoss", SqlDbType.Decimal).Value = line.EstimatedLoss;
                        dCmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 250).Value = (object?)line.Reason ?? DBNull.Value;

                        await dCmd.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
                return newMasterId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(ProductDamageMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateMasterSql = @"
                    UPDATE dbo.ProductDamageOrBrokenOrMisdingMaster 
                    SET DamageReportNo = @DamageReportNo,
                        ReportDate = @ReportDate,
                        ReportType = @ReportType,
                        LocationName = @LocationName,
                        ReportedBy = @ReportedBy,
                        Remarks = @Remarks,
                        Status = @Status
                    WHERE ProductDamageMasterID = @ProductDamageMasterID";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = updateMasterSql;
                    cmd.Parameters.Add("@DamageReportNo", SqlDbType.NVarChar, 30).Value = master.DamageReportNo.Trim();
                    cmd.Parameters.Add("@ReportDate", SqlDbType.Date).Value = master.ReportDate;
                    cmd.Parameters.Add("@ReportType", SqlDbType.NVarChar, 20).Value = master.ReportType;
                    cmd.Parameters.Add("@LocationName", SqlDbType.NVarChar, 100).Value = (object?)master.LocationName ?? DBNull.Value;
                    cmd.Parameters.Add("@ReportedBy", SqlDbType.NVarChar, 100).Value = (object?)master.ReportedBy ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@ProductDamageMasterID", SqlDbType.BigInt).Value = master.ProductDamageMasterID;

                    await cmd.ExecuteNonQueryAsync();
                }

                const string deleteDetailsSql = "DELETE FROM dbo.ProductDamageOrBrokenOrMissingDetails WHERE ProductDamageMasterID = @ProductDamageMasterID";
                using (var delCmd = conn.CreateCommand())
                {
                    delCmd.Transaction = transaction;
                    delCmd.CommandText = deleteDetailsSql;
                    delCmd.Parameters.Add("@ProductDamageMasterID", SqlDbType.BigInt).Value = master.ProductDamageMasterID;
                    await delCmd.ExecuteNonQueryAsync();
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.ProductDamageOrBrokenOrMissingDetails 
                            (ProductDamageMasterID, ProductID, Quantity, EstimatedLoss, Reason)
                        VALUES 
                            (@ProductDamageMasterID, @ProductID, @Quantity, @EstimatedLoss, @Reason)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@ProductDamageMasterID", SqlDbType.BigInt).Value = master.ProductDamageMasterID;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@EstimatedLoss", SqlDbType.Decimal).Value = line.EstimatedLoss;
                        dCmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 250).Value = (object?)line.Reason ?? DBNull.Value;

                        await dCmd.ExecuteNonQueryAsync();
                    }
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

        public async Task<bool> DeleteAsync(long id)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string delDetailsSql = "DELETE FROM dbo.ProductDamageOrBrokenOrMissingDetails WHERE ProductDamageMasterID = @ID";
                using (var dCmd = conn.CreateCommand())
                {
                    dCmd.Transaction = transaction;
                    dCmd.CommandText = delDetailsSql;
                    dCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                    await dCmd.ExecuteNonQueryAsync();
                }

                const string delMasterSql = "DELETE FROM dbo.ProductDamageOrBrokenOrMisdingMaster WHERE ProductDamageMasterID = @ID";
                using (var mCmd = conn.CreateCommand())
                {
                    mCmd.Transaction = transaction;
                    mCmd.CommandText = delMasterSql;
                    mCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                    int rows = await mCmd.ExecuteNonQueryAsync();
                    await transaction.CommitAsync();
                    return rows > 0;
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ExistsByNoAsync(string reportNo, long excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.ProductDamageOrBrokenOrMisdingMaster WHERE DamageReportNo = @ReportNo AND ProductDamageMasterID <> @ExcludeID";
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ReportNo", SqlDbType.NVarChar, 30).Value = reportNo.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.BigInt).Value = excludeId;
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result ?? 0) > 0;
        }

        private static ProductDamageMaster MapMaster(SqlDataReader reader)
        {
            return new ProductDamageMaster
            {
                ProductDamageMasterID = reader.GetInt64(reader.GetOrdinal("ProductDamageMasterID")),
                DamageReportNo = reader.GetString(reader.GetOrdinal("DamageReportNo")),
                ReportDate = reader.GetDateTime(reader.GetOrdinal("ReportDate")),
                ReportType = reader.GetString(reader.GetOrdinal("ReportType")),
                LocationName = reader.IsDBNull(reader.GetOrdinal("LocationName")) ? null : reader.GetString(reader.GetOrdinal("LocationName")),
                ReportedBy = reader.IsDBNull(reader.GetOrdinal("ReportedBy")) ? null : reader.GetString(reader.GetOrdinal("ReportedBy")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                TotalQuantity = reader.IsDBNull(reader.GetOrdinal("TotalQuantity")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalQuantity")),
                TotalLoss = reader.IsDBNull(reader.GetOrdinal("TotalLoss")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalLoss"))
            };
        }

        private static ProductDamageDetails MapDetails(SqlDataReader reader)
        {
            return new ProductDamageDetails
            {
                ProductDamageDetailID = reader.GetInt64(reader.GetOrdinal("ProductDamageDetailID")),
                ProductDamageMasterID = reader.GetInt64(reader.GetOrdinal("ProductDamageMasterID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                Quantity = reader.GetDecimal(reader.GetOrdinal("Quantity")),
                EstimatedLoss = reader.GetDecimal(reader.GetOrdinal("EstimatedLoss")),
                Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason")),
                ProductCode = reader.IsDBNull(reader.GetOrdinal("ProductCode")) ? null : reader.GetString(reader.GetOrdinal("ProductCode")),
                ProductNameText = reader.IsDBNull(reader.GetOrdinal("ProductNameText")) ? null : reader.GetString(reader.GetOrdinal("ProductNameText")),
                UnitName = reader.IsDBNull(reader.GetOrdinal("UnitName")) ? null : reader.GetString(reader.GetOrdinal("UnitName"))
            };
        }
    }
}

