using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IPriceSetupRepository
    {
        Task<List<PriceSetup>> GetAllAsync();
        Task<PriceSetup?> GetByIdAsync(long id);
        Task<(List<PriceSetup> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<long> InsertAsync(PriceSetup model);
        Task<bool> UpdateAsync(PriceSetup model);
        Task<bool> DeleteAsync(long id);
    }
}

