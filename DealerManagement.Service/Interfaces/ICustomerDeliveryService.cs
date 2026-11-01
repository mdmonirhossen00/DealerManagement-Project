using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface ICustomerDeliveryService
    {
        Task<CustomerDeliveryMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<CustomerDeliveryMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<CustomerDeliveryMaster>> InsertAsync(CustomerDeliveryMaster master);
        Task<ResultModel<CustomerDeliveryMaster>> UpdateAsync(CustomerDeliveryMaster master);
        Task<ResultModel<bool>> DeleteAsync(long id);
    }
}

