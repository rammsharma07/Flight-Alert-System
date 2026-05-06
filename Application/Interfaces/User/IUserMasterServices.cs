using Domain.Login;
using Domain.Response;
using Domain.User;

namespace Application.Interfaces.User
{
    public interface IUserMasterServices
    {
        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        Task<Response<UserMaster>> UserLogins(LoginDTO login);

        /// <summary>
        /// Check Valid Email
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        Task<Response<UserMaster>> CheckValidEmail(ForgotPasswordDTO forgotPassword);

        /// <summary>
        /// Get User Details
        /// </summary>
        /// <param name="userMaster"></param>
        /// <returns></returns>
        Task<Response<UserMaster>> GetUserDetails(UserMaster userMaster);

        /// <summary>
        /// Update User Password Details
        /// </summary>
        /// <param name="setPassword"></param>
        /// <returns></returns>
        Task<Response<UserMaster>> UpdateUserPasswordDetails(SetPasswordDTO setPassword);

        /// <summary>
        /// Get User Roles
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Response<UserRoleDetails>> GetAllUserRoles(int userId);


        /// <summary>
        /// Get All Roles
        /// </summary>
        /// <returns></returns>
        Task<Response<Role>> GetAllRoles();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        Task<Response<UserMaster>> GetAllUsers(int userId = 0);

        /// <summary>
        /// RegisterUser
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<Response<UserMaster>> RegisterUser(UserMaster user);

        /// <summary>
        /// UpdateUserOtp
        /// </summary>
        /// <param name="setOtpDTO"></param>
        /// <returns></returns>
        Task<Response<UserMaster>> UpdateUserOtp(SetOtpDTO setOtpDTO);


        /// <summary>
        /// GetAllUsersWithRoles
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Response<UserMasterDto>> GetAllUsersWithRoles(int userId = 0);

        /// <summary>
        /// UpdateRegisterUser
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Response<UserMaster>> UpdateRegisterUser(UserMaster user);

        /// <summary>
        /// UpdateUserEmailId
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<Response<UserMaster>> UpdateUserEmailId(UserMaster user);


        /// <summary>
        /// CheckEmailExists
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<Response<UserMaster>> CheckEmailExists(string email);

        /// <summary>
        /// Get All Archive Users with Roles
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Response<UserMasterDto>> GetAllArciveUsersWithRoles(int userId = 0);

    }
}
