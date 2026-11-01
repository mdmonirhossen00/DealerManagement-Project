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
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _repository;

        public PurchaseOrderService(IPurchaseOrderRepository repository)
        {
            _repository = repository;
        }

        public Task<PurchaseOrderMaster?> GetByIdWithDetailsAsync(long id) => _repository.GetByIdWithDetailsAsync(id);
        public Task<(List<PurchaseOrderMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options) => _repository.GetGridDataAsync(options);

        public async Task<ResultModel<PurchaseOrderMaster>> InsertAsync(PurchaseOrderMaster master)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(master.PurchaseOrderNo))
                {
                    return new ResultModel<PurchaseOrderMaster> { Status = Status.Fail, Message = "Purchase Order No is required." };
                }

                if (master.SupplierID <= 0)
                {
                    return new ResultModel<PurchaseOrderMaster> { Status = Status.Fail, Message = "Supplier is required." };
                }

                if (master.DetailsList == null || master.DetailsList.Count == 0)
                {
                    return new ResultModel<PurchaseOrderMaster> { Status = Status.Fail, Message = "At least one item line is required in Details." };
                }

                if (await _repository.ExistsByNoAsync(master.PurchaseOrderNo))
                {
                    return new ResultModel<PurchaseOrderMaster> { Status = Status.Fail, Message = "A Purchase Order with this number already exists." };
                }

                long newId = await _repository.InsertAsync(master);
                master.PurchaseOrderID = newId;

                return new ResultModel<PurchaseOrderMaster>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = master
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<PurchaseOrderMaster> { Status = Status.Fail, Message = ex.Message };
            }
        }

        public async Task<ResultModel<PurchaseOrderMaster>> UpdateAsync(PurchaseOrderMaster master)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(master.PurchaseOrderNo))
                {
                    return new ResultModel<PurchaseOrderMaster> { Status = Status.Fail, Message = "Purchase Order No is required." };
                }

                if (master.SupplierID <= 0)
                {
                    return new ResultModel<PurchaseOrderMaster> { Status = Status.Fail, Message = "Supplier is required." };
                }

                if (master.DetailsList == null || master.DetailsList.Count == 0)
                {
                    return new ResultModel<PurchaseOrderMaster> { Status = Status.Fail, Message = "At least one item line is required in Details." };
                }

                if (await _repository.ExistsByNoAsync(master.PurchaseOrderNo, master.PurchaseOrderID))
                {
                    return new ResultModel<PurchaseOrderMaster> { Status = Status.Fail, Message = "A Purchase Order with this number already exists." };
                }

                bool success = await _repository.UpdateAsync(master);
                if (success)
                {
                    return new ResultModel<PurchaseOrderMaster>
                    {
                        Status = Status.Success,
                        Message = MessageModel.UpdateSuccess,
                        Data = master
                    };
                }

                return new ResultModel<PurchaseOrderMaster> { Status = Status.Fail, Message = MessageModel.UpdateFail };
            }
            catch (Exception ex)
            {
                return new ResultModel<PurchaseOrderMaster> { Status = Status.Fail, Message = ex.Message };
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
                return new ResultModel<bool>
                {
                    Status = Status.Fail,
                    Message = "Cannot delete this Purchase Order because it has downstream receipts or return records."
                };
            }
        }
    }
}

