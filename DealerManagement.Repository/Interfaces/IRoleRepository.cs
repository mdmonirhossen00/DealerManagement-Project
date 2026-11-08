using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(int id);
        Task<Role?> GetByNameAsync(string name);
        Task<List<Role>> GetAllAsync();
        Task<(List<Role> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<int> InsertAsync(Role role);
        Task<bool> UpdateAsync(Role role);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name, int excludeId = 0);
        Task<List<DropdownModel>> GetRolesDropdownAsync();
    }
}

