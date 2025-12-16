// Models/Order.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rexplor.Models
{
    // Models/Order.cs
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "شناسه کاربر")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "شماره سفارش")]
        public string OrderNumber { get; set; } = GenerateOrderNumber();

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Display(Name = "نام فایل")]
        public string? FileName { get; set; }

        [Display(Name = "قیمت")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "پرداخت شده")]
        public bool IsPaid { get; set; } = false;


        [Display(Name = "تاریخ سفارش")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "شماره پیگیری")]
        public string? TransactionId { get; set; }


        [Display(Name = "شماره مرجع پرداخت")]
        public string? PaymentReference { get; set; }


        [Display(Name = "تاریخ پرداخت")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "وضعیت")]
        public string Status { get; set; } = "در انتظار پرداخت";

        // اگر می‌خوای چند فایل در یک سفارش باشه
        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "مبلغ کل")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "وضعیت پرداخت")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Display(Name = "آیپی کاربر")]
        public string? UserIP { get; set; }

        // Navigation Property
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // متد تولید شماره سفارش
        private static string GenerateOrderNumber()
        {
            return "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-"
                   + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }
    }

    public enum PaymentStatus
    {
        [Display(Name = "در انتظار پرداخت")]
        Pending,

        [Display(Name = "پرداخت موفق")]
        Paid,

        [Display(Name = "پرداخت ناموفق")]
        Failed,

        [Display(Name = "لغو شده")]
        Cancelled,

        [Display(Name = "بازگشت وجه")]
        Refunded
    }

    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [Required]
        public int DataFileId { get; set; }

        [ForeignKey("DataFileId")]
        public virtual DataFile DataFile { get; set; }

        [Display(Name = "قیمت واحد")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "تعداد")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "مبلغ کل")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class ShoppingCartItem
    {
        public int DataFileId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public string CategoryName { get; set; }

        public decimal Total => Price * Quantity;
    }
}