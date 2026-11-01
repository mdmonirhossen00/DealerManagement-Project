using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IProductCategoryRepository
    {
        Task<List<ProductCategory>> GetAllAsync();
        Task<ProductCategory?> GetByIdAsync(int id);
        Task<(List<ProductCategory> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<int> InsertAsync(ProductCategory model);
        Task<bool> UpdateAsync(ProductCategory model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name, int excludeId = 0);
    }
}

