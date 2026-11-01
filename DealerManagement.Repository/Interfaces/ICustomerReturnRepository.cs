using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface ICustomerReturnRepository
    {
        Task<CustomerReturnMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<CustomerReturnMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<long> InsertAsync(CustomerReturnMaster master);
        Task<bool> UpdateAsync(CustomerReturnMaster master);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsByNoAsync(string returnNo, long excludeId = 0);
    }
}

