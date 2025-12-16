using System.ComponentModel.DataAnnotations;

namespace Rexplor.Models
{
    public class RegisterStep3Model
    {
        [Required(ErrorMessage = "نام کامل الزامی است")]
        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [Phone(ErrorMessage = "شماره موبایل معتبر وارد کنید")]
        [Display(Name = "شماره موبایل")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تکرار رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        [Display(Name = "تکرار رمز عبور")]
        [Compare("Password", ErrorMessage = "رمز عبور و تکرار آن یکسان نیست")]
        public string ConfirmPassword { get; set; }
    }
}
