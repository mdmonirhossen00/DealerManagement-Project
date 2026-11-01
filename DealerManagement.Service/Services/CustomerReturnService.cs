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
    public class CustomerReturnService : ICustomerReturnService
    {
        private readonly ICustomerReturnRepository _repo;

        public CustomerReturnService(ICustomerReturnRepository repo)
        {
            _repo = repo;
        }

        public async Task<CustomerReturnMaster?> GetByIdWithDetailsAsync(long id)
        {
            return await _repo.GetByIdWithDetailsAsync(id);
        }

        public async Task<(List<CustomerReturnMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            return await _repo.GetGridDataAsync(options);
        }

        public async Task<ResultModel<CustomerReturnMaster>> InsertAsync(CustomerReturnMaster master)
        {
            var result = new ResultModel<CustomerReturnMaster>();

            if (string.IsNullOrWhiteSpace(master.CustomerReturnNo))
            {
                result.Success = false;
                result.Message = "Sales Return Number is required.";
                return result;
            }

            if (master.CustomerID <= 0)
            {
                result.Success = false;
                result.Message = "Customer / Dealer is required.";
                return result;
            }

            if (master.DetailsList == null || master.DetailsList.Count == 0)
            {
                result.Success = false;
                result.Message = "Please add at least one product item to return.";
                return result;
            }

            foreach (var item in master.DetailsList)
            {
                if (item.ProductID <= 0)
                {
                    result.Success = false;
                    result.Message = "Valid product is required for each line.";
                    return result;
                }
                if (item.Quantity <= 0)
                {
                    result.Success = false;
                    result.Message = "Quantity must be greater than zero.";
                    return result;
                }
            }

            if (await _repo.ExistsByNoAsync(master.CustomerReturnNo))
            {
                result.Success = false;
                result.Message = $"Sales Return No '{master.CustomerReturnNo}' already exists.";
                return result;
            }

            try
            {
                long newId = await _repo.InsertAsync(master);
                master.CustomerReturnID = newId;
                result.Success = true;
                result.Message = "Sales Return created successfully.";
                result.Data = master;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving Sales Return: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<CustomerReturnMaster>> UpdateAsync(CustomerReturnMaster master)
        {
            var result = new ResultModel<CustomerReturnMaster>();

            if (master.CustomerReturnID <= 0)
            {
                result.Success = false;
                result.Message = "Invalid Sales Return ID.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(master.CustomerReturnNo))
            {
                result.Success = false;
                result.Message = "Sales Return Number is required.";
                return result;
            }

            if (master.CustomerID <= 0)
            {
                result.Success = false;
                result.Message = "Customer / Dealer is required.";
                return result;
            }

            if (master.DetailsList == null || master.DetailsList.Count == 0)
            {
                result.Success = false;
                result.Message = "Please add at least one product item to return.";
                return result;
            }

            foreach (var item in master.DetailsList)
            {
                if (item.ProductID <= 0)
                {
                    result.Success = false;
                    result.Message = "Valid product is required for each line.";
                    return result;
                }
                if (item.Quantity <= 0)
                {
                    result.Success = false;
                    result.Message = "Quantity must be greater than zero.";
                    return result;
                }
            }

            if (await _repo.ExistsByNoAsync(master.CustomerReturnNo, master.CustomerReturnID))
            {
                result.Success = false;
                result.Message = $"Sales Return No '{master.CustomerReturnNo}' already exists.";
                return result;
            }

            try
            {
                bool updated = await _repo.UpdateAsync(master);
                result.Success = updated;
                result.Message = updated ? "Sales Return updated successfully." : "Sales Return not found.";
                result.Data = master;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error updating Sales Return: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<bool>> DeleteAsync(long id)
        {
            var result = new ResultModel<bool>();
            try
            {
                bool deleted = await _repo.DeleteAsync(id);
                result.Success = deleted;
                result.Message = deleted ? "Sales Return deleted successfully." : "Sales Return not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting Sales Return: " + ex.Message;
                result.Data = false;
            }
            return result;
        }
    }
}

