using Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.User
{
	public class UserRoleDto
    {
        public UserRoleDto()
        {
            SelectedRoleIds = new List<int>();
        }

        public int Id { get; set; }
        

        public List<Role> Roles { get; set; }

        [DisplayName("Email")]
        [Required(ErrorMessage = "Incorrect first name, please try again.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Incorrect last name, please try again.")]
        public string LastName { get; set; }


        [Required(ErrorMessage = "Incorrect password, please try again..")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*_,.]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters, include one uppercase letter, one digit, and one special character [!,@,#,$,%,&,*,_].")]
        public string Password { get; set; }


        [DisplayName("Email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter valid {0}.")]
        [Required(ErrorMessage = "Incorrect email, please try again.")]
        public string EmailID { get; set; }

        public bool EmailVerificationBypass { get; set; }

        [RegularExpression(@"^\+[1-9]{1}[0-9]{3,14}$", ErrorMessage = "Phone number must include country code and contain only numbers.")]
        [Required(ErrorMessage = "Incorrect mobile no, please try again.")]
        public string MobileNo { get; set; }
        public string Otp { get; set; }
        public bool IsEmailConfirmed { get; set; }

        [Required(ErrorMessage = "Confirm password field is required.")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage = "'Confirm Password' and 'Password' do not match")]
        [NotMapped]
        public string ConfirmPassword { get; set; }

        public int ?  CreatedBy { get; set; }


        public int ? ModifiedBy { get; set; }
        

        public List<int> SelectedRoleIds { get; set; }
    }
}
