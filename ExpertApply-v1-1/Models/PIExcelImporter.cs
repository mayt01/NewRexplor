using OfficeOpenXml;
using Rexplor.Data;
using Rexplor.Models;
using System.IO;

namespace Rexplor.Models
{
    public class PIExcelImporter
    {
        private readonly ApplicationDbContext _context;

        public PIExcelImporter(ApplicationDbContext context)
        {
            _context = context;
        }

        public void ImportFromExcel()
        {
            // مسیر فایل Excel
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PIData.xlsx");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // حتماً اضافه کنید
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // اولین شیت
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // سطر اول Header است
                {
                    var pi = new PI
                    {
                        UniversityName = worksheet.Cells[row, 1].Text, // University
                        PIName = worksheet.Cells[row, 2].Text,         // Professor
                        GoogleScholarUrl = worksheet.Cells[row, 3].Text,
                        WebsiteUrl = worksheet.Cells[row, 4].Text,
                        Email = worksheet.Cells[row, 5].Text,
                        ResearchField = worksheet.Cells[row, 6].Text
                    };

                    _context.PIs.Add(pi);
                }

                _context.SaveChanges();
            }
        }
    }
}
