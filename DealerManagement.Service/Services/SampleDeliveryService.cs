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
    public class SampleDeliveryService : ISampleDeliveryService
    {
        private readonly ISampleDeliveryRepository _repo;

        public SampleDeliveryService(ISampleDeliveryRepository repo)
        {
            _repo = repo;
        }

        public async Task<SampleDeliveryMaster?> GetByIdWithDetailsAsync(long id)
        {
            return await _repo.GetByIdWithDetailsAsync(id);
        }

        public async Task<(List<SampleDeliveryMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            return await _repo.GetGridDataAsync(options);
        }

        public async Task<ResultModel<SampleDeliveryMaster>> InsertAsync(SampleDeliveryMaster master)
        {
            var result = new ResultModel<SampleDeliveryMaster>();

            if (string.IsNullOrWhiteSpace(master.SampleDeliveryNo))
            {
                result.Success = false;
                result.Message = "Sample Delivery Number is required.";
                return result;
            }

            if (master.DetailsList == null || master.DetailsList.Count == 0)
            {
                result.Success = false;
                result.Message = "Please add at least one sample item.";
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

            if (await _repo.ExistsByNoAsync(master.SampleDeliveryNo))
            {
                result.Success = false;
                result.Message = $"Sample Delivery No '{master.SampleDeliveryNo}' already exists.";
                return result;
            }

            try
            {
                long newId = await _repo.InsertAsync(master);
                master.SampleDeliveryID = newId;
                result.Success = true;
                result.Message = "Sample Delivery created successfully.";
                result.Data = master;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving Sample Delivery: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<SampleDeliveryMaster>> UpdateAsync(SampleDeliveryMaster master)
        {
            var result = new ResultModel<SampleDeliveryMaster>();

            if (master.SampleDeliveryID <= 0)
            {
                result.Success = false;
                result.Message = "Invalid Sample Delivery ID.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(master.SampleDeliveryNo))
            {
                result.Success = false;
                result.Message = "Sample Delivery Number is required.";
                return result;
            }

            if (master.DetailsList == null || master.DetailsList.Count == 0)
            {
                result.Success = false;
                result.Message = "Please add at least one sample item.";
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

            if (await _repo.ExistsByNoAsync(master.SampleDeliveryNo, master.SampleDeliveryID))
            {
                result.Success = false;
                result.Message = $"Sample Delivery No '{master.SampleDeliveryNo}' already exists.";
                return result;
            }

            try
            {
                bool updated = await _repo.UpdateAsync(master);
                result.Success = updated;
                result.Message = updated ? "Sample Delivery updated successfully." : "Sample Delivery not found.";
                result.Data = master;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error updating Sample Delivery: " + ex.Message;
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
                result.Message = deleted ? "Sample Delivery deleted successfully." : "Sample Delivery not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting Sample Delivery: " + ex.Message;
                result.Data = false;
            }
            return result;
        }
    }
}

