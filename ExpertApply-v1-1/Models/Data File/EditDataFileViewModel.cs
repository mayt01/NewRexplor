using System.ComponentModel.DataAnnotations;

namespace Rexplor.Models
{
    public class EditDataFileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان فایل الزامی است")]
        [Display(Name = "عنوان فایل")]
        [StringLength(200, ErrorMessage = "عنوان نباید بیش از ۲۰۰ کاراکتر باشد")]
        public string Title { get; set; }

        [Required(ErrorMessage = "توضیحات الزامی است")]
        [Display(Name = "توضیحات")]
        [StringLength(1000, ErrorMessage = "توضیحات نباید بیش از ۱۰۰۰ کاراکتر باشد")]
        public string Description { get; set; }

        [Required(ErrorMessage = "قیمت الزامی است")]
        [Display(Name = "قیمت (تومان)")]
        [Range(0, 1000000000, ErrorMessage = "قیمت باید بین ۰ تا ۱,۰۰۰,۰۰۰,۰۰۰ تومان باشد")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "دسته‌بندی الزامی است")]
        [Display(Name = "دسته‌بندی")]
        public int CategoryId { get; set; }

        [Display(Name = "تگ‌ها (با کاما جدا کنید)")]
        [StringLength(500)]
        public string Tags { get; set; }

        [Display(Name = "فعال باشد")]
        public bool IsActive { get; set; }

        [Display(Name = "آپلود فایل جدید (اختیاری)")]
        public IFormFile? NewFile { get; set; }
    }
}
