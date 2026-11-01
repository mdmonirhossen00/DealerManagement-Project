using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface IPurchaseReceiptService
    {
        Task<PurchaseReceiptMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<PurchaseReceiptMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<PurchaseReceiptMaster>> InsertAsync(PurchaseReceiptMaster master);
        Task<ResultModel<PurchaseReceiptMaster>> UpdateAsync(PurchaseReceiptMaster master);
        Task<ResultModel<bool>> DeleteAsync(long id);
    }
}

