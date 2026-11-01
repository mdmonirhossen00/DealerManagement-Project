using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;

namespace DealerManagement.Service.Interfaces
{
    public interface IMenuService
    {
        Task<ResultModel<List<Menu>>> GetActiveMenusAsync();
        Task<ResultModel<List<Menu>>> GetAllAsync();
    }
}

