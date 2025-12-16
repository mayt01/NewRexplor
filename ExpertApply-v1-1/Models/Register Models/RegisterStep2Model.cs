using System.ComponentModel.DataAnnotations;

namespace Rexplor.Models
{
    public class RegisterStep2Model
    {
        [Required(ErrorMessage = "کد تأیید الزامی است")]
        [Display(Name = "کد تأیید")]
        public string VerificationCode { get; set; }
    }
}
