using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IProductDamageRepository
    {
        Task<ProductDamageMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<ProductDamageMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<long> InsertAsync(ProductDamageMaster master);
        Task<bool> UpdateAsync(ProductDamageMaster master);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsByNoAsync(string reportNo, long excludeId = 0);
    }
}

