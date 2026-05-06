using Application.Interfaces.Login;
using Domain.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Services.Login
{
    public class LoginManager(IHttpContextAccessor contextAccessor) : ILoginManager
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
        public void SignIn(UserMaster Users, List<UserRoleDetails> userroles, bool isPersistent = true)
        {
            var claims = new List<Claim>
            {
                     new Claim("UserId", Users.Id.ToString()),
                     new Claim("FullName", Users.FirstName+" "+Users.LastName),
                     new Claim(ClaimTypes.Email, Users.EmailID)
            };
            if (userroles!=null)
            {
                foreach (var role in userroles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
                }
            }
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
         
            _contextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity)
                , new AuthenticationProperties() { IsPersistent = isPersistent })
                .Wait();
        }
        public void SignOut()
        {
            _contextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                .Wait();
        }
    }
}
