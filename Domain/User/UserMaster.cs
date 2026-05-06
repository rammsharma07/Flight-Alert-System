using Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.User
{
	public class UserMaster : BaseModel
	{
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string  EmailID { get; set; }
        public string MobileNo { get; set; }
        public string Otp { get; set; }
        public bool IsEmailConfirmed { get; set; }

      
      
    }
}
