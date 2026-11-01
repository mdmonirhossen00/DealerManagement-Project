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
    public class PriceSetupService : IPriceSetupService
    {
        private readonly IPriceSetupRepository _repository;

        public PriceSetupService(IPriceSetupRepository repository)
        {
            _repository = repository;
        }

        public Task<List<PriceSetup>> GetAllAsync() => _repository.GetAllAsync();
        public Task<PriceSetup?> GetByIdAsync(long id) => _repository.GetByIdAsync(id);
        public Task<(List<PriceSetup> Items, int TotalCount)> GetGridDataAsync(GridOptions options) => _repository.GetGridDataAsync(options);

        public async Task<ResultModel<PriceSetup>> InsertAsync(PriceSetup model)
        {
            try
            {
                if (model.ProductID <= 0)
                {
                    return new ResultModel<PriceSetup> { Status = Status.Fail, Message = "Product is required." };
                }

                if (model.UnitPrice < 0)
                {
                    return new ResultModel<PriceSetup> { Status = Status.Fail, Message = "Unit price must be non-negative." };
                }

                long newId = await _repository.InsertAsync(model);
                model.PriceSetupID = newId;

                return new ResultModel<PriceSetup>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = model
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<PriceSetup> { Status = Status.Fail, Message = ex.Message };
            }
        }

        public async Task<ResultModel<PriceSetup>> UpdateAsync(PriceSetup model)
        {
            try
            {
                if (model.ProductID <= 0)
                {
                    return new ResultModel<PriceSetup> { Status = Status.Fail, Message = "Product is required." };
                }

                bool success = await _repository.UpdateAsync(model);
                if (success)
                {
                    return new ResultModel<PriceSetup>
                    {
                        Status = Status.Success,
                        Message = MessageModel.UpdateSuccess,
                        Data = model
                    };
                }

                return new ResultModel<PriceSetup> { Status = Status.Fail, Message = MessageModel.UpdateFail };
            }
            catch (Exception ex)
            {
                return new ResultModel<PriceSetup> { Status = Status.Fail, Message = ex.Message };
            }
        }

        public async Task<ResultModel<bool>> DeleteAsync(long id)
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
                return new ResultModel<bool> { Status = Status.Fail, Message = ex.Message };
            }
        }
    }
}

