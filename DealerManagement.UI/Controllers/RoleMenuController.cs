using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Model.ViewModels;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleMenuController : Controller
    {
        private readonly IRoleMenuService _roleMenuService;
        private readonly IRoleService _roleService;

        public RoleMenuController(IRoleMenuService roleMenuService, IRoleService roleService)
        {
            _roleMenuService = roleMenuService;
            _roleService = roleService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetGridData(GridOptions options)
        {
            var (items, total) = await _roleService.GetGridDataAsync(options);
            return Json(new { items, totalCount = total });
        }

        [HttpGet]
        public async Task<IActionResult> RoleMenuEdit(int roleId)
        {
            var roleResult = await _roleService.GetByIdAsync(roleId);
            if (!roleResult.Success || roleResult.Data == null)
            {
                return RedirectToAction("Index");
            }

            var menuDataResult = await _roleMenuService.GetRoleMenuAccessDataAsync(roleId);

            var vm = new RoleMenuVM
            {
                RoleId = roleId,
                RoleName = roleResult.Data.Name,
                Description = roleResult.Data.Description,
                RoleMenuList = menuDataResult.Data ?? new()
            };

            return View("RoleMenuCreateEdit", vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetRoleMenuData(int roleId)
        {
            var result = await _roleMenuService.GetRoleMenuAccessDataAsync(roleId);
            return Json(result.Data ?? new());
        }

        [HttpPost]
        public async Task<IActionResult> SaveRoleMenu([FromBody] RoleMenuSaveModel model)
        {
            if (model == null || model.RoleId <= 0)
            {
                return Json(new ResultModel<bool>
                {
                    Status = Status.Fail,
                    Message = "Invalid Role or submission data."
                });
            }

            string currentUserName = User.Identity?.Name ?? "System";
            var result = await _roleMenuService.SaveRoleMenusAsync(model.RoleId, model.SelectedMenuIds, currentUserName);
            return Json(result);
        }
    }
}

