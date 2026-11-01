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
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            const string sql = @"
                SELECT TOP 1 UserID, UserName, FullName, Email, PhoneNumber, PasswordHash, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
                FROM dbo.Users
                WHERE UserName = @UserName";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = userName.Trim();

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            const string sql = @"
                SELECT TOP 1 UserID, UserName, FullName, Email, PhoneNumber, PasswordHash, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
                FROM dbo.Users
                WHERE UserID = @UserID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task<List<User>> GetAllAsync()
        {
            const string sql = @"
                SELECT UserID, UserName, FullName, Email, PhoneNumber, PasswordHash, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
                FROM dbo.Users
                ORDER BY UserID DESC";

            var list = new List<User>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapUser(reader));
            }
            return list;
        }

        public async Task<(List<User> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT UserID, UserName, FullName, Email, PhoneNumber, PasswordHash, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
                FROM dbo.Users";

            string countQuery = "SELECT COUNT(1) FROM dbo.Users";
            string filterCondition = options.filter != null ? GridQueryBuilder<User>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<User>.Query(options, baseQuery, "UserID DESC");

            var list = new List<User>();
            int totalCount = 0;

            using var conn = GetConnection();
            await conn.OpenAsync();

            // Total count
            using (var countCmd = CreateCommand(countQuery, conn))
            {
                var result = await countCmd.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(result ?? 0);
            }

            // Paged data
            using (var dataCmd = CreateCommand(pagedSql, conn))
            {
                using var reader = await dataCmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(MapUser(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<int> InsertAsync(User user)
        {
            const string sql = @"
                INSERT INTO dbo.Users (UserName, FullName, Email, PhoneNumber, PasswordHash, IsActive, CreatedAt, CreatedBy)
                VALUES (@UserName, @FullName, @Email, @PhoneNumber, @PasswordHash, @IsActive, @CreatedAt, @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = user.UserName.Trim();
            cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = user.FullName.Trim();
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = (object?)user.Email ?? DBNull.Value;
            cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 30).Value = (object?)user.PhoneNumber ?? DBNull.Value;
            cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = user.PasswordHash;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = user.IsActive;
            cmd.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = user.CreatedAt;
            cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 50).Value = (object?)user.CreatedBy ?? DBNull.Value;

            await conn.OpenAsync();
            var newId = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }

        public async Task<int> UpdateAsync(User user)
        {
            const string sql = @"
                UPDATE dbo.Users
                SET FullName = @FullName,
                    Email = @Email,
                    PhoneNumber = @PhoneNumber,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME(),
                    UpdatedBy = @UpdatedBy
                WHERE UserID = @UserID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = user.UserID;
            cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = user.FullName.Trim();
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = (object?)user.Email ?? DBNull.Value;
            cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 30).Value = (object?)user.PhoneNumber ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = user.IsActive;
            cmd.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar, 50).Value = (object?)user.UpdatedBy ?? DBNull.Value;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> UpdateStatusAsync(int userId, bool isActive, string updatedBy)
        {
            const string sql = @"
                UPDATE dbo.Users
                SET IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME(),
                    UpdatedBy = @UpdatedBy
                WHERE UserID = @UserID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
            cmd.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar, 50).Value = (object?)updatedBy ?? DBNull.Value;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> UpdatePasswordAsync(int userId, string passwordHash, string updatedBy)
        {
            const string sql = @"
                UPDATE dbo.Users
                SET PasswordHash = @PasswordHash,
                    UpdatedAt = SYSDATETIME(),
                    UpdatedBy = @UpdatedBy
                WHERE UserID = @UserID";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 500).Value = passwordHash;
            cmd.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar, 50).Value = (object?)updatedBy ?? DBNull.Value;

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> IsUserNameExistsAsync(string userName, int excludeUserId = 0)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.Users
                WHERE UserName = @UserName AND (@ExcludeUserId = 0 OR UserID != @ExcludeUserId)";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = userName.Trim();
            cmd.Parameters.Add("@ExcludeUserId", SqlDbType.Int).Value = excludeUserId;

            await conn.OpenAsync();
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetString(reader.GetOrdinal("UpdatedBy"))
            };
        }
    }
}
