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
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {
        public CustomerRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            const string sql = @"
                SELECT CustomerID, CustomerCode, CustomerName, CustomerType, ContactPerson, Phone, AlternatePhone, Email, Address, Area, District, CreditLimit, OpeningDate, IsActive, CreatedAt
                FROM dbo.Customer
                ORDER BY CustomerName";

            var list = new List<Customer>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapCustomer(reader));
            }
            return list;
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT TOP 1 CustomerID, CustomerCode, CustomerName, CustomerType, ContactPerson, Phone, AlternatePhone, Email, Address, Area, District, CreditLimit, OpeningDate, IsActive, CreatedAt
                FROM dbo.Customer
                WHERE CustomerID = @ID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapCustomer(reader);
            }
            return null;
        }

        public async Task<(List<Customer> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT CustomerID, CustomerCode, CustomerName, CustomerType, ContactPerson, Phone, AlternatePhone, Email, Address, Area, District, CreditLimit, OpeningDate, IsActive, CreatedAt
                FROM dbo.Customer";

            string countQuery = "SELECT COUNT(1) FROM dbo.Customer";

            string filterCondition = options.filter != null ? GridQueryBuilder<Customer>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<Customer>.Query(options, baseQuery, "CustomerID DESC");

            var list = new List<Customer>();
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
                    list.Add(MapCustomer(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<int> InsertAsync(Customer model)
        {
            const string sql = @"
                INSERT INTO dbo.Customer
                    (CustomerCode, CustomerName, CustomerType, ContactPerson, Phone, AlternatePhone, Email, Address, Area, District, CreditLimit, OpeningDate, IsActive, CreatedAt)
                OUTPUT INSERTED.CustomerID
                VALUES
                    (@CustomerCode, @CustomerName, @CustomerType, @ContactPerson, @Phone, @AlternatePhone, @Email, @Address, @Area, @District, @CreditLimit, @OpeningDate, @IsActive, SYSDATETIME())";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@CustomerCode", SqlDbType.NVarChar, 30).Value = model.CustomerCode.Trim();
            cmd.Parameters.Add("@CustomerName", SqlDbType.NVarChar, 150).Value = model.CustomerName.Trim();
            cmd.Parameters.Add("@CustomerType", SqlDbType.NVarChar, 30).Value = model.CustomerType.Trim();
            cmd.Parameters.Add("@ContactPerson", SqlDbType.NVarChar, 100).Value = (object?)model.ContactPerson ?? DBNull.Value;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 30).Value = (object?)model.Phone ?? DBNull.Value;
            cmd.Parameters.Add("@AlternatePhone", SqlDbType.NVarChar, 30).Value = (object?)model.AlternatePhone ?? DBNull.Value;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = (object?)model.Email ?? DBNull.Value;
            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = (object?)model.Address ?? DBNull.Value;
            cmd.Parameters.Add("@Area", SqlDbType.NVarChar, 100).Value = (object?)model.Area ?? DBNull.Value;
            cmd.Parameters.Add("@District", SqlDbType.NVarChar, 100).Value = (object?)model.District ?? DBNull.Value;
            cmd.Parameters.Add("@CreditLimit", SqlDbType.Decimal).Value = model.CreditLimit;
            cmd.Parameters.Add("@OpeningDate", SqlDbType.Date).Value = (object?)model.OpeningDate ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<bool> UpdateAsync(Customer model)
        {
            const string sql = @"
                UPDATE dbo.Customer
                SET CustomerCode = @CustomerCode,
                    CustomerName = @CustomerName,
                    CustomerType = @CustomerType,
                    ContactPerson = @ContactPerson,
                    Phone = @Phone,
                    AlternatePhone = @AlternatePhone,
                    Email = @Email,
                    Address = @Address,
                    Area = @Area,
                    District = @District,
                    CreditLimit = @CreditLimit,
                    OpeningDate = @OpeningDate,
                    IsActive = @IsActive
                WHERE CustomerID = @CustomerID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@CustomerCode", SqlDbType.NVarChar, 30).Value = model.CustomerCode.Trim();
            cmd.Parameters.Add("@CustomerName", SqlDbType.NVarChar, 150).Value = model.CustomerName.Trim();
            cmd.Parameters.Add("@CustomerType", SqlDbType.NVarChar, 30).Value = model.CustomerType.Trim();
            cmd.Parameters.Add("@ContactPerson", SqlDbType.NVarChar, 100).Value = (object?)model.ContactPerson ?? DBNull.Value;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 30).Value = (object?)model.Phone ?? DBNull.Value;
            cmd.Parameters.Add("@AlternatePhone", SqlDbType.NVarChar, 30).Value = (object?)model.AlternatePhone ?? DBNull.Value;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = (object?)model.Email ?? DBNull.Value;
            cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = (object?)model.Address ?? DBNull.Value;
            cmd.Parameters.Add("@Area", SqlDbType.NVarChar, 100).Value = (object?)model.Area ?? DBNull.Value;
            cmd.Parameters.Add("@District", SqlDbType.NVarChar, 100).Value = (object?)model.District ?? DBNull.Value;
            cmd.Parameters.Add("@CreditLimit", SqlDbType.Decimal).Value = model.CreditLimit;
            cmd.Parameters.Add("@OpeningDate", SqlDbType.Date).Value = (object?)model.OpeningDate ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
            cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = model.CustomerID;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM dbo.Customer WHERE CustomerID = @ID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> ExistsByCodeAsync(string code, int excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Customer WHERE CustomerCode = @Code AND CustomerID <> @ExcludeID";
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 30).Value = code.Trim();
            cmd.Parameters.Add("@ExcludeID", SqlDbType.Int).Value = excludeId;

            await conn.OpenAsync();
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        private static Customer MapCustomer(SqlDataReader reader)
        {
            return new Customer
            {
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                CustomerCode = reader.GetString(reader.GetOrdinal("CustomerCode")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                CustomerType = reader.GetString(reader.GetOrdinal("CustomerType")),
                ContactPerson = reader.IsDBNull(reader.GetOrdinal("ContactPerson")) ? null : reader.GetString(reader.GetOrdinal("ContactPerson")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                AlternatePhone = reader.IsDBNull(reader.GetOrdinal("AlternatePhone")) ? null : reader.GetString(reader.GetOrdinal("AlternatePhone")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                Area = reader.IsDBNull(reader.GetOrdinal("Area")) ? null : reader.GetString(reader.GetOrdinal("Area")),
                District = reader.IsDBNull(reader.GetOrdinal("District")) ? null : reader.GetString(reader.GetOrdinal("District")),
                CreditLimit = reader.GetDecimal(reader.GetOrdinal("CreditLimit")),
                OpeningDate = reader.IsDBNull(reader.GetOrdinal("OpeningDate")) ? null : reader.GetDateTime(reader.GetOrdinal("OpeningDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}

