using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface ISupplierFinancialRepository
    {
        // Deposit
        Task<SupplierDeposit?> GetDepositByIdAsync(long id);
        Task<(List<SupplierDeposit> Items, int TotalCount)> GetDepositGridDataAsync(GridOptions options);
        Task<long> InsertDepositAsync(SupplierDeposit model);
        Task<bool> UpdateDepositAsync(SupplierDeposit model);
        Task<bool> DeleteDepositAsync(long id);

        // Commission
        Task<SupplierCommission?> GetCommissionByIdAsync(long id);
        Task<(List<SupplierCommission> Items, int TotalCount)> GetCommissionGridDataAsync(GridOptions options);
        Task<long> InsertCommissionAsync(SupplierCommission model);
        Task<bool> UpdateCommissionAsync(SupplierCommission model);
        Task<bool> DeleteCommissionAsync(long id);

        // Opening Balance
        Task<SupplierOpeningBalance?> GetOpeningBalanceByIdAsync(long id);
        Task<(List<SupplierOpeningBalance> Items, int TotalCount)> GetOpeningBalanceGridDataAsync(GridOptions options);
        Task<long> InsertOpeningBalanceAsync(SupplierOpeningBalance model);
        Task<bool> UpdateOpeningBalanceAsync(SupplierOpeningBalance model);
        Task<bool> DeleteOpeningBalanceAsync(long id);
    }
}

