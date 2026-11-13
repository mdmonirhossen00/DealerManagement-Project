using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DealerManagement.Model.Entities;
using DealerManagement.Model.ViewModels;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.UI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IMenuService _menuService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(
            IMenuService menuService, 
            IUserService userService, 
            IWebHostEnvironment webHostEnvironment)
        {
            _menuService = menuService;
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
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

        [HttpPost]
        public async Task<JsonResult> UpdateMyProfilePicture(IFormFile? profilePictureFile)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
                {
                    return Json(new { success = false, message = "User not identified." });
                }

                if (profilePictureFile == null || profilePictureFile.Length == 0)
                {
                    return Json(new { success = false, message = "No image selected." });
                }

                if (profilePictureFile.Length > 5 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "File size cannot exceed 5 MB." });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(profilePictureFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    return Json(new { success = false, message = "Invalid image format. Allowed formats: JPG, JPEG, PNG, GIF, WEBP." });
                }

                var uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile-pictures");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                // Retrieve existing profile picture to delete old file
                var existingUser = await _userService.GetByIdAsync(userId);
                string? oldPicture = existingUser.Data?.ProfilePicture;

                var uniqueFileName = $"user_{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadDir, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePictureFile.CopyToAsync(stream);
                }

                var newRelativePath = $"/uploads/profile-pictures/{uniqueFileName}";
                string currentUserName = User.Identity?.Name ?? "User";
                var updateResult = await _userService.UpdateProfilePictureAsync(userId, newRelativePath, currentUserName);

                if (updateResult.Success)
                {
                    // Clean up old file
                    if (!string.IsNullOrWhiteSpace(oldPicture) && oldPicture.StartsWith("/uploads/profile-pictures/", StringComparison.OrdinalIgnoreCase))
                    {
                        var oldFullPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile-pictures", Path.GetFileName(oldPicture));
                        if (System.IO.File.Exists(oldFullPath))
                        {
                            System.IO.File.Delete(oldFullPath);
                        }
                    }

                    // Refresh claims in current session cookie
                    if (User.Identity is ClaimsIdentity identity)
                    {
                        var existingClaim = identity.FindFirst("ProfilePicture");
                        if (existingClaim != null) identity.RemoveClaim(existingClaim);
                        identity.AddClaim(new Claim("ProfilePicture", newRelativePath));

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(identity));
                    }

                    return Json(new { success = true, message = "Profile picture updated successfully.", pictureUrl = newRelativePath });
                }

                return Json(new { success = false, message = updateResult.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ChangeMyPassword([FromBody] ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid input data." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
            {
                return Json(new { success = false, message = "User not identified." });
            }

            model.UserID = userId;
            string currentUserName = User.Identity?.Name ?? "User";
            model.UserName = currentUserName;

            var result = await _userService.ChangePasswordAsync(model, currentUserName);
            return Json(new { success = result.Success, message = result.Message });
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
