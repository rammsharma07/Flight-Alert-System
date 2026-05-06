using System.ComponentModel.DataAnnotations;

namespace Domain.Login
{
    public class LoginDTO
    {
        public string Password { get; set; }
        public string EmailID { get; set; }
    }

    public class ForgotPasswordDTO
    {
        public string EmailID { get; set; }
    }

    public class SetPasswordDTO
    {
        public int UserID { get; set; }


        [Required(ErrorMessage = "Incorrect password, please try again..")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*_,.]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters, include one uppercase letter, one digit, and one special character [!,@@,#,$,%,&,*,_].")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password field is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "'Confirm Password' and 'Password' do not match")]
        public string ConfirmPassword { get; set; }
        public string? EmailID { get; set; }
        public bool SetFlag { get; set; }
    }

    public class SetOtpDTO
    {
        public int UserID { get; set; }
        public string? EmailID { get; set; }
        public string Otp { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}
