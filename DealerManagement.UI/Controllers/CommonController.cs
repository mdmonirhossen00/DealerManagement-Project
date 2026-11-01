using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.UI.Controllers
{
    [Authorize]
    public class CommonController : Controller
    {
        private readonly ICommonService _commonService;

        public CommonController(ICommonService commonService)
        {
            _commonService = commonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var data = await _commonService.GetCategoryDropdownAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductNames(int? categoryId = null)
        {
            var data = await _commonService.GetProductNameDropdownAsync(categoryId);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetSizes()
        {
            var data = await _commonService.GetSizeDropdownAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetColors()
        {
            var data = await _commonService.GetColorDropdownAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetGrades()
        {
            var data = await _commonService.GetGradeDropdownAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var data = await _commonService.GetProductDropdownAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var data = await _commonService.GetCustomerDropdownAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetSuppliers()
        {
            var data = await _commonService.GetSupplierDropdownAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrders(int? supplierId = null)
        {
            var data = await _commonService.GetPurchaseOrderDropdownAsync(supplierId);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchaseReceipts(int? supplierId = null)
        {
            var data = await _commonService.GetPurchaseReceiptDropdownAsync(supplierId);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerOrders(int? customerId = null)
        {
            var data = await _commonService.GetCustomerOrderDropdownAsync(customerId);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerDeliveries(int? customerId = null)
        {
            var data = await _commonService.GetCustomerDeliveryDropdownAsync(customerId);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductPrice(int productId, string priceType)
        {
            var price = await _commonService.GetProductPriceAsync(productId, priceType);
            return Json(new { price });
        }
    }
}

