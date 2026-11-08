using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.ViewModels;
using DealerManagement.Repository.Interfaces;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.Service.Services
{
    public class RoleMenuService : IRoleMenuService
    {
        private readonly IRoleMenuRepository _roleMenuRepository;
        private readonly IRoleRepository _roleRepository;

        public RoleMenuService(IRoleMenuRepository roleMenuRepository, IRoleRepository roleRepository)
        {
            _roleMenuRepository = roleMenuRepository;
            _roleRepository = roleRepository;
        }

        public async Task<ResultModel<List<RoleMenuItemVM>>> GetRoleMenuAccessDataAsync(int roleId)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    return new ResultModel<List<RoleMenuItemVM>>
                    {
                        Status = Status.Fail,
                        Message = "Role not found."
                    };
                }

                var list = await _roleMenuRepository.GetRoleMenuAccessDataAsync(roleId);
                return new ResultModel<List<RoleMenuItemVM>>
                {
                    Status = Status.Success,
                    Data = list,
                    Message = MessageModel.DataLoaded
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<List<RoleMenuItemVM>>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResultModel<List<Menu>>> GetMenusByRoleIdAsync(int roleId)
        {
            try
            {
                var list = await _roleMenuRepository.GetMenusByRoleIdAsync(roleId);
                return new ResultModel<List<Menu>>
                {
                    Status = Status.Success,
                    Data = list,
                    Message = MessageModel.DataLoaded
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<List<Menu>>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResultModel<List<Menu>>> GetMenusByUserIdAsync(int userId)
        {
            try
            {
                var list = await _roleMenuRepository.GetMenusByUserIdAsync(userId);
                return new ResultModel<List<Menu>>
                {
                    Status = Status.Success,
                    Data = list,
                    Message = MessageModel.DataLoaded
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<List<Menu>>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResultModel<bool>> SaveRoleMenusAsync(int roleId, List<int> menuIds, string createdBy)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    return new ResultModel<bool>
                    {
                        Status = Status.Fail,
                        Message = "Role not found."
                    };
                }

                bool saved = await _roleMenuRepository.SaveRoleMenusAsync(roleId, menuIds ?? new List<int>(), createdBy);
                return new ResultModel<bool>
                {
                    Status = saved ? Status.Success : Status.Fail,
                    Message = saved ? "Role menu permissions saved successfully." : "Failed to save permissions.",
                    Data = saved
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<bool>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }

        public async Task<bool> IsUserAuthorizedAsync(int userId, string controller, string action)
        {
            try
            {
                return await _roleMenuRepository.IsUserAuthorizedAsync(userId, controller, action);
            }
            catch
            {
                // On error, default to deny for non-system controllers
                return false;
            }
        }
    }
}

