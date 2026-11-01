using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface IPriceSetupService
    {
        Task<List<PriceSetup>> GetAllAsync();
        Task<PriceSetup?> GetByIdAsync(long id);
        Task<(List<PriceSetup> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<PriceSetup>> InsertAsync(PriceSetup model);
        Task<ResultModel<PriceSetup>> UpdateAsync(PriceSetup model);
        Task<ResultModel<bool>> DeleteAsync(long id);
    }
}

