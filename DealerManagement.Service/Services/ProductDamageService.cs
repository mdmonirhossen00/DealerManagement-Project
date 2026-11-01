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
    public class ProductDamageService : IProductDamageService
    {
        private readonly IProductDamageRepository _repo;

        public ProductDamageService(IProductDamageRepository repo)
        {
            _repo = repo;
        }

        public async Task<ProductDamageMaster?> GetByIdWithDetailsAsync(long id)
        {
            return await _repo.GetByIdWithDetailsAsync(id);
        }

        public async Task<(List<ProductDamageMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            return await _repo.GetGridDataAsync(options);
        }

        public async Task<ResultModel<ProductDamageMaster>> InsertAsync(ProductDamageMaster master)
        {
            var result = new ResultModel<ProductDamageMaster>();

            if (string.IsNullOrWhiteSpace(master.DamageReportNo))
            {
                result.Success = false;
                result.Message = "Report Number is required.";
                return result;
            }

            if (master.DetailsList == null || master.DetailsList.Count == 0)
            {
                result.Success = false;
                result.Message = "Please add at least one damaged/broken/missing item.";
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

            if (await _repo.ExistsByNoAsync(master.DamageReportNo))
            {
                result.Success = false;
                result.Message = $"Report No '{master.DamageReportNo}' already exists.";
                return result;
            }

            try
            {
                long newId = await _repo.InsertAsync(master);
                master.ProductDamageMasterID = newId;
                result.Success = true;
                result.Message = "Damage/Loss report created successfully.";
                result.Data = master;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving report: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<ProductDamageMaster>> UpdateAsync(ProductDamageMaster master)
        {
            var result = new ResultModel<ProductDamageMaster>();

            if (master.ProductDamageMasterID <= 0)
            {
                result.Success = false;
                result.Message = "Invalid Report ID.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(master.DamageReportNo))
            {
                result.Success = false;
                result.Message = "Report Number is required.";
                return result;
            }

            if (master.DetailsList == null || master.DetailsList.Count == 0)
            {
                result.Success = false;
                result.Message = "Please add at least one damaged/broken/missing item.";
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

            if (await _repo.ExistsByNoAsync(master.DamageReportNo, master.ProductDamageMasterID))
            {
                result.Success = false;
                result.Message = $"Report No '{master.DamageReportNo}' already exists.";
                return result;
            }

            try
            {
                bool updated = await _repo.UpdateAsync(master);
                result.Success = updated;
                result.Message = updated ? "Report updated successfully." : "Report not found.";
                result.Data = master;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error updating report: " + ex.Message;
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
                result.Message = deleted ? "Report deleted successfully." : "Report not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting report: " + ex.Message;
                result.Data = false;
            }
            return result;
        }
    }
}

