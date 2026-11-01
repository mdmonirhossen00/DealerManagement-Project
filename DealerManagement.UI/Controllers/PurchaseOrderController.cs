using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.UI.Controllers
{
    [Authorize]
    public class PurchaseOrderController : Controller
    {
        private readonly IPurchaseOrderService _service;

        public PurchaseOrderController(IPurchaseOrderService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetGridData(GridOptions options)
        {
            var (items, total) = await _service.GetGridDataAsync(options);
            return Json(new { items, totalCount = total });
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new PurchaseOrderMaster
            {
                Operation = "add",
                Status = "Draft",
                OrderDate = DateTime.Today,
                PurchaseOrderNo = "PO-" + DateTime.Now.ToString("yyyyMMdd-HHmmss")
            };
            return View("CreateEdit", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var model = await _service.GetByIdWithDetailsAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            model.Operation = "update";
            return View("CreateEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEdit([FromBody] PurchaseOrderMaster model)
        {
            if (model == null)
            {
                return Json(new ResultModel<PurchaseOrderMaster>
                {
                    Status = Status.Fail,
                    Message = "Invalid data submitted."
                });
            }

            model.CreatedBy = User.Identity?.Name;

            ResultModel<PurchaseOrderMaster> result;
            if (model.Operation == "update")
            {
                result = await _service.UpdateAsync(model);
            }
            else
            {
                result = await _service.InsertAsync(model);
            }

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            return Json(result);
        }
    }
}

