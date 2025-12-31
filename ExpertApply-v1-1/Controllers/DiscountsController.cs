using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;
using Rexplor.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Rexplor.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    //[Area("Admin")]
    public class DiscountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDiscountService _discountService;

        public DiscountsController(ApplicationDbContext context, IDiscountService discountService)
        {
            _context = context;
            _discountService = discountService;
        }

        // GET: Admin/Discounts
        public async Task<IActionResult> Index()
        {
            var discounts = await _context.Discounts
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            return View(discounts);
        }

        // GET: Admin/Discounts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var discount = await _context.Discounts
                .Include(d => d.FileDiscounts)
                .ThenInclude(fd => fd.File)
                .Include(d => d.DiscountUsages)
                .ThenInclude(du => du.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (discount == null)
            {
                return NotFound();
            }

            return View(discount);
        }

        // GET: Admin/Discounts/Create
        public async Task<IActionResult> Create()
        {
            // لیست فایل‌ها برای انتخاب
            ViewBag.Files = await _context.DataFiles
                .Where(f => f.IsActive)
                .OrderBy(f => f.Title)
                .Select(f => new { Id = f.Id, Title = f.Title })
                .ToListAsync();

            var discount = new Discount
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                IsActive = true,
                IsForAllFiles = true
            };

            return View(discount);
        }

        // POST: Admin/Discounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Discount discount, List<int>? SelectedFileIds)
        {
            if (ModelState.IsValid)
            {
                // بررسی تکراری نبودن کد
                var existingCode = await _context.Discounts
                    .AnyAsync(d => d.Code.ToUpper() == discount.Code.ToUpper());

                if (existingCode)
                {
                    ModelState.AddModelError("Code", "این کد تخفیف قبلاً ثبت شده است");
                    ViewBag.Files = await _context.DataFiles
                        .Where(f => f.IsActive)
                        .OrderBy(f => f.Title)
                        .Select(f => new { Id = f.Id, Title = f.Title })
                        .ToListAsync();
                    return View(discount);
                }

                // تنظیم کد به uppercase
                discount.Code = discount.Code.ToUpper();

                // ذخیره تخفیف
                _context.Discounts.Add(discount);
                await _context.SaveChangesAsync();

                // اگر برای همه فایل‌ها نیست، فایل‌های انتخابی را اضافه کنیم
                if (!discount.IsForAllFiles && SelectedFileIds != null && SelectedFileIds.Any())
                {
                    foreach (var fileId in SelectedFileIds)
                    {
                        var fileDiscount = new FileDiscount
                        {
                            DiscountId = discount.Id,
                            FileId = fileId
                        };
                        _context.FileDiscounts.Add(fileDiscount);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "کد تخفیف با موفقیت ایجاد شد";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Files = await _context.DataFiles
                .Where(f => f.IsActive)
                .OrderBy(f => f.Title)
                .Select(f => new { Id = f.Id, Title = f.Title })
                .ToListAsync();
            return View(discount);
        }

        // GET: Admin/Discounts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var discount = await _context.Discounts
                .Include(d => d.FileDiscounts)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (discount == null)
            {
                return NotFound();
            }

            // لیست فایل‌ها
            ViewBag.Files = await _context.DataFiles
                .Where(f => f.IsActive)
                .OrderBy(f => f.Title)
                .Select(f => new { Id = f.Id, Title = f.Title })
                .ToListAsync();

            // فایل‌های انتخابی
            ViewBag.SelectedFileIds = discount.FileDiscounts.Select(fd => fd.FileId).ToList();

            return View(discount);
        }

        // POST: Admin/Discounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Discount discount, List<int>? SelectedFileIds)
        {
            if (id != discount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // بررسی تکراری نبودن کد (به جز خودش)
                    var existingCode = await _context.Discounts
                        .AnyAsync(d => d.Code.ToUpper() == discount.Code.ToUpper() && d.Id != id);

                    if (existingCode)
                    {
                        ModelState.AddModelError("Code", "این کد تخفیف قبلاً ثبت شده است");
                        ViewBag.Files = await _context.DataFiles
                            .Where(f => f.IsActive)
                            .OrderBy(f => f.Title)
                            .Select(f => new { Id = f.Id, Title = f.Title })
                            .ToListAsync();
                        ViewBag.SelectedFileIds = SelectedFileIds;
                        return View(discount);
                    }

                    // تنظیم کد به uppercase
                    discount.Code = discount.Code.ToUpper();

                    // آپدیت تخفیف
                    _context.Discounts.Update(discount);
                    await _context.SaveChangesAsync();

                    // حذف فایل‌های قبلی و اضافه کردن جدید
                    if (!discount.IsForAllFiles)
                    {
                        // حذف ارتباطات قبلی
                        var existingFileDiscounts = await _context.FileDiscounts
                            .Where(fd => fd.DiscountId == id)
                            .ToListAsync();
                        _context.FileDiscounts.RemoveRange(existingFileDiscounts);

                        // اضافه کردن فایل‌های جدید
                        if (SelectedFileIds != null && SelectedFileIds.Any())
                        {
                            foreach (var fileId in SelectedFileIds)
                            {
                                var fileDiscount = new FileDiscount
                                {
                                    DiscountId = discount.Id,
                                    FileId = fileId
                                };
                                _context.FileDiscounts.Add(fileDiscount);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "کد تخفیف با موفقیت ویرایش شد";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiscountExists(discount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Files = await _context.DataFiles
                .Where(f => f.IsActive)
                .OrderBy(f => f.Title)
                .Select(f => new { Id = f.Id, Title = f.Title })
                .ToListAsync();
            ViewBag.SelectedFileIds = SelectedFileIds;
            return View(discount);
        }

        // GET: Admin/Discounts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }

            return View(discount);
        }

        // POST: Admin/Discounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount != null)
            {
                _context.Discounts.Remove(discount);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "کد تخفیف با موفقیت حذف شد";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Discounts/Stats
        public async Task<IActionResult> Stats()
        {
            var stats = new
            {
                TotalDiscounts = await _context.Discounts.CountAsync(),
                ActiveDiscounts = await _context.Discounts
                    .Where(d => d.IsActive && d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now)
                    .CountAsync(),
                ExpiredDiscounts = await _context.Discounts
                    .Where(d => d.EndDate < DateTime.Now)
                    .CountAsync(),
                TotalUsage = await _context.DiscountUsages.CountAsync(),
                TotalDiscountAmount = await _context.DiscountUsages.SumAsync(du => du.DiscountAmount)
            };

            var recentUsages = await _context.DiscountUsages
                .Include(du => du.User)
                .Include(du => du.Discount)
                .Include(du => du.File)
                .OrderByDescending(du => du.UsedAt)
                .Take(20)
                .ToListAsync();

            ViewBag.RecentUsages = recentUsages;

            return View(stats);
        }

        private bool DiscountExists(int id)
        {
            return _context.Discounts.Any(e => e.Id == id);
        }
    }
}