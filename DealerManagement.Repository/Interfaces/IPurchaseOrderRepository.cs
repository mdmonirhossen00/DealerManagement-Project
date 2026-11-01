using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IPurchaseOrderRepository
    {
        Task<PurchaseOrderMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<PurchaseOrderMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<long> InsertAsync(PurchaseOrderMaster master);
        Task<bool> UpdateAsync(PurchaseOrderMaster master);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsByNoAsync(string orderNo, long excludeId = 0);
    }
}

