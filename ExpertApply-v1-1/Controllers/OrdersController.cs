
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;
using Rexplor.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Zarinpal.AspNetCore.Interfaces;


namespace Rexplor.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        //private readonly ZarinPalService _zarinPalService;
        private readonly IZarinPalService _zarinPalService;


        public OrdersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            IZarinPalService zarinPalService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _zarinPalService = zarinPalService;
        }

        // GET: نمایش سفارش‌های کاربر
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.DataFile)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }


        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                // ساده‌ترین کوئری ممکن
                var order = await _context.Orders
                    .Include(o => o.OrderItems)  // فقط OrderItems
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "سفارش پیدا نشد.";
                    return RedirectToAction(nameof(MyOrders));
                }

                // لود کردن DataFileها به صورت جداگانه
                foreach (var item in order.OrderItems)
                {
                    if (item.DataFile == null)
                    {
                        item.DataFile = await _context.DataFiles
                            .Include(f => f.Category)
                            .FirstOrDefaultAsync(f => f.Id == item.DataFileId);
                    }
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطا در بارگذاری سفارش.";
                Console.WriteLine($"Error: {ex.Message}");
                return RedirectToAction(nameof(MyOrders));
            }
        }

        [HttpGet]
        public async Task<IActionResult> SimpleOrderDetails(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                // فقط اطلاعات پایه سفارش رو بگیر
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

                if (order == null)
                {
                    return Content("سفارش پیدا نشد!");
                }

                return Content($"سفارش پیدا شد!<br>شماره: {order.OrderNumber}<br>مبلغ: {order.TotalAmount}");
            }
            catch (Exception ex)
            {
                return Content($"خطا: {ex.Message}");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Payment(int id)
        {
            try
            {
                Console.WriteLine($"🚀 شروع پرداخت برای سفارش #{id}");

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

                if (order == null)
                {
                    Console.WriteLine($"❌ سفارش #{id} پیدا نشد");
                    TempData["ErrorMessage"] = "سفارش مورد نظر یافت نشد.";
                    return RedirectToAction("MyOrders");
                }

                if (order.IsPaid)
                {
                    Console.WriteLine($"ℹ️ سفارش #{id} قبلاً پرداخت شده");
                    TempData["InfoMessage"] = "این سفارش قبلاً پرداخت شده است.";
                    return RedirectToAction("OrderDetails", new { id });
                }

                // 🔧 استفاده از سرویس زرین‌پال
                var paymentRequest = await _zarinPalService.RequestPaymentAsync(
                    amount: order.TotalAmount,
                    description: $"پرداخت سفارش #{order.OrderNumber}",
                    orderId: order.Id
                );

                if (paymentRequest.IsSuccess)
                {
                    // ذخیره Authority در دیتابیس
                    order.TransactionId = paymentRequest.Authority;
                    order.PaymentStatus = PaymentStatus.Pending;
                    order.Status = "در حال پرداخت";
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ درخواست پرداخت موفق. Authority: {paymentRequest.Authority}");
                    Console.WriteLine($"🔗 هدایت به: {paymentRequest.GatewayUrl}");

                    // هدایت کاربر به درگاه پرداخت
                    return Redirect(paymentRequest.GatewayUrl);
                }
                else
                {
                    Console.WriteLine($"❌ خطا در درخواست پرداخت: {paymentRequest.Message}");
                    TempData["ErrorMessage"] = paymentRequest.Message;
                    return RedirectToAction("OrderDetails", new { id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 خطای سیستمی در Payment: {ex.Message}");
                TempData["ErrorMessage"] = "خطای سیستمی در پردازش پرداخت.";
                return RedirectToAction("MyOrders");
            }
        }


        //[HttpGet]
        //public async Task<IActionResult> VerifyPayment(int id, string Authority, string Status)
        //{
        //    try
        //    {
        //        Console.WriteLine($"🔄 تأیید پرداخت. OrderId: {id}, Authority: {Authority}, Status: {Status}");

        //        var user = await _userManager.GetUserAsync(User);
        //        if (user == null) return RedirectToAction("Login", "Account");

        //        var order = await _context.Orders
        //            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

        //        if (order == null)
        //        {
        //            Console.WriteLine($"❌ سفارش #{id} برای تأیید پیدا نشد");
        //            TempData["ErrorMessage"] = "سفارش مورد نظر یافت نشد.";
        //            return RedirectToAction("MyOrders");
        //        }

        //        // کاربر از پرداخت انصراف داده
        //        if (Status != "OK")
        //        {
        //            Console.WriteLine($"⏹️ کاربر پرداخت را لغو کرد. OrderId: {id}");

        //            order.PaymentStatus = PaymentStatus.Failed;
        //            order.Status = "لغو توسط کاربر";
        //            await _context.SaveChangesAsync();

        //            TempData["ErrorMessage"] = "پرداخت لغو شد.";
        //            return RedirectToAction("OrderDetails", new { id });
        //        }

        //        // تأیید پرداخت با زرین‌پال
        //        var verification = await _zarinPalService.VerifyPaymentAsync(
        //            authority: Authority,
        //            amount: order.TotalAmount
        //        );

        //        if (verification.IsSuccess)
        //        {

        //            HttpContext.Session.Remove("ShoppingCart");

        //            // پرداخت موفق
        //            order.PaymentStatus = PaymentStatus.Paid;
        //            order.IsPaid = true;
        //            order.PaymentDate = DateTime.Now;
        //            order.Status = "پرداخت موفق";
        //            order.PaymentReference = verification.RefId.ToString();

        //            await _context.SaveChangesAsync();

        //            Console.WriteLine($"✅ پرداخت تأیید شد. RefId: {verification.RefId}");

        //            TempData["SuccessMessage"] =
        //                $"✅ پرداخت موفق!<br>" +
        //                $"<strong>کد پیگیری:</strong> {verification.RefId}<br>" +
        //                $"<strong>شماره سفارش:</strong> {order.OrderNumber}";
        //        }
        //        else
        //        {
        //            // پرداخت ناموفق
        //            order.PaymentStatus = PaymentStatus.Failed;
        //            order.Status = "پرداخت ناموفق";
        //            await _context.SaveChangesAsync();

        //            Console.WriteLine($"❌ پرداخت ناموفق. دلیل: {verification.Message}");

        //            TempData["ErrorMessage"] = verification.Message;
        //        }

        //        return RedirectToAction("OrderDetails", new { id });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"🔥 خطای سیستمی در VerifyPayment: {ex.Message}");
        //        TempData["ErrorMessage"] = "خطا در تأیید پرداخت.";
        //        return RedirectToAction("MyOrders");
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> VerifyPayment(int id, string Authority, string Status)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "سفارش مورد نظر یافت نشد.";
                    return RedirectToAction("MyOrders");
                }

                // اگر کاربر پرداخت را لغو کرده
                if (Status != "OK")
                {
                    order.PaymentStatus = PaymentStatus.Failed;
                    order.Status = "لغو توسط کاربر";
                    await _context.SaveChangesAsync();

                    TempData["ErrorMessage"] = "پرداخت لغو شد.";
                    return RedirectToAction("OrderDetails", new { id });
                }

                // تأیید پرداخت با زرین‌پال
                var verification = await _zarinPalService.VerifyPaymentAsync(
                    authority: Authority,
                    amount: order.TotalAmount
                );

                if (verification.IsSuccess)
                {
                    // حذف سبد خرید از Session
                    HttpContext.Session.Remove("ShoppingCart");

                    // آپدیت سفارش
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.IsPaid = true;
                    order.PaymentDate = DateTime.Now;
                    order.Status = "پرداخت موفق";
                    order.PaymentReference = verification.RefId.ToString();

                    await _context.SaveChangesAsync();

                    // 🆕 اگر سفارش کد تخفیف داشت و آن کد برای بازاریاب بود
                    if (!string.IsNullOrEmpty(order.UsedDiscountCode))
                    {
                        await UpdateMarketerStats(order);
                    }

                    TempData["SuccessMessage"] = $"✅ پرداخت موفق! کد پیگیری: {verification.RefId}";
                }
                else
                {
                    order.PaymentStatus = PaymentStatus.Failed;
                    order.Status = "پرداخت ناموفق";
                    await _context.SaveChangesAsync();

                    TempData["ErrorMessage"] = verification.Message;
                }

                return RedirectToAction("OrderDetails", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطا در تأیید پرداخت.";
                return RedirectToAction("MyOrders");
            }
        }

        // 🆕 متد جدید: آپدیت آمار بازاریاب
        private async Task UpdateMarketerStats(Order order)
        {
            try
            {
                // پیدا کردن کد تخفیف استفاده شده
                var discount = await _context.Discounts
                    .FirstOrDefaultAsync(d => d.Code == order.UsedDiscountCode);

                // اگر کد تخفیف پیدا شد و برای بازاریاب بود
                if (discount != null && discount.IsForMarketer)
                {
                    // افزایش تعداد فروش
                    discount.SalesCount++;
                    _context.Discounts.Update(discount);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ فروش #{order.OrderNumber} با کد تخفیف {discount.Code} ثبت شد.");
                    Console.WriteLine($"📊 تعداد فروش این کد: {discount.SalesCount}");

                    // 🆕 ارسال ایمیل به بازاریاب (اگر ایمیل داشت)
                    if (!string.IsNullOrEmpty(discount.MarketerEmail))
                    {
                        await SendEmailToMarketer(discount, order);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ خطا در آپدیت آمار بازاریاب: {ex.Message}");
            }
        }

        // 🆕 متد جدید: ارسال ایمیل به بازاریاب
        private async Task SendEmailToMarketer(Discount discount, Order order)
        {
            try
            {
                var emailService = HttpContext.RequestServices.GetService<EmailService>();

                if (emailService != null)
                {
                    // موضوع ایمیل
                    var subject = $"🎯 فروش جدید با کد تخفیف شما - #{order.OrderNumber}";

                    // متن ایمیل
                    var message = $@"
سلام {(string.IsNullOrEmpty(discount.MarketerName) ? "بازاریاب گرامی" : discount.MarketerName)},

یک فروش جدید با استفاده از کد تخفیف شما انجام شد!

📋 **مشخصات فروش:**
• شماره سفارش: #{order.OrderNumber}
• مبلغ فروش: {order.TotalAmount:N0} تومان
• تاریخ: {DateTime.Now:yyyy/MM/dd ساعت HH:mm}

📊 **آمار کد تخفیف شما ({discount.Code}):**
• تعداد فروش موفق: {discount.SalesCount} بار
• درصد تخفیف: {discount.DiscountPercent}%

با تشکر از همکاری شما

ارادتمند،
{User.Identity?.Name ?? "تیم فروش"}
            ";

                    // ارسال ایمیل
                    await emailService.SendEmailAsync(discount.MarketerEmail, subject, message);

                    Console.WriteLine($"📧 ایمیل اطلاع‌رسانی به {discount.MarketerEmail} ارسال شد.");

                    // 🆕 (اختیاری) ایمیل کپی برای خودتان
                    // await emailService.SendEmailAsync("admin@yoursite.com", 
                    //     $"کپی: فروش با کد {discount.Code}", 
                    //     $"کد تخفیف {discount.Code} یک فروش جدید ثبت کرد.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ خطا در ارسال ایمیل: {ex.Message}");
            }
        }

        // GET: شبیه‌سازی پرداخت (موقت - باید با زرین‌پال جایگزین شود)
        [HttpGet]
        public async Task<IActionResult> PaymentSimulation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: تأیید پرداخت شبیه‌سازی شده
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null) return NotFound();

            // شبیه‌سازی پرداخت موفق
            order.PaymentStatus = PaymentStatus.Paid;
            order.IsPaid = true;
            order.PaymentDate = DateTime.Now;
            order.Status = "پرداخت موفق";
            order.TransactionId = "SIM-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            order.PaymentReference = "شبیه‌سازی پرداخت";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ پرداخت با موفقیت انجام شد. می‌توانید فایل‌ها را دانلود کنید.";
            return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
        }

        // GET: دانلود فایل
        [HttpGet]
        public async Task<IActionResult> Download(int orderId, int fileId)
        {
            var user = await _userManager.GetUserAsync(User);

            // بررسی وجود سفارش و دسترسی کاربر
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order == null)
            {
                return Forbid("شما دسترسی به این سفارش را ندارید.");
            }

            if (order.PaymentStatus != PaymentStatus.Paid)
            {
                return Forbid("این سفارش پرداخت نشده است.");
            }

            // بررسی اینکه فایل متعلق به این سفارش باشد
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.DataFileId == fileId);
            if (orderItem == null)
            {
                return NotFound("فایل در این سفارش یافت نشد.");
            }

            // پیدا کردن فایل در دیتابیس
            var dataFile = await _context.DataFiles.FindAsync(fileId);
            if (dataFile == null || string.IsNullOrEmpty(dataFile.FilePath))
            {
                return NotFound("فایل پیدا نشد.");
            }

            // بررسی وجود فایل فیزیکی
            if (!System.IO.File.Exists(dataFile.FilePath))
            {
                return NotFound("فایل روی سرور پیدا نشد.");
            }

            // افزایش تعداد دانلود
            dataFile.DownloadCount++;
            _context.Update(dataFile);
            await _context.SaveChangesAsync();

            // خواندن و ارسال فایل
            var fileBytes = await System.IO.File.ReadAllBytesAsync(dataFile.FilePath);
            var contentType = GetContentType(dataFile.FileExtension);

            return File(fileBytes, contentType, $"{dataFile.Title}{dataFile.FileExtension}");
        }

        // GET: لیست فایل‌های قابل دانلود یک سفارش
        [HttpGet]
        public async Task<IActionResult> DownloadableFiles(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.DataFile)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null || order.PaymentStatus != PaymentStatus.Paid)
            {
                return Forbid();
            }

            return View(order);
        }

        // POST: لغو سفارش
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            // فقط سفارش‌های پرداخت نشده قابل لغو هستند
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["ErrorMessage"] = "سفارش‌های پرداخت شده قابل لغو نیستند.";
                return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
            }

            order.PaymentStatus = PaymentStatus.Cancelled;
            order.Status = "لغو شده";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ سفارش با موفقیت لغو شد.";
            return RedirectToAction(nameof(MyOrders));
        }

        // ========== متدهای کمکی ==========
        private string GetContentType(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".csv" => "text/csv",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        // متد قدیمی Create را غیرفعال می‌کنیم چون از Cart استفاده می‌شود
        [HttpGet]
        [Obsolete("این متد قدیمی است. لطفاً از CartController استفاده کنید.")]
        public IActionResult Create()
        {
            TempData["InfoMessage"] = "لطفاً از سبد خرید برای ایجاد سفارش استفاده کنید.";
            return RedirectToAction("Index", "Cart");
        }
    }
}
