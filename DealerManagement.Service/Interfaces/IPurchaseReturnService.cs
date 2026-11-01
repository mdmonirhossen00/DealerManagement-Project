using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface IPurchaseReturnService
    {
        Task<PurchaseReturnMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<PurchaseReturnMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<PurchaseReturnMaster>> InsertAsync(PurchaseReturnMaster master);
        Task<ResultModel<PurchaseReturnMaster>> UpdateAsync(PurchaseReturnMaster master);
        Task<ResultModel<bool>> DeleteAsync(long id);
    }
}

