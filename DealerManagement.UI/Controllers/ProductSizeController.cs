using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.UI.Controllers
{
    [Authorize]
    public class ProductSizeController : Controller
    {
        private readonly IProductAttributeService _service;

        public ProductSizeController(IProductAttributeService service)
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
            var (items, total) = await _service.GetSizeGridDataAsync(options);
            return Json(new { items, totalCount = total });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("CreateEdit", new ProductSize { Operation = "add", IsActive = true, SortOrder = 1 });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.GetSizeByIdAsync(id);
            if (model == null) return NotFound();
            model.Operation = "update";
            return View("CreateEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEdit([FromBody] ProductSize model)
        {
            var res = await _service.SaveSizeAsync(model);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _service.DeleteSizeAsync(id);
            return Json(res);
        }
    }
}

