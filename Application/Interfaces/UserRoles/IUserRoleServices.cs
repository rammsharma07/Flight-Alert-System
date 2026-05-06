using Domain.Login;
using Domain.Response;
using Domain.User;

namespace Application.Interfaces.User
{
    public interface IUserRoleServices
    {

        Task<Response<UserRole>> InsertUserRole(UserRole user);

        Task<int> DeleteUserRoleAsync(int userId);

        Task<List<UserRole>> GetAllAsync(int userId);
        Task<int> DeleteUserAsync(int userId);
        Task<int> ArchiveUserAsync(int userId);
        Task<int> ActivateUserAsync(int userId);
    }
}
