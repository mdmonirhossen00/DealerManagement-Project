using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface ICustomerOrderRepository
    {
        Task<CustomerOrderMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<CustomerOrderMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<long> InsertAsync(CustomerOrderMaster master);
        Task<bool> UpdateAsync(CustomerOrderMaster master);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsByNoAsync(string orderNo, long excludeId = 0);
    }
}

