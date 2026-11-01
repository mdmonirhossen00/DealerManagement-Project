using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface ISupplierRepository
    {
        Task<List<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(int id);
        Task<(List<Supplier> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<int> InsertAsync(Supplier model);
        Task<bool> UpdateAsync(Supplier model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByCodeAsync(string code, int excludeId = 0);
    }
}

