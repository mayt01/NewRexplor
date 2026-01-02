using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rexplor.Models
{
    public class Discount
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "کد تخفیف الزامی است")]
        [StringLength(50, ErrorMessage = "کد تخفیف نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [Display(Name = "کد تخفیف")]
        public string Code { get; set; }

        [Required(ErrorMessage = "درصد تخفیف الزامی است")]
        [Range(1, 100, ErrorMessage = "درصد تخفیف باید بین 1 تا 100 باشد")]
        [Display(Name = "درصد تخفیف")]
        public int DiscountPercent { get; set; }

        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "تاریخ پایان الزامی است")]
        [Display(Name = "تاریخ پایان")]
        public DateTime EndDate { get; set; }

        [Display(Name = "محدودیت تعداد استفاده")]
        public int? UsageLimit { get; set; }

        [Display(Name = "تعداد استفاده شده")]
        public int UsedCount { get; set; } = 0;

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "حداقل مبلغ خرید")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinPurchaseAmount { get; set; }

        [Display(Name = "برای همه فایل‌ها")]
        public bool IsForAllFiles { get; set; } = true;

        [Display(Name = "حداکثر تخفیف (تومان)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // -------- فیلدهای جدید برای بازاریاب --------
        [Display(Name = "برای بازاریاب؟")]
        public bool IsForMarketer { get; set; } = false; // 🆕 این خط را اضافه کنید

        [Display(Name = "ایمیل بازاریاب")]
        [EmailAddress(ErrorMessage = "لطفا یک ایمیل معتبر وارد کنید")]
        public string? MarketerEmail { get; set; } // 🆕 این خط را اضافه کنید

        [Display(Name = "نام بازاریاب")]
        public string? MarketerName { get; set; } // 🆕 این خط را اضافه کنید (اختیاری)

        [Display(Name = "شماره تماس بازاریاب")]
        public string? MarketerPhone { get; set; } // 🆕 این خط را اضافه کنید

        // -------- فیلدهای آمار --------
        [Display(Name = "تعداد فروش با این کد")]
        public int SalesCount { get; set; } = 0; // 🆕 این خط را اضافه کنید

        // Navigation Properties
        public virtual ICollection<FileDiscount> FileDiscounts { get; set; } = new List<FileDiscount>();
        public virtual ICollection<DiscountUsage> DiscountUsages { get; set; } = new List<DiscountUsage>();

        [NotMapped]
        public bool IsValid
        {
            get
            {
                if (!IsActive) return false;
                if (DateTime.Now < StartDate) return false;
                if (DateTime.Now > EndDate) return false;
                if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value) return false;
                return true;
            }
        }

        [NotMapped]
        public bool HasExpired => DateTime.Now > EndDate;

        [NotMapped]
        public bool IsAvailable => IsValid && !HasExpired;

        public decimal CalculateDiscount(decimal originalPrice)
        {
            var discountAmount = originalPrice * DiscountPercent / 100;

            if (MaxDiscountAmount.HasValue && discountAmount > MaxDiscountAmount.Value)
            {
                discountAmount = MaxDiscountAmount.Value;
            }

            return Math.Round(discountAmount, 2);
        }

        public decimal CalculateFinalPrice(decimal originalPrice)
        {
            var discount = CalculateDiscount(originalPrice);
            return originalPrice - discount;
        }
    }

    public class FileDiscount
    {
        public int FileId { get; set; }
        [ForeignKey("FileId")]
        public virtual DataFile File { get; set; }

        public int DiscountId { get; set; }
        [ForeignKey("DiscountId")]
        public virtual Discount Discount { get; set; }
    }

    public class DiscountUsage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int DiscountId { get; set; }

        public int? FileId { get; set; }

        public int? OrderId { get; set; }

        [Required]
        public DateTime UsedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FinalAmount { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("DiscountId")]
        public virtual Discount Discount { get; set; }

        [ForeignKey("FileId")]
        public virtual DataFile File { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}