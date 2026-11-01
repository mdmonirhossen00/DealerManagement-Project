using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<(List<Customer> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<Customer>> InsertAsync(Customer model);
        Task<ResultModel<Customer>> UpdateAsync(Customer model);
        Task<ResultModel<bool>> DeleteAsync(int id);
    }
}

