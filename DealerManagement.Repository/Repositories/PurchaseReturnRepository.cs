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
    public class PurchaseReturnRepository : BaseRepository, IPurchaseReturnRepository
    {
        public PurchaseReturnRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<PurchaseReturnMaster?> GetByIdWithDetailsAsync(long id)
        {
            const string masterSql = @"
                SELECT 
                    prm.PurchaseReturnID, prm.PurchaseReturnNo, prm.SupplierID, prm.PurchaseReceiptID,
                    prm.ReturnDate, prm.Reason, prm.Status, prm.CreatedAt,
                    s.SupplierName, rcpt.ReceiptNo,
                    ISNULL((SELECT SUM(prd.Quantity * prd.UnitPrice) FROM dbo.PurchaseReturnDetails prd WHERE prd.PurchaseReturnID = prm.PurchaseReturnID), 0) AS TotalAmount
                FROM dbo.PurchaseReturnMaster prm
                INNER JOIN dbo.Supplier s ON prm.SupplierID = s.SupplierID
                LEFT JOIN dbo.PurchaseReceiptMaster rcpt ON prm.PurchaseReceiptID = rcpt.PurchaseReceiptID
                WHERE prm.PurchaseReturnID = @ID";

            const string detailsSql = @"
                SELECT 
                    prd.PurchaseReturnDetailID, prd.PurchaseReturnID, prd.ProductID,
                    prd.Quantity, prd.UnitPrice, prd.Reason,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.PurchaseReturnDetails prd
                INNER JOIN dbo.ProductSetup p ON prd.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                WHERE prd.PurchaseReturnID = @ID";

            PurchaseReturnMaster? master = null;

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

        public async Task<(List<PurchaseReturnMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    prm.PurchaseReturnID, prm.PurchaseReturnNo, prm.SupplierID, prm.PurchaseReceiptID,
                    prm.ReturnDate, prm.Reason, prm.Status, prm.CreatedAt,
                    s.SupplierName, rcpt.ReceiptNo,
                    ISNULL((SELECT SUM(prd.Quantity * prd.UnitPrice) FROM dbo.PurchaseReturnDetails prd WHERE prd.PurchaseReturnID = prm.PurchaseReturnID), 0) AS TotalAmount
                FROM dbo.PurchaseReturnMaster prm
                INNER JOIN dbo.Supplier s ON prm.SupplierID = s.SupplierID
                LEFT JOIN dbo.PurchaseReceiptMaster rcpt ON prm.PurchaseReceiptID = rcpt.PurchaseReceiptID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.PurchaseReturnMaster prm
                INNER JOIN dbo.Supplier s ON prm.SupplierID = s.SupplierID
                LEFT JOIN dbo.PurchaseReceiptMaster rcpt ON prm.PurchaseReceiptID = rcpt.PurchaseReceiptID";

            string filterCondition = options.filter != null ? GridQueryBuilder<PurchaseReturnMaster>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<PurchaseReturnMaster>.Query(options, baseQuery, "prm.PurchaseReturnID DESC");

            var list = new List<PurchaseReturnMaster>();
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

        public async Task<long> InsertAsync(PurchaseReturnMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertMasterSql = @"
                    INSERT INTO dbo.PurchaseReturnMaster 
                        (PurchaseReturnNo, SupplierID, PurchaseReceiptID, ReturnDate, Reason, Status, CreatedAt)
                    OUTPUT INSERTED.PurchaseReturnID
                    VALUES 
                        (@PurchaseReturnNo, @SupplierID, @PurchaseReceiptID, @ReturnDate, @Reason, @Status, SYSDATETIME())";

                long newMasterId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertMasterSql;
                    cmd.Parameters.Add("@PurchaseReturnNo", SqlDbType.NVarChar, 30).Value = master.PurchaseReturnNo.Trim();
                    cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = master.SupplierID;
                    cmd.Parameters.Add("@PurchaseReceiptID", SqlDbType.BigInt).Value = (object?)master.PurchaseReceiptID ?? DBNull.Value;
                    cmd.Parameters.Add("@ReturnDate", SqlDbType.Date).Value = master.ReturnDate;
                    cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 500).Value = (object?)master.Reason ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;

                    newMasterId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.PurchaseReturnDetails 
                            (PurchaseReturnID, ProductID, Quantity, UnitPrice, Reason)
                        VALUES 
                            (@PurchaseReturnID, @ProductID, @Quantity, @UnitPrice, @Reason)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@PurchaseReturnID", SqlDbType.BigInt).Value = newMasterId;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = line.UnitPrice;
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

        public async Task<bool> UpdateAsync(PurchaseReturnMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateMasterSql = @"
                    UPDATE dbo.PurchaseReturnMaster 
                    SET PurchaseReturnNo = @PurchaseReturnNo,
                        SupplierID = @SupplierID,
                        PurchaseReceiptID = @PurchaseReceiptID,
                        ReturnDate = @ReturnDate,
                        Reason = @Reason,
                        Status = @Status
                    WHERE PurchaseReturnID = @PurchaseReturnID";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = updateMasterSql;
                    cmd.Parameters.Add("@PurchaseReturnNo", SqlDbType.NVarChar, 30).Value = master.PurchaseReturnNo.Trim();
                    cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = master.SupplierID;
                    cmd.Parameters.Add("@PurchaseReceiptID", SqlDbType.BigInt).Value = (object?)master.PurchaseReceiptID ?? DBNull.Value;
                    cmd.Parameters.Add("@ReturnDate", SqlDbType.Date).Value = master.ReturnDate;
                    cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 500).Value = (object?)master.Reason ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@PurchaseReturnID", SqlDbType.BigInt).Value = master.PurchaseReturnID;

                    await cmd.ExecuteNonQueryAsync();
                }

                const string deleteDetailsSql = "DELETE FROM dbo.PurchaseReturnDetails WHERE PurchaseReturnID = @PurchaseReturnID";
                using (var delCmd = conn.CreateCommand())
                {
                    delCmd.Transaction = transaction;
                    delCmd.CommandText = deleteDetailsSql;
                    delCmd.Parameters.Add("@PurchaseReturnID", SqlDbType.BigInt).Value = master.PurchaseReturnID;
                    await delCmd.ExecuteNonQueryAsync();
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.PurchaseReturnDetails 
                            (PurchaseReturnID, ProductID, Quantity, UnitPrice, Reason)
                        VALUES 
                            (@PurchaseReturnID, @ProductID, @Quantity, @UnitPrice, @Reason)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@PurchaseReturnID", SqlDbType.BigInt).Value = master.PurchaseReturnID;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = line.UnitPrice;
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
                const string delDetailsSql = "DELETE FROM dbo.PurchaseReturnDetails WHERE PurchaseReturnID = @ID";
                using (var dCmd = conn.CreateCommand())
                {
                    dCmd.Transaction = transaction;
                    dCmd.CommandText = delDetailsSql;
                    dCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                    await dCmd.ExecuteNonQueryAsync();
                }

                const string delMasterSql = "DELETE FROM dbo.PurchaseReturnMaster WHERE PurchaseReturnID = @ID";
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

        public async Task<bool> ExistsByNoAsync(string returnNo, long excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.PurchaseReturnMaster WHERE PurchaseReturnNo = @PurchaseReturnNo AND PurchaseReturnID <> @ExcludeID";
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@PurchaseReturnNo", SqlDbType.NVarChar, 30).Value = returnNo.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.BigInt).Value = excludeId;
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result ?? 0) > 0;
        }

        private static PurchaseReturnMaster MapMaster(SqlDataReader reader)
        {
            return new PurchaseReturnMaster
            {
                PurchaseReturnID = reader.GetInt64(reader.GetOrdinal("PurchaseReturnID")),
                PurchaseReturnNo = reader.GetString(reader.GetOrdinal("PurchaseReturnNo")),
                SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                PurchaseReceiptID = reader.IsDBNull(reader.GetOrdinal("PurchaseReceiptID")) ? null : reader.GetInt64(reader.GetOrdinal("PurchaseReceiptID")),
                ReturnDate = reader.GetDateTime(reader.GetOrdinal("ReturnDate")),
                Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                SupplierName = reader.IsDBNull(reader.GetOrdinal("SupplierName")) ? null : reader.GetString(reader.GetOrdinal("SupplierName")),
                ReceiptNo = reader.IsDBNull(reader.GetOrdinal("ReceiptNo")) ? null : reader.GetString(reader.GetOrdinal("ReceiptNo")),
                TotalAmount = reader.IsDBNull(reader.GetOrdinal("TotalAmount")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
            };
        }

        private static PurchaseReturnDetails MapDetails(SqlDataReader reader)
        {
            return new PurchaseReturnDetails
            {
                PurchaseReturnDetailID = reader.GetInt64(reader.GetOrdinal("PurchaseReturnDetailID")),
                PurchaseReturnID = reader.GetInt64(reader.GetOrdinal("PurchaseReturnID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                Quantity = reader.GetDecimal(reader.GetOrdinal("Quantity")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason")),
                ProductCode = reader.IsDBNull(reader.GetOrdinal("ProductCode")) ? null : reader.GetString(reader.GetOrdinal("ProductCode")),
                ProductNameText = reader.IsDBNull(reader.GetOrdinal("ProductNameText")) ? null : reader.GetString(reader.GetOrdinal("ProductNameText")),
                UnitName = reader.IsDBNull(reader.GetOrdinal("UnitName")) ? null : reader.GetString(reader.GetOrdinal("UnitName"))
            };
        }
    }
}

