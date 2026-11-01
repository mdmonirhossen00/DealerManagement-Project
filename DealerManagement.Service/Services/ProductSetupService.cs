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
    public class ProductSetupService : IProductSetupService
    {
        private readonly IProductSetupRepository _repository;

        public ProductSetupService(IProductSetupRepository repository)
        {
            _repository = repository;
        }

        public Task<List<ProductSetup>> GetAllAsync() => _repository.GetAllAsync();
        public Task<ProductSetup?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<(List<ProductSetup> Items, int TotalCount)> GetGridDataAsync(GridOptions options) => _repository.GetGridDataAsync(options);

        public async Task<ResultModel<ProductSetup>> InsertAsync(ProductSetup model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.ProductCode))
                {
                    return new ResultModel<ProductSetup> { Status = Status.Fail, Message = "Product code is required." };
                }

                if (await _repository.ExistsByCodeAsync(model.ProductCode))
                {
                    return new ResultModel<ProductSetup> { Status = Status.Fail, Message = "A product with this code already exists." };
                }

                if (!string.IsNullOrWhiteSpace(model.Barcode) && await _repository.ExistsByBarcodeAsync(model.Barcode))
                {
                    return new ResultModel<ProductSetup> { Status = Status.Fail, Message = "A product with this barcode already exists." };
                }

                int newId = await _repository.InsertAsync(model);
                model.ProductID = newId;

                return new ResultModel<ProductSetup>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = model
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<ProductSetup> { Status = Status.Fail, Message = ex.Message };
            }
        }

        public async Task<ResultModel<ProductSetup>> UpdateAsync(ProductSetup model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.ProductCode))
                {
                    return new ResultModel<ProductSetup> { Status = Status.Fail, Message = "Product code is required." };
                }

                if (await _repository.ExistsByCodeAsync(model.ProductCode, model.ProductID))
                {
                    return new ResultModel<ProductSetup> { Status = Status.Fail, Message = "A product with this code already exists." };
                }

                if (!string.IsNullOrWhiteSpace(model.Barcode) && await _repository.ExistsByBarcodeAsync(model.Barcode, model.ProductID))
                {
                    return new ResultModel<ProductSetup> { Status = Status.Fail, Message = "A product with this barcode already exists." };
                }

                bool success = await _repository.UpdateAsync(model);
                if (success)
                {
                    return new ResultModel<ProductSetup>
                    {
                        Status = Status.Success,
                        Message = MessageModel.UpdateSuccess,
                        Data = model
                    };
                }

                return new ResultModel<ProductSetup> { Status = Status.Fail, Message = MessageModel.UpdateFail };
            }
            catch (Exception ex)
            {
                return new ResultModel<ProductSetup> { Status = Status.Fail, Message = ex.Message };
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
            catch (Exception)
            {
                return new ResultModel<bool>
                {
                    Status = Status.Fail,
                    Message = "Cannot delete this product because it has associated transaction or pricing records."
                };
            }
        }
    }
}

