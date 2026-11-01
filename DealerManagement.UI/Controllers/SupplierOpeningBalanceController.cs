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
    public class SupplierOpeningBalanceController : Controller
    {
        private readonly ISupplierFinancialService _service;

        public SupplierOpeningBalanceController(ISupplierFinancialService service)
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
            var (items, total) = await _service.GetOpeningBalanceGridDataAsync(options);
            return Json(new { items, totalCount = total });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("CreateEdit", new SupplierOpeningBalance
            {
                Operation = "add",
                OpeningDate = DateTime.Today
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var model = await _service.GetOpeningBalanceByIdAsync(id);
            if (model == null) return NotFound();
            model.Operation = "update";
            return View("CreateEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEdit([FromBody] SupplierOpeningBalance model)
        {
            var res = await _service.SaveOpeningBalanceAsync(model);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var res = await _service.DeleteOpeningBalanceAsync(id);
            return Json(res);
        }
    }
}

