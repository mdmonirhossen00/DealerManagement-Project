using System;
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
        private readonly IRoleMenuService _roleMenuService;

        public MenuAuthorizeFilter(IRoleMenuService roleMenuService)
        {
            _roleMenuService = roleMenuService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Check if action or controller allows anonymous access
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor == null)
            {
                await next();
                return;
            }

            bool hasAllowAnonymous = actionDescriptor.EndpointMetadata != null &&
                context.ActionDescriptor.EndpointMetadata.Any(em => em is AllowAnonymousAttribute);

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

            // 3. System controllers accessible to all authenticated users
            string controllerName = actionDescriptor.ControllerName;
            string actionName = actionDescriptor.ActionName;

            if (string.Equals(controllerName, "Home", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(controllerName, "Login", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(controllerName, "Common", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            // 4. Admin role bypasses all menu restrictions
            bool isAdmin = user.IsInRole("Admin") ||
                           string.Equals(user.Identity?.Name, "admin", StringComparison.OrdinalIgnoreCase) ||
                           user.Claims.Any(c => c.Type == ClaimTypes.Role && string.Equals(c.Value, "Admin", StringComparison.OrdinalIgnoreCase));

            if (isAdmin)
            {
                await next();
                return;
            }

            // 5. Retrieve logged-in user id
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            // 6. Check role-based menu authorization
            bool isAuthorized = await _roleMenuService.IsUserAuthorizedAsync(userId, controllerName, actionName);

            if (!isAuthorized)
            {
                bool isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                              (context.HttpContext.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase)) ||
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
                return;
            }

            await next();
        }
    }
}

