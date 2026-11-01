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
    public class PurchaseReceiptRepository : BaseRepository, IPurchaseReceiptRepository
    {
        public PurchaseReceiptRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<PurchaseReceiptMaster?> GetByIdWithDetailsAsync(long id)
        {
            const string masterSql = @"
                SELECT 
                    prm.PurchaseReceiptID, prm.ReceiptNo, prm.SupplierID, prm.PurchaseOrderID, prm.ReceiptDate,
                    prm.SupplierInvoiceNo, prm.Remarks, prm.Status, prm.CreatedAt,
                    s.SupplierName, pom.PurchaseOrderNo,
                    ISNULL((SELECT SUM(prd.AcceptedQuantity * prd.UnitCost) FROM dbo.PurchaseReceiptDetails prd WHERE prd.PurchaseReceiptID = prm.PurchaseReceiptID), 0) AS TotalAmount
                FROM dbo.PurchaseReceiptMaster prm
                INNER JOIN dbo.Supplier s ON prm.SupplierID = s.SupplierID
                LEFT JOIN dbo.PurchaseOrderMaster pom ON prm.PurchaseOrderID = pom.PurchaseOrderID
                WHERE prm.PurchaseReceiptID = @ID";

            const string detailsSql = @"
                SELECT 
                    prd.PurchaseReceiptDetailID, prd.PurchaseReceiptID, prd.PurchaseOrderDetailID, prd.ProductID,
                    prd.Quantity, prd.AcceptedQuantity, prd.RejectedQuantity, prd.UnitCost,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.PurchaseReceiptDetails prd
                INNER JOIN dbo.ProductSetup p ON prd.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                WHERE prd.PurchaseReceiptID = @ID";

            PurchaseReceiptMaster? master = null;

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

        public async Task<(List<PurchaseReceiptMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    prm.PurchaseReceiptID, prm.ReceiptNo, prm.SupplierID, prm.PurchaseOrderID, prm.ReceiptDate,
                    prm.SupplierInvoiceNo, prm.Remarks, prm.Status, prm.CreatedAt,
                    s.SupplierName, pom.PurchaseOrderNo,
                    ISNULL((SELECT SUM(prd.AcceptedQuantity * prd.UnitCost) FROM dbo.PurchaseReceiptDetails prd WHERE prd.PurchaseReceiptID = prm.PurchaseReceiptID), 0) AS TotalAmount
                FROM dbo.PurchaseReceiptMaster prm
                INNER JOIN dbo.Supplier s ON prm.SupplierID = s.SupplierID
                LEFT JOIN dbo.PurchaseOrderMaster pom ON prm.PurchaseOrderID = pom.PurchaseOrderID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.PurchaseReceiptMaster prm
                INNER JOIN dbo.Supplier s ON prm.SupplierID = s.SupplierID
                LEFT JOIN dbo.PurchaseOrderMaster pom ON prm.PurchaseOrderID = pom.PurchaseOrderID";

            string filterCondition = options.filter != null ? GridQueryBuilder<PurchaseReceiptMaster>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<PurchaseReceiptMaster>.Query(options, baseQuery, "prm.PurchaseReceiptID DESC");

            var list = new List<PurchaseReceiptMaster>();
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

        public async Task<long> InsertAsync(PurchaseReceiptMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertMasterSql = @"
                    INSERT INTO dbo.PurchaseReceiptMaster 
                        (ReceiptNo, SupplierID, PurchaseOrderID, ReceiptDate, SupplierInvoiceNo, Remarks, Status, CreatedAt)
                    OUTPUT INSERTED.PurchaseReceiptID
                    VALUES 
                        (@ReceiptNo, @SupplierID, @PurchaseOrderID, @ReceiptDate, @SupplierInvoiceNo, @Remarks, @Status, SYSDATETIME())";

                long newMasterId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertMasterSql;
                    cmd.Parameters.Add("@ReceiptNo", SqlDbType.NVarChar, 30).Value = master.ReceiptNo.Trim();
                    cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = master.SupplierID;
                    cmd.Parameters.Add("@PurchaseOrderID", SqlDbType.BigInt).Value = (object?)master.PurchaseOrderID ?? DBNull.Value;
                    cmd.Parameters.Add("@ReceiptDate", SqlDbType.Date).Value = master.ReceiptDate;
                    cmd.Parameters.Add("@SupplierInvoiceNo", SqlDbType.NVarChar, 50).Value = (object?)master.SupplierInvoiceNo ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;

                    newMasterId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.PurchaseReceiptDetails 
                            (PurchaseReceiptID, PurchaseOrderDetailID, ProductID, Quantity, AcceptedQuantity, RejectedQuantity, UnitCost)
                        VALUES 
                            (@PurchaseReceiptID, @PurchaseOrderDetailID, @ProductID, @Quantity, @AcceptedQuantity, @RejectedQuantity, @UnitCost)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@PurchaseReceiptID", SqlDbType.BigInt).Value = newMasterId;
                        dCmd.Parameters.Add("@PurchaseOrderDetailID", SqlDbType.BigInt).Value = (object?)line.PurchaseOrderDetailID ?? DBNull.Value;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@AcceptedQuantity", SqlDbType.Decimal).Value = line.AcceptedQuantity;
                        dCmd.Parameters.Add("@RejectedQuantity", SqlDbType.Decimal).Value = line.RejectedQuantity;
                        dCmd.Parameters.Add("@UnitCost", SqlDbType.Decimal).Value = line.UnitCost;

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

        public async Task<bool> UpdateAsync(PurchaseReceiptMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateMasterSql = @"
                    UPDATE dbo.PurchaseReceiptMaster 
                    SET ReceiptNo = @ReceiptNo,
                        SupplierID = @SupplierID,
                        PurchaseOrderID = @PurchaseOrderID,
                        ReceiptDate = @ReceiptDate,
                        SupplierInvoiceNo = @SupplierInvoiceNo,
                        Remarks = @Remarks,
                        Status = @Status
                    WHERE PurchaseReceiptID = @PurchaseReceiptID";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = updateMasterSql;
                    cmd.Parameters.Add("@ReceiptNo", SqlDbType.NVarChar, 30).Value = master.ReceiptNo.Trim();
                    cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = master.SupplierID;
                    cmd.Parameters.Add("@PurchaseOrderID", SqlDbType.BigInt).Value = (object?)master.PurchaseOrderID ?? DBNull.Value;
                    cmd.Parameters.Add("@ReceiptDate", SqlDbType.Date).Value = master.ReceiptDate;
                    cmd.Parameters.Add("@SupplierInvoiceNo", SqlDbType.NVarChar, 50).Value = (object?)master.SupplierInvoiceNo ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@PurchaseReceiptID", SqlDbType.BigInt).Value = master.PurchaseReceiptID;

                    await cmd.ExecuteNonQueryAsync();
                }

                const string deleteDetailsSql = "DELETE FROM dbo.PurchaseReceiptDetails WHERE PurchaseReceiptID = @PurchaseReceiptID";
                using (var delCmd = conn.CreateCommand())
                {
                    delCmd.Transaction = transaction;
                    delCmd.CommandText = deleteDetailsSql;
                    delCmd.Parameters.Add("@PurchaseReceiptID", SqlDbType.BigInt).Value = master.PurchaseReceiptID;
                    await delCmd.ExecuteNonQueryAsync();
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.PurchaseReceiptDetails 
                            (PurchaseReceiptID, PurchaseOrderDetailID, ProductID, Quantity, AcceptedQuantity, RejectedQuantity, UnitCost)
                        VALUES 
                            (@PurchaseReceiptID, @PurchaseOrderDetailID, @ProductID, @Quantity, @AcceptedQuantity, @RejectedQuantity, @UnitCost)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@PurchaseReceiptID", SqlDbType.BigInt).Value = master.PurchaseReceiptID;
                        dCmd.Parameters.Add("@PurchaseOrderDetailID", SqlDbType.BigInt).Value = (object?)line.PurchaseOrderDetailID ?? DBNull.Value;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@AcceptedQuantity", SqlDbType.Decimal).Value = line.AcceptedQuantity;
                        dCmd.Parameters.Add("@RejectedQuantity", SqlDbType.Decimal).Value = line.RejectedQuantity;
                        dCmd.Parameters.Add("@UnitCost", SqlDbType.Decimal).Value = line.UnitCost;

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
                const string deleteDetailsSql = "DELETE FROM dbo.PurchaseReceiptDetails WHERE PurchaseReceiptID = @ID";
                using (var dCmd = conn.CreateCommand())
                {
                    dCmd.Transaction = transaction;
                    dCmd.CommandText = deleteDetailsSql;
                    dCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                    await dCmd.ExecuteNonQueryAsync();
                }

                const string deleteMasterSql = "DELETE FROM dbo.PurchaseReceiptMaster WHERE PurchaseReceiptID = @ID";
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

        public async Task<bool> ExistsByNoAsync(string receiptNo, long excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.PurchaseReceiptMaster WHERE ReceiptNo = @No AND PurchaseReceiptID <> @ExcludeID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@No", SqlDbType.NVarChar, 30).Value = receiptNo.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.BigInt).Value = excludeId;

            await conn.OpenAsync();
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        private static PurchaseReceiptMaster MapMaster(SqlDataReader reader)
        {
            return new PurchaseReceiptMaster
            {
                PurchaseReceiptID = reader.GetInt64(reader.GetOrdinal("PurchaseReceiptID")),
                ReceiptNo = reader.GetString(reader.GetOrdinal("ReceiptNo")),
                SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                PurchaseOrderID = reader.IsDBNull(reader.GetOrdinal("PurchaseOrderID")) ? null : reader.GetInt64(reader.GetOrdinal("PurchaseOrderID")),
                ReceiptDate = reader.GetDateTime(reader.GetOrdinal("ReceiptDate")),
                SupplierInvoiceNo = reader.IsDBNull(reader.GetOrdinal("SupplierInvoiceNo")) ? null : reader.GetString(reader.GetOrdinal("SupplierInvoiceNo")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                SupplierName = reader.GetString(reader.GetOrdinal("SupplierName")),
                PurchaseOrderNo = reader.IsDBNull(reader.GetOrdinal("PurchaseOrderNo")) ? null : reader.GetString(reader.GetOrdinal("PurchaseOrderNo")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
            };
        }

        private static PurchaseReceiptDetails MapDetails(SqlDataReader reader)
        {
            return new PurchaseReceiptDetails
            {
                PurchaseReceiptDetailID = reader.GetInt64(reader.GetOrdinal("PurchaseReceiptDetailID")),
                PurchaseReceiptID = reader.GetInt64(reader.GetOrdinal("PurchaseReceiptID")),
                PurchaseOrderDetailID = reader.IsDBNull(reader.GetOrdinal("PurchaseOrderDetailID")) ? null : reader.GetInt64(reader.GetOrdinal("PurchaseOrderDetailID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                Quantity = reader.GetDecimal(reader.GetOrdinal("Quantity")),
                AcceptedQuantity = reader.GetDecimal(reader.GetOrdinal("AcceptedQuantity")),
                RejectedQuantity = reader.GetDecimal(reader.GetOrdinal("RejectedQuantity")),
                UnitCost = reader.GetDecimal(reader.GetOrdinal("UnitCost")),
                ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                ProductNameText = reader.GetString(reader.GetOrdinal("ProductNameText")),
                UnitName = reader.GetString(reader.GetOrdinal("UnitName"))
            };
        }
    }
}

