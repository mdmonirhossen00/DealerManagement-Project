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
    public class PurchaseOrderRepository : BaseRepository, IPurchaseOrderRepository
    {
        public PurchaseOrderRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<PurchaseOrderMaster?> GetByIdWithDetailsAsync(long id)
        {
            const string masterSql = @"
                SELECT 
                    pom.PurchaseOrderID, pom.PurchaseOrderNo, pom.SupplierID, pom.OrderDate, pom.ExpectedDate,
                    pom.Remarks, pom.Status, pom.CreatedBy, pom.CreatedAt,
                    s.SupplierName,
                    ISNULL((SELECT SUM(pod.LineTotal) FROM dbo.PurchaseOrderDetails pod WHERE pod.PurchaseOrderID = pom.PurchaseOrderID), 0) AS TotalAmount
                FROM dbo.PurchaseOrderMaster pom
                INNER JOIN dbo.Supplier s ON pom.SupplierID = s.SupplierID
                WHERE pom.PurchaseOrderID = @ID";

            const string detailsSql = @"
                SELECT 
                    pod.PurchaseOrderDetailID, pod.PurchaseOrderID, pod.ProductID, pod.Quantity, pod.UnitPrice,
                    pod.DiscountAmount, pod.TaxAmount, pod.LineTotal, pod.Remarks,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.PurchaseOrderDetails pod
                INNER JOIN dbo.ProductSetup p ON pod.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                WHERE pod.PurchaseOrderID = @ID";

            PurchaseOrderMaster? master = null;

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

        public async Task<(List<PurchaseOrderMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    pom.PurchaseOrderID, pom.PurchaseOrderNo, pom.SupplierID, pom.OrderDate, pom.ExpectedDate,
                    pom.Remarks, pom.Status, pom.CreatedBy, pom.CreatedAt,
                    s.SupplierName,
                    ISNULL((SELECT SUM(pod.LineTotal) FROM dbo.PurchaseOrderDetails pod WHERE pod.PurchaseOrderID = pom.PurchaseOrderID), 0) AS TotalAmount
                FROM dbo.PurchaseOrderMaster pom
                INNER JOIN dbo.Supplier s ON pom.SupplierID = s.SupplierID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.PurchaseOrderMaster pom
                INNER JOIN dbo.Supplier s ON pom.SupplierID = s.SupplierID";

            string filterCondition = options.filter != null ? GridQueryBuilder<PurchaseOrderMaster>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<PurchaseOrderMaster>.Query(options, baseQuery, "pom.PurchaseOrderID DESC");

            var list = new List<PurchaseOrderMaster>();
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

        public async Task<long> InsertAsync(PurchaseOrderMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertMasterSql = @"
                    INSERT INTO dbo.PurchaseOrderMaster 
                        (PurchaseOrderNo, SupplierID, OrderDate, ExpectedDate, Remarks, Status, CreatedBy, CreatedAt)
                    OUTPUT INSERTED.PurchaseOrderID
                    VALUES 
                        (@PurchaseOrderNo, @SupplierID, @OrderDate, @ExpectedDate, @Remarks, @Status, @CreatedBy, SYSDATETIME())";

                long newMasterId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertMasterSql;
                    cmd.Parameters.Add("@PurchaseOrderNo", SqlDbType.NVarChar, 30).Value = master.PurchaseOrderNo.Trim();
                    cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = master.SupplierID;
                    cmd.Parameters.Add("@OrderDate", SqlDbType.Date).Value = master.OrderDate;
                    cmd.Parameters.Add("@ExpectedDate", SqlDbType.Date).Value = (object?)master.ExpectedDate ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 100).Value = (object?)master.CreatedBy ?? DBNull.Value;

                    newMasterId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                // Insert Details rows
                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.PurchaseOrderDetails 
                            (PurchaseOrderID, ProductID, Quantity, UnitPrice, DiscountAmount, TaxAmount, Remarks)
                        VALUES 
                            (@PurchaseOrderID, @ProductID, @Quantity, @UnitPrice, @DiscountAmount, @TaxAmount, @Remarks)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@PurchaseOrderID", SqlDbType.BigInt).Value = newMasterId;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = line.UnitPrice;
                        dCmd.Parameters.Add("@DiscountAmount", SqlDbType.Decimal).Value = line.DiscountAmount;
                        dCmd.Parameters.Add("@TaxAmount", SqlDbType.Decimal).Value = line.TaxAmount;
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

        public async Task<bool> UpdateAsync(PurchaseOrderMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateMasterSql = @"
                    UPDATE dbo.PurchaseOrderMaster 
                    SET PurchaseOrderNo = @PurchaseOrderNo,
                        SupplierID = @SupplierID,
                        OrderDate = @OrderDate,
                        ExpectedDate = @ExpectedDate,
                        Remarks = @Remarks,
                        Status = @Status
                    WHERE PurchaseOrderID = @PurchaseOrderID";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = updateMasterSql;
                    cmd.Parameters.Add("@PurchaseOrderNo", SqlDbType.NVarChar, 30).Value = master.PurchaseOrderNo.Trim();
                    cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = master.SupplierID;
                    cmd.Parameters.Add("@OrderDate", SqlDbType.Date).Value = master.OrderDate;
                    cmd.Parameters.Add("@ExpectedDate", SqlDbType.Date).Value = (object?)master.ExpectedDate ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@PurchaseOrderID", SqlDbType.BigInt).Value = master.PurchaseOrderID;

                    await cmd.ExecuteNonQueryAsync();
                }

                // Delete old details
                const string deleteDetailsSql = "DELETE FROM dbo.PurchaseOrderDetails WHERE PurchaseOrderID = @PurchaseOrderID";
                using (var delCmd = conn.CreateCommand())
                {
                    delCmd.Transaction = transaction;
                    delCmd.CommandText = deleteDetailsSql;
                    delCmd.Parameters.Add("@PurchaseOrderID", SqlDbType.BigInt).Value = master.PurchaseOrderID;
                    await delCmd.ExecuteNonQueryAsync();
                }

                // Re-insert details
                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.PurchaseOrderDetails 
                            (PurchaseOrderID, ProductID, Quantity, UnitPrice, DiscountAmount, TaxAmount, Remarks)
                        VALUES 
                            (@PurchaseOrderID, @ProductID, @Quantity, @UnitPrice, @DiscountAmount, @TaxAmount, @Remarks)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@PurchaseOrderID", SqlDbType.BigInt).Value = master.PurchaseOrderID;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = line.UnitPrice;
                        dCmd.Parameters.Add("@DiscountAmount", SqlDbType.Decimal).Value = line.DiscountAmount;
                        dCmd.Parameters.Add("@TaxAmount", SqlDbType.Decimal).Value = line.TaxAmount;
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
                const string deleteDetailsSql = "DELETE FROM dbo.PurchaseOrderDetails WHERE PurchaseOrderID = @ID";
                using (var dCmd = conn.CreateCommand())
                {
                    dCmd.Transaction = transaction;
                    dCmd.CommandText = deleteDetailsSql;
                    dCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                    await dCmd.ExecuteNonQueryAsync();
                }

                const string deleteMasterSql = "DELETE FROM dbo.PurchaseOrderMaster WHERE PurchaseOrderID = @ID";
                using (var mCmd = conn.CreateCommand())
                {
                    mCmd.Transaction = transaction;
                    mCmd.CommandText = deleteMasterSql;
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

        public async Task<bool> ExistsByNoAsync(string orderNo, long excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.PurchaseOrderMaster WHERE PurchaseOrderNo = @No AND PurchaseOrderID <> @ExcludeID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@No", SqlDbType.NVarChar, 30).Value = orderNo.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.BigInt).Value = excludeId;

            await conn.OpenAsync();
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        private static PurchaseOrderMaster MapMaster(SqlDataReader reader)
        {
            return new PurchaseOrderMaster
            {
                PurchaseOrderID = reader.GetInt64(reader.GetOrdinal("PurchaseOrderID")),
                PurchaseOrderNo = reader.GetString(reader.GetOrdinal("PurchaseOrderNo")),
                SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                ExpectedDate = reader.IsDBNull(reader.GetOrdinal("ExpectedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ExpectedDate")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                SupplierName = reader.GetString(reader.GetOrdinal("SupplierName")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
            };
        }

        private static PurchaseOrderDetails MapDetails(SqlDataReader reader)
        {
            return new PurchaseOrderDetails
            {
                PurchaseOrderDetailID = reader.GetInt64(reader.GetOrdinal("PurchaseOrderDetailID")),
                PurchaseOrderID = reader.GetInt64(reader.GetOrdinal("PurchaseOrderID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                Quantity = reader.GetDecimal(reader.GetOrdinal("Quantity")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                TaxAmount = reader.GetDecimal(reader.GetOrdinal("TaxAmount")),
                LineTotal = reader.GetDecimal(reader.GetOrdinal("LineTotal")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                ProductNameText = reader.GetString(reader.GetOrdinal("ProductNameText")),
                UnitName = reader.GetString(reader.GetOrdinal("UnitName"))
            };
        }
    }
}

