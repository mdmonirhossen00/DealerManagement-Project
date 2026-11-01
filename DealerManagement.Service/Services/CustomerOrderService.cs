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
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly ICustomerOrderRepository _repository;

        public CustomerOrderService(ICustomerOrderRepository repository)
        {
            _repository = repository;
        }

        public Task<CustomerOrderMaster?> GetByIdWithDetailsAsync(long id) => _repository.GetByIdWithDetailsAsync(id);
        public Task<(List<CustomerOrderMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options) => _repository.GetGridDataAsync(options);

        public async Task<ResultModel<CustomerOrderMaster>> InsertAsync(CustomerOrderMaster master)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(master.CustomerOrderNo))
                {
                    return new ResultModel<CustomerOrderMaster> { Status = Status.Fail, Message = "Order No is required." };
                }

                if (master.CustomerID <= 0)
                {
                    return new ResultModel<CustomerOrderMaster> { Status = Status.Fail, Message = "Customer / Dealer is required." };
                }

                if (master.DetailsList == null || master.DetailsList.Count == 0)
                {
                    return new ResultModel<CustomerOrderMaster> { Status = Status.Fail, Message = "At least one item line is required in Details." };
                }

                if (await _repository.ExistsByNoAsync(master.CustomerOrderNo))
                {
                    return new ResultModel<CustomerOrderMaster> { Status = Status.Fail, Message = "An order with this number already exists." };
                }

                long newId = await _repository.InsertAsync(master);
                master.CustomerOrderID = newId;

                return new ResultModel<CustomerOrderMaster>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = master
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<CustomerOrderMaster> { Status = Status.Fail, Message = ex.Message };
            }
        }

        public async Task<ResultModel<CustomerOrderMaster>> UpdateAsync(CustomerOrderMaster master)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(master.CustomerOrderNo))
                {
                    return new ResultModel<CustomerOrderMaster> { Status = Status.Fail, Message = "Order No is required." };
                }

                if (master.CustomerID <= 0)
                {
                    return new ResultModel<CustomerOrderMaster> { Status = Status.Fail, Message = "Customer / Dealer is required." };
                }

                if (master.DetailsList == null || master.DetailsList.Count == 0)
                {
                    return new ResultModel<CustomerOrderMaster> { Status = Status.Fail, Message = "At least one item line is required in Details." };
                }

                if (await _repository.ExistsByNoAsync(master.CustomerOrderNo, master.CustomerOrderID))
                {
                    return new ResultModel<CustomerOrderMaster> { Status = Status.Fail, Message = "An order with this number already exists." };
                }

                bool success = await _repository.UpdateAsync(master);
                if (success)
                {
                    return new ResultModel<CustomerOrderMaster>
                    {
                        Status = Status.Success,
                        Message = MessageModel.UpdateSuccess,
                        Data = master
                    };
                }

                return new ResultModel<CustomerOrderMaster> { Status = Status.Fail, Message = MessageModel.UpdateFail };
            }
            catch (Exception ex)
            {
                return new ResultModel<CustomerOrderMaster> { Status = Status.Fail, Message = ex.Message };
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
            catch (Exception)
            {
                return new ResultModel<bool>
                {
                    Status = Status.Fail,
                    Message = "Cannot delete this Customer Order because it has downstream delivery records."
                };
            }
        }
    }
}

