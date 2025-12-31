// Controllers/CategoriesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;

namespace Rexplor.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DataFileCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataFileCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: لیست دسته‌بندی‌ها
        public async Task<IActionResult> Index()
        {
            var categories = await _context.DataFileCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            // محاسبه تعداد فایل‌های هر دسته
            foreach (var category in categories)
            {
                category.FileCount = await _context.DataFiles
                    .CountAsync(f => f.CategoryId == category.Id && f.IsActive);
            }

            return View(categories);
        }

        // GET: /Categories/Details/5 - جزئیات دسته‌بندی
        //public async Task<IActionResult> Details(int id)
        //{
        //    var category = await _context.DataFileCategories
        //        .Include(c => c.DataFiles.Where(f => f.IsActive))
        //        .ThenInclude(f => f.Category)
        //        .FirstOrDefaultAsync(c => c.Id == id);

        //    if (category == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(category);
        //}

        // GET: /Categories/Details/5 - جزئیات دسته‌بندی
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.DataFileCategories
                .Include(c => c.DataFiles.Where(f => f.IsActive)) // فقط فایل‌های فعال
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: /Categories/Create - فرم ایجاد دسته‌بندی
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create - ذخیره دسته‌بندی جدید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DataFileCategory category)
        {
            //if (ModelState.IsValid)
            //{
            // تنظیم مقادیر پیش‌فرض
            if (string.IsNullOrEmpty(category.IconClass))
            {
                category.IconClass = "bi-folder";
            }
            category.IconClass = "";
            _context.Add(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ دسته‌بندی با موفقیت ایجاد شد.";
            return RedirectToAction(nameof(Index));
            //}

            return View(category);
        }

        // GET: /Categories/Edit/5 - فرم ویرایش دسته‌بندی
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.DataFileCategories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: /Categories/Edit/5 - ذخیره ویرایش دسته‌بندی
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, DataFileCategory category)
        //{
        //    if (id != category.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(category);
        //            await _context.SaveChangesAsync();

        //            TempData["SuccessMessage"] = "✅ تغییرات با موفقیت ذخیره شد.";
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!CategoryExists(category.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(category);
        //}

        // POST: /Categories/Edit/5 - ذخیره ویرایش دسته‌بندی
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DataFileCategory category)
        {
            // حذف خطاهای اعتبارسنجی برای DataFiles
            ModelState.Remove("DataFiles");
            ModelState.Remove("DataFiles.DataFileCategory");
            ModelState.Remove("DataFiles.CategoryId");

            // همچنین می‌توانید تمام کلیدهای مربوط به DataFiles را پیدا و حذف کنید
            var keysToRemove = ModelState.Keys
                .Where(key => key.StartsWith("DataFiles"))
                .ToList();

            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _context.DataFileCategories.FindAsync(id);

                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    // به‌روزرسانی فقط فیلدهای مستقیم
                    existingCategory.Name = category.Name;
                    existingCategory.Description = category.Description;
                    existingCategory.IconClass = category.IconClass;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✅ تغییرات با موفقیت ذخیره شد.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(id))
                        return NotFound();
                    throw;
                }
            }

            // نمایش خطاها برای دیباگ
            Console.WriteLine("Validation errors after removing DataFiles:");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"- {error.ErrorMessage}");
            }

            return View(category);
        }
        // GET: /Categories/Delete/5 - فرم تأیید حذف
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.DataFileCategories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            // بررسی اینکه آیا فایلی در این دسته‌بندی وجود دارد
            var hasFiles = await _context.DataFiles
                .AnyAsync(f => f.CategoryId == id);

            ViewBag.HasFiles = hasFiles;
            ViewBag.FileCount = hasFiles ?
                await _context.DataFiles.CountAsync(f => f.CategoryId == id) : 0;

            return View(category);
        }

        // POST: /Categories/Delete/5 - حذف نهایی
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.DataFileCategories.FindAsync(id);

            if (category != null)
            {
                // بررسی اینکه دسته‌بندی خالی باشد
                var hasFiles = await _context.DataFiles
                    .AnyAsync(f => f.CategoryId == id);

                if (hasFiles)
                {
                    TempData["ErrorMessage"] = "❌ نمی‌توان دسته‌بندی‌ای که فایل دارد را حذف کرد. ابتدا فایل‌های این دسته را حذف یا انتقال دهید.";
                    return RedirectToAction(nameof(Index));
                }

                _context.DataFileCategories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ دسته‌بندی با موفقیت حذف شد.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.DataFileCategories.Any(e => e.Id == id);
        }
    }
}
