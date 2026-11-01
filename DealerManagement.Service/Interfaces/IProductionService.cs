using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface IProductionService
    {
        Task<ProductionMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<ProductionMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<ProductionMaster>> InsertAsync(ProductionMaster master);
        Task<ResultModel<ProductionMaster>> UpdateAsync(ProductionMaster master);
        Task<ResultModel<bool>> DeleteAsync(long id);
    }
}

