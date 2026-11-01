using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IProductSetupRepository
    {
        Task<List<ProductSetup>> GetAllAsync();
        Task<ProductSetup?> GetByIdAsync(int id);
        Task<(List<ProductSetup> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<int> InsertAsync(ProductSetup model);
        Task<bool> UpdateAsync(ProductSetup model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByCodeAsync(string code, int excludeId = 0);
        Task<bool> ExistsByBarcodeAsync(string barcode, int excludeId = 0);
    }
}

