using Application.Interfaces.User;
using Domain.Response;
using Domain.User;
using Persistance.Interfaces.User;
using Persistance.Interfaces.UserRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.UserRoles
{
    public class UserRoleServices(IUserRoleRepository userRoleRepository) : IUserRoleServices
    {
        private readonly IUserRoleRepository userRoleRepository = userRoleRepository;
        public async Task<Response<UserRole>> InsertUserRole(UserRole user)
        {
            return await userRoleRepository.Insert(user);
        }
        public async Task<int> DeleteUserRoleAsync(int userId)
        {
            return await userRoleRepository.DeleteUserRoleAsync(userId);
        }
        public async Task<List<UserRole>> GetAllAsync(int userId)
        {
            var filters = new Dictionary<string, object> {
                                                            { "UserId", userId },

            };
            var result = await userRoleRepository.GetAllAsync(filters).ConfigureAwait(false);
            return result.Data;
        }

        public async Task<int> DeleteUserAsync(int userId)
        {
            return await userRoleRepository.DeleteUserAsync(userId);
        }

        public async Task<int> ArchiveUserAsync(int userId)
        {
            return await userRoleRepository.ArchiveUserAsync(userId);
        }
        public async Task<int> ActivateUserAsync(int userId)
        {
            return await userRoleRepository.ActivateUserAsync(userId);
        }
    }
}
