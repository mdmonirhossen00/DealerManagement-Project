using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.UI.Controllers
{
    [Authorize]
    public class ProductColorController : Controller
    {
        private readonly IProductAttributeService _service;

        public ProductColorController(IProductAttributeService service)
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
            try
            {
                var (items, total) = await _service.GetColorGridDataAsync(options);
                return Json(new { items, totalCount = total });
            }
            catch (System.Exception ex)
            {
                return Json(new { items = new System.Collections.Generic.List<ProductColor>(), totalCount = 0, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("CreateEdit", new ProductColor { Operation = "add", IsActive = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.GetColorByIdAsync(id);
            if (model == null) return NotFound();
            model.Operation = "update";
            return View("CreateEdit", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEdit([FromBody] ProductColor model)
        {
            var res = await _service.SaveColorAsync(model);
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _service.DeleteColorAsync(id);
            return Json(res);
        }
    }
}

