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
    public class ProductAttributeService : IProductAttributeService
    {
        private readonly IProductAttributeRepository _repo;

        public ProductAttributeService(IProductAttributeRepository repo)
        {
            _repo = repo;
        }

        #region Color
        public async Task<ProductColor?> GetColorByIdAsync(int id)
        {
            return await _repo.GetColorByIdAsync(id);
        }

        public async Task<(List<ProductColor> Items, int TotalCount)> GetColorGridDataAsync(GridOptions options)
        {
            return await _repo.GetColorGridDataAsync(options);
        }

        public async Task<ResultModel<ProductColor>> SaveColorAsync(ProductColor model)
        {
            var result = new ResultModel<ProductColor>();

            if (string.IsNullOrWhiteSpace(model.ColorName))
            {
                result.Success = false;
                result.Message = "Color Name is required.";
                return result;
            }

            try
            {
                if (model.Operation == "update" && model.ProductColorID > 0)
                {
                    bool updated = await _repo.UpdateColorAsync(model);
                    result.Success = updated;
                    result.Message = updated ? "Color updated successfully." : "Color not found.";
                    result.Data = model;
                }
                else
                {
                    int id = await _repo.InsertColorAsync(model);
                    model.ProductColorID = id;
                    result.Success = true;
                    result.Message = "Color added successfully.";
                    result.Data = model;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving color: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<bool>> DeleteColorAsync(int id)
        {
            var result = new ResultModel<bool>();
            try
            {
                bool deleted = await _repo.DeleteColorAsync(id);
                result.Success = deleted;
                result.Message = deleted ? "Color deleted successfully." : "Color not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting color: " + ex.Message;
            }
            return result;
        }
        #endregion

        #region Size
        public async Task<ProductSize?> GetSizeByIdAsync(int id)
        {
            return await _repo.GetSizeByIdAsync(id);
        }

        public async Task<(List<ProductSize> Items, int TotalCount)> GetSizeGridDataAsync(GridOptions options)
        {
            return await _repo.GetSizeGridDataAsync(options);
        }

        public async Task<ResultModel<ProductSize>> SaveSizeAsync(ProductSize model)
        {
            var result = new ResultModel<ProductSize>();

            if (string.IsNullOrWhiteSpace(model.SizeName))
            {
                result.Success = false;
                result.Message = "Size Name is required.";
                return result;
            }

            try
            {
                if (model.Operation == "update" && model.ProductSizeID > 0)
                {
                    bool updated = await _repo.UpdateSizeAsync(model);
                    result.Success = updated;
                    result.Message = updated ? "Size updated successfully." : "Size not found.";
                    result.Data = model;
                }
                else
                {
                    int id = await _repo.InsertSizeAsync(model);
                    model.ProductSizeID = id;
                    result.Success = true;
                    result.Message = "Size added successfully.";
                    result.Data = model;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving size: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<bool>> DeleteSizeAsync(int id)
        {
            var result = new ResultModel<bool>();
            try
            {
                bool deleted = await _repo.DeleteSizeAsync(id);
                result.Success = deleted;
                result.Message = deleted ? "Size deleted successfully." : "Size not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting size: " + ex.Message;
            }
            return result;
        }
        #endregion

        #region Grade
        public async Task<ProductGrade?> GetGradeByIdAsync(int id)
        {
            return await _repo.GetGradeByIdAsync(id);
        }

        public async Task<(List<ProductGrade> Items, int TotalCount)> GetGradeGridDataAsync(GridOptions options)
        {
            return await _repo.GetGradeGridDataAsync(options);
        }

        public async Task<ResultModel<ProductGrade>> SaveGradeAsync(ProductGrade model)
        {
            var result = new ResultModel<ProductGrade>();

            if (string.IsNullOrWhiteSpace(model.GradeName))
            {
                result.Success = false;
                result.Message = "Grade Name is required.";
                return result;
            }

            try
            {
                if (model.Operation == "update" && model.ProductGradeID > 0)
                {
                    bool updated = await _repo.UpdateGradeAsync(model);
                    result.Success = updated;
                    result.Message = updated ? "Grade updated successfully." : "Grade not found.";
                    result.Data = model;
                }
                else
                {
                    int id = await _repo.InsertGradeAsync(model);
                    model.ProductGradeID = id;
                    result.Success = true;
                    result.Message = "Grade added successfully.";
                    result.Data = model;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving grade: " + ex.Message;
            }

            return result;
        }

        public async Task<ResultModel<bool>> DeleteGradeAsync(int id)
        {
            var result = new ResultModel<bool>();
            try
            {
                bool deleted = await _repo.DeleteGradeAsync(id);
                result.Success = deleted;
                result.Message = deleted ? "Grade deleted successfully." : "Grade not found.";
                result.Data = deleted;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting grade: " + ex.Message;
            }
            return result;
        }
        #endregion
    }
}

