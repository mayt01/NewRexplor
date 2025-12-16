// Controllers/CartController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;
using System.Security.Claims;
using System.Text.Json;

namespace Rexplor.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: سبد خرید
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // POST: اضافه کردن به سبد خرید
        [HttpPost]
        public async Task<IActionResult> AddToCart(int fileId, int quantity = 1)
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

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new ShoppingCartItem
                {
                    DataFileId = file.Id,
                    Title = file.Title,
                    Price = file.Price,
                    Quantity = quantity,
                    CategoryName = file.Category?.Name ?? "بدون دسته"
                });
            }

            SaveCart(cart);

            TempData["SuccessMessage"] = $"✅ «{file.Title}» به سبد خرید اضافه شد.";
            return RedirectToAction("Details", "DataFiles", new { id = fileId });
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
                return RedirectToAction(nameof(Index));
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(item => item.DataFileId == fileId);

            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: خالی کردن سبد خرید
        [HttpPost]
        public IActionResult ClearCart()
        {
            ClearCartSession();
            TempData["SuccessMessage"] = "✅ سبد خرید خالی شد.";
            return RedirectToAction(nameof(Index));
        }

        // GET: صفحه پرداخت
        public IActionResult Checkout()
        {
            var cart = GetCart();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "سبد خرید شما خالی است.";
                return RedirectToAction(nameof(Index));
            }

            return View(cart);
        }

        // POST: ایجاد سفارش
        [HttpPost]
        //public async Task<IActionResult> CreateOrder()
        //{
        //    var cart = GetCart();

        //    if (!cart.Any())
        //    {
        //        TempData["ErrorMessage"] = "سبد خرید شما خالی است.";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    // گرفتن کاربر جاری
        //    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        //    var user = await _context.Users.FindAsync(userId);

        //    if (user == null)
        //    {
        //        return Unauthorized();
        //    }

        //    // ایجاد سفارش جدید با مدل درست
        //    var order = new Order
        //    {
        //        UserId = userId,
        //        OrderDate = DateTime.Now,
        //        TotalAmount = cart.Sum(item => item.Total),
        //        PaymentStatus = PaymentStatus.Pending,
        //        UserIP = HttpContext.Connection.RemoteIpAddress?.ToString()
        //        // ❌ FileName نداریم! 
        //        // ❌ Price نداریم (کل قیمت در TotalAmount هست)
        //    };

        //    // اضافه کردن آیتم‌ها به سفارش
        //    foreach (var cartItem in cart)
        //    {
        //        var file = await _context.DataFiles.FindAsync(cartItem.DataFileId);

        //        if (file != null)
        //        {
        //            var orderItem = new OrderItem
        //            {
        //                DataFileId = file.Id,
        //                UnitPrice = file.Price,
        //                Quantity = cartItem.Quantity,
        //                Order = order
        //            };

        //            // اضافه کردن OrderItem به Order
        //            order.OrderItems.Add(orderItem);
        //        }
        //    }

        //    // ذخیره سفارش
        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    // خالی کردن سبد خرید
        //    ClearCartSession();

        //    TempData["SuccessMessage"] = $"✅ سفارش شما با شماره {order.OrderNumber} ثبت شد.";

        //    // هدایت به صفحه جزئیات سفارش
        //    return RedirectToAction("OrderDetails", "Orders", new { id = order.Id });
        //}

        // POST: ایجاد سفارش
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

            // گرفتن کاربر جاری
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // ایجاد سفارش جدید
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.Sum(item => item.Total),
                    PaymentStatus = PaymentStatus.Pending,
                    Status = "در انتظار پرداخت",
                    UserIP = HttpContext.Connection.RemoteIpAddress?.ToString()
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

                TempData["SuccessMessage"] = $"✅ سفارش شما با شماره {order.OrderNumber} ثبت شد.";

                // هدایت به صفحه جزئیات سفارش
                return RedirectToAction("OrderDetails", "Orders", new { id = order.Id });
            }
            catch (Exception ex)
            {
                // لاگ خطا
                Console.WriteLine($"Error creating order: {ex.Message}");
                TempData["ErrorMessage"] = "خطا در ایجاد سفارش. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction(nameof(Checkout));
            }
        }

        // Helper Methods
        private List<ShoppingCartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("ShoppingCart");

            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<ShoppingCartItem>();
            }

            return JsonSerializer.Deserialize<List<ShoppingCartItem>>(cartJson)
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
    }
}