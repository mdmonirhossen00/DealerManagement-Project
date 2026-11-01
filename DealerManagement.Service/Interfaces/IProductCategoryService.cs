using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface IProductCategoryService
    {
        Task<List<ProductCategory>> GetAllAsync();
        Task<ProductCategory?> GetByIdAsync(int id);
        Task<(List<ProductCategory> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<ProductCategory>> InsertAsync(ProductCategory model);
        Task<ResultModel<ProductCategory>> UpdateAsync(ProductCategory model);
        Task<ResultModel<bool>> DeleteAsync(int id);
    }
}

