using System.ComponentModel.DataAnnotations;

namespace Rexplor.Models
{
    public class RegisterStep1Model
    {
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر وارد کنید")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }
    }
}
