using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rexplor.Models
{
    public class Consultation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "کاربر")]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }

        [Required(ErrorMessage = "نوع مشاوره الزامی است")]
        [Display(Name = "نوع مشاوره")]
        public ConsultationType Type { get; set; }

        [Required(ErrorMessage = "موضوع مشاوره الزامی است")]
        [Display(Name = "موضوع مشاوره")]
        [StringLength(200, ErrorMessage = "موضوع نمی‌تواند بیش از ۲۰۰ کاراکتر باشد")]
        public string Topic { get; set; }

        [Required(ErrorMessage = "توضیحات الزامی است")]
        [Display(Name = "توضیحات کامل")]
        [StringLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از ۱۰۰۰ کاراکتر باشد")]
        public string Description { get; set; }

        [Display(Name = "فایل‌های پیوست")]
        public string? Attachments { get; set; } // مسیر فایل‌های آپلود شده

        [Required(ErrorMessage = "تاریخ جلسه الزامی است")]
        [Display(Name = "تاریخ جلسه")]
        [DataType(DataType.Date)]
        public DateTime SessionDate { get; set; }

        [Required(ErrorMessage = "ساعت جلسه الزامی است")]
        [Display(Name = "ساعت جلسه")]
        [DataType(DataType.Time)]
        public TimeSpan SessionTime { get; set; }

        [Display(Name = "مدت جلسه (دقیقه)")]
        [Range(30, 180, ErrorMessage = "مدت جلسه باید بین ۳۰ تا ۱۸۰ دقیقه باشد")]
        public int Duration { get; set; } = 60;

        [Required(ErrorMessage = "روش ارتباط الزامی است")]
        [Display(Name = "روش ارتباط")]
        public ContactMethod ContactMethod { get; set; }

        [Display(Name = "شماره تماس/لینک ارتباط")]
        [StringLength(200)]
        public string? ContactInfo { get; set; }

        [Display(Name = "هزینه مشاوره")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "وضعیت پرداخت")]
        public bool IsPaid { get; set; } = false;

        [Display(Name = "وضعیت جلسه")]
        public ConsultationStatus Status { get; set; } = ConsultationStatus.Pending;

        [Display(Name = "تاریخ ثبت درخواست")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تاریخ بروزرسانی")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Display(Name = "یادداشت‌های مدیر")]
        [StringLength(500)]
        public string? AdminNotes { get; set; }

        [Display(Name = "شماره پیگیری")]
        public string TrackingCode { get; set; } = GenerateTrackingCode();

        // محاسبات
        public DateTime SessionDateTime => SessionDate.Add(SessionTime);
        public DateTime EndDateTime => SessionDateTime.AddMinutes(Duration);

        private static string GenerateTrackingCode()
        {
            return "CONS-" + DateTime.Now.ToString("yyyyMMdd") + "-"
                   + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }
    }

    public enum ConsultationType
    {
        [Display(Name = "مشاوره پژوهشی")]
        Research,

        [Display(Name = "ویرایش طرح پژوهشی")]
        ResearchProposal,

        [Display(Name = "ویرایش مقاله")]
        ArticleEditing,

        [Display(Name = "آموزش پایتون")]
        PythonTraining,

        [Display(Name = "تحلیل آماری")]
        StatisticalAnalysis,

        [Display(Name = "نگارش علمی")]
        ScientificWriting,

        [Display(Name = "سایر")]
        Other
    }

    public enum ContactMethod
    {
        [Display(Name = "تماس تلفنی")]
        PhoneCall,

        [Display(Name = "واتس‌اپ")]
        WhatsApp,

        [Display(Name = "اسکایپ")]
        Skype,

        [Display(Name = "گوگل میت")]
        GoogleMeet,

        [Display(Name = "ادوبی کانکت")]
        AdobeConnect,

        [Display(Name = "حضوری")]
        InPerson
    }

    public enum ConsultationStatus
    {
        [Display(Name = "در انتظار تایید")]
        Pending,

        [Display(Name = "تایید شده")]
        Approved,

        [Display(Name = "رد شده")]
        Rejected,

        [Display(Name = "لغو شده")]
        Cancelled,

        [Display(Name = "تکمیل شده")]
        Completed,

        [Display(Name = "در حال انجام")]
        InProgress
    }

    public class ConsultationViewModel
    {
        [Required(ErrorMessage = "نوع مشاوره الزامی است")]
        [Display(Name = "نوع مشاوره")]
        public ConsultationType Type { get; set; }

        [Required(ErrorMessage = "موضوع مشاوره الزامی است")]
        [Display(Name = "موضوع مشاوره")]
        [StringLength(200)]
        public string Topic { get; set; }

        [Required(ErrorMessage = "توضیحات الزامی است")]
        [Display(Name = "توضیحات کامل")]
        [StringLength(1000)]
        public string Description { get; set; }

        [Display(Name = "فایل‌های پیوست (اختیاری)")]
        public List<IFormFile>? AttachmentFiles { get; set; }

        [Required(ErrorMessage = "تاریخ جلسه الزامی است")]
        [Display(Name = "تاریخ جلسه")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "تاریخ باید آینده باشد")]
        public DateTime SessionDate { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "ساعت جلسه الزامی است")]
        [Display(Name = "ساعت جلسه")]
        [DataType(DataType.Time)]
        public TimeSpan SessionTime { get; set; } = new TimeSpan(14, 0, 0);

        [Display(Name = "مدت جلسه (دقیقه)")]
        [Range(30, 180)]
        public int Duration { get; set; } = 60;

        [Required(ErrorMessage = "روش ارتباط الزامی است")]
        [Display(Name = "روش ارتباط")]
        public ContactMethod ContactMethod { get; set; }

        [Display(Name = "شماره تماس/لینک ارتباط")]
        [StringLength(200)]
        public string? ContactInfo { get; set; }

        [Display(Name = "ایمیل برای اطلاع‌رسانی")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        public string? NotificationEmail { get; set; }
    }

    // Validator برای تاریخ آینده
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime date)
            {
                return date > DateTime.Now.Date;
            }
            return false;
        }
    }
}