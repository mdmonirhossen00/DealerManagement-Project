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
    public class CustomerDeliveryService : ICustomerDeliveryService
    {
        private readonly ICustomerDeliveryRepository _repository;

        public CustomerDeliveryService(ICustomerDeliveryRepository repository)
        {
            _repository = repository;
        }

        public Task<CustomerDeliveryMaster?> GetByIdWithDetailsAsync(long id) => _repository.GetByIdWithDetailsAsync(id);
        public Task<(List<CustomerDeliveryMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options) => _repository.GetGridDataAsync(options);

        public async Task<ResultModel<CustomerDeliveryMaster>> InsertAsync(CustomerDeliveryMaster master)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(master.DeliveryNo))
                {
                    return new ResultModel<CustomerDeliveryMaster> { Status = Status.Fail, Message = "Delivery No is required." };
                }

                if (master.CustomerID <= 0)
                {
                    return new ResultModel<CustomerDeliveryMaster> { Status = Status.Fail, Message = "Customer / Dealer is required." };
                }

                if (master.DetailsList == null || master.DetailsList.Count == 0)
                {
                    return new ResultModel<CustomerDeliveryMaster> { Status = Status.Fail, Message = "At least one item line is required in Delivery Challan." };
                }

                if (await _repository.ExistsByNoAsync(master.DeliveryNo))
                {
                    return new ResultModel<CustomerDeliveryMaster> { Status = Status.Fail, Message = "A delivery with this number already exists." };
                }

                long newId = await _repository.InsertAsync(master);
                master.CustomerDeliveryID = newId;

                return new ResultModel<CustomerDeliveryMaster>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = master
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<CustomerDeliveryMaster> { Status = Status.Fail, Message = ex.Message };
            }
        }

        public async Task<ResultModel<CustomerDeliveryMaster>> UpdateAsync(CustomerDeliveryMaster master)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(master.DeliveryNo))
                {
                    return new ResultModel<CustomerDeliveryMaster> { Status = Status.Fail, Message = "Delivery No is required." };
                }

                if (master.CustomerID <= 0)
                {
                    return new ResultModel<CustomerDeliveryMaster> { Status = Status.Fail, Message = "Customer / Dealer is required." };
                }

                if (master.DetailsList == null || master.DetailsList.Count == 0)
                {
                    return new ResultModel<CustomerDeliveryMaster> { Status = Status.Fail, Message = "At least one item line is required in Delivery Challan." };
                }

                if (await _repository.ExistsByNoAsync(master.DeliveryNo, master.CustomerDeliveryID))
                {
                    return new ResultModel<CustomerDeliveryMaster> { Status = Status.Fail, Message = "A delivery with this number already exists." };
                }

                bool success = await _repository.UpdateAsync(master);
                if (success)
                {
                    return new ResultModel<CustomerDeliveryMaster>
                    {
                        Status = Status.Success,
                        Message = MessageModel.UpdateSuccess,
                        Data = master
                    };
                }

                return new ResultModel<CustomerDeliveryMaster> { Status = Status.Fail, Message = MessageModel.UpdateFail };
            }
            catch (Exception ex)
            {
                return new ResultModel<CustomerDeliveryMaster> { Status = Status.Fail, Message = ex.Message };
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
                    Message = "Cannot delete this Delivery record because it has related sales return records."
                };
            }
        }
    }
}

