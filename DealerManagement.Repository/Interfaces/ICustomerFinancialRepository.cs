using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface ICustomerFinancialRepository
    {
        // Deposit
        Task<CustomerDeposit?> GetDepositByIdAsync(long id);
        Task<(List<CustomerDeposit> Items, int TotalCount)> GetDepositGridDataAsync(GridOptions options);
        Task<long> InsertDepositAsync(CustomerDeposit model);
        Task<bool> UpdateDepositAsync(CustomerDeposit model);
        Task<bool> DeleteDepositAsync(long id);

        // Commission
        Task<CustomerCommission?> GetCommissionByIdAsync(long id);
        Task<(List<CustomerCommission> Items, int TotalCount)> GetCommissionGridDataAsync(GridOptions options);
        Task<long> InsertCommissionAsync(CustomerCommission model);
        Task<bool> UpdateCommissionAsync(CustomerCommission model);
        Task<bool> DeleteCommissionAsync(long id);

        // Opening Balance
        Task<CustomerOpeningBalance?> GetOpeningBalanceByIdAsync(long id);
        Task<(List<CustomerOpeningBalance> Items, int TotalCount)> GetOpeningBalanceGridDataAsync(GridOptions options);
        Task<long> InsertOpeningBalanceAsync(CustomerOpeningBalance model);
        Task<bool> UpdateOpeningBalanceAsync(CustomerOpeningBalance model);
        Task<bool> DeleteOpeningBalanceAsync(long id);
    }
}

