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
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repository;

        public SupplierService(ISupplierRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Supplier>> GetAllAsync() => _repository.GetAllAsync();
        public Task<Supplier?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<(List<Supplier> Items, int TotalCount)> GetGridDataAsync(GridOptions options) => _repository.GetGridDataAsync(options);

        public async Task<ResultModel<Supplier>> InsertAsync(Supplier model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.SupplierCode))
                {
                    return new ResultModel<Supplier> { Status = Status.Fail, Message = "Supplier code is required." };
                }

                if (string.IsNullOrWhiteSpace(model.SupplierName))
                {
                    return new ResultModel<Supplier> { Status = Status.Fail, Message = "Supplier name is required." };
                }

                if (await _repository.ExistsByCodeAsync(model.SupplierCode))
                {
                    return new ResultModel<Supplier> { Status = Status.Fail, Message = "A supplier with this code already exists." };
                }

                int newId = await _repository.InsertAsync(model);
                model.SupplierID = newId;

                return new ResultModel<Supplier>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = model
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<Supplier> { Status = Status.Fail, Message = ex.Message };
            }
        }

        public async Task<ResultModel<Supplier>> UpdateAsync(Supplier model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.SupplierCode))
                {
                    return new ResultModel<Supplier> { Status = Status.Fail, Message = "Supplier code is required." };
                }

                if (string.IsNullOrWhiteSpace(model.SupplierName))
                {
                    return new ResultModel<Supplier> { Status = Status.Fail, Message = "Supplier name is required." };
                }

                if (await _repository.ExistsByCodeAsync(model.SupplierCode, model.SupplierID))
                {
                    return new ResultModel<Supplier> { Status = Status.Fail, Message = "A supplier with this code already exists." };
                }

                bool success = await _repository.UpdateAsync(model);
                if (success)
                {
                    return new ResultModel<Supplier>
                    {
                        Status = Status.Success,
                        Message = MessageModel.UpdateSuccess,
                        Data = model
                    };
                }

                return new ResultModel<Supplier> { Status = Status.Fail, Message = MessageModel.UpdateFail };
            }
            catch (Exception ex)
            {
                return new ResultModel<Supplier> { Status = Status.Fail, Message = ex.Message };
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
                    Message = "Cannot delete this supplier because they have related purchase or transaction records."
                };
            }
        }
    }
}

