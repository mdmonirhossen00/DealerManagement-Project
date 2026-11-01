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
    public class CustomerDeliveryRepository : BaseRepository, ICustomerDeliveryRepository
    {
        public CustomerDeliveryRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<CustomerDeliveryMaster?> GetByIdWithDetailsAsync(long id)
        {
            const string masterSql = @"
                SELECT 
                    cdm.CustomerDeliveryID, cdm.DeliveryNo, cdm.CustomerID, cdm.CustomerOrderID, cdm.DeliveryDate,
                    cdm.DeliveryAddress, cdm.VehicleNo, cdm.DriverName, cdm.Remarks, cdm.Status, cdm.CreatedAt,
                    c.CustomerName, com.CustomerOrderNo,
                    ISNULL((SELECT SUM(cdd.Quantity * cdd.UnitPrice) FROM dbo.CustomerDeliveryDetails cdd WHERE cdd.CustomerDeliveryID = cdm.CustomerDeliveryID), 0) AS TotalAmount
                FROM dbo.CustomerDeliveryMaster cdm
                INNER JOIN dbo.Customer c ON cdm.CustomerID = c.CustomerID
                LEFT JOIN dbo.CustomerOrderMaster com ON cdm.CustomerOrderID = com.CustomerOrderID
                WHERE cdm.CustomerDeliveryID = @ID";

            const string detailsSql = @"
                SELECT 
                    cdd.CustomerDeliveryDetailID, cdd.CustomerDeliveryID, cdd.CustomerOrderDetailID, cdd.ProductID,
                    cdd.Quantity, cdd.UnitPrice, cdd.Remarks,
                    p.ProductCode, pn.ProductName AS ProductNameText, p.UnitName
                FROM dbo.CustomerDeliveryDetails cdd
                INNER JOIN dbo.ProductSetup p ON cdd.ProductID = p.ProductID
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                WHERE cdd.CustomerDeliveryID = @ID";

            CustomerDeliveryMaster? master = null;

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

        public async Task<(List<CustomerDeliveryMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    cdm.CustomerDeliveryID, cdm.DeliveryNo, cdm.CustomerID, cdm.CustomerOrderID, cdm.DeliveryDate,
                    cdm.DeliveryAddress, cdm.VehicleNo, cdm.DriverName, cdm.Remarks, cdm.Status, cdm.CreatedAt,
                    c.CustomerName, com.CustomerOrderNo,
                    ISNULL((SELECT SUM(cdd.Quantity * cdd.UnitPrice) FROM dbo.CustomerDeliveryDetails cdd WHERE cdd.CustomerDeliveryID = cdm.CustomerDeliveryID), 0) AS TotalAmount
                FROM dbo.CustomerDeliveryMaster cdm
                INNER JOIN dbo.Customer c ON cdm.CustomerID = c.CustomerID
                LEFT JOIN dbo.CustomerOrderMaster com ON cdm.CustomerOrderID = com.CustomerOrderID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.CustomerDeliveryMaster cdm
                INNER JOIN dbo.Customer c ON cdm.CustomerID = c.CustomerID
                LEFT JOIN dbo.CustomerOrderMaster com ON cdm.CustomerOrderID = com.CustomerOrderID";

            string filterCondition = options.filter != null ? GridQueryBuilder<CustomerDeliveryMaster>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<CustomerDeliveryMaster>.Query(options, baseQuery, "cdm.CustomerDeliveryID DESC");

            var list = new List<CustomerDeliveryMaster>();
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

        public async Task<long> InsertAsync(CustomerDeliveryMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string insertMasterSql = @"
                    INSERT INTO dbo.CustomerDeliveryMaster 
                        (DeliveryNo, CustomerID, CustomerOrderID, DeliveryDate, DeliveryAddress, VehicleNo, DriverName, Remarks, Status, CreatedAt)
                    OUTPUT INSERTED.CustomerDeliveryID
                    VALUES 
                        (@DeliveryNo, @CustomerID, @CustomerOrderID, @DeliveryDate, @DeliveryAddress, @VehicleNo, @DriverName, @Remarks, @Status, SYSDATETIME())";

                long newMasterId;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = insertMasterSql;
                    cmd.Parameters.Add("@DeliveryNo", SqlDbType.NVarChar, 30).Value = master.DeliveryNo.Trim();
                    cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = master.CustomerID;
                    cmd.Parameters.Add("@CustomerOrderID", SqlDbType.BigInt).Value = (object?)master.CustomerOrderID ?? DBNull.Value;
                    cmd.Parameters.Add("@DeliveryDate", SqlDbType.Date).Value = master.DeliveryDate;
                    cmd.Parameters.Add("@DeliveryAddress", SqlDbType.NVarChar, 500).Value = (object?)master.DeliveryAddress ?? DBNull.Value;
                    cmd.Parameters.Add("@VehicleNo", SqlDbType.NVarChar, 50).Value = (object?)master.VehicleNo ?? DBNull.Value;
                    cmd.Parameters.Add("@DriverName", SqlDbType.NVarChar, 100).Value = (object?)master.DriverName ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;

                    newMasterId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.CustomerDeliveryDetails 
                            (CustomerDeliveryID, CustomerOrderDetailID, ProductID, Quantity, UnitPrice, Remarks)
                        VALUES 
                            (@CustomerDeliveryID, @CustomerOrderDetailID, @ProductID, @Quantity, @UnitPrice, @Remarks)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@CustomerDeliveryID", SqlDbType.BigInt).Value = newMasterId;
                        dCmd.Parameters.Add("@CustomerOrderDetailID", SqlDbType.BigInt).Value = (object?)line.CustomerOrderDetailID ?? DBNull.Value;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = line.UnitPrice;
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

        public async Task<bool> UpdateAsync(CustomerDeliveryMaster master)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                const string updateMasterSql = @"
                    UPDATE dbo.CustomerDeliveryMaster 
                    SET DeliveryNo = @DeliveryNo,
                        CustomerID = @CustomerID,
                        CustomerOrderID = @CustomerOrderID,
                        DeliveryDate = @DeliveryDate,
                        DeliveryAddress = @DeliveryAddress,
                        VehicleNo = @VehicleNo,
                        DriverName = @DriverName,
                        Remarks = @Remarks,
                        Status = @Status
                    WHERE CustomerDeliveryID = @CustomerDeliveryID";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = updateMasterSql;
                    cmd.Parameters.Add("@DeliveryNo", SqlDbType.NVarChar, 30).Value = master.DeliveryNo.Trim();
                    cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = master.CustomerID;
                    cmd.Parameters.Add("@CustomerOrderID", SqlDbType.BigInt).Value = (object?)master.CustomerOrderID ?? DBNull.Value;
                    cmd.Parameters.Add("@DeliveryDate", SqlDbType.Date).Value = master.DeliveryDate;
                    cmd.Parameters.Add("@DeliveryAddress", SqlDbType.NVarChar, 500).Value = (object?)master.DeliveryAddress ?? DBNull.Value;
                    cmd.Parameters.Add("@VehicleNo", SqlDbType.NVarChar, 50).Value = (object?)master.VehicleNo ?? DBNull.Value;
                    cmd.Parameters.Add("@DriverName", SqlDbType.NVarChar, 100).Value = (object?)master.DriverName ?? DBNull.Value;
                    cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)master.Remarks ?? DBNull.Value;
                    cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = master.Status;
                    cmd.Parameters.Add("@CustomerDeliveryID", SqlDbType.BigInt).Value = master.CustomerDeliveryID;

                    await cmd.ExecuteNonQueryAsync();
                }

                const string deleteDetailsSql = "DELETE FROM dbo.CustomerDeliveryDetails WHERE CustomerDeliveryID = @CustomerDeliveryID";
                using (var delCmd = conn.CreateCommand())
                {
                    delCmd.Transaction = transaction;
                    delCmd.CommandText = deleteDetailsSql;
                    delCmd.Parameters.Add("@CustomerDeliveryID", SqlDbType.BigInt).Value = master.CustomerDeliveryID;
                    await delCmd.ExecuteNonQueryAsync();
                }

                if (master.DetailsList != null && master.DetailsList.Count > 0)
                {
                    const string insertDetailsSql = @"
                        INSERT INTO dbo.CustomerDeliveryDetails 
                            (CustomerDeliveryID, CustomerOrderDetailID, ProductID, Quantity, UnitPrice, Remarks)
                        VALUES 
                            (@CustomerDeliveryID, @CustomerOrderDetailID, @ProductID, @Quantity, @UnitPrice, @Remarks)";

                    foreach (var line in master.DetailsList)
                    {
                        using var dCmd = conn.CreateCommand();
                        dCmd.Transaction = transaction;
                        dCmd.CommandText = insertDetailsSql;
                        dCmd.Parameters.Add("@CustomerDeliveryID", SqlDbType.BigInt).Value = master.CustomerDeliveryID;
                        dCmd.Parameters.Add("@CustomerOrderDetailID", SqlDbType.BigInt).Value = (object?)line.CustomerOrderDetailID ?? DBNull.Value;
                        dCmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = line.ProductID;
                        dCmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = line.Quantity;
                        dCmd.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = line.UnitPrice;
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
                const string deleteDetailsSql = "DELETE FROM dbo.CustomerDeliveryDetails WHERE CustomerDeliveryID = @ID";
                using (var dCmd = conn.CreateCommand())
                {
                    dCmd.Transaction = transaction;
                    dCmd.CommandText = deleteDetailsSql;
                    dCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
                    await dCmd.ExecuteNonQueryAsync();
                }

                const string deleteMasterSql = "DELETE FROM dbo.CustomerDeliveryMaster WHERE CustomerDeliveryID = @ID";
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

        public async Task<bool> ExistsByNoAsync(string deliveryNo, long excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.CustomerDeliveryMaster WHERE DeliveryNo = @No AND CustomerDeliveryID <> @ExcludeID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@No", SqlDbType.NVarChar, 30).Value = deliveryNo.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.BigInt).Value = excludeId;

            await conn.OpenAsync();
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        private static CustomerDeliveryMaster MapMaster(SqlDataReader reader)
        {
            return new CustomerDeliveryMaster
            {
                CustomerDeliveryID = reader.GetInt64(reader.GetOrdinal("CustomerDeliveryID")),
                DeliveryNo = reader.GetString(reader.GetOrdinal("DeliveryNo")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                CustomerOrderID = reader.IsDBNull(reader.GetOrdinal("CustomerOrderID")) ? null : reader.GetInt64(reader.GetOrdinal("CustomerOrderID")),
                DeliveryDate = reader.GetDateTime(reader.GetOrdinal("DeliveryDate")),
                DeliveryAddress = reader.IsDBNull(reader.GetOrdinal("DeliveryAddress")) ? null : reader.GetString(reader.GetOrdinal("DeliveryAddress")),
                VehicleNo = reader.IsDBNull(reader.GetOrdinal("VehicleNo")) ? null : reader.GetString(reader.GetOrdinal("VehicleNo")),
                DriverName = reader.IsDBNull(reader.GetOrdinal("DriverName")) ? null : reader.GetString(reader.GetOrdinal("DriverName")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                CustomerOrderNo = reader.IsDBNull(reader.GetOrdinal("CustomerOrderNo")) ? null : reader.GetString(reader.GetOrdinal("CustomerOrderNo")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
            };
        }

        private static CustomerDeliveryDetails MapDetails(SqlDataReader reader)
        {
            return new CustomerDeliveryDetails
            {
                CustomerDeliveryDetailID = reader.GetInt64(reader.GetOrdinal("CustomerDeliveryDetailID")),
                CustomerDeliveryID = reader.GetInt64(reader.GetOrdinal("CustomerDeliveryID")),
                CustomerOrderDetailID = reader.IsDBNull(reader.GetOrdinal("CustomerOrderDetailID")) ? null : reader.GetInt64(reader.GetOrdinal("CustomerOrderDetailID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                Quantity = reader.GetDecimal(reader.GetOrdinal("Quantity")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                ProductCode = reader.GetString(reader.GetOrdinal("ProductCode")),
                ProductNameText = reader.GetString(reader.GetOrdinal("ProductNameText")),
                UnitName = reader.GetString(reader.GetOrdinal("UnitName"))
            };
        }
    }
}

