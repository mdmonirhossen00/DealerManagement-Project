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
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IProductCategoryRepository _repository;

        public ProductCategoryService(IProductCategoryRepository repository)
        {
            _repository = repository;
        }

        public Task<List<ProductCategory>> GetAllAsync() => _repository.GetAllAsync();
        public Task<ProductCategory?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<(List<ProductCategory> Items, int TotalCount)> GetGridDataAsync(GridOptions options) => _repository.GetGridDataAsync(options);

        public async Task<ResultModel<ProductCategory>> InsertAsync(ProductCategory model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.CategoryName))
                {
                    return new ResultModel<ProductCategory>
                    {
                        Status = Status.Fail,
                        Message = "Category name is required."
                    };
                }

                if (await _repository.ExistsByNameAsync(model.CategoryName))
                {
                    return new ResultModel<ProductCategory>
                    {
                        Status = Status.Fail,
                        Message = "A category with this name already exists."
                    };
                }

                int newId = await _repository.InsertAsync(model);
                model.ProductCategoryID = newId;

                return new ResultModel<ProductCategory>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = model
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<ProductCategory>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResultModel<ProductCategory>> UpdateAsync(ProductCategory model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.CategoryName))
                {
                    return new ResultModel<ProductCategory>
                    {
                        Status = Status.Fail,
                        Message = "Category name is required."
                    };
                }

                if (await _repository.ExistsByNameAsync(model.CategoryName, model.ProductCategoryID))
                {
                    return new ResultModel<ProductCategory>
                    {
                        Status = Status.Fail,
                        Message = "A category with this name already exists."
                    };
                }

                bool success = await _repository.UpdateAsync(model);
                if (success)
                {
                    return new ResultModel<ProductCategory>
                    {
                        Status = Status.Success,
                        Message = MessageModel.UpdateSuccess,
                        Data = model
                    };
                }

                return new ResultModel<ProductCategory>
                {
                    Status = Status.Fail,
                    Message = MessageModel.UpdateFail
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<ProductCategory>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResultModel<bool>> DeleteAsync(int id)
        {
            try
            {
                bool success = await _repository.DeleteAsync(id);
                return new ResultModel<bool>
                {
                    Status = success ? Status.Success : Status.Fail,
                    Message = success ? MessageModel.DeleteSuccess : MessageModel.DeleteFail,
                    Data = success
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<bool>
                {
                    Status = Status.Fail,
                    Message = "Cannot delete this category because it is referenced by other records."
                };
            }
        }
    }
}

