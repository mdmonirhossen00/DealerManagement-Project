using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<PurchaseOrderMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<PurchaseOrderMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<PurchaseOrderMaster>> InsertAsync(PurchaseOrderMaster master);
        Task<ResultModel<PurchaseOrderMaster>> UpdateAsync(PurchaseOrderMaster master);
        Task<ResultModel<bool>> DeleteAsync(long id);
    }
}

