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
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly IPurchaseReturnRepository _repo;

        public PurchaseReturnService(IPurchaseReturnRepository repo)
        {
            _repo = repo;
        }

        public async Task<PurchaseReturnMaster?> GetByIdWithDetailsAsync(long id)
        {
            return await _repo.GetByIdWithDetailsAsync(id);
        }

        public async Task<(List<PurchaseReturnMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            return await _repo.GetGridDataAsync(options);
        }

        public async Task<ResultModel<PurchaseReturnMaster>> InsertAsync(PurchaseReturnMaster master)
        {
            var result = new ResultModel<PurchaseReturnMaster>();

            if (string.IsNullOrWhiteSpace(master.PurchaseReturnNo))
            {
                result.Success = false;
                result.Message = "Purchase Return Number is required.";
                return result;
            }

            if (master.SupplierID <= 0)
            {
                result.Success = false;
                result.Message = "Supplier is required.";
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

            if (await _repo.ExistsByNoAsync(master.PurchaseReturnNo))
            {
                result.Success = false;
                result.Message = $"Purchase Return No '{master.PurchaseReturnNo}' already exists.";
                return result;
            }

            try
            {
                long newId = await _repo.InsertAsync(master);
                master.PurchaseReturnID = newId;
                result.Success = true;
                result.Message = "Purchase Return created successfully.";
                result.Data = master;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving Purchase Return: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<PurchaseReturnMaster>> UpdateAsync(PurchaseReturnMaster master)
        {
            var result = new ResultModel<PurchaseReturnMaster>();

            if (master.PurchaseReturnID <= 0)
            {
                result.Success = false;
                result.Message = "Invalid Purchase Return ID.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(master.PurchaseReturnNo))
            {
                result.Success = false;
                result.Message = "Purchase Return Number is required.";
                return result;
            }

            if (master.SupplierID <= 0)
            {
                result.Success = false;
                result.Message = "Supplier is required.";
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

            if (await _repo.ExistsByNoAsync(master.PurchaseReturnNo, master.PurchaseReturnID))
            {
                result.Success = false;
                result.Message = $"Purchase Return No '{master.PurchaseReturnNo}' already exists.";
                return result;
            }

            try
            {
                bool updated = await _repo.UpdateAsync(master);
                result.Success = updated;
                result.Message = updated ? "Purchase Return updated successfully." : "Purchase Return not found.";
                result.Data = master;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error updating Purchase Return: " + ex.Message;
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
                result.Message = deleted ? "Purchase Return deleted successfully." : "Purchase Return not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting Purchase Return: " + ex.Message;
                result.Data = false;
            }
            return result;
        }
    }
}

