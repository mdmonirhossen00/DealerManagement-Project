using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface IProductSetupService
    {
        Task<List<ProductSetup>> GetAllAsync();
        Task<ProductSetup?> GetByIdAsync(int id);
        Task<(List<ProductSetup> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<ProductSetup>> InsertAsync(ProductSetup model);
        Task<ResultModel<ProductSetup>> UpdateAsync(ProductSetup model);
        Task<ResultModel<bool>> DeleteAsync(int id);
    }
}

