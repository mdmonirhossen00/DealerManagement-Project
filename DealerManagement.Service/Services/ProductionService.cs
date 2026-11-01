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
    public class ProductionService : IProductionService
    {
        private readonly IProductionRepository _repo;

        public ProductionService(IProductionRepository repo)
        {
            _repo = repo;
        }

        public async Task<ProductionMaster?> GetByIdWithDetailsAsync(long id)
        {
            return await _repo.GetByIdWithDetailsAsync(id);
        }

        public async Task<(List<ProductionMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            return await _repo.GetGridDataAsync(options);
        }

        public async Task<ResultModel<ProductionMaster>> InsertAsync(ProductionMaster master)
        {
            var result = new ResultModel<ProductionMaster>();

            if (string.IsNullOrWhiteSpace(master.ProductionNo))
            {
                result.Success = false;
                result.Message = "Production Number is required.";
                return result;
            }

            if (master.DetailsList == null || master.DetailsList.Count == 0)
            {
                result.Success = false;
                result.Message = "Please add at least one produced product item.";
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

            if (await _repo.ExistsByNoAsync(master.ProductionNo))
            {
                result.Success = false;
                result.Message = $"Production No '{master.ProductionNo}' already exists.";
                return result;
            }

            try
            {
                long newId = await _repo.InsertAsync(master);
                master.ProductionID = newId;
                result.Success = true;
                result.Message = "Production record created successfully.";
                result.Data = master;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving production: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<ProductionMaster>> UpdateAsync(ProductionMaster master)
        {
            var result = new ResultModel<ProductionMaster>();

            if (master.ProductionID <= 0)
            {
                result.Success = false;
                result.Message = "Invalid Production ID.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(master.ProductionNo))
            {
                result.Success = false;
                result.Message = "Production Number is required.";
                return result;
            }

            if (master.DetailsList == null || master.DetailsList.Count == 0)
            {
                result.Success = false;
                result.Message = "Please add at least one produced product item.";
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

            if (await _repo.ExistsByNoAsync(master.ProductionNo, master.ProductionID))
            {
                result.Success = false;
                result.Message = $"Production No '{master.ProductionNo}' already exists.";
                return result;
            }

            try
            {
                bool updated = await _repo.UpdateAsync(master);
                result.Success = updated;
                result.Message = updated ? "Production record updated successfully." : "Production record not found.";
                result.Data = master;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error updating production: " + ex.Message;
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
                result.Message = deleted ? "Production record deleted successfully." : "Production record not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting production: " + ex.Message;
                result.Data = false;
            }
            return result;
        }
    }
}

