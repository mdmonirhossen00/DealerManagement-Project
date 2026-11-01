using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Repository.Interfaces;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.Service.Services
{
    public class SupplierFinancialService : ISupplierFinancialService
    {
        private readonly ISupplierFinancialRepository _repo;

        public SupplierFinancialService(ISupplierFinancialRepository repo)
        {
            _repo = repo;
        }

        #region Deposit
        public async Task<SupplierDeposit?> GetDepositByIdAsync(long id)
        {
            return await _repo.GetDepositByIdAsync(id);
        }

        public async Task<(List<SupplierDeposit> Items, int TotalCount)> GetDepositGridDataAsync(GridOptions options)
        {
            return await _repo.GetDepositGridDataAsync(options);
        }

        public async Task<ResultModel<SupplierDeposit>> SaveDepositAsync(SupplierDeposit model)
        {
            var result = new ResultModel<SupplierDeposit>();

            if (string.IsNullOrWhiteSpace(model.DepositNo))
            {
                result.Success = false;
                result.Message = "Payment / Voucher Number is required.";
                return result;
            }

            if (model.SupplierID <= 0)
            {
                result.Success = false;
                result.Message = "Supplier is required.";
                return result;
            }

            if (model.Amount <= 0)
            {
                result.Success = false;
                result.Message = "Amount must be greater than zero.";
                return result;
            }

            try
            {
                if (model.Operation == "update" && model.SupplierDepositID > 0)
                {
                    bool updated = await _repo.UpdateDepositAsync(model);
                    result.Success = updated;
                    result.Message = updated ? "Supplier payment updated successfully." : "Payment record not found.";
                    result.Data = model;
                }
                else
                {
                    long id = await _repo.InsertDepositAsync(model);
                    model.SupplierDepositID = id;
                    result.Success = true;
                    result.Message = "Supplier payment recorded successfully.";
                    result.Data = model;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving payment: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<bool>> DeleteDepositAsync(long id)
        {
            var result = new ResultModel<bool>();
            try
            {
                bool deleted = await _repo.DeleteDepositAsync(id);
                result.Success = deleted;
                result.Message = deleted ? "Supplier payment deleted successfully." : "Payment record not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting payment: " + ex.Message;
            }
            return result;
        }
        #endregion

        #region Commission
        public async Task<SupplierCommission?> GetCommissionByIdAsync(long id)
        {
            return await _repo.GetCommissionByIdAsync(id);
        }

        public async Task<(List<SupplierCommission> Items, int TotalCount)> GetCommissionGridDataAsync(GridOptions options)
        {
            return await _repo.GetCommissionGridDataAsync(options);
        }

        public async Task<ResultModel<SupplierCommission>> SaveCommissionAsync(SupplierCommission model)
        {
            var result = new ResultModel<SupplierCommission>();

            if (model.SupplierID <= 0)
            {
                result.Success = false;
                result.Message = "Supplier is required.";
                return result;
            }

            if (model.CommissionAmount < 0)
            {
                result.Success = false;
                result.Message = "Amount must be non-negative.";
                return result;
            }

            try
            {
                if (model.Operation == "update" && model.SupplierCommissionID > 0)
                {
                    bool updated = await _repo.UpdateCommissionAsync(model);
                    result.Success = updated;
                    result.Message = updated ? "Rebate / Discount updated successfully." : "Rebate record not found.";
                    result.Data = model;
                }
                else
                {
                    long id = await _repo.InsertCommissionAsync(model);
                    model.SupplierCommissionID = id;
                    result.Success = true;
                    result.Message = "Rebate / Discount recorded successfully.";
                    result.Data = model;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving rebate: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<bool>> DeleteCommissionAsync(long id)
        {
            var result = new ResultModel<bool>();
            try
            {
                bool deleted = await _repo.DeleteCommissionAsync(id);
                result.Success = deleted;
                result.Message = deleted ? "Rebate / Discount deleted successfully." : "Rebate record not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting rebate: " + ex.Message;
            }
            return result;
        }
        #endregion

        #region Opening Balance
        public async Task<SupplierOpeningBalance?> GetOpeningBalanceByIdAsync(long id)
        {
            return await _repo.GetOpeningBalanceByIdAsync(id);
        }

        public async Task<(List<SupplierOpeningBalance> Items, int TotalCount)> GetOpeningBalanceGridDataAsync(GridOptions options)
        {
            return await _repo.GetOpeningBalanceGridDataAsync(options);
        }

        public async Task<ResultModel<SupplierOpeningBalance>> SaveOpeningBalanceAsync(SupplierOpeningBalance model)
        {
            var result = new ResultModel<SupplierOpeningBalance>();

            if (model.SupplierID <= 0)
            {
                result.Success = false;
                result.Message = "Supplier is required.";
                return result;
            }

            try
            {
                if (model.Operation == "update" && model.SupplierOpeningBalanceID > 0)
                {
                    bool updated = await _repo.UpdateOpeningBalanceAsync(model);
                    result.Success = updated;
                    result.Message = updated ? "Opening Balance updated successfully." : "Opening Balance not found.";
                    result.Data = model;
                }
                else
                {
                    long id = await _repo.InsertOpeningBalanceAsync(model);
                    model.SupplierOpeningBalanceID = id;
                    result.Success = true;
                    result.Message = "Opening Balance recorded successfully.";
                    result.Data = model;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving Opening Balance: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<bool>> DeleteOpeningBalanceAsync(long id)
        {
            var result = new ResultModel<bool>();
            try
            {
                bool deleted = await _repo.DeleteOpeningBalanceAsync(id);
                result.Success = deleted;
                result.Message = deleted ? "Opening Balance deleted successfully." : "Opening Balance not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting Opening Balance: " + ex.Message;
            }
            return result;
        }
        #endregion
    }
}

