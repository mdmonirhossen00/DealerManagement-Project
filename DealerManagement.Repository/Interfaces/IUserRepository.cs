using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(string userName);
        Task<User?> GetByIdAsync(int userId);
        Task<List<User>> GetAllAsync();
        Task<(List<User> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<int> InsertAsync(User user);
        Task<int> UpdateAsync(User user);
        Task<int> UpdateStatusAsync(int userId, bool isActive, string updatedBy);
        Task<int> UpdatePasswordAsync(int userId, string passwordHash, string updatedBy);
        Task<bool> IsUserNameExistsAsync(string userName, int excludeUserId = 0);
        Task<bool> AssignRoleToUserAsync(int userId, int roleId, string? updatedBy);
        Task<int> GetActiveAdminCountAsync();
    }
}

