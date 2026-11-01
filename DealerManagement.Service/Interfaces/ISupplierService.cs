using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface ISupplierService
    {
        Task<List<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(int id);
        Task<(List<Supplier> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<Supplier>> InsertAsync(Supplier model);
        Task<ResultModel<Supplier>> UpdateAsync(Supplier model);
        Task<ResultModel<bool>> DeleteAsync(int id);
    }
}

