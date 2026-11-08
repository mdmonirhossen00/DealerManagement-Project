using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface IRoleService
    {
        Task<ResultModel<Role?>> GetByIdAsync(int id);
        Task<ResultModel<List<Role>>> GetAllAsync();
        Task<(List<Role> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<Role>> InsertAsync(Role role, string createdBy);
        Task<ResultModel<Role>> UpdateAsync(Role role, string updatedBy);
        Task<ResultModel<bool>> DeleteAsync(int id);
        Task<ResultModel<List<DropdownModel>>> GetRolesDropdownAsync();
    }
}

