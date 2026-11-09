using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DealerManagement.Model.Common;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Model.ViewModels;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public UserController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetUserGrid(GridOptions options)
        {
            try
            {
                var gridData = await _userService.GetGridDataAsync(options);
                return Json(gridData);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new UserVM { IsActive = true };
            var rolesResult = await _roleService.GetRolesDropdownAsync();
            ViewBag.Roles = rolesResult.Data ?? new();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserVM model)
        {
            if (!ModelState.IsValid)
            {
                var rolesResult = await _roleService.GetRolesDropdownAsync();
                ViewBag.Roles = rolesResult.Data ?? new();
                return View(model);
            }

            string currentUserName = User.Identity?.Name ?? "System";
            var result = await _userService.CreateAsync(model, currentUserName);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            var rolesRes = await _roleService.GetRolesDropdownAsync();
            ViewBag.Roles = rolesRes.Data ?? new();
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _userService.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            var rolesResult = await _roleService.GetRolesDropdownAsync();
            ViewBag.Roles = rolesResult.Data ?? new();

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserVM model)
        {
            if (!ModelState.IsValid)
            {
                var rolesResult = await _roleService.GetRolesDropdownAsync();
                ViewBag.Roles = rolesResult.Data ?? new();
                return View(model);
            }

            string currentUserName = User.Identity?.Name ?? "System";
            var result = await _userService.UpdateAsync(model, currentUserName);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            var rolesRes = await _roleService.GetRolesDropdownAsync();
            ViewBag.Roles = rolesRes.Data ?? new();
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ToggleStatus(int id)
        {
            string currentUserName = User.Identity?.Name ?? "System";
            var result = await _userService.ToggleStatusAsync(id, currentUserName);
            return Json(new { success = result.Success, message = result.Message, data = result.Data });
        }

        [HttpPost]
        public async Task<JsonResult> ChangePassword([FromBody] ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid input data." });
            }

            string currentUserName = User.Identity?.Name ?? "System";
            var result = await _userService.ChangePasswordAsync(model, currentUserName);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
