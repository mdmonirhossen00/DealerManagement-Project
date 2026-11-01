using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface ICustomerFinancialService
    {
        // Deposit
        Task<CustomerDeposit?> GetDepositByIdAsync(long id);
        Task<(List<CustomerDeposit> Items, int TotalCount)> GetDepositGridDataAsync(GridOptions options);
        Task<ResultModel<CustomerDeposit>> SaveDepositAsync(CustomerDeposit model);
        Task<ResultModel<bool>> DeleteDepositAsync(long id);

        // Commission
        Task<CustomerCommission?> GetCommissionByIdAsync(long id);
        Task<(List<CustomerCommission> Items, int TotalCount)> GetCommissionGridDataAsync(GridOptions options);
        Task<ResultModel<CustomerCommission>> SaveCommissionAsync(CustomerCommission model);
        Task<ResultModel<bool>> DeleteCommissionAsync(long id);

        // Opening Balance
        Task<CustomerOpeningBalance?> GetOpeningBalanceByIdAsync(long id);
        Task<(List<CustomerOpeningBalance> Items, int TotalCount)> GetOpeningBalanceGridDataAsync(GridOptions options);
        Task<ResultModel<CustomerOpeningBalance>> SaveOpeningBalanceAsync(CustomerOpeningBalance model);
        Task<ResultModel<bool>> DeleteOpeningBalanceAsync(long id);
    }
}

