using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface ICustomerOrderService
    {
        Task<CustomerOrderMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<CustomerOrderMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<CustomerOrderMaster>> InsertAsync(CustomerOrderMaster master);
        Task<ResultModel<CustomerOrderMaster>> UpdateAsync(CustomerOrderMaster master);
        Task<ResultModel<bool>> DeleteAsync(long id);
    }
}

