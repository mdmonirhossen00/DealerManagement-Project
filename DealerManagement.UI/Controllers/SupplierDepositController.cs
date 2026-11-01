using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.UI.Controllers
{
    [Authorize]
    public class SupplierDepositController : Controller
    {
        private readonly ISupplierFinancialService _service;

        public SupplierDepositController(ISupplierFinancialService service)
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
            var (items, total) = await _service.GetDepositGridDataAsync(options);
            return Json(new { items, totalCount = total });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("CreateEdit", new SupplierDeposit
            {
                Operation = "add",
                Status = "Posted",
                DepositDate = DateTime.Today,
                DepositNo = "SPAY-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"),
                PaymentMethod = "Bank"
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var model = await _service.GetDepositByIdAsync(id);
            if (model == null) return NotFound();
            model.Operation = "update";
            return View("CreateEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEdit([FromBody] SupplierDeposit model)
        {
            var res = await _service.SaveDepositAsync(model);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var res = await _service.DeleteDepositAsync(id);
            return Json(res);
        }
    }
}

