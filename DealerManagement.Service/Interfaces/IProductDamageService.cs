using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface IProductDamageService
    {
        Task<ProductDamageMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<ProductDamageMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<ProductDamageMaster>> InsertAsync(ProductDamageMaster master);
        Task<ResultModel<ProductDamageMaster>> UpdateAsync(ProductDamageMaster master);
        Task<ResultModel<bool>> DeleteAsync(long id);
    }
}

