using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;
using Rexplor.Models;
using Rexplor.Models.Main_Page;
using System.Diagnostics;

namespace Rexplor.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var images = _context.MainPageSliderImages.ToList(); // Retrieve all images from the database
            var voices = _context.MainPageVoices.ToList(); // Retrieve all images from the database
            AchievementsAndFeedbacks af = new AchievementsAndFeedbacks();
            af.Images = images;
            af.Voices = voices;
            var viewModel = new HomeViewModel
            {
                AchievementsAndFeedbacks = af,  
                // جدیدترین فایل‌ها
                LatestFiles = await _context.DataFiles
                    .Include(f => f.Category)
                    .Where(f => f.IsActive)
                    .OrderByDescending(f => f.Price)
                    .Take(6)
                    .ToListAsync(),

                // پربازدیدترین فایل‌ها
                PopularFiles = await _context.DataFiles
                    .Include(f => f.Category)
                    .Where(f => f.IsActive)
                    .OrderByDescending(f => f.ViewCount)
                    .Take(6)
                    .ToListAsync(),

                // دسته‌بندی‌ها با تعداد فایل
                Categories = await _context.DataFileCategories
                    .Select(c => new CategoryWithCount
                    {
                        Category = c,
                        FileCount = _context.DataFiles
                            .Count(f => f.CategoryId == c.Id && f.IsActive)
                    })
                    .Where(x => x.FileCount > 0)
                    .OrderByDescending(x => x.FileCount)
                    .Take(8)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    

        //public IActionResult Index()
        //{

        //    var images = _context.MainPageSliderImages.ToList(); // Retrieve all images from the database
        //    var voices = _context.MainPageVoices.ToList(); // Retrieve all images from the database
        //    AchievementsAndFeedbacks af = new AchievementsAndFeedbacks();
        //    af.Images = images;
        //    af.Voices = voices;
        //    return View(af); // Pass the images to the view
        //}

        [HttpPost]
        public IActionResult Contact(string name, string email, string message)
        {
            // Logic to handle the contact form submission.
            ViewBag.Message = "Your message has been sent!";
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
