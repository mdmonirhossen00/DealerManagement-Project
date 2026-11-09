using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using DealerManagement.Model.Common;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.UI.Filters
{
    public class MenuAuthorizeFilter : IAsyncActionFilter
    {
        private static readonly HashSet<string> SystemControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Home", "Login", "Common"
        };

        private static readonly HashSet<string> AdminOnlyControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "User", "Role", "RoleMenu"
        };

        private readonly IRoleMenuService _roleMenuService;

        public MenuAuthorizeFilter(IRoleMenuService roleMenuService)
        {
            _roleMenuService = roleMenuService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor == null)
            {
                await next();
                return;
            }

            // 1. Check if action or controller allows anonymous access
            bool hasAllowAnonymous = actionDescriptor.EndpointMetadata != null &&
                actionDescriptor.EndpointMetadata.Any(em => em is AllowAnonymousAttribute);

            if (hasAllowAnonymous)
            {
                await next();
                return;
            }

            // 2. Check if user is authenticated
            var user = context.HttpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                await next();
                return;
            }

            string controllerName = actionDescriptor.ControllerName;
            string actionName = actionDescriptor.ActionName;

            // 3. System controllers are accessible to all authenticated users
            if (SystemControllers.Contains(controllerName))
            {
                await next();
                return;
            }

            bool isAdmin = user.IsInRole("Admin");

            // 4. Admin-only controllers (User, Role, RoleMenu): strictly enforce Admin role
            if (AdminOnlyControllers.Contains(controllerName))
            {
                if (isAdmin)
                {
                    await next();
                    return;
                }

                DenyAccess(context);
                return;
            }

            // 5. Admin role bypasses all business menu restrictions
            if (isAdmin)
            {
                await next();
                return;
            }

            // 6. Non-Admin users: retrieve logged-in user id
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            // 7. Check menu authorization explicitly granted by Admin
            bool isAuthorized = await _roleMenuService.IsUserAuthorizedAsync(userId, controllerName, actionName);
            if (!isAuthorized)
            {
                DenyAccess(context);
                return;
            }

            await next();
        }

        private static void DenyAccess(ActionExecutingContext context)
        {
            bool isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                          context.HttpContext.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
                          (context.HttpContext.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false);

            if (isAjax)
            {
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new JsonResult(new ResultModel<object>
                {
                    Status = Status.Fail,
                    Message = "Access Denied: You do not have permission to access this resource."
                });
            }
            else
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
            }
        }
    }
}
