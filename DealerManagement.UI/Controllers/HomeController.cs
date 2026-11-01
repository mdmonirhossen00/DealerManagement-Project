using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.UI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IMenuService _menuService;

        public HomeController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> _leftSideBar()
        {
            var result = await _menuService.GetActiveMenusAsync();
            var menus = result.Data ?? new();
            ViewBag.ShouldRenderMenu = menus.Count > 0;
            return PartialView("_leftSideBar", menus);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
