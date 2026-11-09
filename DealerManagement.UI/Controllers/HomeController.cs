using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DealerManagement.Model.Entities;
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var menus = new List<Menu>();

            if (int.TryParse(userIdClaim, out int userId))
            {
                var result = await _menuService.GetAuthorizedMenusForUserAsync(userId);
                menus = result.Data ?? new List<Menu>();
            }

            ViewBag.ShouldRenderMenu = menus.Count > 0;
            return PartialView("_leftSideBar", menus);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
