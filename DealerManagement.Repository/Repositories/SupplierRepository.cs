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
    public class SupplierRepository : BaseRepository, ISupplierRepository
    {
        public SupplierRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<Supplier>> GetAllAsync()
        {
            const string sql = @"
                SELECT SupplierID, SupplierCode, SupplierName, ContactPerson, Phone, Email, Address, TaxRegistrationNo, OpeningDate, IsActive, CreatedAt
                FROM dbo.Supplier
                ORDER BY SupplierName";

            var list = new List<Supplier>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapSupplier(reader));
            }
            return list;
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT TOP 1 SupplierID, SupplierCode, SupplierName, ContactPerson, Phone, Email, Address, TaxRegistrationNo, OpeningDate, IsActive, CreatedAt
                FROM dbo.Supplier
                WHERE SupplierID = @ID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapSupplier(reader);
            }
            return null;
        }

        public async Task<(List<Supplier> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT SupplierID, SupplierCode, SupplierName, ContactPerson, Phone, Email, Address, TaxRegistrationNo, OpeningDate, IsActive, CreatedAt
                FROM dbo.Supplier";

            string countQuery = "SELECT COUNT(1) FROM dbo.Supplier";

            string filterCondition = options.filter != null ? GridQueryBuilder<Supplier>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<Supplier>.Query(options, baseQuery, "SupplierID DESC");

            var list = new List<Supplier>();
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
                    list.Add(MapSupplier(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<int> InsertAsync(Supplier model)
        {
            const string sql = @"
                INSERT INTO dbo.Supplier
                    (SupplierCode, SupplierName, ContactPerson, Phone, Email, Address, TaxRegistrationNo, OpeningDate, IsActive, CreatedAt)
                OUTPUT INSERTED.SupplierID
                VALUES
                    (@SupplierCode, @SupplierName, @ContactPerson, @Phone, @Email, @Address, @TaxRegistrationNo, @OpeningDate, @IsActive, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@SupplierCode", SqlDbType.NVarChar, 30).Value = model.SupplierCode.Trim();
            cmd.Parameters.Add("@SupplierName", SqlDbType.NVarChar, 150).Value = model.SupplierName.Trim();
            cmd.Parameters.Add("@ContactPerson", SqlDbType.NVarChar, 100).Value = (object?)model.ContactPerson ?? DBNull.Value;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 30).Value = (object?)model.Phone ?? DBNull.Value;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = (object?)model.Email ?? DBNull.Value;
            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = (object?)model.Address ?? DBNull.Value;
            cmd.Parameters.Add("@TaxRegistrationNo", SqlDbType.NVarChar, 50).Value = (object?)model.TaxRegistrationNo ?? DBNull.Value;
            cmd.Parameters.Add("@OpeningDate", SqlDbType.Date).Value = (object?)model.OpeningDate ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<bool> UpdateAsync(Supplier model)
        {
            const string sql = @"
                UPDATE dbo.Supplier
                SET SupplierCode = @SupplierCode,
                    SupplierName = @SupplierName,
                    ContactPerson = @ContactPerson,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address,
                    TaxRegistrationNo = @TaxRegistrationNo,
                    OpeningDate = @OpeningDate,
                    IsActive = @IsActive
                WHERE SupplierID = @SupplierID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@SupplierCode", SqlDbType.NVarChar, 30).Value = model.SupplierCode.Trim();
            cmd.Parameters.Add("@SupplierName", SqlDbType.NVarChar, 150).Value = model.SupplierName.Trim();
            cmd.Parameters.Add("@ContactPerson", SqlDbType.NVarChar, 100).Value = (object?)model.ContactPerson ?? DBNull.Value;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 30).Value = (object?)model.Phone ?? DBNull.Value;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = (object?)model.Email ?? DBNull.Value;
            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = (object?)model.Address ?? DBNull.Value;
            cmd.Parameters.Add("@TaxRegistrationNo", SqlDbType.NVarChar, 50).Value = (object?)model.TaxRegistrationNo ?? DBNull.Value;
            cmd.Parameters.Add("@OpeningDate", SqlDbType.Date).Value = (object?)model.OpeningDate ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
            cmd.Parameters.Add("@SupplierID", SqlDbType.Int).Value = model.SupplierID;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM dbo.Supplier WHERE SupplierID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> ExistsByCodeAsync(string code, int excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Supplier WHERE SupplierCode = @Code AND SupplierID <> @ExcludeID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 30).Value = code.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.Int).Value = excludeId;

            await conn.OpenAsync();
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        private static Supplier MapSupplier(SqlDataReader reader)
        {
            return new Supplier
            {
                SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                SupplierCode = reader.GetString(reader.GetOrdinal("SupplierCode")),
                SupplierName = reader.GetString(reader.GetOrdinal("SupplierName")),
                ContactPerson = reader.IsDBNull(reader.GetOrdinal("ContactPerson")) ? null : reader.GetString(reader.GetOrdinal("ContactPerson")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                TaxRegistrationNo = reader.IsDBNull(reader.GetOrdinal("TaxRegistrationNo")) ? null : reader.GetString(reader.GetOrdinal("TaxRegistrationNo")),
                OpeningDate = reader.IsDBNull(reader.GetOrdinal("OpeningDate")) ? null : reader.GetDateTime(reader.GetOrdinal("OpeningDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}

