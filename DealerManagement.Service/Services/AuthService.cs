using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Repository.Interfaces;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<string> _passwordHasher;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<string>();
        }

        public async Task<ResultModel<User>> AuthenticateAsync(string userName, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                {
                    return new ResultModel<User>
                    {
                        Status = Status.Fail,
                        Message = MessageModel.InvalidCredentials
                    };
                }

                var user = await _userRepository.GetByUserNameAsync(userName);
                if (user == null)
                {
                    return new ResultModel<User>
                    {
                        Status = Status.Fail,
                        Message = MessageModel.InvalidCredentials
                    };
                }

                if (!user.IsActive)
                {
                    return new ResultModel<User>
                    {
                        Status = Status.Fail,
                        Message = MessageModel.InactiveUser
                    };
                }

                bool isPasswordValid = VerifyPassword(password, user.PasswordHash);
                if (!isPasswordValid)
                {
                    return new ResultModel<User>
                    {
                        Status = Status.Fail,
                        Message = MessageModel.InvalidCredentials
                    };
                }

                return new ResultModel<User>
                {
                    Status = Status.Success,
                    Message = "Login successful.",
                    Data = user
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<User>
                {
                    Status = Status.Fail,
                    Message = "An error occurred during authentication.",
                    Exception = ex
                };
            }
        }

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword("DealerManagement", password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                var result = _passwordHasher.VerifyHashedPassword("DealerManagement", passwordHash, password);
                return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
            }
            catch
            {
                return false;
            }
        }
    }
}

