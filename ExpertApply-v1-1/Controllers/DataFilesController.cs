// Controllers/DataFilesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;
using System.Text.Json;

namespace Rexplor.Controllers
{
    public class DataFilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public DataFilesController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /DataFiles - نمایش همه فایل‌ها
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var dataFiles = await _context.DataFiles
                .Include(f => f.Category)
                .Where(f => f.IsActive)
                .OrderByDescending(f => f.UpdatedDate)
                .ToListAsync();

            return View(dataFiles);
        }

        // GET: /DataFiles/Category/{id} - نمایش بر اساس دسته‌بندی
        [AllowAnonymous]
        public async Task<IActionResult> Category(int id)
        {
            var category = await _context.DataFileCategories
                .Include(c => c.DataFiles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            ViewBag.CategoryName = category.Name;
            return View("Index", category.DataFiles.Where(f => f.IsActive).ToList());
        }

        // GET: /DataFiles/Details/{id} - نمایش جزئیات فایل
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var dataFile = await _context.DataFiles
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == id && f.IsActive);

            if (dataFile == null)
            {
                return NotFound();
            }

            // اگر کاربر لاگین کرده، سبد خریدش را بگیر
            List<ShoppingCartItem> userCart = new();
            if (User.Identity.IsAuthenticated)
            {
                var cartJson = HttpContext.Session.GetString("ShoppingCart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    userCart = JsonSerializer.Deserialize<List<ShoppingCartItem>>(cartJson)
                              ?? new List<ShoppingCartItem>();
                }
            }

            ViewBag.UserCart = userCart;

            // افزایش تعداد بازدید
            dataFile.ViewCount++;
            _context.Update(dataFile);
            await _context.SaveChangesAsync();

            return View(dataFile);
        }

        // GET: /DataFiles/Create - فرم آپلود فایل جدید
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.DataFileCategories.ToListAsync();
            return View();
        }

        // POST: /DataFiles/Create - ذخیره فایل جدید
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DataFileViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // آپلود فایل
                    // آپلود فایل
                    string fileName = null;
                    string filePath = null;

                    if (model.UploadedFile != null && model.UploadedFile.Length > 0)
                    {
                        // ایجاد پوشه آپلود اگر وجود ندارد
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "datafiles");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // تولید نام یکتا برای فایل
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.UploadedFile.FileName);
                        filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // ذخیره فایل
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.UploadedFile.CopyToAsync(fileStream);
                        }

                        fileName = uniqueFileName;
                    }

                    // ایجاد شیء DataFile
                    var dataFile = new DataFile
                    {
                        Title = model.Title,
                        Description = model.Description,
                        Price = model.Price,
                        Category = _context.DataFileCategories.Where(c => c.Id == model.CategoryId).FirstOrDefault(),
                        CategoryId = model.CategoryId,
                        Tags = model.Tags,
                        IsActive = model.IsActive,
                        FilePath = filePath,
                        FileSize = model.UploadedFile?.Length ?? 0,
                        FileExtension = Path.GetExtension(model.UploadedFile?.FileName ?? ""),
                        UpdatedDate = DateTime.Now
                    };

                    _context.Add(dataFile);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "فایل با موفقیت آپلود شد.";
                    return RedirectToAction(nameof(Details), new { id = dataFile.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "خطا در آپلود فایل: " + ex.Message);
                }
            }

            ViewBag.Categories = await _context.DataFileCategories.ToListAsync();
            return View(model);
        }


        // GET: ویرایش فایل
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var dataFile = await _context.DataFiles.FindAsync(id);

            if (dataFile == null)
            {
                return NotFound();
            }

            var model = new EditDataFileViewModel
            {
                Id = dataFile.Id,
                Title = dataFile.Title,
                Description = dataFile.Description,
                Price = dataFile.Price,
                CategoryId = dataFile.CategoryId,
                Tags = dataFile.Tags,
                IsActive = dataFile.IsActive
            };

            ViewBag.Categories = await _context.DataFileCategories.ToListAsync();
            return View(model);
        }

        // POST: ویرایش فایل
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditDataFileViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var dataFile = await _context.DataFiles.FindAsync(id);

            if (dataFile == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // آپلود فایل جدید (اگر ارسال شده)
                    if (model.NewFile != null && model.NewFile.Length > 0)
                    {
                        // اعتبارسنجی فایل جدید
                        var allowedExtensions = new[] { ".xlsx", ".xls", ".csv" };
                        var fileExtension = Path.GetExtension(model.NewFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("NewFile",
                                $"فقط فایل‌های {string.Join(", ", allowedExtensions)} مجاز هستند");
                            ViewBag.Categories = await _context.DataFileCategories.ToListAsync();
                            return View(model);
                        }

                        // حذف فایل قبلی
                        if (System.IO.File.Exists(dataFile.FilePath))
                        {
                            System.IO.File.Delete(dataFile.FilePath);
                        }

                        // آپلود فایل جدید
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "datafiles");
                        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                        var newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(newFilePath, FileMode.Create))
                        {
                            await model.NewFile.CopyToAsync(stream);
                        }

                        dataFile.FilePath = newFilePath;
                        dataFile.FileSize = model.NewFile.Length;
                        dataFile.FileExtension = fileExtension;
                    }

                    // بروزرسانی اطلاعات
                    dataFile.Title = model.Title.Trim();
                    dataFile.Description = model.Description.Trim();
                    dataFile.Price = model.Price;
                    dataFile.CategoryId = model.CategoryId;
                    dataFile.Tags = string.IsNullOrWhiteSpace(model.Tags) ? null : model.Tags.Trim();
                    dataFile.IsActive = model.IsActive;
                    dataFile.UpdatedDate = DateTime.Now;

                    _context.Update(dataFile);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✅ تغییرات با موفقیت ذخیره شد.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"خطا: {ex.Message}");
                }
            }

            ViewBag.Categories = await _context.DataFileCategories.ToListAsync();
            return View(model);
        }

        // GET: تأیید حذف
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var dataFile = await _context.DataFiles
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (dataFile == null)
            {
                return NotFound();
            }

            return View(dataFile);
        }

        // POST: حذف قطعی
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dataFile = await _context.DataFiles.FindAsync(id);

            if (dataFile != null)
            {
                try
                {
                    // حذف فایل فیزیکی
                    if (System.IO.File.Exists(dataFile.FilePath))
                    {
                        System.IO.File.Delete(dataFile.FilePath);
                    }

                    // حذف از دیتابیس
                    _context.DataFiles.Remove(dataFile);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "✅ فایل با موفقیت حذف شد.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"❌ خطا در حذف فایل: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /DataFiles/Search - جستجوی فایل‌ها
        //[AllowAnonymous]
        //public async Task<IActionResult> Search(string query)
        //{
        //    if (string.IsNullOrWhiteSpace(query))
        //    {
        //        return RedirectToAction("Index");
        //    }

        //    var results = await _context.DataFiles
        //        .Include(f => f.Category)
        //        .Where(f => f.IsActive &&
        //            (f.Title.Contains(query) ||
        //             f.Description.Contains(query) ||
        //             f.Tags.Contains(query)))
        //        .OrderByDescending(f => f.UpdatedDate)
        //        .ToListAsync();

        //    ViewBag.SearchQuery = query;
        //    return View("Index", results);
        //}

        // GET: جستجوی فایل‌ها
        //[AllowAnonymous]
        //public async Task<IActionResult> Search(string query, int? categoryId,
        //    decimal? minPrice, decimal? maxPrice, string sortBy = "newest")
        //{
        //    var filesQuery = _context.DataFiles
        //        .Include(f => f.Category)
        //        .Where(f => f.IsActive);

        //    // فیلتر بر اساس جستجو
        //    if (!string.IsNullOrWhiteSpace(query))
        //    {
        //        filesQuery = filesQuery.Where(f =>
        //            f.Title.Contains(query) ||
        //            f.Description.Contains(query) ||
        //            f.Tags.Contains(query));
        //    }

        //    // فیلتر بر اساس دسته‌بندی
        //    if (categoryId.HasValue)
        //    {
        //        filesQuery = filesQuery.Where(f => f.CategoryId == categoryId.Value);
        //    }

        //    // فیلتر بر اساس قیمت
        //    if (minPrice.HasValue)
        //    {
        //        filesQuery = filesQuery.Where(f => f.Price >= minPrice.Value);
        //    }

        //    if (maxPrice.HasValue)
        //    {
        //        filesQuery = filesQuery.Where(f => f.Price <= maxPrice.Value);
        //    }

        //    // مرتب‌سازی
        //    filesQuery = sortBy switch
        //    {
        //        "price-asc" => filesQuery.OrderBy(f => f.Price),
        //        "price-desc" => filesQuery.OrderByDescending(f => f.Price),
        //        "popular" => filesQuery.OrderByDescending(f => f.DownloadCount),
        //        "rating" => filesQuery.OrderByDescending(f => f.Rating),
        //        _ => filesQuery.OrderByDescending(f => f.Price) // جدیدترین
        //    };

        //    var files = await filesQuery.ToListAsync();

        //    ViewBag.SearchQuery = query;
        //    ViewBag.CategoryId = categoryId;
        //    ViewBag.MinPrice = minPrice;
        //    ViewBag.MaxPrice = maxPrice;
        //    ViewBag.SortBy = sortBy;
        //    ViewBag.Categories = await _context.DataFileCategories.ToListAsync();

        //    return View("Index", files);
        //}

        // GET: جستجوی پیشرفته
        //[AllowAnonymous]
        //public async Task<IActionResult> Search(
        //    string query = "",
        //    int? categoryId = null,
        //    decimal? minPrice = null,
        //    decimal? maxPrice = null,
        //    string sortBy = "newest",
        //    int page = 1,
        //    int pageSize = 12)
        //{
        //    var filesQuery = _context.DataFiles
        //        .Include(f => f.Category)
        //        .Where(f => f.IsActive);

        //    // جستجوی متنی
        //    if (!string.IsNullOrWhiteSpace(query))
        //    {
        //        filesQuery = filesQuery.Where(f =>
        //            f.Title.Contains(query) ||
        //            f.Description.Contains(query) ||
        //            f.Tags.Contains(query));
        //    }

        //    // فیلتر دسته‌بندی
        //    if (categoryId.HasValue && categoryId > 0)
        //    {
        //        filesQuery = filesQuery.Where(f => f.CategoryId == categoryId.Value);
        //    }

        //    // فیلتر قیمت
        //    if (minPrice.HasValue)
        //    {
        //        filesQuery = filesQuery.Where(f => f.Price >= minPrice.Value);
        //    }

        //    if (maxPrice.HasValue)
        //    {
        //        filesQuery = filesQuery.Where(f => f.Price <= maxPrice.Value);
        //    }

        //    // مرتب‌سازی
        //    filesQuery = sortBy switch
        //    {
        //        "price-asc" => filesQuery.OrderBy(f => f.Price),
        //        "price-desc" => filesQuery.OrderByDescending(f => f.Price),
        //        "popular" => filesQuery.OrderByDescending(f => f.DownloadCount),
        //        "views" => filesQuery.OrderByDescending(f => f.ViewCount),
        //        "rating" => filesQuery.OrderByDescending(f => f.Rating),
        //        _ => filesQuery.OrderByDescending(f => f.Price)
        //    };

        //    // Pagination
        //    var totalItems = await filesQuery.CountAsync();
        //    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        //    var files = await filesQuery
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    // ViewData برای فیلترها
        //    ViewBag.SearchQuery = query;
        //    ViewBag.CategoryId = categoryId;
        //    ViewBag.MinPrice = minPrice;
        //    ViewBag.MaxPrice = maxPrice;
        //    ViewBag.SortBy = sortBy;
        //    ViewBag.CurrentPage = page;
        //    ViewBag.TotalPages = totalPages;
        //    ViewBag.PageSize = pageSize;
        //    ViewBag.TotalItems = totalItems;

        //    // لیست دسته‌بندی‌ها برای dropdown
        //    ViewBag.Categories = await _context.DataFileCategories
        //        .Where(c => c.Id > 0)
        //        .ToListAsync();

        //    return View("Index", files);
        //}

        // GET: جستجوی پیشرفته
        [AllowAnonymous]
        public async Task<IActionResult> Search(
            string query = "",
            string uniqueCode = "", // ✅ اضافه شد
            int? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string sortBy = "newest",
            bool? isActive = null, // ✅ اضافه شد
            int page = 1,
            int pageSize = 12)
        {
            var filesQuery = _context.DataFiles
                .Include(f => f.Category)
                .AsQueryable(); // به Where شرط IsActive را در ادامه اضافه می‌کنیم

            // ✅ فیلتر جستجوی کد یکتا (اولویت اول)
            if (!string.IsNullOrWhiteSpace(uniqueCode))
            {
                // تبدیل به uppercase برای جستجوی بدون حساسیت به بزرگی/کوچکی حروف
                var codeUpper = uniqueCode.Trim().ToUpper();

                // اگر کد با DF شروع نشده، آن را اضافه کن
                if (!codeUpper.StartsWith("DF") && codeUpper.Length > 0)
                {
                    codeUpper = "DF" + codeUpper;
                }

                filesQuery = filesQuery.Where(f => f.UniqueCode.Contains(codeUpper));
            }

            // ✅ فیلتر وضعیت فعال/غیرفعال
            if (isActive.HasValue)
            {
                filesQuery = filesQuery.Where(f => f.IsActive == isActive.Value);
            }
            else
            {
                // پیش‌فرض: فقط فایل‌های فعال
                filesQuery = filesQuery.Where(f => f.IsActive);
            }

            // جستجوی متنی (اگر کد وارد نشده باشد)
            if (!string.IsNullOrWhiteSpace(query) && string.IsNullOrWhiteSpace(uniqueCode))
            {
                filesQuery = filesQuery.Where(f =>
                    f.Title.Contains(query) ||
                    f.Description.Contains(query) ||
                    f.Tags.Contains(query));
            }

            // فیلتر دسته‌بندی
            if (categoryId.HasValue && categoryId > 0)
            {
                filesQuery = filesQuery.Where(f => f.CategoryId == categoryId.Value);
            }

            // فیلتر قیمت
            if (minPrice.HasValue)
            {
                filesQuery = filesQuery.Where(f => f.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                filesQuery = filesQuery.Where(f => f.Price <= maxPrice.Value);
            }

            // مرتب‌سازی
            filesQuery = sortBy switch
            {
                "oldest" => filesQuery.OrderBy(f => f.UpdatedDate),
                "price-asc" => filesQuery.OrderBy(f => f.Price),
                "price-desc" => filesQuery.OrderByDescending(f => f.Price),
                "popular" => filesQuery.OrderByDescending(f => f.DownloadCount),
                "views" => filesQuery.OrderByDescending(f => f.ViewCount),
                "downloads" => filesQuery.OrderByDescending(f => f.DownloadCount),
                "rating" => filesQuery.OrderByDescending(f => f.Rating),
                "title-asc" => filesQuery.OrderBy(f => f.Title),
                "title-desc" => filesQuery.OrderByDescending(f => f.Title),
                _ => filesQuery.OrderByDescending(f => f.UpdatedDate) // newest
            };

            // Pagination
            var totalItems = await filesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var files = await filesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ViewData برای فیلترها
            ViewBag.SearchQuery = query;
            ViewBag.SearchUniqueCode = uniqueCode; // ✅ ذخیره کد جستجو شده
            ViewBag.CategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;
            ViewBag.IsActive = isActive; // ✅ ذخیره وضعیت
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            // لیست دسته‌بندی‌ها برای dropdown
            ViewBag.Categories = await _context.DataFileCategories
                .Where(c => c.Id > 0)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // ✅ نمایش پیام اگر جستجوی کد نتیجه نداشت
            if (!string.IsNullOrWhiteSpace(uniqueCode) && totalItems == 0)
            {
                TempData["WarningMessage"] = $"دیتافایلی با کد '{uniqueCode}' یافت نشد.";
            }

            return View("Index", files);
        }

        // GET: فایل‌های یک دسته‌بندی خاص
        [AllowAnonymous]
        public async Task<IActionResult> Category(int id, int page = 1)
        {
            var category = await _context.DataFileCategories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var files = await _context.DataFiles
                .Include(f => f.Category)
                .Where(f => f.CategoryId == id && f.IsActive)
                .OrderByDescending(f => f.Price)
                .ToListAsync();

            ViewBag.CategoryName = category.Name;
            ViewBag.CategoryId = id;

            return View("Index", files);
        }
    }
}