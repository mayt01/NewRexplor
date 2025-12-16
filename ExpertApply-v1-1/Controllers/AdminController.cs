using Microsoft.AspNetCore.Mvc;
using Rexplor.Data;
using Rexplor.Models;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.ComponentModel;

namespace Rexplor.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Action برای Import فایل Excel به جدول PI
        [HttpGet]
        public IActionResult ImportPI()
        {
            // مسیر فایل داخل wwwroot
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PIData.xlsx");

            if (!System.IO.File.Exists(filePath))
            {
                return Content("Excel file not found!");
            }

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // اولین شیت
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // سطر اول Header است
                {
                    var email = worksheet.Cells[row, 5].Text;

                    // بررسی رکورد تکراری بر اساس Email
                    if (_context.PIs.Any(p => p.Email == email))
                        continue;

                    var pi = new PI
                    {
                        UniversityName = worksheet.Cells[row, 1].Text,
                        PIName = worksheet.Cells[row, 2].Text,
                        GoogleScholarUrl = worksheet.Cells[row, 3].Text,
                        WebsiteUrl = worksheet.Cells[row, 4].Text,
                        Email = email,
                        ResearchField = worksheet.Cells[row, 6].Text
                    };

                    _context.PIs.Add(pi);
                }

                _context.SaveChanges();
            }

            return Content("PI data imported successfully!");
        }
    }
}

