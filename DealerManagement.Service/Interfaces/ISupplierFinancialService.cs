using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface ISupplierFinancialService
    {
        // Deposit
        Task<SupplierDeposit?> GetDepositByIdAsync(long id);
        Task<(List<SupplierDeposit> Items, int TotalCount)> GetDepositGridDataAsync(GridOptions options);
        Task<ResultModel<SupplierDeposit>> SaveDepositAsync(SupplierDeposit model);
        Task<ResultModel<bool>> DeleteDepositAsync(long id);

        // Commission
        Task<SupplierCommission?> GetCommissionByIdAsync(long id);
        Task<(List<SupplierCommission> Items, int TotalCount)> GetCommissionGridDataAsync(GridOptions options);
        Task<ResultModel<SupplierCommission>> SaveCommissionAsync(SupplierCommission model);
        Task<ResultModel<bool>> DeleteCommissionAsync(long id);

        // Opening Balance
        Task<SupplierOpeningBalance?> GetOpeningBalanceByIdAsync(long id);
        Task<(List<SupplierOpeningBalance> Items, int TotalCount)> GetOpeningBalanceGridDataAsync(GridOptions options);
        Task<ResultModel<SupplierOpeningBalance>> SaveOpeningBalanceAsync(SupplierOpeningBalance model);
        Task<ResultModel<bool>> DeleteOpeningBalanceAsync(long id);
    }
}

