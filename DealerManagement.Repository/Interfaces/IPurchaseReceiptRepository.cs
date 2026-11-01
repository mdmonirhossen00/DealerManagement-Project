using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IPurchaseReceiptRepository
    {
        Task<PurchaseReceiptMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<PurchaseReceiptMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<long> InsertAsync(PurchaseReceiptMaster master);
        Task<bool> UpdateAsync(PurchaseReceiptMaster master);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsByNoAsync(string receiptNo, long excludeId = 0);
    }
}

