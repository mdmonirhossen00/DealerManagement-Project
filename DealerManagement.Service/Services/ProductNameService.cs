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
    public class ProductNameService : IProductNameService
    {
        private readonly IProductNameRepository _repository;

        public ProductNameService(IProductNameRepository repository)
        {
            _repository = repository;
        }

        public Task<List<ProductName>> GetAllAsync() => _repository.GetAllAsync();
        public Task<ProductName?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<(List<ProductName> Items, int TotalCount)> GetGridDataAsync(GridOptions options) => _repository.GetGridDataAsync(options);

        public async Task<ResultModel<ProductName>> InsertAsync(ProductName model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.ProductCode))
                {
                    return new ResultModel<ProductName>
                    {
                        Status = Status.Fail,
                        Message = "Product code is required."
                    };
                }

                if (await _repository.ExistsByCodeAsync(model.ProductCode))
                {
                    return new ResultModel<ProductName>
                    {
                        Status = Status.Fail,
                        Message = "A product with this code already exists."
                    };
                }

                int newId = await _repository.InsertAsync(model);
                model.ProductNameID = newId;

                return new ResultModel<ProductName>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = model
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<ProductName>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResultModel<ProductName>> UpdateAsync(ProductName model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.ProductCode))
                {
                    return new ResultModel<ProductName>
                    {
                        Status = Status.Fail,
                        Message = "Product code is required."
                    };
                }

                if (await _repository.ExistsByCodeAsync(model.ProductCode, model.ProductNameID))
                {
                    return new ResultModel<ProductName>
                    {
                        Status = Status.Fail,
                        Message = "A product with this code already exists."
                    };
                }

                bool success = await _repository.UpdateAsync(model);
                if (success)
                {
                    return new ResultModel<ProductName>
                    {
                        Status = Status.Success,
                        Message = MessageModel.UpdateSuccess,
                        Data = model
                    };
                }

                return new ResultModel<ProductName>
                {
                    Status = Status.Fail,
                    Message = MessageModel.UpdateFail
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<ProductName>
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
                    Message = "Cannot delete this product name because it is referenced in product setups."
                };
            }
        }
    }
}

