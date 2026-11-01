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
    public class CustomerOrderRepository : BaseRepository, ICustomerOrderRepository
    {
        public CustomerOrderRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<CustomerOrderMaster?> GetByIdWithDetailsAsync(long id)
        {
            const string masterSql = @"
                SELECT 
                    com.CustomerOrderID, com.CustomerOrderNo, com.CustomerID, com.OrderDate, com.RequiredDate,
                    com.CustomerReference, com.Remarks, com.Status, com.CreatedBy, com.CreatedAt,
                    c.CustomerName,
                    ISNULL((SELECT SUM(cod.LineTotal) FROM dbo.CustomerOrderDetails cod WHERE cod.CustomerOrderID = com.CustomerOrderID), 0) AS TotalAmount
                FROM dbo.CustomerOrderMaster com
                INNER JOIN dbo.Customer c ON com.CustomerID = c.CustomerID
                WHERE com.CustomerOrderID = @ID";

            const string detailsSql = @"
                SELECT 
                    cod.CustomerOrderDetailID, cod.CustomerOrderID, cod.ProductID, cod.Quantity, cod.UnitPrice,
                    cod.DiscountAmount, cod.TaxAmount, cod.LineTotal, cod.Remarks,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.CustomerOrderDetails cod
                INNER JOIN dbo.ProductSetup p ON cod.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                WHERE cod.CustomerOrderID = @ID";

            CustomerOrderMaster? master = null;

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

        public async Task<(List<CustomerOrderMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    com.CustomerOrderID, com.CustomerOrderNo, com.CustomerID, com.OrderDate, com.RequiredDate,
                    com.CustomerReference, com.Remarks, com.Status, com.CreatedBy, com.CreatedAt,
                    c.CustomerName,
                    ISNULL((SELECT SUM(cod.LineTotal) FROM dbo.CustomerOrderDetails cod WHERE cod.CustomerOrderID = com.CustomerOrderID), 0) AS TotalAmount
                FROM dbo.CustomerOrderMaster com
                INNER JOIN dbo.Customer c ON com.CustomerID = c.CustomerID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.CustomerOrderMaster com
                INNER JOIN dbo.Customer c ON com.CustomerID = c.CustomerID";

            string filterCondition = options.filter != null ? GridQueryBuilder<CustomerOrderMaster>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<CustomerOrderMaster>.Query(options, baseQuery, "com.CustomerOrderID DESC");

            var list = new List<CustomerOrderMaster>();
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

        public async Task<long> InsertAsync(CustomerOrderMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertMasterSql = @"
                    INSERT INTO dbo.CustomerOrderMaster 
                        (CustomerOrderNo, CustomerID, OrderDate, RequiredDate, CustomerReference, Remarks, Status, CreatedBy, CreatedAt)
                    OUTPUT INSERTED.CustomerOrderID
                    VALUES 
                        (@CustomerOrderNo, @CustomerID, @OrderDate, @RequiredDate, @CustomerReference, @Remarks, @Status, @CreatedBy, SYSDATETIME())";

                long newMasterId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertMasterSql;
                    cmd.Parameters.Add("@CustomerOrderNo", SqlDbType.NVarChar, 30).Value = master.CustomerOrderNo.Trim();
                    cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = master.CustomerID;
                    cmd.Parameters.Add("@OrderDate", SqlDbType.Date).Value = master.OrderDate;
                    cmd.Parameters.Add("@RequiredDate", SqlDbType.Date).Value = (object?)master.RequiredDate ?? DBNull.Value;
                    cmd.Parameters.Add("@CustomerReference", SqlDbType.NVarChar, 100).Value = (object?)master.CustomerReference ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 100).Value = (object?)master.CreatedBy ?? DBNull.Value;

                    newMasterId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.CustomerOrderDetails 
                            (CustomerOrderID, ProductID, Quantity, UnitPrice, DiscountAmount, TaxAmount, Remarks)
                        VALUES 
                            (@CustomerOrderID, @ProductID, @Quantity, @UnitPrice, @DiscountAmount, @TaxAmount, @Remarks)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@CustomerOrderID", SqlDbType.BigInt).Value = newMasterId;
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

        public async Task<bool> UpdateAsync(CustomerOrderMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateMasterSql = @"
                    UPDATE dbo.CustomerOrderMaster 
                    SET CustomerOrderNo = @CustomerOrderNo,
                        CustomerID = @CustomerID,
                        OrderDate = @OrderDate,
                        RequiredDate = @RequiredDate,
                        CustomerReference = @CustomerReference,
                        Remarks = @Remarks,
                        Status = @Status
                    WHERE CustomerOrderID = @CustomerOrderID";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = updateMasterSql;
                    cmd.Parameters.Add("@CustomerOrderNo", SqlDbType.NVarChar, 30).Value = master.CustomerOrderNo.Trim();
                    cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = master.CustomerID;
                    cmd.Parameters.Add("@OrderDate", SqlDbType.Date).Value = master.OrderDate;
                    cmd.Parameters.Add("@RequiredDate", SqlDbType.Date).Value = (object?)master.RequiredDate ?? DBNull.Value;
                    cmd.Parameters.Add("@CustomerReference", SqlDbType.NVarChar, 100).Value = (object?)master.CustomerReference ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@CustomerOrderID", SqlDbType.BigInt).Value = master.CustomerOrderID;

                    await cmd.ExecuteNonQueryAsync();
                }

                const string deleteDetailsSql = "DELETE FROM dbo.CustomerOrderDetails WHERE CustomerOrderID = @CustomerOrderID";
                using (var delCmd = conn.CreateCommand())
                {
                    delCmd.Transaction = transaction;
                    delCmd.CommandText = deleteDetailsSql;
                    delCmd.Parameters.Add("@CustomerOrderID", SqlDbType.BigInt).Value = master.CustomerOrderID;
                    await delCmd.ExecuteNonQueryAsync();
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.CustomerOrderDetails 
                            (CustomerOrderID, ProductID, Quantity, UnitPrice, DiscountAmount, TaxAmount, Remarks)
                        VALUES 
                            (@CustomerOrderID, @ProductID, @Quantity, @UnitPrice, @DiscountAmount, @TaxAmount, @Remarks)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@CustomerOrderID", SqlDbType.BigInt).Value = master.CustomerOrderID;
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
                const string deleteDetailsSql = "DELETE FROM dbo.CustomerOrderDetails WHERE CustomerOrderID = @ID";
                using (var dCmd = conn.CreateCommand())
                {
                    dCmd.Transaction = transaction;
                    dCmd.CommandText = deleteDetailsSql;
                    dCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                    await dCmd.ExecuteNonQueryAsync();
                }

                const string deleteMasterSql = "DELETE FROM dbo.CustomerOrderMaster WHERE CustomerOrderID = @ID";
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
            const string sql = "SELECT COUNT(1) FROM dbo.CustomerOrderMaster WHERE CustomerOrderNo = @No AND CustomerOrderID <> @ExcludeID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@No", SqlDbType.NVarChar, 30).Value = orderNo.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.BigInt).Value = excludeId;

            await conn.OpenAsync();
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        private static CustomerOrderMaster MapMaster(SqlDataReader reader)
        {
            return new CustomerOrderMaster
            {
                CustomerOrderID = reader.GetInt64(reader.GetOrdinal("CustomerOrderID")),
                CustomerOrderNo = reader.GetString(reader.GetOrdinal("CustomerOrderNo")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                RequiredDate = reader.IsDBNull(reader.GetOrdinal("RequiredDate")) ? null : reader.GetDateTime(reader.GetOrdinal("RequiredDate")),
                CustomerReference = reader.IsDBNull(reader.GetOrdinal("CustomerReference")) ? null : reader.GetString(reader.GetOrdinal("CustomerReference")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
            };
        }

        private static CustomerOrderDetails MapDetails(SqlDataReader reader)
        {
            return new CustomerOrderDetails
            {
                CustomerOrderDetailID = reader.GetInt64(reader.GetOrdinal("CustomerOrderDetailID")),
                CustomerOrderID = reader.GetInt64(reader.GetOrdinal("CustomerOrderID")),
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

