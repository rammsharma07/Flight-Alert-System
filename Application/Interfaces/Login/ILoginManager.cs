using Domain.User;

namespace Application.Interfaces.Login
{
	public interface ILoginManager
    {
        void SignIn(UserMaster Users, List<UserRoleDetails> userroles, bool isPersistent = true);
        void SignOut();
    }
}
