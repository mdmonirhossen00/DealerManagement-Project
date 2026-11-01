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
    public class CustomerFinancialService : ICustomerFinancialService
    {
        private readonly ICustomerFinancialRepository _repo;

        public CustomerFinancialService(ICustomerFinancialRepository repo)
        {
            _repo = repo;
        }

        #region Deposit
        public async Task<CustomerDeposit?> GetDepositByIdAsync(long id)
        {
            return await _repo.GetDepositByIdAsync(id);
        }

        public async Task<(List<CustomerDeposit> Items, int TotalCount)> GetDepositGridDataAsync(GridOptions options)
        {
            return await _repo.GetDepositGridDataAsync(options);
        }

        public async Task<ResultModel<CustomerDeposit>> SaveDepositAsync(CustomerDeposit model)
        {
            var result = new ResultModel<CustomerDeposit>();

            if (string.IsNullOrWhiteSpace(model.DepositNo))
            {
                result.Success = false;
                result.Message = "Deposit Number is required.";
                return result;
            }

            if (model.CustomerID <= 0)
            {
                result.Success = false;
                result.Message = "Customer / Dealer is required.";
                return result;
            }

            if (model.Amount <= 0)
            {
                result.Success = false;
                result.Message = "Deposit amount must be greater than zero.";
                return result;
            }

            try
            {
                if (model.Operation == "update" && model.CustomerDepositID > 0)
                {
                    bool updated = await _repo.UpdateDepositAsync(model);
                    result.Success = updated;
                    result.Message = updated ? "Deposit updated successfully." : "Deposit not found.";
                    result.Data = model;
                }
                else
                {
                    long id = await _repo.InsertDepositAsync(model);
                    model.CustomerDepositID = id;
                    result.Success = true;
                    result.Message = "Deposit recorded successfully.";
                    result.Data = model;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving deposit: " + ex.Message;
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
                result.Message = deleted ? "Deposit deleted successfully." : "Deposit not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting deposit: " + ex.Message;
            }
            return result;
        }
        #endregion

        #region Commission
        public async Task<CustomerCommission?> GetCommissionByIdAsync(long id)
        {
            return await _repo.GetCommissionByIdAsync(id);
        }

        public async Task<(List<CustomerCommission> Items, int TotalCount)> GetCommissionGridDataAsync(GridOptions options)
        {
            return await _repo.GetCommissionGridDataAsync(options);
        }

        public async Task<ResultModel<CustomerCommission>> SaveCommissionAsync(CustomerCommission model)
        {
            var result = new ResultModel<CustomerCommission>();

            if (model.CustomerID <= 0)
            {
                result.Success = false;
                result.Message = "Customer / Dealer is required.";
                return result;
            }

            if (model.CommissionAmount < 0)
            {
                result.Success = false;
                result.Message = "Commission amount must be non-negative.";
                return result;
            }

            try
            {
                if (model.Operation == "update" && model.CustomerCommissionID > 0)
                {
                    bool updated = await _repo.UpdateCommissionAsync(model);
                    result.Success = updated;
                    result.Message = updated ? "Commission updated successfully." : "Commission not found.";
                    result.Data = model;
                }
                else
                {
                    long id = await _repo.InsertCommissionAsync(model);
                    model.CustomerCommissionID = id;
                    result.Success = true;
                    result.Message = "Commission recorded successfully.";
                    result.Data = model;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving commission: " + ex.Message;
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
                result.Message = deleted ? "Commission deleted successfully." : "Commission not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting commission: " + ex.Message;
            }
            return result;
        }
        #endregion

        #region Opening Balance
        public async Task<CustomerOpeningBalance?> GetOpeningBalanceByIdAsync(long id)
        {
            return await _repo.GetOpeningBalanceByIdAsync(id);
        }

        public async Task<(List<CustomerOpeningBalance> Items, int TotalCount)> GetOpeningBalanceGridDataAsync(GridOptions options)
        {
            return await _repo.GetOpeningBalanceGridDataAsync(options);
        }

        public async Task<ResultModel<CustomerOpeningBalance>> SaveOpeningBalanceAsync(CustomerOpeningBalance model)
        {
            var result = new ResultModel<CustomerOpeningBalance>();

            if (model.CustomerID <= 0)
            {
                result.Success = false;
                result.Message = "Customer / Dealer is required.";
                return result;
            }

            try
            {
                if (model.Operation == "update" && model.CustomerOpeningBalanceID > 0)
                {
                    bool updated = await _repo.UpdateOpeningBalanceAsync(model);
                    result.Success = updated;
                    result.Message = updated ? "Opening Balance updated successfully." : "Opening Balance not found.";
                    result.Data = model;
                }
                else
                {
                    long id = await _repo.InsertOpeningBalanceAsync(model);
                    model.CustomerOpeningBalanceID = id;
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

