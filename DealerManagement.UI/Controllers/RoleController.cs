using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
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
        public IActionResult Create()
        {
            var model = new Role
            {
                Operation = "add",
                IsActive = true
            };
            return View("CreateEdit", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _roleService.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
            {
                return NotFound();
            }

            var model = result.Data;
            model.Operation = "update";
            return View("CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(Role model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new ResultModel<Role>
                {
                    Status = Status.Fail,
                    Message = "Please correct validation errors."
                });
            }

            string currentUserName = User.Identity?.Name ?? "System";
            ResultModel<Role> result;

            if (model.Operation == "update")
            {
                result = await _roleService.UpdateAsync(model, currentUserName);
            }
            else
            {
                result = await _roleService.InsertAsync(model, currentUserName);
            }

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roleService.DeleteAsync(id);
            return Json(result);
        }
    }
}

