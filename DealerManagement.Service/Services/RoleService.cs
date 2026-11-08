using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;
using DealerManagement.Repository.Interfaces;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.Service.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<ResultModel<Role?>> GetByIdAsync(int id)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(id);
                if (role == null)
                {
                    return new ResultModel<Role?>
                    {
                        Status = Status.Fail,
                        Message = "Role not found."
                    };
                }

                return new ResultModel<Role?>
                {
                    Status = Status.Success,
                    Data = role,
                    Message = MessageModel.DataLoaded
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<Role?>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResultModel<List<Role>>> GetAllAsync()
        {
            try
            {
                var list = await _roleRepository.GetAllAsync();
                return new ResultModel<List<Role>>
                {
                    Status = Status.Success,
                    Data = list,
                    Message = MessageModel.DataLoaded
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<List<Role>>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }

        public async Task<(List<Role> Items, int TotalCount)> GetGridDataAsync(GridOptions options)
        {
            return await _roleRepository.GetGridDataAsync(options);
        }

        public async Task<ResultModel<Role>> InsertAsync(Role role, string createdBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(role.Name))
                {
                    return new ResultModel<Role>
                    {
                        Status = Status.Fail,
                        Message = "Role Name is required."
                    };
                }

                if (await _roleRepository.ExistsByNameAsync(role.Name))
                {
                    return new ResultModel<Role>
                    {
                        Status = Status.Fail,
                        Message = "A role with this name already exists."
                    };
                }

                role.CreatedBy = createdBy;
                int newId = await _roleRepository.InsertAsync(role);
                role.Id = newId;

                return new ResultModel<Role>
                {
                    Status = Status.Success,
                    Message = MessageModel.InsertSuccess,
                    Data = role
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<Role>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResultModel<Role>> UpdateAsync(Role role, string updatedBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(role.Name))
                {
                    return new ResultModel<Role>
                    {
                        Status = Status.Fail,
                        Message = "Role Name is required."
                    };
                }

                if (await _roleRepository.ExistsByNameAsync(role.Name, role.Id))
                {
                    return new ResultModel<Role>
                    {
                        Status = Status.Fail,
                        Message = "Another role with this name already exists."
                    };
                }

                role.UpdatedBy = updatedBy;
                bool updated = await _roleRepository.UpdateAsync(role);

                if (!updated)
                {
                    return new ResultModel<Role>
                    {
                        Status = Status.Fail,
                        Message = "Failed to update role."
                    };
                }

                return new ResultModel<Role>
                {
                    Status = Status.Success,
                    Message = MessageModel.UpdateSuccess,
                    Data = role
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<Role>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResultModel<bool>> DeleteAsync(int id)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(id);
                if (role == null)
                {
                    return new ResultModel<bool>
                    {
                        Status = Status.Fail,
                        Message = "Role not found."
                    };
                }

                if (string.Equals(role.Name, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return new ResultModel<bool>
                    {
                        Status = Status.Fail,
                        Message = "The Admin role cannot be deleted."
                    };
                }

                bool deleted = await _roleRepository.DeleteAsync(id);
                return new ResultModel<bool>
                {
                    Status = deleted ? Status.Success : Status.Fail,
                    Message = deleted ? MessageModel.DeleteSuccess : "Failed to delete role.",
                    Data = deleted
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

        public async Task<ResultModel<List<DropdownModel>>> GetRolesDropdownAsync()
        {
            try
            {
                var list = await _roleRepository.GetRolesDropdownAsync();
                return new ResultModel<List<DropdownModel>>
                {
                    Status = Status.Success,
                    Data = list,
                    Message = MessageModel.DataLoaded
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<List<DropdownModel>>
                {
                    Status = Status.Fail,
                    Message = ex.Message
                };
            }
        }
    }
}

