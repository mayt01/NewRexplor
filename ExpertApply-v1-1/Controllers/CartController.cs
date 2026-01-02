
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;
using Rexplor.Services;
using Rexplor.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Text.Json;

namespace Rexplor.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDiscountService _discountService;

        public CartController(ApplicationDbContext context, IDiscountService discountService)
        {
            _context = context;
            _discountService = discountService;
        }


        public IActionResult Index()
        {
            var cart = GetCart();

            // اطلاعات تخفیف از Session
            var discountData = GetDiscountFromSession();
            if (discountData != null)
            {
                ViewBag.DiscountCode = discountData.Code;
                ViewBag.DiscountPercent = discountData.DiscountPercent;
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int fileId, int quantity = 1,
    string discountCode = null, decimal? discountAmount = null, decimal? finalPrice = null)
        {
            var file = await _context.DataFiles
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == fileId && f.IsActive);

            if (file == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(item => item.DataFileId == fileId);

            // **بررسی: اگر قبلاً در سبد خرید موجود است**
            if (existingItem != null)
            {
                TempData["ErrorMessage"] = $"⚠️ «{file.Title}» قبلاً به سبد خرید اضافه شده است.";

                // اگر درخواست AJAX باشد
                if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = $"«{file.Title}» قبلاً به سبد خرید اضافه شده است."
                    });
                }

                return RedirectToAction("Details", "DataFiles", new { id = fileId });
            }

            // قیمت نهایی
            decimal itemFinalPrice = file.Price;
            decimal itemOriginalPrice = file.Price;

            // اگر قیمت نهایی ارسال شده (از تخفیف)
            if (finalPrice.HasValue && finalPrice.Value > 0)
            {
                itemFinalPrice = finalPrice.Value;
            }
            // اگر کد تخفیف ارسال شده
            else if (!string.IsNullOrEmpty(discountCode))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var validation = await _discountService.ValidateDiscountAsync(
                    discountCode, fileId, file.Price, userId);

                if (validation.IsValid)
                {
                    itemFinalPrice = validation.FinalAmount;
                }
            }

            // **همیشه quantity = 1 برای فایل‌های دیجیتال**
            cart.Add(new ShoppingCartItem
            {
                DataFileId = file.Id,
                Title = file.Title,
                Price = itemFinalPrice,
                OriginalPrice = itemOriginalPrice,
                Quantity = 1, // **همیشه 1**
                CategoryName = file.Category?.Name ?? "بدون دسته"
            });

            SaveCart(cart);

            TempData["SuccessMessage"] = $"✅ «{file.Title}» به سبد خرید اضافه شد.";

            // اگر درخواست AJAX باشد
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    message = $"«{file.Title}» به سبد خرید اضافه شد.",
                    cartCount = cart.Sum(item => item.Quantity)
                });
            }

            return RedirectToAction("Details", "DataFiles", new { id = fileId });
        }

        [HttpPost]
        public async Task<IActionResult> ApplyDiscount([FromForm] string discountCode)
        {
            try
            {
                var cart = GetCart();
                if (!cart.Any())
                {
                    return Json(new { success = false, message = "سبد خرید شما خالی است" });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // 🟢 **مهم: بررسی کنیم کد تخفیف برای کدوم فایل‌ها معتبره**
                var validItems = new List<ShoppingCartItem>();
                var invalidItems = new List<ShoppingCartItem>();

                foreach (var item in cart)
                {
                    var validation = await _discountService.ValidateDiscountAsync(
                        discountCode,
                        item.DataFileId, // 🟢 برای هر فایل چک کنیم
                        item.OriginalPrice * item.Quantity,
                        userId);

                    if (validation.IsValid)
                    {
                        validItems.Add(item);
                    }
                    else
                    {
                        invalidItems.Add(item);
                    }
                }

                // اگر برای هیچ فایلی معتبر نیست
                if (!validItems.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "این کد تخفیف برای فایل‌های سبد خرید شما معتبر نیست"
                    });
                }

                // 🟢 **فقط برای فایل‌های معتبر تخفیف اعمال کن**
                decimal totalDiscount = 0;
                decimal totalFinalAmount = 0;

                foreach (var item in validItems)
                {
                    var validation = await _discountService.ValidateDiscountAsync(
                        discountCode,
                        item.DataFileId,
                        item.OriginalPrice * item.Quantity,
                        userId);

                    // محاسبه تخفیف برای این آیتم
                    decimal itemDiscountPercent = validation.DiscountPercent;
                    decimal itemDiscountAmount = item.OriginalPrice * itemDiscountPercent / 100;

                    // اعمال محدودیت حداکثر تخفیف
                    if (validation.MaxDiscountAmount.HasValue)
                    {
                        itemDiscountAmount = Math.Min(itemDiscountAmount, validation.MaxDiscountAmount.Value / item.Quantity);
                    }

                    item.Price = item.OriginalPrice - itemDiscountAmount;
                    totalDiscount += itemDiscountAmount * item.Quantity;
                }

                SaveCart(cart);

                // ذخیره اطلاعات تخفیف در Session
                SaveDiscountToSession(new DiscountSessionData
                {
                    Code = discountCode,
                    DiscountPercent = validItems.First().OriginalPrice > 0 ?
                        (int)((totalDiscount / validItems.Sum(i => i.OriginalPrice * i.Quantity)) * 100) : 0,
                    DiscountAmount = totalDiscount,
                    FinalAmount = cart.Sum(item => item.Price * item.Quantity),
                    AppliedAt = DateTime.Now
                });

                // 🟢 **پیام مناسب نمایش بده**
                string message;
                if (invalidItems.Any())
                {
                    message = $"تخفیف فقط برای {validItems.Count} فایل از {cart.Count} فایل اعمال شد. " +
                             $"فایل‌های دیگر شامل این تخفیف نمی‌شوند.";
                }
                else
                {
                    message = "تخفیف با موفقیت اعمال شد";
                }

                return Json(new
                {
                    success = true,
                    message = message,
                    discountAmount = totalDiscount,
                    discountPercent = validItems.First().OriginalPrice > 0 ?
                        (int)((totalDiscount / validItems.Sum(i => i.OriginalPrice * i.Quantity)) * 100) : 0,
                    finalAmount = cart.Sum(item => item.Price * item.Quantity),
                    appliedToItems = validItems.Select(i => i.DataFileId).ToList(),
                    notAppliedToItems = invalidItems.Select(i => i.DataFileId).ToList()
                });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error applying discount");
                return Json(new
                {
                    success = false,
                    message = "خطا در اعمال تخفیف: " + ex.Message
                });
            }
        }

        // POST: حذف تخفیف
        [HttpPost]
        public IActionResult RemoveDiscount()
        {
            try
            {
                var cart = GetCart();

                // بازگرداندن قیمت اصلی به آیتم‌ها
                foreach (var item in cart)
                {
                    item.Price = item.OriginalPrice;
                }

                SaveCart(cart);

                // حذف اطلاعات تخفیف از Session
                ClearDiscountFromSession();

                // محاسبه مبالغ جدید
                var newSubtotal = cart.Sum(item => item.OriginalPrice * item.Quantity);
                var newTotal = cart.Sum(item => item.Price * item.Quantity);
                var newDiscountAmount = newSubtotal - newTotal;

                return Json(new
                {
                    success = true,
                    message = "تخفیف با موفقیت حذف شد",
                    subtotal = newSubtotal,
                    total = newTotal,
                    discountAmount = newDiscountAmount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "خطا در حذف تخفیف: " + ex.Message
                });
            }
        }

        // POST: حذف از سبد خرید
        [HttpPost]
        public IActionResult RemoveFromCart(int fileId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(item => item.DataFileId == fileId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);

                // اگر سبد خرید خالی شد، تخفیف هم حذف شود
                if (!cart.Any())
                {
                    ClearDiscountFromSession();
                }

                TempData["SuccessMessage"] = $"❌ «{item.Title}» از سبد خرید حذف شد.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: بروزرسانی تعداد
        [HttpPost]
        public IActionResult UpdateQuantity(int fileId, int quantity)
        {
            if (quantity < 1)
            {
                TempData["ErrorMessage"] = "تعداد باید حداقل 1 باشد.";
                return RedirectToAction(nameof(Index));
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(item => item.DataFileId == fileId);

            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
                TempData["SuccessMessage"] = "تعداد با موفقیت به‌روزرسانی شد.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: خالی کردن سبد خرید
        [HttpPost]
        public IActionResult ClearCart()
        {
            ClearCartSession();
            ClearDiscountFromSession();
            TempData["SuccessMessage"] = "✅ سبد خرید خالی شد.";
            return RedirectToAction(nameof(Index));
        }

        //}
        // متد Checkout را به این شکل به‌روزرسانی کنید:
        public IActionResult Checkout()
        {
            var cart = GetCart();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "سبد خرید شما خالی است.";
                return RedirectToAction(nameof(Index));
            }

            // اطلاعات تخفیف از Session
            var discountData = GetDiscountFromSession();
            if (discountData != null)
            {
                ViewBag.DiscountCode = discountData.Code;
                ViewBag.DiscountPercent = discountData.DiscountPercent;
            }

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder()
        {
            var cart = GetCart();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "سبد خرید شما خالی است.";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var discountData = GetDiscountFromSession();
                string discountCode = discountData?.Code ?? string.Empty;

                // 🆕 محاسبه مبلغ تخفیف
                var subtotal = cart.Sum(item => item.OriginalPrice * item.Quantity);
                var total = cart.Sum(item => item.Price * item.Quantity);
                var discountAmount = subtotal - total;

                // ایجاد سفارش جدید
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.Sum(item => item.Total),
                    PaymentStatus = PaymentStatus.Pending,
                    Status = "در انتظار پرداخت",
                    UserIP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UsedDiscountCode = discountCode, // ✅ این خط رو اضافه کنید
                    DiscountAmount = discountAmount // این هم اگر لازمه
                };

                // اضافه کردن آیتم‌ها به سفارش
                foreach (var cartItem in cart)
                {
                    var file = await _context.DataFiles.FindAsync(cartItem.DataFileId);

                    if (file != null)
                    {
                        var orderItem = new OrderItem
                        {
                            DataFileId = file.Id,
                            UnitPrice = file.Price,
                            Quantity = cartItem.Quantity
                        };

                        order.OrderItems.Add(orderItem);
                    }
                }

                // ذخیره سفارش
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();


                // خالی کردن سبد خرید
                ClearCartSession();
                ClearDiscountFromSession();
                TempData["SuccessMessage"] = $"✅ سفارش شما با شماره {order.OrderNumber} ثبت شد.";

                // ✅ تغییر مهم اینجاست: به صفحه Payment هدایت کن نه OrderDetails
                return RedirectToAction("Payment", "Orders", new { id = order.Id });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order: {ex.Message}");
                TempData["ErrorMessage"] = "خطا در ایجاد سفارش. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction(nameof(Checkout));
            }
        }

        // ========== متدهای کمکی ==========

        private List<ShoppingCartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("ShoppingCart");
            return string.IsNullOrEmpty(cartJson)
                ? new List<ShoppingCartItem>()
                : JsonSerializer.Deserialize<List<ShoppingCartItem>>(cartJson)
                  ?? new List<ShoppingCartItem>();
        }

        private void SaveCart(List<ShoppingCartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("ShoppingCart", cartJson);
        }

        private void ClearCartSession()
        {
            HttpContext.Session.Remove("ShoppingCart");
        }

        private DiscountSessionData GetDiscountFromSession()
        {
            var discountJson = HttpContext.Session.GetString("AppliedDiscount");
            return string.IsNullOrEmpty(discountJson)
                ? null
                : JsonSerializer.Deserialize<DiscountSessionData>(discountJson);
        }

        private void SaveDiscountToSession(DiscountSessionData discountData)
        {
            var discountJson = JsonSerializer.Serialize(discountData);
            HttpContext.Session.SetString("AppliedDiscount", discountJson);
        }

        private void ClearDiscountFromSession()
        {
            HttpContext.Session.Remove("AppliedDiscount");
        }

        // متد برای نمایش تعداد آیتم‌های سبد خرید (برای استفاده در لایوت)
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = GetCart();
            var count = cart.Sum(item => item.Quantity);
            return Json(new { count });
        }
    }

    // ========== کلاس‌های کمکی ==========

    public class DiscountSessionData
    {
        public string Code { get; set; }
        public int DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    public class CartIndexViewModel
    {
        public List<ShoppingCartItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public string DiscountCode { get; set; }
        public int? DiscountPercent { get; set; }

        [NotMapped]
        public bool HasDiscount => DiscountAmount > 0;
    }

    public class CheckoutViewModel
    {
        public List<ShoppingCartItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public string DiscountCode { get; set; }
        public int? DiscountPercent { get; set; }
    }
}