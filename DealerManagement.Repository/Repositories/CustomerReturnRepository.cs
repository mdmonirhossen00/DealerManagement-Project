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
    public class CustomerReturnRepository : BaseRepository, ICustomerReturnRepository
    {
        public CustomerReturnRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<CustomerReturnMaster?> GetByIdWithDetailsAsync(long id)
        {
            const string masterSql = @"
                SELECT 
                    crm.CustomerReturnID, crm.CustomerReturnNo, crm.CustomerID, crm.CustomerDeliveryID,
                    crm.ReturnDate, crm.Reason, crm.Status, crm.CreatedAt,
                    c.CustomerName, cdm.DeliveryNo,
                    ISNULL((SELECT SUM(crd.Quantity * crd.UnitPrice) FROM dbo.CustomerReturnDetails crd WHERE crd.CustomerReturnID = crm.CustomerReturnID), 0) AS TotalAmount
                FROM dbo.CustomerReturnMaster crm
                INNER JOIN dbo.Customer c ON crm.CustomerID = c.CustomerID
                LEFT JOIN dbo.CustomerDeliveryMaster cdm ON crm.CustomerDeliveryID = cdm.CustomerDeliveryID
                WHERE crm.CustomerReturnID = @ID";

            const string detailsSql = @"
                SELECT 
                    crd.CustomerReturnDetailID, crd.CustomerReturnID, crd.ProductID,
                    crd.Quantity, crd.UnitPrice, crd.ReturnCondition, crd.Reason,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.CustomerReturnDetails crd
                INNER JOIN dbo.ProductSetup p ON crd.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                WHERE crd.CustomerReturnID = @ID";

            CustomerReturnMaster? master = null;

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

        public async Task<(List<CustomerReturnMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    crm.CustomerReturnID, crm.CustomerReturnNo, crm.CustomerID, crm.CustomerDeliveryID,
                    crm.ReturnDate, crm.Reason, crm.Status, crm.CreatedAt,
                    c.CustomerName, cdm.DeliveryNo,
                    ISNULL((SELECT SUM(crd.Quantity * crd.UnitPrice) FROM dbo.CustomerReturnDetails crd WHERE crd.CustomerReturnID = crm.CustomerReturnID), 0) AS TotalAmount
                FROM dbo.CustomerReturnMaster crm
                INNER JOIN dbo.Customer c ON crm.CustomerID = c.CustomerID
                LEFT JOIN dbo.CustomerDeliveryMaster cdm ON crm.CustomerDeliveryID = cdm.CustomerDeliveryID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.CustomerReturnMaster crm
                INNER JOIN dbo.Customer c ON crm.CustomerID = c.CustomerID
                LEFT JOIN dbo.CustomerDeliveryMaster cdm ON crm.CustomerDeliveryID = cdm.CustomerDeliveryID";

            string filterCondition = options.filter != null ? GridQueryBuilder<CustomerReturnMaster>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<CustomerReturnMaster>.Query(options, baseQuery, "crm.CustomerReturnID DESC");

            var list = new List<CustomerReturnMaster>();
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

        public async Task<long> InsertAsync(CustomerReturnMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertMasterSql = @"
                    INSERT INTO dbo.CustomerReturnMaster 
                        (CustomerReturnNo, CustomerID, CustomerDeliveryID, ReturnDate, Reason, Status, CreatedAt)
                    OUTPUT INSERTED.CustomerReturnID
                    VALUES 
                        (@CustomerReturnNo, @CustomerID, @CustomerDeliveryID, @ReturnDate, @Reason, @Status, SYSDATETIME())";

                long newMasterId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertMasterSql;
                    cmd.Parameters.Add("@CustomerReturnNo", SqlDbType.NVarChar, 30).Value = master.CustomerReturnNo.Trim();
                    cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = master.CustomerID;
                    cmd.Parameters.Add("@CustomerDeliveryID", SqlDbType.BigInt).Value = (object?)master.CustomerDeliveryID ?? DBNull.Value;
                    cmd.Parameters.Add("@ReturnDate", SqlDbType.Date).Value = master.ReturnDate;
                    cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 500).Value = (object?)master.Reason ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;

                    newMasterId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.CustomerReturnDetails 
                            (CustomerReturnID, ProductID, Quantity, UnitPrice, ReturnCondition, Reason)
                        VALUES 
                            (@CustomerReturnID, @ProductID, @Quantity, @UnitPrice, @ReturnCondition, @Reason)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@CustomerReturnID", SqlDbType.BigInt).Value = newMasterId;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = line.UnitPrice;
                        dCmd.Parameters.Add("@ReturnCondition", SqlDbType.NVarChar, 30).Value = (object?)line.ReturnCondition ?? "Good";
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

        public async Task<bool> UpdateAsync(CustomerReturnMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateMasterSql = @"
                    UPDATE dbo.CustomerReturnMaster 
                    SET CustomerReturnNo = @CustomerReturnNo,
                        CustomerID = @CustomerID,
                        CustomerDeliveryID = @CustomerDeliveryID,
                        ReturnDate = @ReturnDate,
                        Reason = @Reason,
                        Status = @Status
                    WHERE CustomerReturnID = @CustomerReturnID";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = updateMasterSql;
                    cmd.Parameters.Add("@CustomerReturnNo", SqlDbType.NVarChar, 30).Value = master.CustomerReturnNo.Trim();
                    cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = master.CustomerID;
                    cmd.Parameters.Add("@CustomerDeliveryID", SqlDbType.BigInt).Value = (object?)master.CustomerDeliveryID ?? DBNull.Value;
                    cmd.Parameters.Add("@ReturnDate", SqlDbType.Date).Value = master.ReturnDate;
                    cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 500).Value = (object?)master.Reason ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@CustomerReturnID", SqlDbType.BigInt).Value = master.CustomerReturnID;

                    await cmd.ExecuteNonQueryAsync();
                }

                const string deleteDetailsSql = "DELETE FROM dbo.CustomerReturnDetails WHERE CustomerReturnID = @CustomerReturnID";
                using (var delCmd = conn.CreateCommand())
                {
                    delCmd.Transaction = transaction;
                    delCmd.CommandText = deleteDetailsSql;
                    delCmd.Parameters.Add("@CustomerReturnID", SqlDbType.BigInt).Value = master.CustomerReturnID;
                    await delCmd.ExecuteNonQueryAsync();
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.CustomerReturnDetails 
                            (CustomerReturnID, ProductID, Quantity, UnitPrice, ReturnCondition, Reason)
                        VALUES 
                            (@CustomerReturnID, @ProductID, @Quantity, @UnitPrice, @ReturnCondition, @Reason)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@CustomerReturnID", SqlDbType.BigInt).Value = master.CustomerReturnID;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = line.UnitPrice;
                        dCmd.Parameters.Add("@ReturnCondition", SqlDbType.NVarChar, 30).Value = (object?)line.ReturnCondition ?? "Good";
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
                const string delDetailsSql = "DELETE FROM dbo.CustomerReturnDetails WHERE CustomerReturnID = @ID";
                using (var dCmd = conn.CreateCommand())
                {
                    dCmd.Transaction = transaction;
                    dCmd.CommandText = delDetailsSql;
                    dCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                    await dCmd.ExecuteNonQueryAsync();
                }

                const string delMasterSql = "DELETE FROM dbo.CustomerReturnMaster WHERE CustomerReturnID = @ID";
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
            const string sql = "SELECT COUNT(1) FROM dbo.CustomerReturnMaster WHERE CustomerReturnNo = @CustomerReturnNo AND CustomerReturnID <> @ExcludeID";
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@CustomerReturnNo", SqlDbType.NVarChar, 30).Value = returnNo.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.BigInt).Value = excludeId;
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result ?? 0) > 0;
        }

        private static CustomerReturnMaster MapMaster(SqlDataReader reader)
        {
            return new CustomerReturnMaster
            {
                CustomerReturnID = reader.GetInt64(reader.GetOrdinal("CustomerReturnID")),
                CustomerReturnNo = reader.GetString(reader.GetOrdinal("CustomerReturnNo")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                CustomerDeliveryID = reader.IsDBNull(reader.GetOrdinal("CustomerDeliveryID")) ? null : reader.GetInt64(reader.GetOrdinal("CustomerDeliveryID")),
                ReturnDate = reader.GetDateTime(reader.GetOrdinal("ReturnDate")),
                Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName")),
                DeliveryNo = reader.IsDBNull(reader.GetOrdinal("DeliveryNo")) ? null : reader.GetString(reader.GetOrdinal("DeliveryNo")),
                TotalAmount = reader.IsDBNull(reader.GetOrdinal("TotalAmount")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
            };
        }

        private static CustomerReturnDetails MapDetails(SqlDataReader reader)
        {
            return new CustomerReturnDetails
            {
                CustomerReturnDetailID = reader.GetInt64(reader.GetOrdinal("CustomerReturnDetailID")),
                CustomerReturnID = reader.GetInt64(reader.GetOrdinal("CustomerReturnID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                Quantity = reader.GetDecimal(reader.GetOrdinal("Quantity")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                ReturnCondition = reader.IsDBNull(reader.GetOrdinal("ReturnCondition")) ? null : reader.GetString(reader.GetOrdinal("ReturnCondition")),
                Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason")),
                ProductCode = reader.IsDBNull(reader.GetOrdinal("ProductCode")) ? null : reader.GetString(reader.GetOrdinal("ProductCode")),
                ProductNameText = reader.IsDBNull(reader.GetOrdinal("ProductNameText")) ? null : reader.GetString(reader.GetOrdinal("ProductNameText")),
                UnitName = reader.IsDBNull(reader.GetOrdinal("UnitName")) ? null : reader.GetString(reader.GetOrdinal("UnitName"))
            };
        }
    }
}

