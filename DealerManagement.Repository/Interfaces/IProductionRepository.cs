using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IProductionRepository
    {
        Task<ProductionMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<ProductionMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<long> InsertAsync(ProductionMaster master);
        Task<bool> UpdateAsync(ProductionMaster master);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsByNoAsync(string prodNo, long excludeId = 0);
    }
}

