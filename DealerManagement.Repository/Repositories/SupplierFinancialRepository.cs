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
    public class SupplierFinancialRepository : BaseRepository, ISupplierFinancialRepository
    {
        public SupplierFinancialRepository(IConfiguration configuration) : base(configuration)
        {
        }

        #region Supplier Deposit
        public async Task<SupplierDeposit?> GetDepositByIdAsync(long id)
        {
            const string sql = @"
                SELECT sd.SupplierDepositID, sd.DepositNo, sd.SupplierID, sd.DepositDate,
                       sd.Amount, sd.PaymentMethod, sd.ReferenceNo, sd.Remarks, sd.Status, sd.CreatedAt,
                       s.SupplierName
                FROM dbo.SupplierDeposit sd
                INNER JOIN dbo.Supplier s ON sd.SupplierID = s.SupplierID
                WHERE sd.SupplierDepositID = @ID";

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

        public async Task<(List<SupplierDeposit> Items, int TotalCount)> GetDepositGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT sd.SupplierDepositID, sd.DepositNo, sd.SupplierID, sd.DepositDate,
                       sd.Amount, sd.PaymentMethod, sd.ReferenceNo, sd.Remarks, sd.Status, sd.CreatedAt,
                       s.SupplierName
                FROM dbo.SupplierDeposit sd
                INNER JOIN dbo.Supplier s ON sd.SupplierID = s.SupplierID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.SupplierDeposit sd
                INNER JOIN dbo.Supplier s ON sd.SupplierID = s.SupplierID";

            string filterCondition = options.filter != null ? GridQueryBuilder<SupplierDeposit>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<SupplierDeposit>.Query(options, baseQuery, "sd.SupplierDepositID DESC");

            var list = new List<SupplierDeposit>();
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

        public async Task<long> InsertDepositAsync(SupplierDeposit model)
        {
            const string sql = @"
                INSERT INTO dbo.SupplierDeposit 
                    (DepositNo, SupplierID, DepositDate, Amount, PaymentMethod, ReferenceNo, Remarks, Status, CreatedAt)
                OUTPUT INSERTED.SupplierDepositID
                VALUES 
                    (@DepositNo, @SupplierID, @DepositDate, @Amount, @PaymentMethod, @ReferenceNo, @Remarks, @Status, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@DepositNo", SqlDbType.NVarChar, 30).Value = model.DepositNo.Trim();
            cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = model.SupplierID;
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

        public async Task<bool> UpdateDepositAsync(SupplierDeposit model)
        {
            const string sql = @"
                UPDATE dbo.SupplierDeposit 
                SET DepositNo = @DepositNo,
                    SupplierID = @SupplierID,
                    DepositDate = @DepositDate,
                    Amount = @Amount,
                    PaymentMethod = @PaymentMethod,
                    ReferenceNo = @ReferenceNo,
                    Remarks = @Remarks,
                    Status = @Status
                WHERE SupplierDepositID = @SupplierDepositID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@DepositNo", SqlDbType.NVarChar, 30).Value = model.DepositNo.Trim();
            cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = model.SupplierID;
            cmd.Parameters.Add("@DepositDate", SqlDbType.Date).Value = model.DepositDate;
            cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = model.Amount;
            cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 30).Value = (object?)model.PaymentMethod ?? DBNull.Value;
            cmd.Parameters.Add("@ReferenceNo", SqlDbType.NVarChar, 100).Value = (object?)model.ReferenceNo ?? DBNull.Value;
            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)model.Remarks ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = model.Status;
            cmd.Parameters.Add("@SupplierDepositID", SqlDbType.BigInt).Value = model.SupplierDepositID;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteDepositAsync(long id)
        {
            const string sql = "DELETE FROM dbo.SupplierDeposit WHERE SupplierDepositID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static SupplierDeposit MapDeposit(SqlDataReader reader)
        {
            return new SupplierDeposit
            {
                SupplierDepositID = reader.GetInt64(reader.GetOrdinal("SupplierDepositID")),
                DepositNo = reader.GetString(reader.GetOrdinal("DepositNo")),
                SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                DepositDate = reader.GetDateTime(reader.GetOrdinal("DepositDate")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                ReferenceNo = reader.IsDBNull(reader.GetOrdinal("ReferenceNo")) ? null : reader.GetString(reader.GetOrdinal("ReferenceNo")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                SupplierName = reader.IsDBNull(reader.GetOrdinal("SupplierName")) ? null : reader.GetString(reader.GetOrdinal("SupplierName"))
            };
        }
        #endregion

        #region Supplier Commission
        public async Task<SupplierCommission?> GetCommissionByIdAsync(long id)
        {
            const string sql = @"
                SELECT sc.SupplierCommissionID, sc.SupplierID, sc.CommissionDate, sc.CommissionType,
                       sc.ReferenceNo, sc.CommissionAmount, sc.Remarks, sc.Status, sc.CreatedAt,
                       s.SupplierName
                FROM dbo.SupplierCommission sc
                INNER JOIN dbo.Supplier s ON sc.SupplierID = s.SupplierID
                WHERE sc.SupplierCommissionID = @ID";

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

        public async Task<(List<SupplierCommission> Items, int TotalCount)> GetCommissionGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT sc.SupplierCommissionID, sc.SupplierID, sc.CommissionDate, sc.CommissionType,
                       sc.ReferenceNo, sc.CommissionAmount, sc.Remarks, sc.Status, sc.CreatedAt,
                       s.SupplierName
                FROM dbo.SupplierCommission sc
                INNER JOIN dbo.Supplier s ON sc.SupplierID = s.SupplierID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.SupplierCommission sc
                INNER JOIN dbo.Supplier s ON sc.SupplierID = s.SupplierID";

            string filterCondition = options.filter != null ? GridQueryBuilder<SupplierCommission>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<SupplierCommission>.Query(options, baseQuery, "sc.SupplierCommissionID DESC");

            var list = new List<SupplierCommission>();
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

        public async Task<long> InsertCommissionAsync(SupplierCommission model)
        {
            const string sql = @"
                INSERT INTO dbo.SupplierCommission 
                    (SupplierID, CommissionDate, CommissionType, ReferenceNo, CommissionAmount, Remarks, Status, CreatedAt)
                OUTPUT INSERTED.SupplierCommissionID
                VALUES 
                    (@SupplierID, @CommissionDate, @CommissionType, @ReferenceNo, @CommissionAmount, @Remarks, @Status, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = model.SupplierID;
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

        public async Task<bool> UpdateCommissionAsync(SupplierCommission model)
        {
            const string sql = @"
                UPDATE dbo.SupplierCommission 
                SET SupplierID = @SupplierID,
                    CommissionDate = @CommissionDate,
                    CommissionType = @CommissionType,
                    ReferenceNo = @ReferenceNo,
                    CommissionAmount = @CommissionAmount,
                    Remarks = @Remarks,
                    Status = @Status
                WHERE SupplierCommissionID = @SupplierCommissionID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = model.SupplierID;
            cmd.Parameters.Add("@CommissionDate", SqlDbType.Date).Value = model.CommissionDate;
            cmd.Parameters.Add("@CommissionType", SqlDbType.NVarChar, 50).Value = model.CommissionType;
            cmd.Parameters.Add("@ReferenceNo", SqlDbType.NVarChar, 50).Value = (object?)model.ReferenceNo ?? DBNull.Value;
            cmd.Parameters.Add("@CommissionAmount", SqlDbType.Decimal).Value = model.CommissionAmount;
            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 500).Value = (object?)model.Remarks ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = model.Status;
            cmd.Parameters.Add("@SupplierCommissionID", SqlDbType.BigInt).Value = model.SupplierCommissionID;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteCommissionAsync(long id)
        {
            const string sql = "DELETE FROM dbo.SupplierCommission WHERE SupplierCommissionID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static SupplierCommission MapCommission(SqlDataReader reader)
        {
            return new SupplierCommission
            {
                SupplierCommissionID = reader.GetInt64(reader.GetOrdinal("SupplierCommissionID")),
                SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                CommissionDate = reader.GetDateTime(reader.GetOrdinal("CommissionDate")),
                CommissionType = reader.GetString(reader.GetOrdinal("CommissionType")),
                ReferenceNo = reader.IsDBNull(reader.GetOrdinal("ReferenceNo")) ? null : reader.GetString(reader.GetOrdinal("ReferenceNo")),
                CommissionAmount = reader.GetDecimal(reader.GetOrdinal("CommissionAmount")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                SupplierName = reader.IsDBNull(reader.GetOrdinal("SupplierName")) ? null : reader.GetString(reader.GetOrdinal("SupplierName"))
            };
        }
        #endregion

        #region Supplier Opening Balance
        public async Task<SupplierOpeningBalance?> GetOpeningBalanceByIdAsync(long id)
        {
            const string sql = @"
                SELECT sob.SupplierOpeningBalanceID, sob.SupplierID, sob.OpeningDate, sob.BalanceAmount, sob.Remarks, sob.CreatedAt,
                       s.SupplierName
                FROM dbo.SupplierOpeningBalance sob
                INNER JOIN dbo.Supplier s ON sob.SupplierID = s.SupplierID
                WHERE sob.SupplierOpeningBalanceID = @ID";

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

        public async Task<(List<SupplierOpeningBalance> Items, int TotalCount)> GetOpeningBalanceGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT sob.SupplierOpeningBalanceID, sob.SupplierID, sob.OpeningDate, sob.BalanceAmount, sob.Remarks, sob.CreatedAt,
                       s.SupplierName
                FROM dbo.SupplierOpeningBalance sob
                INNER JOIN dbo.Supplier s ON sob.SupplierID = s.SupplierID";

            string countQuery = @"
                SELECT COUNT(1) 
                FROM dbo.SupplierOpeningBalance sob
                INNER JOIN dbo.Supplier s ON sob.SupplierID = s.SupplierID";

            string filterCondition = options.filter != null ? GridQueryBuilder<SupplierOpeningBalance>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<SupplierOpeningBalance>.Query(options, baseQuery, "sob.SupplierOpeningBalanceID DESC");

            var list = new List<SupplierOpeningBalance>();
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

        public async Task<long> InsertOpeningBalanceAsync(SupplierOpeningBalance model)
        {
            const string sql = @"
                INSERT INTO dbo.SupplierOpeningBalance 
                    (SupplierID, OpeningDate, BalanceAmount, Remarks, CreatedAt)
                OUTPUT INSERTED.SupplierOpeningBalanceID
                VALUES 
                    (@SupplierID, @OpeningDate, @BalanceAmount, @Remarks, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = model.SupplierID;
            cmd.Parameters.Add("@OpeningDate", SqlDbType.Date).Value = model.OpeningDate;
            cmd.Parameters.Add("@BalanceAmount", SqlDbType.Decimal).Value = model.BalanceAmount;
            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 250).Value = (object?)model.Remarks ?? DBNull.Value;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(id);
        }

        public async Task<bool> UpdateOpeningBalanceAsync(SupplierOpeningBalance model)
        {
            const string sql = @"
                UPDATE dbo.SupplierOpeningBalance 
                SET SupplierID = @SupplierID,
                    OpeningDate = @OpeningDate,
                    BalanceAmount = @BalanceAmount,
                    Remarks = @Remarks
                WHERE SupplierOpeningBalanceID = @SupplierOpeningBalanceID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = model.SupplierID;
            cmd.Parameters.Add("@OpeningDate", SqlDbType.Date).Value = model.OpeningDate;
            cmd.Parameters.Add("@BalanceAmount", SqlDbType.Decimal).Value = model.BalanceAmount;
            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar, 250).Value = (object?)model.Remarks ?? DBNull.Value;
            cmd.Parameters.Add("@SupplierOpeningBalanceID", SqlDbType.BigInt).Value = model.SupplierOpeningBalanceID;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteOpeningBalanceAsync(long id)
        {
            const string sql = "DELETE FROM dbo.SupplierOpeningBalance WHERE SupplierOpeningBalanceID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = id;
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static SupplierOpeningBalance MapOpeningBalance(SqlDataReader reader)
        {
            return new SupplierOpeningBalance
            {
                SupplierOpeningBalanceID = reader.GetInt64(reader.GetOrdinal("SupplierOpeningBalanceID")),
                SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                OpeningDate = reader.GetDateTime(reader.GetOrdinal("OpeningDate")),
                BalanceAmount = reader.GetDecimal(reader.GetOrdinal("BalanceAmount")),
                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                SupplierName = reader.IsDBNull(reader.GetOrdinal("SupplierName")) ? null : reader.GetString(reader.GetOrdinal("SupplierName"))
            };
        }
        #endregion
    }
}

