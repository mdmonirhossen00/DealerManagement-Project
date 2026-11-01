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
    public class CustomerFinancialRepository : BaseRepository, ICustomerFinancialRepository
    {
        public CustomerFinancialRepository(IConfiguration configuration) : base(configuration)
        {
        }

        #region Customer Deposit
        public async Task<CustomerDeposit?> GetDepositByIdAsync(long id)
        {
            const string sql = @"
                SELECT cd.CustomerDepositID, cd.DepositNo, cd.CustomerID, cd.DepositDate,
                       cd.Amount, cd.PaymentMethod, cd.ReferenceNo, cd.Remarks, cd.Status, cd.CreatedAt,
                       c.CustomerName
                FROM dbo.CustomerDeposit cd
                INNER JOIN dbo.Customer c ON cd.CustomerID = c.CustomerID
                WHERE cd.CustomerDepositID = @ID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapDeposit(reader);
            }
            return null;
        }

        public async Task<(List<CustomerDeposit> Items, int TotalCount)> GetDepositGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT cd.CustomerDepositID, cd.DepositNo, cd.CustomerID, cd.DepositDate,
                       cd.Amount, cd.PaymentMethod, cd.ReferenceNo, cd.Remarks, cd.Status, cd.CreatedAt,
                       c.CustomerName
                FROM dbo.CustomerDeposit cd
                INNER JOIN dbo.Customer c ON cd.CustomerID = c.CustomerID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.CustomerDeposit cd
                INNER JOIN dbo.Customer c ON cd.CustomerID = c.CustomerID";

            string filterCondition = options.filter != null ? GridQueryBuilder<CustomerDeposit>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<CustomerDeposit>.Query(options, baseQuery, "cd.CustomerDepositID DESC");

            var list = new List<CustomerDeposit>();
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
                    list.Add(MapDeposit(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<long> InsertDepositAsync(CustomerDeposit model)
        {
            const string sql = @"
                INSERT INTO dbo.CustomerDeposit 
                    (DepositNo, CustomerID, DepositDate, Amount, PaymentMethod, ReferenceNo, Remarks, Status, CreatedAt)
                OUTPUT INSERTED.CustomerDepositID
                VALUES 
                    (@DepositNo, @CustomerID, @DepositDate, @Amount, @PaymentMethod, @ReferenceNo, @Remarks, @Status, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@DepositNo", SqlDbType.NVarChar, 30).Value = model.DepositNo.Trim();
            cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = model.CustomerID;
            cmd.Parameters.Add("@DepositDate", SqlDbType.Date).Value = model.DepositDate;
            cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = model.Amount;
            cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 30).Value = (object?)model.PaymentMethod ?? DBNull.Value;
            cmd.Parameters.Add("@ReferenceNo", SqlDbType.NVarChar, 100).Value = (object?)model.ReferenceNo ?? DBNull.Value;
            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)model.Remarks ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = model.Status;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(id);
        }

        public async Task<bool> UpdateDepositAsync(CustomerDeposit model)
        {
            const string sql = @"
                UPDATE dbo.CustomerDeposit 
                SET DepositNo = @DepositNo,
                    CustomerID = @CustomerID,
                    DepositDate = @DepositDate,
                    Amount = @Amount,
                    PaymentMethod = @PaymentMethod,
                    ReferenceNo = @ReferenceNo,
                    Remarks = @Remarks,
                    Status = @Status
                WHERE CustomerDepositID = @CustomerDepositID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@DepositNo", SqlDbType.NVarChar, 30).Value = model.DepositNo.Trim();
            cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = model.CustomerID;
            cmd.Parameters.Add("@DepositDate", SqlDbType.Date).Value = model.DepositDate;
            cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = model.Amount;
            cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 30).Value = (object?)model.PaymentMethod ?? DBNull.Value;
            cmd.Parameters.Add("@ReferenceNo", SqlDbType.NVarChar, 100).Value = (object?)model.ReferenceNo ?? DBNull.Value;
            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)model.Remarks ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = model.Status;
            cmd.Parameters.Add("@CustomerDepositID", SqlDbType.BigInt).Value = model.CustomerDepositID;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteDepositAsync(long id)
        {
            const string sql = "DELETE FROM dbo.CustomerDeposit WHERE CustomerDepositID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static CustomerDeposit MapDeposit(SqlDataReader reader)
        {
            return new CustomerDeposit
            {
                CustomerDepositID = reader.GetInt64(reader.GetOrdinal("CustomerDepositID")),
                DepositNo = reader.GetString(reader.GetOrdinal("DepositNo")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                DepositDate = reader.GetDateTime(reader.GetOrdinal("DepositDate")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                ReferenceNo = reader.IsDBNull(reader.GetOrdinal("ReferenceNo")) ? null : reader.GetString(reader.GetOrdinal("ReferenceNo")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName"))
            };
        }
        #endregion

        #region Customer Commission
        public async Task<CustomerCommission?> GetCommissionByIdAsync(long id)
        {
            const string sql = @"
                SELECT cc.CustomerCommissionID, cc.CustomerID, cc.CommissionDate, cc.CommissionType,
                       cc.ReferenceNo, cc.CommissionAmount, cc.Remarks, cc.Status, cc.CreatedAt,
                       c.CustomerName
                FROM dbo.CustomerCommission cc
                INNER JOIN dbo.Customer c ON cc.CustomerID = c.CustomerID
                WHERE cc.CustomerCommissionID = @ID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapCommission(reader);
            }
            return null;
        }

        public async Task<(List<CustomerCommission> Items, int TotalCount)> GetCommissionGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT cc.CustomerCommissionID, cc.CustomerID, cc.CommissionDate, cc.CommissionType,
                       cc.ReferenceNo, cc.CommissionAmount, cc.Remarks, cc.Status, cc.CreatedAt,
                       c.CustomerName
                FROM dbo.CustomerCommission cc
                INNER JOIN dbo.Customer c ON cc.CustomerID = c.CustomerID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.CustomerCommission cc
                INNER JOIN dbo.Customer c ON cc.CustomerID = c.CustomerID";

            string filterCondition = options.filter != null ? GridQueryBuilder<CustomerCommission>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<CustomerCommission>.Query(options, baseQuery, "cc.CustomerCommissionID DESC");

            var list = new List<CustomerCommission>();
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
                    list.Add(MapCommission(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<long> InsertCommissionAsync(CustomerCommission model)
        {
            const string sql = @"
                INSERT INTO dbo.CustomerCommission 
                    (CustomerID, CommissionDate, CommissionType, ReferenceNo, CommissionAmount, Remarks, Status, CreatedAt)
                OUTPUT INSERTED.CustomerCommissionID
                VALUES 
                    (@CustomerID, @CommissionDate, @CommissionType, @ReferenceNo, @CommissionAmount, @Remarks, @Status, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = model.CustomerID;
            cmd.Parameters.Add("@CommissionDate", SqlDbType.Date).Value = model.CommissionDate;
            cmd.Parameters.Add("@CommissionType", SqlDbType.NVarChar, 50).Value = model.CommissionType;
            cmd.Parameters.Add("@ReferenceNo", SqlDbType.NVarChar, 50).Value = (object?)model.ReferenceNo ?? DBNull.Value;
            cmd.Parameters.Add("@CommissionAmount", SqlDbType.Decimal).Value = model.CommissionAmount;
            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)model.Remarks ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = model.Status;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(id);
        }

        public async Task<bool> UpdateCommissionAsync(CustomerCommission model)
        {
            const string sql = @"
                UPDATE dbo.CustomerCommission 
                SET CustomerID = @CustomerID,
                    CommissionDate = @CommissionDate,
                    CommissionType = @CommissionType,
                    ReferenceNo = @ReferenceNo,
                    CommissionAmount = @CommissionAmount,
                    Remarks = @Remarks,
                    Status = @Status
                WHERE CustomerCommissionID = @CustomerCommissionID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = model.CustomerID;
            cmd.Parameters.Add("@CommissionDate", SqlDbType.Date).Value = model.CommissionDate;
            cmd.Parameters.Add("@CommissionType", SqlDbType.NVarChar, 50).Value = model.CommissionType;
            cmd.Parameters.Add("@ReferenceNo", SqlDbType.NVarChar, 50).Value = (object?)model.ReferenceNo ?? DBNull.Value;
            cmd.Parameters.Add("@CommissionAmount", SqlDbType.Decimal).Value = model.CommissionAmount;
            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)model.Remarks ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = model.Status;
            cmd.Parameters.Add("@CustomerCommissionID", SqlDbType.BigInt).Value = model.CustomerCommissionID;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteCommissionAsync(long id)
        {
            const string sql = "DELETE FROM dbo.CustomerCommission WHERE CustomerCommissionID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static CustomerCommission MapCommission(SqlDataReader reader)
        {
            return new CustomerCommission
            {
                CustomerCommissionID = reader.GetInt64(reader.GetOrdinal("CustomerCommissionID")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                CommissionDate = reader.GetDateTime(reader.GetOrdinal("CommissionDate")),
                CommissionType = reader.GetString(reader.GetOrdinal("CommissionType")),
                ReferenceNo = reader.IsDBNull(reader.GetOrdinal("ReferenceNo")) ? null : reader.GetString(reader.GetOrdinal("ReferenceNo")),
                CommissionAmount = reader.GetDecimal(reader.GetOrdinal("CommissionAmount")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName"))
            };
        }
        #endregion

        #region Customer Opening Balance
        public async Task<CustomerOpeningBalance?> GetOpeningBalanceByIdAsync(long id)
        {
            const string sql = @"
                SELECT cob.CustomerOpeningBalanceID, cob.CustomerID, cob.OpeningDate, cob.BalanceAmount, cob.Remarks, cob.CreatedAt,
                       c.CustomerName
                FROM dbo.CustomerOpeningBalance cob
                INNER JOIN dbo.Customer c ON cob.CustomerID = c.CustomerID
                WHERE cob.CustomerOpeningBalanceID = @ID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapOpeningBalance(reader);
            }
            return null;
        }

        public async Task<(List<CustomerOpeningBalance> Items, int TotalCount)> GetOpeningBalanceGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT cob.CustomerOpeningBalanceID, cob.CustomerID, cob.OpeningDate, cob.BalanceAmount, cob.Remarks, cob.CreatedAt,
                       c.CustomerName
                FROM dbo.CustomerOpeningBalance cob
                INNER JOIN dbo.Customer c ON cob.CustomerID = c.CustomerID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.CustomerOpeningBalance cob
                INNER JOIN dbo.Customer c ON cob.CustomerID = c.CustomerID";

            string filterCondition = options.filter != null ? GridQueryBuilder<CustomerOpeningBalance>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<CustomerOpeningBalance>.Query(options, baseQuery, "cob.CustomerOpeningBalanceID DESC");

            var list = new List<CustomerOpeningBalance>();
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
                    list.Add(MapOpeningBalance(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<long> InsertOpeningBalanceAsync(CustomerOpeningBalance model)
        {
            const string sql = @"
                INSERT INTO dbo.CustomerOpeningBalance 
                    (CustomerID, OpeningDate, BalanceAmount, Remarks, CreatedAt)
                OUTPUT INSERTED.CustomerOpeningBalanceID
                VALUES 
                    (@CustomerID, @OpeningDate, @BalanceAmount, @Remarks, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = model.CustomerID;
            cmd.Parameters.Add("@OpeningDate", SqlDbType.Date).Value = model.OpeningDate;
            cmd.Parameters.Add("@BalanceAmount", SqlDbType.Decimal).Value = model.BalanceAmount;
            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 250).Value = (object?)model.Remarks ?? DBNull.Value;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(id);
        }

        public async Task<bool> UpdateOpeningBalanceAsync(CustomerOpeningBalance model)
        {
            const string sql = @"
                UPDATE dbo.CustomerOpeningBalance 
                SET CustomerID = @CustomerID,
                    OpeningDate = @OpeningDate,
                    BalanceAmount = @BalanceAmount,
                    Remarks = @Remarks
                WHERE CustomerOpeningBalanceID = @CustomerOpeningBalanceID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = model.CustomerID;
            cmd.Parameters.Add("@OpeningDate", SqlDbType.Date).Value = model.OpeningDate;
            cmd.Parameters.Add("@BalanceAmount", SqlDbType.Decimal).Value = model.BalanceAmount;
            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 250).Value = (object?)model.Remarks ?? DBNull.Value;
            cmd.Parameters.Add("@CustomerOpeningBalanceID", SqlDbType.BigInt).Value = model.CustomerOpeningBalanceID;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteOpeningBalanceAsync(long id)
        {
            const string sql = "DELETE FROM dbo.CustomerOpeningBalance WHERE CustomerOpeningBalanceID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static CustomerOpeningBalance MapOpeningBalance(SqlDataReader reader)
        {
            return new CustomerOpeningBalance
            {
                CustomerOpeningBalanceID = reader.GetInt64(reader.GetOrdinal("CustomerOpeningBalanceID")),
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                OpeningDate = reader.GetDateTime(reader.GetOrdinal("OpeningDate")),
                BalanceAmount = reader.GetDecimal(reader.GetOrdinal("BalanceAmount")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName"))
            };
        }
        #endregion
    }
}

