// Controllers/DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;

namespace Rexplor.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // GET: داشبورد اصلی
        public async Task<IActionResult> Index()
        {
            var dashboard = new DashboardViewModel
            {
                // آمار کلی
                TotalFiles = await _context.DataFiles.CountAsync(),
                ActiveFiles = await _context.DataFiles.CountAsync(f => f.IsActive),
                TotalCategories = await _context.DataFileCategories.CountAsync(),
                //ActiveCategories = await _context.DataFileCategories.CountAsync(c => c.IsActive),
                
                // آمار روزانه                
                TodayViews = await _context.DataFiles
                    .SumAsync(f => f.ViewCount),
                TodayDownloads = await _context.DataFiles
                    .SumAsync(f => f.DownloadCount),
                
                // جدیدترین فایل‌ها
                RecentFiles = await _context.DataFiles
                    .Include(f => f.Category)
                    .Where(f => f.IsActive)
                    .OrderByDescending(f => f.Price)
                    .Take(5)
                    .ToListAsync(),
                
                // پربازدیدترین فایل‌ها
                PopularFiles = await _context.DataFiles
                    .Include(f => f.Category)
                    .Where(f => f.IsActive)
                    .OrderByDescending(f => f.ViewCount)
                    .Take(5)
                    .ToListAsync(),
                
                // دسته‌بندی‌ها با بیشترین فایل
                TopCategories = await _context.DataFileCategories
                    .Select(c => new CategoryStats
                    {
                        Category = c,
                        FileCount = _context.DataFiles.Count(f => f.CategoryId == c.Id && f.IsActive),
                        TotalViews = _context.DataFiles.Where(f => f.CategoryId == c.Id).Sum(f => f.ViewCount),
                        TotalDownloads = _context.DataFiles.Where(f => f.CategoryId == c.Id).Sum(f => f.DownloadCount)
                    })
                    .Where(x => x.FileCount > 0)
                    .OrderByDescending(x => x.FileCount)
                    .Take(5)
                    .ToListAsync()
            };
            
            return View(dashboard);
        }
        
        // GET: گزارش فایل‌ها
        public async Task<IActionResult> FilesReport(string period = "monthly")
        {
            var files = await _context.DataFiles
                .Include(f => f.Category)
                .OrderByDescending(f => f.Price)
                .ToListAsync();
            
            ViewBag.Period = period;
            return View(files);
        }
        
        // GET: گزارش مالی
        public async Task<IActionResult> FinancialReport()
        {
            var financialData = new FinancialReportViewModel
            {
                TotalRevenue = await _context.DataFiles.SumAsync(f => f.Price * f.DownloadCount),
                TotalDownloads = await _context.DataFiles.SumAsync(f => f.DownloadCount),
                AveragePrice = await _context.DataFiles.AverageAsync(f => f.Price),
                
                TopSellingFiles = await _context.DataFiles
                    .Include(f => f.Category)
                    .OrderByDescending(f => f.DownloadCount)
                    .Take(10)
                    .Select(f => new FileSales
                    {
                        File = f,
                        Revenue = f.Price * f.DownloadCount
                    })
                    .ToListAsync(),
                
                RevenueByCategory = await _context.DataFileCategories
                    .Select(c => new CategoryRevenue
                    {
                        Category = c,
                        FileCount = _context.DataFiles.Count(f => f.CategoryId == c.Id),
                        TotalDownloads = _context.DataFiles.Where(f => f.CategoryId == c.Id).Sum(f => f.DownloadCount),
                        TotalRevenue = _context.DataFiles.Where(f => f.CategoryId == c.Id).Sum(f => f.Price * f.DownloadCount)
                    })
                    .Where(x => x.FileCount > 0)
                    .OrderByDescending(x => x.TotalRevenue)
                    .ToListAsync()
            };
            
            return View(financialData);
        }
        
        // GET: تنظیمات
        public IActionResult Settings()
        {
            return View();
        }
    }
    
    // ViewModel‌ها
    public class DashboardViewModel
    {
        public int TotalFiles { get; set; }
        public int ActiveFiles { get; set; }
        public int TotalCategories { get; set; }
        public int TodayViews { get; set; }
        public int TodayDownloads { get; set; }
        public List<DataFile> RecentFiles { get; set; }
        public List<DataFile> PopularFiles { get; set; }
        public List<CategoryStats> TopCategories { get; set; }
    }
    
    public class CategoryStats
    {
        public DataFileCategory Category { get; set; }
        public int FileCount { get; set; }
        public int TotalViews { get; set; }
        public int TotalDownloads { get; set; }
    }
    
    public class FinancialReportViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalDownloads { get; set; }
        public decimal AveragePrice { get; set; }
        public List<FileSales> TopSellingFiles { get; set; }
        public List<CategoryRevenue> RevenueByCategory { get; set; }
    }
    
    public class FileSales
    {
        public DataFile File { get; set; }
        public decimal Revenue { get; set; }
    }
    
    public class CategoryRevenue
    {
        public DataFileCategory Category { get; set; }
        public int FileCount { get; set; }
        public int TotalDownloads { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}