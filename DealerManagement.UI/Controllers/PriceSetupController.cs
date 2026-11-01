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
    public class PriceSetupController : Controller
    {
        private readonly IPriceSetupService _service;

        public PriceSetupController(IPriceSetupService service)
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
            var model = new PriceSetup
            {
                Operation = "add",
                IsActive = true,
                PriceType = "Dealer"
            };
            return View("CreateEdit", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var model = await _service.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            model.Operation = "update";
            return View("CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(PriceSetup model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new ResultModel<PriceSetup>
                {
                    Status = Status.Fail,
                    Message = "Please correct validation errors."
                });
            }

            ResultModel<PriceSetup> result;
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

