using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Rexplor.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserDashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: داشبورد کاربر
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var dashboard = new UserDashboardViewModel
            {
                User = user,

                // آمار سفارش‌ها
                TotalOrders = await _context.Orders
                    .CountAsync(o => o.UserId == user.Id),

                PaidOrders = await _context.Orders
                    .CountAsync(o => o.UserId == user.Id && o.IsPaid),

                PendingOrders = await _context.Orders
                    .CountAsync(o => o.UserId == user.Id && !o.IsPaid),

                TotalSpent = await _context.Orders
                    .Where(o => o.UserId == user.Id && o.IsPaid)
                    .SumAsync(o => o.TotalAmount),

                // آخرین سفارش‌ها
                RecentOrders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.UserId == user.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync(),

                // فایل‌های خریداری شده
                PurchasedFiles = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.DataFile)
                    .ThenInclude(df => df.Category)
                    .Where(oi => oi.Order.UserId == user.Id && oi.Order.IsPaid)
                    .Select(oi => oi.DataFile)
                    .Distinct()
                    .Take(10)
                    .ToListAsync(),

                // آخرین دانلودها
                RecentDownloads = await _context.OrderItems
                    .Include(oi => oi.DataFile)
                    .Include(oi => oi.Order)
                    .Where(oi => oi.Order.UserId == user.Id && oi.Order.IsPaid)
                    .OrderByDescending(oi => oi.Order.PaymentDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboard);
        }

        // GET: پروفایل کاربر
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        // GET: تنظیمات حساب کاربری
        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }
    }

    // ViewModel برای داشبورد کاربر
    public class UserDashboardViewModel
    {
        public IdentityUser User { get; set; }
        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<DataFile> PurchasedFiles { get; set; }
        public List<OrderItem> RecentDownloads { get; set; }
    }
}