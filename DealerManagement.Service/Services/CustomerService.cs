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
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;

        public CustomerService(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Customer>> GetAllAsync() => _repository.GetAllAsync();
        public Task<Customer?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
        public Task<(List<Customer> Items, int TotalCount)> GetGridDataAsync(GridOptions options) => _repository.GetGridDataAsync(options);

        public async Task<ResultModel<Customer>> InsertAsync(Customer model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.CustomerCode))
                {
                    return new ResultModel<Customer> { Status = Status.Fail, Message = "Customer code is required." };
                }

                if (string.IsNullOrWhiteSpace(model.CustomerName))
                {
                    return new ResultModel<Customer> { Status = Status.Fail, Message = "Customer name is required." };
                }

                if (await _repository.ExistsByCodeAsync(model.CustomerCode))
                {
                    return new ResultModel<Customer> { Status = Status.Fail, Message = "A customer with this code already exists." };
                }

                int newId = await _repository.InsertAsync(model);
                model.CustomerID = newId;

                return new ResultModel<Customer>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = model
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<Customer> { Status = Status.Fail, Message = ex.Message };
            }
        }

        public async Task<ResultModel<Customer>> UpdateAsync(Customer model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.CustomerCode))
                {
                    return new ResultModel<Customer> { Status = Status.Fail, Message = "Customer code is required." };
                }

                if (string.IsNullOrWhiteSpace(model.CustomerName))
                {
                    return new ResultModel<Customer> { Status = Status.Fail, Message = "Customer name is required." };
                }

                if (await _repository.ExistsByCodeAsync(model.CustomerCode, model.CustomerID))
                {
                    return new ResultModel<Customer> { Status = Status.Fail, Message = "A customer with this code already exists." };
                }

                bool success = await _repository.UpdateAsync(model);
                if (success)
                {
                    return new ResultModel<Customer>
                    {
                        Status = Status.Success,
                        Message = MessageModel.UpdateSuccess,
                        Data = model
                    };
                }

                return new ResultModel<Customer> { Status = Status.Fail, Message = MessageModel.UpdateFail };
            }
            catch (Exception ex)
            {
                return new ResultModel<Customer> { Status = Status.Fail, Message = ex.Message };
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
                    Message = "Cannot delete this customer because they have related order, deposit, or delivery records."
                };
            }
        }
    }
}

