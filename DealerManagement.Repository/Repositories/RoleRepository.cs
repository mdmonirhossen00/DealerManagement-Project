using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Repository.Base;
using DealerManagement.Repository.Interfaces;

namespace DealerManagement.Repository.Repositories
{
    public class RoleRepository : BaseRepository, IRoleRepository
    {
        public RoleRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT 
                    r.Id, r.Name, r.Description, r.IsActive, r.CreatedAt, r.CreatedBy, r.UpdatedAt, r.UpdatedBy,
                    (SELECT COUNT(1) FROM dbo.RoleMenu rm WHERE rm.RoleId = r.Id) AS AssignedMenuCount
                FROM dbo.Role r
                WHERE r.Id = @Id";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapRole(reader);
            }
            return null;
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            const string sql = @"
                SELECT TOP 1
                    r.Id, r.Name, r.Description, r.IsActive, r.CreatedAt, r.CreatedBy, r.UpdatedAt, r.UpdatedBy,
                    (SELECT COUNT(1) FROM dbo.RoleMenu rm WHERE rm.RoleId = r.Id) AS AssignedMenuCount
                FROM dbo.Role r
                WHERE r.Name = @Name";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name.Trim();

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapRole(reader);
            }
            return null;
        }

        public async Task<List<Role>> GetAllAsync()
        {
            const string sql = @"
                SELECT 
                    r.Id, r.Name, r.Description, r.IsActive, r.CreatedAt, r.CreatedBy, r.UpdatedAt, r.UpdatedBy,
                    (SELECT COUNT(1) FROM dbo.RoleMenu rm WHERE rm.RoleId = r.Id) AS AssignedMenuCount
                FROM dbo.Role r
                ORDER BY r.Name ASC";

            var list = new List<Role>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapRole(reader));
            }
            return list;
        }

        public async Task<(List<Role> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            const string baseQuery = @"
                SELECT 
                    r.Id, r.Name, r.Description, r.IsActive, r.CreatedAt, r.CreatedBy, r.UpdatedAt, r.UpdatedBy,
                    (SELECT COUNT(1) FROM dbo.RoleMenu rm WHERE rm.RoleId = r.Id) AS AssignedMenuCount
                FROM dbo.Role r";

            string countQuery = "SELECT COUNT(1) FROM dbo.Role r";
            string filterCondition = options.filter != null ? GridQueryBuilder<Role>.FilterCondition(options.filter) : "";
            if (!string.IsNullOrEmpty(filterCondition))
            {
                countQuery += " WHERE " + filterCondition;
            }

            string pagedSql = GridQueryBuilder<Role>.Query(options, baseQuery, "Id ASC");

            var list = new List<Role>();
            int totalCount = 0;

            using var conn = GetConnection();
            await conn.OpenAsync();

            using (var countCmd = CreateCommand(countQuery, conn))
            {
                var result = await countCmd.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(result ?? 0);
            }

            using (var cmd = CreateCommand(pagedSql, conn))
            {
                cmd.Parameters.Add("@Skip", SqlDbType.Int).Value = options.skip;
                cmd.Parameters.Add("@Take", SqlDbType.Int).Value = options.take > 0 ? options.take : 10;

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(MapRole(reader));
                }
            }

            return (list, totalCount);
        }

        public async Task<int> InsertAsync(Role role)
        {
            const string sql = @"
                INSERT INTO dbo.Role (Name, Description, IsActive, CreatedAt, CreatedBy)
                VALUES (@Name, @Description, @IsActive, GETDATE(), @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);

            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = role.Name.Trim();
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 250).Value = (object?)role.Description ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = role.IsActive;
            cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 100).Value = (object?)role.CreatedBy ?? DBNull.Value;

            await conn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task<bool> UpdateAsync(Role role)
        {
            const string sql = @"
                UPDATE dbo.Role
                SET Name = @Name,
                    Description = @Description,
                    IsActive = @IsActive,
                    UpdatedAt = GETDATE(),
                    UpdatedBy = @UpdatedBy
                WHERE Id = @Id";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);

            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = role.Id;
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = role.Name.Trim();
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 250).Value = (object?)role.Description ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = role.IsActive;
            cmd.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar, 100).Value = (object?)role.UpdatedBy ?? DBNull.Value;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM dbo.Role WHERE Id = @Id";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> ExistsByNameAsync(string name, int excludeId = 0)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Role WHERE Name = @Name AND Id <> @ExcludeId";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name.Trim();
            cmd.Parameters.Add("@ExcludeId", SqlDbType.Int).Value = excludeId;

            await conn.OpenAsync();
            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync() ?? 0);
            return count > 0;
        }

        public async Task<List<DropdownModel>> GetRolesDropdownAsync()
        {
            const string sql = "SELECT Id, Name FROM dbo.Role WHERE IsActive = 1 ORDER BY Name ASC";

            var list = new List<DropdownModel>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new DropdownModel
                {
                    Name = reader.GetInt32(reader.GetOrdinal("Id")).ToString(),
                    Value = reader.GetString(reader.GetOrdinal("Name"))
                });
            }
            return list;
        }

        private static Role MapRole(SqlDataReader reader)
        {
            return new Role
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetString(reader.GetOrdinal("UpdatedBy")),
                AssignedMenuCount = reader.IsDBNull(reader.GetOrdinal("AssignedMenuCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("AssignedMenuCount"))
            };
        }
    }
}
