using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(
            IUserService userService, 
            IRoleService roleService, 
            IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            _roleService = roleService;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Create(UserVM model, IFormFile? profilePictureFile)
        {
            if (!ModelState.IsValid)
            {
                var rolesResult = await _roleService.GetRolesDropdownAsync();
                ViewBag.Roles = rolesResult.Data ?? new();
                return View(model);
            }

            try
            {
                if (profilePictureFile != null && profilePictureFile.Length > 0)
                {
                    model.ProfilePicture = await SaveProfilePictureAsync(profilePictureFile);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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

            // Cleanup picture if save failed
            if (!string.IsNullOrEmpty(model.ProfilePicture))
            {
                DeleteProfilePictureFile(model.ProfilePicture);
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
        public async Task<IActionResult> Edit(UserVM model, IFormFile? profilePictureFile)
        {
            if (!ModelState.IsValid)
            {
                var rolesResult = await _roleService.GetRolesDropdownAsync();
                ViewBag.Roles = rolesResult.Data ?? new();
                return View(model);
            }

            try
            {
                var existingUser = await _userService.GetByIdAsync(model.UserID);
                string? oldPicture = existingUser.Data?.ProfilePicture;

                if (model.RemoveProfilePicture)
                {
                    DeleteProfilePictureFile(oldPicture);
                    model.ProfilePicture = null;
                }
                else if (profilePictureFile != null && profilePictureFile.Length > 0)
                {
                    var newPicture = await SaveProfilePictureAsync(profilePictureFile);
                    DeleteProfilePictureFile(oldPicture);
                    model.ProfilePicture = newPicture;
                }
                else
                {
                    model.ProfilePicture = oldPicture;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
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

        private async Task<string?> SaveProfilePictureAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            if (file.Length > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException("Profile picture file size cannot exceed 5 MB.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                throw new InvalidOperationException("Invalid image format. Allowed formats: JPG, JPEG, PNG, GIF, WEBP.");
            }

            var uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile-pictures");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var uniqueFileName = $"user_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/profile-pictures/{uniqueFileName}";
        }

        private void DeleteProfilePictureFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;
            try
            {
                if (relativePath.StartsWith("/uploads/profile-pictures/", StringComparison.OrdinalIgnoreCase))
                {
                    var fileName = Path.GetFileName(relativePath);
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile-pictures", fileName);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
