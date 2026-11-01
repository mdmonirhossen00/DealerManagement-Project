using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;

namespace DealerManagement.Repository.Interfaces
{
    public interface IMenuRepository
    {
        Task<List<Menu>> GetActiveMenusAsync();
        Task<List<Menu>> GetAllAsync();
        Task<Menu?> GetByIdAsync(int id);
    }
}

