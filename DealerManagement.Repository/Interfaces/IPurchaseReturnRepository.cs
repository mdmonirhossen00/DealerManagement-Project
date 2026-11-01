using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IPurchaseReturnRepository
    {
        Task<PurchaseReturnMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<PurchaseReturnMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<long> InsertAsync(PurchaseReturnMaster master);
        Task<bool> UpdateAsync(PurchaseReturnMaster master);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsByNoAsync(string returnNo, long excludeId = 0);
    }
}

