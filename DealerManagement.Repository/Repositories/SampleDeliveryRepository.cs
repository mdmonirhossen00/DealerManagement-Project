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
    public class SampleDeliveryRepository : BaseRepository, ISampleDeliveryRepository
    {
        public SampleDeliveryRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<SampleDeliveryMaster?> GetByIdWithDetailsAsync(long id)
        {
            const string masterSql = @"
                SELECT 
                    sdm.SampleDeliveryID, sdm.SampleDeliveryNo, sdm.CustomerID, sdm.DeliveryTo,
                    sdm.DeliveryDate, sdm.Purpose, sdm.Remarks, sdm.Status, sdm.CreatedAt,
                    c.CustomerName
                FROM dbo.SampleDeliveryMaster sdm
                LEFT JOIN dbo.Customer c ON sdm.CustomerID = c.CustomerID
                WHERE sdm.SampleDeliveryID = @ID";

            const string detailsSql = @"
                SELECT 
                    sdd.SampleDeliveryDetailID, sdd.SampleDeliveryID, sdd.ProductID,
                    sdd.Quantity, sdd.UnitCost, sdd.IsReturnable, sdd.ExpectedReturnDate, sdd.Remarks,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.SampleDeliveryDetails sdd
                INNER JOIN dbo.ProductSetup p ON sdd.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                WHERE sdd.SampleDeliveryID = @ID";

            SampleDeliveryMaster? master = null;

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

        public async Task<(List<SampleDeliveryMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    sdm.SampleDeliveryID, sdm.SampleDeliveryNo, sdm.CustomerID, sdm.DeliveryTo,
                    sdm.DeliveryDate, sdm.Purpose, sdm.Remarks, sdm.Status, sdm.CreatedAt,
                    c.CustomerName
                FROM dbo.SampleDeliveryMaster sdm
                LEFT JOIN dbo.Customer c ON sdm.CustomerID = c.CustomerID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.SampleDeliveryMaster sdm
                LEFT JOIN dbo.Customer c ON sdm.CustomerID = c.CustomerID";

            string filterCondition = options.filter != null ? GridQueryBuilder<SampleDeliveryMaster>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<SampleDeliveryMaster>.Query(options, baseQuery, "sdm.SampleDeliveryID DESC");

            var list = new List<SampleDeliveryMaster>();
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

        public async Task<long> InsertAsync(SampleDeliveryMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertMasterSql = @"
                    INSERT INTO dbo.SampleDeliveryMaster 
                        (SampleDeliveryNo, CustomerID, DeliveryTo, DeliveryDate, Purpose, Remarks, Status, CreatedAt)
                    OUTPUT INSERTED.SampleDeliveryID
                    VALUES 
                        (@SampleDeliveryNo, @CustomerID, @DeliveryTo, @DeliveryDate, @Purpose, @Remarks, @Status, SYSDATETIME())";

                long newMasterId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertMasterSql;
                    cmd.Parameters.Add("@SampleDeliveryNo", SqlDbType.NVarChar, 30).Value = master.SampleDeliveryNo.Trim();
                    cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = (object?)master.CustomerID ?? DBNull.Value;
                    cmd.Parameters.Add("@DeliveryTo", SqlDbType.NVarChar, 150).Value = (object?)master.DeliveryTo ?? DBNull.Value;
                    cmd.Parameters.Add("@DeliveryDate", SqlDbType.Date).Value = master.DeliveryDate;
                    cmd.Parameters.Add("@Purpose", SqlDbType.NVarChar, 250).Value = (object?)master.Purpose ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;

                    newMasterId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.SampleDeliveryDetails 
                            (SampleDeliveryID, ProductID, Quantity, UnitCost, IsReturnable, ExpectedReturnDate, Remarks)
                        VALUES 
                            (@SampleDeliveryID, @ProductID, @Quantity, @UnitCost, @IsReturnable, @ExpectedReturnDate, @Remarks)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@SampleDeliveryID", SqlDbType.BigInt).Value = newMasterId;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitCost", SqlDbType.Decimal).Value = (object?)line.UnitCost ?? DBNull.Value;
                        dCmd.Parameters.Add("@IsReturnable", SqlDbType.Bit).Value = line.IsReturnable;
                        dCmd.Parameters.Add("@ExpectedReturnDate", SqlDbType.Date).Value = (object?)line.ExpectedReturnDate ?? DBNull.Value;
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

        public async Task<bool> UpdateAsync(SampleDeliveryMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateMasterSql = @"
                    UPDATE dbo.SampleDeliveryMaster 
                    SET SampleDeliveryNo = @SampleDeliveryNo,
                        CustomerID = @CustomerID,
                        DeliveryTo = @DeliveryTo,
                        DeliveryDate = @DeliveryDate,
                        Purpose = @Purpose,
                        Remarks = @Remarks,
                        Status = @Status
                    WHERE SampleDeliveryID = @SampleDeliveryID";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = updateMasterSql;
                    cmd.Parameters.Add("@SampleDeliveryNo", SqlDbType.NVarChar, 30).Value = master.SampleDeliveryNo.Trim();
                    cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = (object?)master.CustomerID ?? DBNull.Value;
                    cmd.Parameters.Add("@DeliveryTo", SqlDbType.NVarChar, 150).Value = (object?)master.DeliveryTo ?? DBNull.Value;
                    cmd.Parameters.Add("@DeliveryDate", SqlDbType.Date).Value = master.DeliveryDate;
                    cmd.Parameters.Add("@Purpose", SqlDbType.NVarChar, 250).Value = (object?)master.Purpose ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@SampleDeliveryID", SqlDbType.BigInt).Value = master.SampleDeliveryID;

                    await cmd.ExecuteNonQueryAsync();
                }

                const string deleteDetailsSql = "DELETE FROM dbo.SampleDeliveryDetails WHERE SampleDeliveryID = @SampleDeliveryID";
                using (var delCmd = conn.CreateCommand())
                {
                    delCmd.Transaction = transaction;
                    delCmd.CommandText = deleteDetailsSql;
                    delCmd.Parameters.Add("@SampleDeliveryID", SqlDbType.BigInt).Value = master.SampleDeliveryID;
                    await delCmd.ExecuteNonQueryAsync();
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.SampleDeliveryDetails 
                            (SampleDeliveryID, ProductID, Quantity, UnitCost, IsReturnable, ExpectedReturnDate, Remarks)
                        VALUES 
                            (@SampleDeliveryID, @ProductID, @Quantity, @UnitCost, @IsReturnable, @ExpectedReturnDate, @Remarks)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@SampleDeliveryID", SqlDbType.BigInt).Value = master.SampleDeliveryID;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitCost", SqlDbType.Decimal).Value = (object?)line.UnitCost ?? DBNull.Value;
                        dCmd.Parameters.Add("@IsReturnable", SqlDbType.Bit).Value = line.IsReturnable;
                        dCmd.Parameters.Add("@ExpectedReturnDate", SqlDbType.Date).Value = (object?)line.ExpectedReturnDate ?? DBNull.Value;
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
                const string delDetailsSql = "DELETE FROM dbo.SampleDeliveryDetails WHERE SampleDeliveryID = @ID";
                using (var dCmd = conn.CreateCommand())
                {
                    dCmd.Transaction = transaction;
                    dCmd.CommandText = delDetailsSql;
                    dCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                    await dCmd.ExecuteNonQueryAsync();
                }

                const string delMasterSql = "DELETE FROM dbo.SampleDeliveryMaster WHERE SampleDeliveryID = @ID";
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

        public async Task<bool> ExistsByNoAsync(string sampleNo, long excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.SampleDeliveryMaster WHERE SampleDeliveryNo = @SampleDeliveryNo AND SampleDeliveryID <> @ExcludeID";
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@SampleDeliveryNo", SqlDbType.NVarChar, 30).Value = sampleNo.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.BigInt).Value = excludeId;
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result ?? 0) > 0;
        }

        private static SampleDeliveryMaster MapMaster(SqlDataReader reader)
        {
            return new SampleDeliveryMaster
            {
                SampleDeliveryID = reader.GetInt64(reader.GetOrdinal("SampleDeliveryID")),
                SampleDeliveryNo = reader.GetString(reader.GetOrdinal("SampleDeliveryNo")),
                CustomerID = reader.IsDBNull(reader.GetOrdinal("CustomerID")) ? null : reader.GetInt32(reader.GetOrdinal("CustomerID")),
                DeliveryTo = reader.IsDBNull(reader.GetOrdinal("DeliveryTo")) ? null : reader.GetString(reader.GetOrdinal("DeliveryTo")),
                DeliveryDate = reader.GetDateTime(reader.GetOrdinal("DeliveryDate")),
                Purpose = reader.IsDBNull(reader.GetOrdinal("Purpose")) ? null : reader.GetString(reader.GetOrdinal("Purpose")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName"))
            };
        }

        private static SampleDeliveryDetails MapDetails(SqlDataReader reader)
        {
            return new SampleDeliveryDetails
            {
                SampleDeliveryDetailID = reader.GetInt64(reader.GetOrdinal("SampleDeliveryDetailID")),
                SampleDeliveryID = reader.GetInt64(reader.GetOrdinal("SampleDeliveryID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                Quantity = reader.GetDecimal(reader.GetOrdinal("Quantity")),
                UnitCost = reader.IsDBNull(reader.GetOrdinal("UnitCost")) ? null : reader.GetDecimal(reader.GetOrdinal("UnitCost")),
                IsReturnable = reader.GetBoolean(reader.GetOrdinal("IsReturnable")),
                ExpectedReturnDate = reader.IsDBNull(reader.GetOrdinal("ExpectedReturnDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ExpectedReturnDate")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                ProductCode = reader.IsDBNull(reader.GetOrdinal("ProductCode")) ? null : reader.GetString(reader.GetOrdinal("ProductCode")),
                ProductNameText = reader.IsDBNull(reader.GetOrdinal("ProductNameText")) ? null : reader.GetString(reader.GetOrdinal("ProductNameText")),
                UnitName = reader.IsDBNull(reader.GetOrdinal("UnitName")) ? null : reader.GetString(reader.GetOrdinal("UnitName"))
            };
        }
    }
}

