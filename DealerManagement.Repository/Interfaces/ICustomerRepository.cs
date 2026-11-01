using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<(List<Customer> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<int> InsertAsync(Customer model);
        Task<bool> UpdateAsync(Customer model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsByCodeAsync(string code, int excludeId = 0);
    }
}

