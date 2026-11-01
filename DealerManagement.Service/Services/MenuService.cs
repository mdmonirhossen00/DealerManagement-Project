using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Repository.Interfaces;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.Service.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<ResultModel<List<Menu>>> GetActiveMenusAsync()
        {
            try
            {
                var menus = await _menuRepository.GetActiveMenusAsync();
                return new ResultModel<List<Menu>>
                {
                    Status = Status.Success,
                    Message = MessageModel.DataLoaded,
                    Data = menus
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<List<Menu>>
                {
                    Status = Status.Fail,
                    Message = ex.Message,
                    Exception = ex,
                    Data = new List<Menu>()
                };
            }
        }

        public async Task<ResultModel<List<Menu>>> GetAllAsync()
        {
            try
            {
                var menus = await _menuRepository.GetAllAsync();
                return new ResultModel<List<Menu>>
                {
                    Status = Status.Success,
                    Message = MessageModel.DataLoaded,
                    Data = menus
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<List<Menu>>
                {
                    Status = Status.Fail,
                    Message = ex.Message,
                    Exception = ex,
                    Data = new List<Menu>()
                };
            }
        }
    }
}

