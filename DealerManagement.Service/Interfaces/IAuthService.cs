using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;

namespace DealerManagement.Service.Interfaces
{
    public interface IAuthService
    {
        Task<ResultModel<User>> AuthenticateAsync(string userName, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}

