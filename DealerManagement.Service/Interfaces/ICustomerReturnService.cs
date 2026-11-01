using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface ICustomerReturnService
    {
        Task<CustomerReturnMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<CustomerReturnMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<CustomerReturnMaster>> InsertAsync(CustomerReturnMaster master);
        Task<ResultModel<CustomerReturnMaster>> UpdateAsync(CustomerReturnMaster master);
        Task<ResultModel<bool>> DeleteAsync(long id);
    }
}

