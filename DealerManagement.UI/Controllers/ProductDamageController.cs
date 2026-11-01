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
    public class ProductDamageController : Controller
    {
        private readonly IProductDamageService _service;

        public ProductDamageController(IProductDamageService service)
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
            var model = new ProductDamageMaster
            {
                Operation = "add",
                Status = "Draft",
                ReportDate = DateTime.Today,
                DamageReportNo = "DMG-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"),
                ReportType = "Damaged",
                ReportedBy = User.Identity?.Name ?? "Admin"
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
        public async Task<IActionResult> CreateEdit([FromBody] ProductDamageMaster model)
        {
            if (model == null)
            {
                return Json(new ResultModel<ProductDamageMaster>
                {
                    Status = Status.Fail,
                    Message = "Invalid data submitted."
                });
            }

            ResultModel<ProductDamageMaster> result;
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

