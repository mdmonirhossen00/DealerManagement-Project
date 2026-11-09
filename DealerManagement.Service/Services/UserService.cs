using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Model.ViewModels;
using DealerManagement.Repository.Interfaces;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public UserService(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<ResultModel<UserVM>> GetByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ResultModel<UserVM>
                    {
                        Status = Status.Fail,
                        Message = MessageModel.DataLoadedFailed
                    };
                }

                return new ResultModel<UserVM>
                {
                    Status = Status.Success,
                    Message = MessageModel.DataLoaded,
                    Data = MapToViewModel(user)
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<UserVM>
                {
                    Status = Status.Fail,
                    Message = ex.Message,
                    Exception = ex
                };
            }
        }

        public async Task<ResultModel<List<UserVM>>> GetAllAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var vms = users.Select(MapToViewModel).ToList();

                return new ResultModel<List<UserVM>>
                {
                    Status = Status.Success,
                    Message = MessageModel.DataLoaded,
                    Data = vms
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<List<UserVM>>
                {
                    Status = Status.Fail,
                    Message = ex.Message,
                    Exception = ex
                };
            }
        }

        public async Task<GridEntity<UserVM>> GetGridDataAsync(GridOptions options)
        {
            try
            {
                var (items, totalCount) = await _userRepository.GetGridDataAsync(options);
                var vms = items.Select(MapToViewModel).ToList();

                return new GridResult<UserVM>().Data(vms, totalCount);
            }
            catch (Exception)
            {
                return new GridResult<UserVM>().Data(new List<UserVM>(), 0);
            }
        }

        public async Task<ResultModel<UserVM>> CreateAsync(UserVM model, string createdBy)
        {
            try
            {
                if (model == null)
                {
                    return new ResultModel<UserVM>
                    {
                        Status = Status.Warning,
                        Message = MessageModel.NotFoundForSave
                    };
                }

                // Check username exists
                bool exists = await _userRepository.IsUserNameExistsAsync(model.UserName);
                if (exists)
                {
                    return new ResultModel<UserVM>
                    {
                        Status = Status.Fail,
                        Message = $"Username '{model.UserName}' already exists. Please choose a different username."
                    };
                }

                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    return new ResultModel<UserVM>
                    {
                        Status = Status.Fail,
                        Message = "Password is required for new user."
                    };
                }

                string passwordHash = _authService.HashPassword(model.Password);

                var user = new User
                {
                    UserName = model.UserName.Trim(),
                    FullName = model.FullName.Trim(),
                    Email = model.Email?.Trim(),
                    PhoneNumber = model.PhoneNumber?.Trim(),
                    PasswordHash = passwordHash,
                    IsActive = model.IsActive,
                    RoleID = model.RoleID,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy
                };

                int newId = await _userRepository.InsertAsync(user);
                user.UserID = newId;

                return new ResultModel<UserVM>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = MapToViewModel(user)
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<UserVM>
                {
                    Status = Status.Fail,
                    Message = ex.Message,
                    Exception = ex
                };
            }
        }

        public async Task<ResultModel<UserVM>> UpdateAsync(UserVM model, string updatedBy)
        {
            try
            {
                if (model == null || model.UserID <= 0)
                {
                    return new ResultModel<UserVM>
                    {
                        Status = Status.Warning,
                        Message = MessageModel.NotFoundForUpdate
                    };
                }

                var existing = await _userRepository.GetByIdAsync(model.UserID);
                if (existing == null)
                {
                    return new ResultModel<UserVM>
                    {
                        Status = Status.Fail,
                        Message = "User not found."
                    };
                }

                // Prevent deactivating or removing Admin role from the last active Admin
                bool wasActiveAdmin = existing.IsActive && string.Equals(existing.RoleName, "Admin", StringComparison.OrdinalIgnoreCase);
                if (wasActiveAdmin && (!model.IsActive || model.RoleID != existing.RoleID))
                {
                    int activeAdminCount = await _userRepository.GetActiveAdminCountAsync();
                    if (activeAdminCount <= 1)
                    {
                        return new ResultModel<UserVM>
                        {
                            Status = Status.Fail,
                            Message = "Cannot deactivate or change the role of the last active Admin user."
                        };
                    }
                }

                existing.FullName = model.FullName.Trim();
                existing.Email = model.Email?.Trim();
                existing.PhoneNumber = model.PhoneNumber?.Trim();
                existing.IsActive = model.IsActive;
                existing.RoleID = model.RoleID;
                existing.UpdatedBy = updatedBy;

                await _userRepository.UpdateAsync(existing);

                return new ResultModel<UserVM>
                {
                    Status = Status.Success,
                    Message = MessageModel.UpdateSuccess,
                    Data = MapToViewModel(existing)
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<UserVM>
                {
                    Status = Status.Fail,
                    Message = ex.Message,
                    Exception = ex
                };
            }
        }

        public async Task<ResultModel<bool>> ToggleStatusAsync(int userId, string updatedBy)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ResultModel<bool>
                    {
                        Status = Status.Fail,
                        Message = "User not found.",
                        Data = false
                    };
                }

                // Prevent deactivating the last active Admin
                bool wasActiveAdmin = user.IsActive && string.Equals(user.RoleName, "Admin", StringComparison.OrdinalIgnoreCase);
                if (wasActiveAdmin)
                {
                    int activeAdminCount = await _userRepository.GetActiveAdminCountAsync();
                    if (activeAdminCount <= 1)
                    {
                        return new ResultModel<bool>
                        {
                            Status = Status.Fail,
                            Message = "Cannot deactivate the last active Admin user.",
                            Data = false
                        };
                    }
                }

                bool newStatus = !user.IsActive;
                await _userRepository.UpdateStatusAsync(userId, newStatus, updatedBy);

                return new ResultModel<bool>
                {
                    Status = Status.Success,
                    Message = newStatus ? "User activated successfully." : "User deactivated successfully.",
                    Data = newStatus
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<bool>
                {
                    Status = Status.Fail,
                    Message = ex.Message,
                    Exception = ex,
                    Data = false
                };
            }
        }

        public async Task<ResultModel<bool>> ChangePasswordAsync(ChangePasswordVM model, string updatedBy)
        {
            try
            {
                if (model == null || model.UserID <= 0)
                {
                    return new ResultModel<bool>
                    {
                        Status = Status.Fail,
                        Message = "Invalid user.",
                        Data = false
                    };
                }

                if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 6)
                {
                    return new ResultModel<bool>
                    {
                        Status = Status.Fail,
                        Message = "Password must be at least 6 characters.",
                        Data = false
                    };
                }

                var user = await _userRepository.GetByIdAsync(model.UserID);
                if (user == null)
                {
                    return new ResultModel<bool>
                    {
                        Status = Status.Fail,
                        Message = "User not found.",
                        Data = false
                    };
                }

                string newHash = _authService.HashPassword(model.NewPassword);
                await _userRepository.UpdatePasswordAsync(model.UserID, newHash, updatedBy);

                return new ResultModel<bool>
                {
                    Status = Status.Success,
                    Message = "Password changed successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<bool>
                {
                    Status = Status.Fail,
                    Message = ex.Message,
                    Exception = ex,
                    Data = false
                };
            }
        }

        public async Task<ResultModel<bool>> AssignRoleAsync(int userId, int roleId, string updatedBy)
        {
            try
            {
                bool success = await _userRepository.AssignRoleToUserAsync(userId, roleId, updatedBy);
                return new ResultModel<bool>
                {
                    Status = success ? Status.Success : Status.Fail,
                    Message = success ? "Role assigned to user successfully." : "Failed to assign role.",
                    Data = success
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<bool>
                {
                    Status = Status.Fail,
                    Message = ex.Message,
                    Data = false
                };
            }
        }

        private static UserVM MapToViewModel(User user)
        {
            return new UserVM
            {
                UserID = user.UserID,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                RoleID = user.RoleID,
                RoleName = user.RoleName,
                CreatedAt = user.CreatedAt
            };
        }
    }
}

