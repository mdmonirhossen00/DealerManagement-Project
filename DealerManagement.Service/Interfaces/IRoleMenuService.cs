using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.ViewModels;

namespace DealerManagement.Service.Interfaces
{
    public interface IRoleMenuService
    {
        Task<ResultModel<List<RoleMenuItemVM>>> GetRoleMenuAccessDataAsync(int roleId);
        Task<ResultModel<List<Menu>>> GetMenusByRoleIdAsync(int roleId);
        Task<ResultModel<List<Menu>>> GetMenusByUserIdAsync(int userId);
        Task<ResultModel<bool>> SaveRoleMenusAsync(int roleId, List<int> menuIds, string createdBy);
        Task<bool> IsUserAuthorizedAsync(int userId, string controller, string action);
    }
}

