using Domain.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.User
{
	public class CreateUserWithRoleDto 
	{
        public UserMaster UserMaster { get; set; }
       
     

        public List<int> SelectedRoleIds { get; set; }
    }
}
