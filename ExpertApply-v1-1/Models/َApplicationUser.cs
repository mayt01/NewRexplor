using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Rexplor.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "نام")]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [StringLength(50)]
        public string? LastName { get; set; }

        [Display(Name = "نقش")]
        [StringLength(50)]
        public string? Role { get; set; }

        [Display(Name = "عکس پروفایل")]
        public string? ProfilePicture { get; set; }

        public int? Credit { get; set; }

        [Display(Name = "تاریخ ثبت نام")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<DataFile> UploadedFiles { get; set; } = new List<DataFile>();

        // Property محاسباتی برای نام کامل
        [Display(Name = "نام کامل")]
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}


//using Microsoft.AspNetCore.Identity;
//using System.ComponentModel.DataAnnotations;

//namespace Rexplor.Models
//{
//    public class ApplicationUser : IdentityUser
//    {
//        [Display(Name = "نام")]
//        [StringLength(50)]
//        public string? FirstName { get; set; }

//        [Display(Name = "نام خانوادگی")]
//        [StringLength(50)]
//        public string? LastName { get; set; }

//        [Display(Name = "نقش")]
//        [StringLength(50)]
//        public string? Role { get; set; }

//        [Display(Name = "عکس پروفایل")]
//        public string? ProfilePicture { get; set; }

//        [Display(Name = "تاریخ ثبت نام")]
//        public DateTime RegistrationDate { get; set; } = DateTime.Now;

//        [Display(Name = "آخرین بازدید")]
//        public DateTime? LastLogin { get; set; }

//        [Display(Name = "بیوگرافی")]
//        [StringLength(500, ErrorMessage = "بیوگرافی نمی‌تواند بیش از ۵۰۰ کاراکتر باشد")]
//        public string? Bio { get; set; }

//        [Display(Name = "وب‌سایت")]
//        [Url(ErrorMessage = "آدرس وب‌سایت معتبر نیست")]
//        public string? Website { get; set; }

//        [Display(Name = "شهر")]
//        public string? City { get; set; }

//        [Display(Name = "کشور")]
//        public string? Country { get; set; }

//        // Navigation Properties
//        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

//        // Property محاسباتی برای نام کامل
//        [Display(Name = "نام کامل")]
//        public string FullName => $"{FirstName} {LastName}".Trim();

//        [Display(Name = "نام نمایشی")]
//        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : UserName;
//    }
//}