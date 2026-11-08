using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Model.ViewModels;

namespace DealerManagement.Service.Interfaces
{
    public interface IUserService
    {
        Task<ResultModel<UserVM>> GetByIdAsync(int userId);
        Task<ResultModel<List<UserVM>>> GetAllAsync();
        Task<GridEntity<UserVM>> GetGridDataAsync(GridOptions options);
        Task<ResultModel<UserVM>> CreateAsync(UserVM model, string createdBy);
        Task<ResultModel<UserVM>> UpdateAsync(UserVM model, string updatedBy);
        Task<ResultModel<bool>> ToggleStatusAsync(int userId, string updatedBy);
        Task<ResultModel<bool>> ChangePasswordAsync(ChangePasswordVM model, string updatedBy);
        Task<ResultModel<bool>> AssignRoleAsync(int userId, int roleId, string updatedBy);
    }
}

