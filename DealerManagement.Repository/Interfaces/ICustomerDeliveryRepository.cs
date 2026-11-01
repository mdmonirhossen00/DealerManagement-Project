using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface ICustomerDeliveryRepository
    {
        Task<CustomerDeliveryMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<CustomerDeliveryMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<long> InsertAsync(CustomerDeliveryMaster master);
        Task<bool> UpdateAsync(CustomerDeliveryMaster master);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsByNoAsync(string deliveryNo, long excludeId = 0);
    }
}

