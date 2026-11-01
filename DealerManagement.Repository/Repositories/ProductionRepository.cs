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
    public class ProductionRepository : BaseRepository, IProductionRepository
    {
        public ProductionRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<ProductionMaster?> GetByIdWithDetailsAsync(long id)
        {
            const string masterSql = @"
                SELECT 
                    pm.ProductionID, pm.ProductionNo, pm.ProductionDate, pm.Remarks,
                    pm.Status, pm.CreatedBy, pm.CreatedAt,
                    ISNULL((SELECT SUM(pd.Quantity) FROM dbo.ProductionDetails pd WHERE pd.ProductionID = pm.ProductionID), 0) AS TotalQuantity
                FROM dbo.ProductionMaster pm
                WHERE pm.ProductionID = @ID";

            const string detailsSql = @"
                SELECT 
                    pd.ProductionDetailID, pd.ProductionID, pd.ProductID,
                    pd.Quantity, pd.UnitCost, pd.Remarks,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.ProductionDetails pd
                INNER JOIN dbo.ProductSetup p ON pd.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                WHERE pd.ProductionID = @ID";

            ProductionMaster? master = null;

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

        public async Task<(List<ProductionMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    pm.ProductionID, pm.ProductionNo, pm.ProductionDate, pm.Remarks,
                    pm.Status, pm.CreatedBy, pm.CreatedAt,
                    ISNULL((SELECT SUM(pd.Quantity) FROM dbo.ProductionDetails pd WHERE pd.ProductionID = pm.ProductionID), 0) AS TotalQuantity
                FROM dbo.ProductionMaster pm";

            string countQuery = @"SELECT COUNT(1) FROM dbo.ProductionMaster pm";

            string filterCondition = options.filter != null ? GridQueryBuilder<ProductionMaster>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<ProductionMaster>.Query(options, baseQuery, "pm.ProductionID DESC");

            var list = new List<ProductionMaster>();
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

        public async Task<long> InsertAsync(ProductionMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertMasterSql = @"
                    INSERT INTO dbo.ProductionMaster 
                        (ProductionNo, ProductionDate, Remarks, Status, CreatedBy, CreatedAt)
                    OUTPUT INSERTED.ProductionID
                    VALUES 
                        (@ProductionNo, @ProductionDate, @Remarks, @Status, @CreatedBy, SYSDATETIME())";

                long newMasterId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertMasterSql;
                    cmd.Parameters.Add("@ProductionNo", SqlDbType.NVarChar, 30).Value = master.ProductionNo.Trim();
                    cmd.Parameters.Add("@ProductionDate", SqlDbType.Date).Value = master.ProductionDate;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 100).Value = (object?)master.CreatedBy ?? DBNull.Value;

                    newMasterId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.ProductionDetails 
                            (ProductionID, ProductID, Quantity, UnitCost, Remarks)
                        VALUES 
                            (@ProductionID, @ProductID, @Quantity, @UnitCost, @Remarks)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@ProductionID", SqlDbType.BigInt).Value = newMasterId;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitCost", SqlDbType.Decimal).Value = (object?)line.UnitCost ?? DBNull.Value;
                        dCmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 250).Value = (object?)line.Remarks ?? DBNull.Value;

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

        public async Task<bool> UpdateAsync(ProductionMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateMasterSql = @"
                    UPDATE dbo.ProductionMaster 
                    SET ProductionNo = @ProductionNo,
                        ProductionDate = @ProductionDate,
                        Remarks = @Remarks,
                        Status = @Status
                    WHERE ProductionID = @ProductionID";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = updateMasterSql;
                    cmd.Parameters.Add("@ProductionNo", SqlDbType.NVarChar, 30).Value = master.ProductionNo.Trim();
                    cmd.Parameters.Add("@ProductionDate", SqlDbType.Date).Value = master.ProductionDate;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@ProductionID", SqlDbType.BigInt).Value = master.ProductionID;

                    await cmd.ExecuteNonQueryAsync();
                }

                const string deleteDetailsSql = "DELETE FROM dbo.ProductionDetails WHERE ProductionID = @ProductionID";
                using (var delCmd = conn.CreateCommand())
                {
                    delCmd.Transaction = transaction;
                    delCmd.CommandText = deleteDetailsSql;
                    delCmd.Parameters.Add("@ProductionID", SqlDbType.BigInt).Value = master.ProductionID;
                    await delCmd.ExecuteNonQueryAsync();
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.ProductionDetails 
                            (ProductionID, ProductID, Quantity, UnitCost, Remarks)
                        VALUES 
                            (@ProductionID, @ProductID, @Quantity, @UnitCost, @Remarks)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@ProductionID", SqlDbType.BigInt).Value = master.ProductionID;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitCost", SqlDbType.Decimal).Value = (object?)line.UnitCost ?? DBNull.Value;
                        dCmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 250).Value = (object?)line.Remarks ?? DBNull.Value;

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
                const string delDetailsSql = "DELETE FROM dbo.ProductionDetails WHERE ProductionID = @ID";
                using (var dCmd = conn.CreateCommand())
                {
                    dCmd.Transaction = transaction;
                    dCmd.CommandText = delDetailsSql;
                    dCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                    await dCmd.ExecuteNonQueryAsync();
                }

                const string delMasterSql = "DELETE FROM dbo.ProductionMaster WHERE ProductionID = @ID";
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

        public async Task<bool> ExistsByNoAsync(string prodNo, long excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.ProductionMaster WHERE ProductionNo = @ProductionNo AND ProductionID <> @ExcludeID";
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ProductionNo", SqlDbType.NVarChar, 30).Value = prodNo.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.BigInt).Value = excludeId;
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result ?? 0) > 0;
        }

        private static ProductionMaster MapMaster(SqlDataReader reader)
        {
            return new ProductionMaster
            {
                ProductionID = reader.GetInt64(reader.GetOrdinal("ProductionID")),
                ProductionNo = reader.GetString(reader.GetOrdinal("ProductionNo")),
                ProductionDate = reader.GetDateTime(reader.GetOrdinal("ProductionDate")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                TotalQuantity = reader.IsDBNull(reader.GetOrdinal("TotalQuantity")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalQuantity"))
            };
        }

        private static ProductionDetails MapDetails(SqlDataReader reader)
        {
            return new ProductionDetails
            {
                ProductionDetailID = reader.GetInt64(reader.GetOrdinal("ProductionDetailID")),
                ProductionID = reader.GetInt64(reader.GetOrdinal("ProductionID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                Quantity = reader.GetDecimal(reader.GetOrdinal("Quantity")),
                UnitCost = reader.IsDBNull(reader.GetOrdinal("UnitCost")) ? null : reader.GetDecimal(reader.GetOrdinal("UnitCost")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                ProductCode = reader.IsDBNull(reader.GetOrdinal("ProductCode")) ? null : reader.GetString(reader.GetOrdinal("ProductCode")),
                ProductNameText = reader.IsDBNull(reader.GetOrdinal("ProductNameText")) ? null : reader.GetString(reader.GetOrdinal("ProductNameText")),
                UnitName = reader.IsDBNull(reader.GetOrdinal("UnitName")) ? null : reader.GetString(reader.GetOrdinal("UnitName"))
            };
        }
    }
}

