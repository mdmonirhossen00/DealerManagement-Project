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
    public class PurchaseReceiptService : IPurchaseReceiptService
    {
        private readonly IPurchaseReceiptRepository _repository;

        public PurchaseReceiptService(IPurchaseReceiptRepository repository)
        {
            _repository = repository;
        }

        public Task<PurchaseReceiptMaster?> GetByIdWithDetailsAsync(long id) => _repository.GetByIdWithDetailsAsync(id);
        public Task<(List<PurchaseReceiptMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options) => _repository.GetGridDataAsync(options);

        public async Task<ResultModel<PurchaseReceiptMaster>> InsertAsync(PurchaseReceiptMaster master)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(master.ReceiptNo))
                {
                    return new ResultModel<PurchaseReceiptMaster> { Status = Status.Fail, Message = "Receipt No is required." };
                }

                if (master.SupplierID <= 0)
                {
                    return new ResultModel<PurchaseReceiptMaster> { Status = Status.Fail, Message = "Supplier is required." };
                }

                if (master.DetailsList == null || master.DetailsList.Count == 0)
                {
                    return new ResultModel<PurchaseReceiptMaster> { Status = Status.Fail, Message = "At least one item line is required." };
                }

                if (await _repository.ExistsByNoAsync(master.ReceiptNo))
                {
                    return new ResultModel<PurchaseReceiptMaster> { Status = Status.Fail, Message = "A receipt with this number already exists." };
                }

                long newId = await _repository.InsertAsync(master);
                master.PurchaseReceiptID = newId;

                return new ResultModel<PurchaseReceiptMaster>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = master
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<PurchaseReceiptMaster> { Status = Status.Fail, Message = ex.Message };
            }
        }

        public async Task<ResultModel<PurchaseReceiptMaster>> UpdateAsync(PurchaseReceiptMaster master)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(master.ReceiptNo))
                {
                    return new ResultModel<PurchaseReceiptMaster> { Status = Status.Fail, Message = "Receipt No is required." };
                }

                if (master.SupplierID <= 0)
                {
                    return new ResultModel<PurchaseReceiptMaster> { Status = Status.Fail, Message = "Supplier is required." };
                }

                if (master.DetailsList == null || master.DetailsList.Count == 0)
                {
                    return new ResultModel<PurchaseReceiptMaster> { Status = Status.Fail, Message = "At least one item line is required." };
                }

                if (await _repository.ExistsByNoAsync(master.ReceiptNo, master.PurchaseReceiptID))
                {
                    return new ResultModel<PurchaseReceiptMaster> { Status = Status.Fail, Message = "A receipt with this number already exists." };
                }

                bool success = await _repository.UpdateAsync(master);
                if (success)
                {
                    return new ResultModel<PurchaseReceiptMaster>
                    {
                        Status = Status.Success,
                        Message = MessageModel.UpdateSuccess,
                        Data = master
                    };
                }

                return new ResultModel<PurchaseReceiptMaster> { Status = Status.Fail, Message = MessageModel.UpdateFail };
            }
            catch (Exception ex)
            {
                return new ResultModel<PurchaseReceiptMaster> { Status = Status.Fail, Message = ex.Message };
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
                    Message = "Cannot delete this Receipt record because it has related purchase return records."
                };
            }
        }
    }
}

