using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rexplor.Models
{
    //public class DataFile
    //{
    //    [Key]
    //    public int Id { get; set; }

    //    [Required(ErrorMessage = "عنوان فایل الزامی است")]
    //    [Display(Name = "عنوان فایل")]
    //    [StringLength(200, ErrorMessage = "عنوان نباید بیش از ۲۰۰ کاراکتر باشد")]
    //    public string Title { get; set; }

    //    [Required(ErrorMessage = "توضیحات فایل الزامی است")]
    //    [Display(Name = "توضیحات")]
    //    [StringLength(1000, ErrorMessage = "توضیحات نباید بیش از ۱۰۰۰ کاراکتر باشد")]
    //    public string Description { get; set; }

    //    [Display(Name = "قیمت (تومان)")]
    //    [Range(0, 1000000000, ErrorMessage = "قیمت باید بین ۰ تا ۱,۰۰۰,۰۰۰,۰۰۰ تومان باشد")]
    //    [Column(TypeName = "decimal(18,2)")] // ✅ مهم: برای دقت اعشار در دیتابیس
    //    public decimal Price { get; set; }

    //    [Display(Name = "دسته‌بندی")]
    //    public int CategoryId { get; set; }

    //    [ForeignKey("CategoryId")]
    //    public virtual DataFileCategory? Category { get; set; }

    //    [Display(Name = "تاریخ بروزرسانی")]
    //    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    //    [Display(Name = "فعال")]
    //    public bool IsActive { get; set; } = true;

    //    // اطلاعات فایل
    //    [Display(Name = "مسیر فایل")]
    //    public string FilePath { get; set; }


    //    [Display(Name = "حجم فایل (بایت)")]
    //    public long FileSize { get; set; }

    //    [Display(Name = "فرمت فایل")]
    //    [StringLength(10)]
    //    public string FileExtension { get; set; }

    //    // اطلاعات اضافی
    //    [Display(Name = "تعداد دانلود")]
    //    public int DownloadCount { get; set; } = 0;

    //    [Display(Name = "تعداد بازدید")]
    //    public int ViewCount { get; set; } = 0;

    //    [Display(Name = "امتیاز")]
    //    [Range(0, 5)]
    //    public double Rating { get; set; } = 0;

    //    [Display(Name = "تعداد نظرات")]
    //    public int ReviewCount { get; set; } = 0;

    //    [Display(Name = "تگ‌ها")]
    //    [StringLength(500)]
    //    public string Tags { get; set; }
    //}
    public class DataFile
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان فایل الزامی است")]
        [Display(Name = "عنوان فایل")]
        [StringLength(200, ErrorMessage = "عنوان نباید بیش از ۲۰۰ کاراکتر باشد")]
        public string Title { get; set; }

        // ✅ کد یکتا برای دیتافایل
        [Display(Name = "کد یکتا")]
        [StringLength(20, ErrorMessage = "کد نباید بیش از ۲۰ کاراکتر باشد")]
        [Column(TypeName = "nvarchar(20)")]
        public string UniqueCode { get; set; } = GenerateUniqueCode();

        [Required(ErrorMessage = "توضیحات فایل الزامی است")]
        [Display(Name = "توضیحات")]
        [StringLength(1000, ErrorMessage = "توضیحات نباید بیش از ۱۰۰۰ کاراکتر باشد")]
        public string Description { get; set; }

        [Display(Name = "قیمت (تومان)")]
        [Range(0, 1000000000, ErrorMessage = "قیمت باید بین ۰ تا ۱,۰۰۰,۰۰۰,۰۰۰ تومان باشد")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "دسته‌بندی")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual DataFileCategory? Category { get; set; }

        [Display(Name = "تاریخ بروزرسانی")]
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // اطلاعات فایل
        [Display(Name = "مسیر فایل")]
        public string FilePath { get; set; }

        [Display(Name = "حجم فایل (بایت)")]
        public long FileSize { get; set; }

        [Display(Name = "فرمت فایل")]
        [StringLength(10)]
        public string FileExtension { get; set; }

        // اطلاعات اضافی
        [Display(Name = "تعداد دانلود")]
        public int DownloadCount { get; set; } = 0;

        [Display(Name = "تعداد بازدید")]
        public int ViewCount { get; set; } = 0;

        [Display(Name = "امتیاز")]
        [Range(0, 5)]
        public double Rating { get; set; } = 0;

        [Display(Name = "تعداد نظرات")]
        public int ReviewCount { get; set; } = 0;

        [Display(Name = "تگ‌ها")]
        [StringLength(500)]
        public string Tags { get; set; }

        // ✅ متد برای تولید کد یکتا
        private static string GenerateUniqueCode()
        {
            // ترکیب حروف و اعداد برای کد
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            // تولید کد ۱۰ رقمی
            var code = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // اضافه کردن پیشوند DF (DataFile)
            return "DF" + code;
        }

        // ✅ متد برای تولید کد جدید (مثلاً در صورت بروز مشکل تکراری بودن)
        public void RegenerateUniqueCode()
        {
            this.UniqueCode = GenerateUniqueCode();
        }
    }

    public class DataFileCategory
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "نام دسته‌بندی الزامی است")]
        [Display(Name = "نام دسته‌بندی")]
        [StringLength(100, ErrorMessage = "نام نباید بیش از ۱۰۰ کاراکتر باشد")]
        public string Name { get; set; }


        [Display(Name = "توضیحات")]
        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "آیکون")]
        public string? IconClass { get; set; } = "bi-folder";

        [Display(Name = "تعداد فایل‌ها")]
        [NotMapped] // در دیتابیس ذخیره نمی‌شود
        public int? FileCount { get; set; }

        // Navigation Property
        public virtual ICollection<DataFile> DataFiles { get; set; }
    }

    public class DataFileViewModel
    {
        [Required(ErrorMessage = "عنوان فایل الزامی است")]
        [Display(Name = "عنوان فایل")]
        public string Title { get; set; }

        [Required(ErrorMessage = "توضیحات الزامی است")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Required(ErrorMessage = "قیمت الزامی است")]
        [Display(Name = "قیمت (تومان)")]
        [Range(0, 1000000000, ErrorMessage = "قیمت باید بین ۰ تا ۱,۰۰۰,۰۰۰,۰۰۰ تومان باشد")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "دسته‌بندی الزامی است")]
        [Display(Name = "دسته‌بندی")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "فایل اکسل الزامی است")]
        [Display(Name = "فایل اکسل")]
        [DataType(DataType.Upload)]
        public IFormFile UploadedFile { get; set; }

        [Display(Name = "تگ‌ها (با کاما جدا کنید)")]
        public string Tags { get; set; }

        [Display(Name = "فعال باشد")]
        public bool IsActive { get; set; } = true;

        // ✅ نمایش کد یکتا (فقط خواندنی)
        [Display(Name = "کد یکتا")]
        [ReadOnly(true)]
        public string? UniqueCode { get; set; }
    }
}