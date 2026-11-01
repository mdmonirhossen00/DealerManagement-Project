using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface IProductNameService
    {
        Task<List<ProductName>> GetAllAsync();
        Task<ProductName?> GetByIdAsync(int id);
        Task<(List<ProductName> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<ProductName>> InsertAsync(ProductName model);
        Task<ResultModel<ProductName>> UpdateAsync(ProductName model);
        Task<ResultModel<bool>> DeleteAsync(int id);
    }
}

