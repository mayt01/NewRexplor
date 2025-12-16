//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Rexplor.Data;
//using Rexplor.Models;
//using System.Threading.Tasks;
//using System.Linq;

//[Authorize]
//public class OrdersController : Controller
//{
//    private readonly ApplicationDbContext _context;
//    private readonly UserManager<IdentityUser> _userManager;

//    public OrdersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
//    {
//        _context = context;
//        _userManager = userManager;
//    }

//    // نمایش فرم سفارش
//    [HttpGet]
//    public IActionResult Create()
//    {
//        return View();
//    }

//    // ثبت سفارش
//    [HttpPost]
//    public async Task<IActionResult> Create(string fileName, decimal price)
//    {
//        var user = await _userManager.GetUserAsync(User);
//        if (user == null) return RedirectToAction("Login", "Account");

//        var order = new Order
//        {
//            UserId = user.Id,
//            FileName = fileName,
//            Price = price,
//            IsPaid = false
//        };

//        _context.Orders.Add(order);
//        await _context.SaveChangesAsync();

//        // شبیه‌سازی پرداخت: می‌توانیم بعد از پرداخت IsPaid را true کنیم
//        return RedirectToAction("PaymentSimulation", new { id = order.Id });
//    }

//    // شبیه‌سازی پرداخت
//    [HttpGet]
//    public async Task<IActionResult> PaymentSimulation(int id)
//    {
//        var user = await _userManager.GetUserAsync(User);
//        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);
//        if (order == null) return NotFound();

//        // فرض کنید کاربر پرداخت موفق انجام داد
//        order.IsPaid = true;
//        await _context.SaveChangesAsync();

//        return RedirectToAction("MyOrders");
//    }

//    // نمایش سفارش‌های کاربر
//    [HttpGet]
//    public async Task<IActionResult> MyOrders()
//    {
//        var user = await _userManager.GetUserAsync(User);
//        if (user == null) return RedirectToAction("Login", "Account");

//        var orders = await _context.Orders
//            .Where(o => o.UserId == user.Id)
//            .OrderByDescending(o => o.OrderDate)
//            .ToListAsync();

//        return View(orders);
//    }

//    // دانلود فایل (اگر پرداخت شده باشد)
//    [HttpGet]
//    public async Task<IActionResult> Download(int id)
//    {
//        var user = await _userManager.GetUserAsync(User);
//        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

//        if (order == null || !order.IsPaid) return Forbid();

//        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", order.FileName);
//        if (!System.IO.File.Exists(filePath)) return NotFound();

//        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
//        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", order.FileName);
//    }
//}


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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        //private readonly ZarinPalService _zarinPalService;
        private readonly IZarinPalService _zarinPalService;


        public OrdersController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
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

        //// GET: جزئیات سفارش
        //[HttpGet]
        //public async Task<IActionResult> OrderDetails(int id)
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    var order = await _context.Orders
        //        .Include(o => o.OrderItems)
        //        .ThenInclude(oi => oi.DataFile)
        //        .ThenInclude(df => df.Category)
        //        .Include(o => o.User)
        //        .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(order);
        //}

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

        // GET: پرداخت سفارش
        //[HttpGet]
        //public async Task<IActionResult> Payment(int id)
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    var order = await _context.Orders
        //        .Include(o => o.OrderItems)
        //        .ThenInclude(oi => oi.DataFile)
        //        .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    if (order.PaymentStatus == PaymentStatus.Paid)
        //    {
        //        TempData["InfoMessage"] = "این سفارش قبلاً پرداخت شده است.";
        //        return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
        //    }



        //    //// TODO: در اینجا باید درگاه زرین‌پال را فراخوانی کنید
        //    try
        //    {
        //        // ایجاد Callback URL
        //        var callbackUrl = Url.Action("VerifyPayment", "Orders",
        //            new { id = order.Id },
        //            protocol: HttpContext.Request.Scheme);

        //        // درخواست پرداخت به زرین‌پال
        //        var paymentResponse = await _zarinPalService.RequestPaymentAsync(
        //            amount: order.TotalAmount,
        //            description: $"پرداخت سفارش #{order.OrderNumber} - {order.OrderItems.Count} فایل",
        //            callbackUrl: callbackUrl,
        //            email: user.Email);

        //        if (paymentResponse.Status == 100) // موفق
        //        {
        //            // ذخیره Authority در دیتابیس
        //            order.TransactionId = paymentResponse.Authority;
        //            await _context.SaveChangesAsync();

        //            // هدایت به درگاه زرین‌پال
        //            return Redirect(paymentResponse.GatewayURL);
        //        }
        //        else
        //        {
        //            TempData["ErrorMessage"] = $"خطا در اتصال به درگاه پرداخت: {paymentResponse.Message}";
        //            return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = $"خطا: {ex.Message}";
        //        return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
        //    }
        //    //// TODO: انتهای فراخوانی زرین پال

        //    //ViewBag.Order = order;
        //    //ViewBag.TotalAmount = order.TotalAmount;

        //    //// برای تست، مستقیماً به شبیه‌سازی پرداخت هدایت می‌کنیم
        //    //return RedirectToAction("PaymentSimulation", new { id = order.Id });
        //}

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
        //        var user = await _userManager.GetUserAsync(User);
        //        var order = await _context.Orders
        //            .Include(o => o.OrderItems)
        //            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

        //        if (order == null)
        //        {
        //            return NotFound();
        //        }

        //        // کاربر از پرداخت انصراف داد
        //        if (Status != "OK")
        //        {
        //            TempData["ErrorMessage"] = "پرداخت لغو شد.";
        //            return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
        //        }

        //        // تأیید پرداخت با زرین‌پال
        //        var verification = await _zarinPalService.VerifyPaymentAsync(
        //            authority: Authority,
        //            amount: order.TotalAmount);

        //        if (verification.Status == 100 || verification.Status == 101)
        //        {
        //            // پرداخت موفق
        //            order.PaymentStatus = PaymentStatus.Paid;
        //            order.IsPaid = true;
        //            order.PaymentDate = DateTime.Now;
        //            order.Status = "پرداخت موفق";
        //            order.TransactionId = verification.RefID.ToString();
        //            order.PaymentReference = verification.RefID.ToString();

        //            await _context.SaveChangesAsync();

        //            TempData["SuccessMessage"] =
        //                $"✅ پرداخت با موفقیت انجام شد. " +
        //                $"کد پیگیری: {verification.RefID}";

        //            return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
        //        }
        //        else
        //        {
        //            // پرداخت ناموفق
        //            TempData["ErrorMessage"] =
        //                $"پرداخت ناموفق بود. " +
        //                $"کد خطا: {verification.Status} - {verification.Message}";

        //            return RedirectToAction(nameof(OrderDetails), new { id = order.Id });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = $"خطا در تأیید پرداخت: {ex.Message}";
        //        return RedirectToAction(nameof(MyOrders));
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> VerifyPayment(int id, string Authority, string Status)
        {
            try
            {
                Console.WriteLine($"🔄 تأیید پرداخت. OrderId: {id}, Authority: {Authority}, Status: {Status}");

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

                if (order == null)
                {
                    Console.WriteLine($"❌ سفارش #{id} برای تأیید پیدا نشد");
                    TempData["ErrorMessage"] = "سفارش مورد نظر یافت نشد.";
                    return RedirectToAction("MyOrders");
                }

                // کاربر از پرداخت انصراف داده
                if (Status != "OK")
                {
                    Console.WriteLine($"⏹️ کاربر پرداخت را لغو کرد. OrderId: {id}");

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
                    // پرداخت موفق
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.IsPaid = true;
                    order.PaymentDate = DateTime.Now;
                    order.Status = "پرداخت موفق";
                    order.PaymentReference = verification.RefId.ToString();

                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ پرداخت تأیید شد. RefId: {verification.RefId}");

                    TempData["SuccessMessage"] =
                        $"✅ پرداخت موفق!<br>" +
                        $"<strong>کد پیگیری:</strong> {verification.RefId}<br>" +
                        $"<strong>شماره سفارش:</strong> {order.OrderNumber}";
                }
                else
                {
                    // پرداخت ناموفق
                    order.PaymentStatus = PaymentStatus.Failed;
                    order.Status = "پرداخت ناموفق";
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"❌ پرداخت ناموفق. دلیل: {verification.Message}");

                    TempData["ErrorMessage"] = verification.Message;
                }

                return RedirectToAction("OrderDetails", new { id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 خطای سیستمی در VerifyPayment: {ex.Message}");
                TempData["ErrorMessage"] = "خطا در تأیید پرداخت.";
                return RedirectToAction("MyOrders");
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
