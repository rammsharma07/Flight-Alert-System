using Application.Interfaces.User;
using Domain.Login;
using Domain.Response;
using Domain.User;
using Persistance.CommonFunctions;
using Persistance.Interfaces.User;
using System.Net;

namespace Application.Services.User
{
    public class UserMasterServices(IUserMasterRepository userMasterRepository) : IUserMasterServices
    {
        private readonly IUserMasterRepository userMasterRepository = userMasterRepository;
        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public async Task<Response<UserMaster>> UserLogins(LoginDTO login)
        {
            var filters = new Dictionary<string, object> { { "EmailID", login.EmailID} };
            var objUserMaster = await userMasterRepository.GetAllAsync(filters).ConfigureAwait(false);
            if (objUserMaster.Data != null && objUserMaster.Data.Count > 0)
            {
                if (!objUserMaster.Data[0].IsActive || objUserMaster.Data[0].IsDeleted)
                {
                    return new Response<UserMaster>
                    {
                        Message = "User is not active.",
                        Status = HttpStatusCode.NotAcceptable
                    };
                }
                var passwordEntered = GeneralFunctions.GeneratePassword(0, login.Password);

                if (objUserMaster.Data[0].Password == passwordEntered)
                {
                    return new Response<UserMaster>
                    {
                        Data = objUserMaster.Data,
                        Message = "User Login Success.",
                        Status = HttpStatusCode.OK
                    };
                }
                else
                {
                    return new Response<UserMaster>
                    {
                        Message = "Invalid login attempt/Activation pending.",
                        Status = HttpStatusCode.NotAcceptable
                    };
                }
            }
            else
            {
                return new Response<UserMaster>
                {
                    Message = "User not found.",
                    Status = HttpStatusCode.NotFound
                };
            }
        }


        /// <summary>
        /// Register User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Response<UserMaster>> RegisterUser(UserMaster user)
        {  
            user.Password = GeneralFunctions.GeneratePassword(0, user.Password);
            user.CreateDate = DateTime.UtcNow;
            user.IsEmailConfirmed = false;
            return await this.userMasterRepository.Insert(user);
        }


        /// <summary>
        /// Update Register User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>

        public async Task<Response<UserMaster>> UpdateRegisterUser(UserMaster user)
        {
            var filters = new Dictionary<string, object> {
                                                            { "Id", user.Id },
            };

            var res = await userMasterRepository.GetAllAsync(filters).ConfigureAwait(false);
            if (res.Data.Count > 0)
            {

                var updateFields = new Dictionary<string, object> {
                                                            { "FirstName", user.FirstName },
                                                            { "LastName", user.LastName },
                                                            { "ModifiedBy", user.ModifiedBy  },
                                                            { "MobileNo", user.MobileNo },
                                                            { "EmailId", user.EmailID },
                                                            { "ModifiedDate", DateTime.UtcNow },
                                                         };
                return await userMasterRepository.PartialUpdate(updateFields, "Id", res.Data[0]).ConfigureAwait(false);
            }
            else
            {
                return new Response<UserMaster>
                {
                    Message = "User not found.",
                    Status = HttpStatusCode.NotFound
                };
            }
        }

        /// <summary>
        /// Update Register User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>

        public async Task<Response<UserMaster>> UpdateUserEmailId(UserMaster user)
        {
            var filters = new Dictionary<string, object> {
                                                            { "Id", user.Id },
            };

            var res = await userMasterRepository.GetAllAsync(filters).ConfigureAwait(false);
            if (res.Data.Count > 0)
            {

                var updateFields = new Dictionary<string, object> {
                                                            { "EmailID", user.EmailID },
                                                            { "ModifiedDate", DateTime.UtcNow },
                                                         };
                return await userMasterRepository.PartialUpdate(updateFields, "Id", res.Data[0]).ConfigureAwait(false);
            }
            else
            {
                return new Response<UserMaster>
                {
                    Message = "User not found.",
                    Status = HttpStatusCode.NotFound
                };
            }
        }

        /// <summary>
        /// Check  email  exists
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public async Task<Response<UserMaster>> CheckEmailExists(string  email)
        {
            var filters = new Dictionary<string, object> { { "EmailID", email } };
            var objUserMaster = await userMasterRepository.GetAllAsync(filters).ConfigureAwait(false);
            if (objUserMaster.Data != null && objUserMaster.Data.Count > 0)
            {
                return new Response<UserMaster>
                {
                    Message = "Email already exists.",
                    Status = HttpStatusCode.OK
                };
            }
            else
            {
                return new Response<UserMaster>
                {
                    Message = "Email not found.",
                    Status = HttpStatusCode.NotFound
                };
            }
        }

        /// <summary>
        /// Check Valid Email
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public async Task<Response<UserMaster>> CheckValidEmail(ForgotPasswordDTO forgotPassword)
        {
            var filters = new Dictionary<string, object> { { "EmailID", forgotPassword.EmailID } };
            var objUserMaster = await userMasterRepository.GetAllAsync(filters).ConfigureAwait(false);
            if (objUserMaster.Data != null && objUserMaster.Data.Count > 0)
            {
                if (!objUserMaster.Data[0].IsActive || objUserMaster.Data[0].IsDeleted)
                {
                    return new Response<UserMaster>
                    {
                        Message = "EmailID is not active.",
                        Status = HttpStatusCode.NotAcceptable
                    };
                }
                else
                {
                    return objUserMaster;
                }
            }
            else
            {
                return new Response<UserMaster>
                {
                    Message = "Sorry, we couldn't find an account with that email address. Please check the email address and try again.",
                    Status = HttpStatusCode.NotFound
                };
            }
        }

        /// <summary>
        /// Get User Details
        /// </summary>
        /// <param name="userMaster"></param>
        /// <returns></returns>
        public async Task<Response<UserMaster>> GetUserDetails(UserMaster userMaster)
        {
            var filters = new Dictionary<string, object> {
                                                            { "EmailID", userMaster.EmailID },
                                                            { "Id", userMaster.Id },
                                                            { "IsActive", userMaster.IsActive },
                                                            { "IsDeleted", userMaster.IsDeleted }
                                                         };
            return await userMasterRepository.GetAllAsync(filters).ConfigureAwait(false);

        }

        /// <summary>
        /// Get All Roles
        /// </summary>
        /// <returns></returns>
        public async Task<Response<Role>> GetAllRoles()
        {

            return await userMasterRepository.GetRoles().ConfigureAwait(false);
        }


        /// <summary>
        /// Get All User Roles
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Response<UserRoleDetails>> GetAllUserRoles(int userId)
        {

            return await userMasterRepository.GetUserRoles(userId).ConfigureAwait(false);
        }

        /// <summary>
        /// Update User Password Details
        /// </summary>
        /// <param name="setPassword"></param>
        /// <returns></returns>
        public async Task<Response<UserMaster>> UpdateUserPasswordDetails(SetPasswordDTO setPassword)
        {
            var filters = new Dictionary<string, object> {
                                                            { "EmailID", setPassword.EmailID },
                                                            { "Id", setPassword.UserID },
                                                            { "IsActive", true },
                                                            { "IsDeleted", false }
                                                         };
            var res = await userMasterRepository.GetAllAsync(filters).ConfigureAwait(false);
            if (res.Data.Count > 0)
            {
                var passwordEncrypt = GeneralFunctions.GeneratePassword(0, setPassword.Password);
                var updateFields = new Dictionary<string, object> {
                                                            { "Password", passwordEncrypt },
                                                            { "ModifiedBy", setPassword.UserID },
                                                            { "ModifiedDate", DateTime.UtcNow },
                                                         };
                return await userMasterRepository.PartialUpdate(updateFields, "Id", res.Data[0]).ConfigureAwait(false);
            }
            else
            {
                return new Response<UserMaster>
                {
                    Message = "User not found.",
                    Status = HttpStatusCode.NotFound
                };
            }
        }

        /// <summary>
        /// Update User OTP
        /// </summary>
        /// <param name="setPassword"></param>
        /// <returns></returns>
        public async Task<Response<UserMaster>> UpdateUserOtp(SetOtpDTO setOtpDTO)
        {
            var filters = new Dictionary<string, object> {
                                                            { "EmailID", setOtpDTO.EmailID },
                                                            { "Id", setOtpDTO.UserID },
                                                            { "IsActive", true },
                                                            
                                                         };
            var res = await userMasterRepository.GetAllAsync(filters).ConfigureAwait(false);
            if (res.Data.Count > 0)
            {
               
                var updateFields = new Dictionary<string, object> {
                                                            { "ModifiedBy", setOtpDTO.UserID },
                                                            { "ModifiedDate", DateTime.UtcNow },
                                                            { "IsDeleted", false },
                                                            { "IsEmailConfirmed", setOtpDTO.IsEmailConfirmed }
                                                         };
                return await userMasterRepository.PartialUpdate(updateFields, "Id", res.Data[0]).ConfigureAwait(false);
            }
            else
            {
                return new Response<UserMaster>
                {
                    Message = "User not found.",
                    Status = HttpStatusCode.NotFound
                };
            }
        }

        /// <summary>
        /// Get All Users
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Response<UserMaster>> GetAllUsers(int userId = 0)
        {
            return await userMasterRepository.GetAllUsers(userId);
        }

        /// <summary>
        /// Get All Users With Roles
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Response<UserMasterDto>> GetAllUsersWithRoles(int userId = 0)
        {
            return await userMasterRepository.GetAllUsersWithRoles();
        }

        /// <summary>
        /// Get All Users With Roles
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Response<UserMasterDto>> GetAllArciveUsersWithRoles(int userId = 0)
        {
            return await userMasterRepository.GetAllArchiveUsersWithRoles();
        }
    }
}
