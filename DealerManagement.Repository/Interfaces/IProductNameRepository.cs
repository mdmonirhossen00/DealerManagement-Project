using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IProductNameRepository
    {
        Task<List<ProductName>> GetAllAsync();
        Task<ProductName?> GetByIdAsync(int id);
        Task<(List<ProductName> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<int> InsertAsync(ProductName model);
        Task<bool> UpdateAsync(ProductName model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByCodeAsync(string code, int excludeId = 0);
    }
}

