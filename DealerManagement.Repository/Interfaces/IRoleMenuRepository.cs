using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.ViewModels;

namespace DealerManagement.Repository.Interfaces
{
    public interface IRoleMenuRepository
    {
        Task<List<RoleMenuItemVM>> GetRoleMenuAccessDataAsync(int roleId);
        Task<List<Menu>> GetMenusByRoleIdAsync(int roleId);
        Task<List<Menu>> GetMenusByUserIdAsync(int userId);
        Task<bool> SaveRoleMenusAsync(int roleId, List<int> menuIds, string? createdBy);
        Task<bool> IsUserAuthorizedAsync(int userId, string controller, string action);
    }
}

