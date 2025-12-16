// Extensions/FileExtensions.cs
using System.Text.RegularExpressions;

namespace Rexplor.Extensions
{
    public static class FileExtensions
    {
        public static string ToFileSize(this long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public static bool IsValidExcelFile(this IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var allowedExtensions = new[] { ".xlsx", ".xls", ".xlsm", ".xlsb" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return allowedExtensions.Contains(extension);
        }

        public static string GenerateSafeFileName(string fileName)
        {
            // حذف کاراکترهای غیرمجاز
            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = new string(fileName
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray());

            // جایگزینی فاصله با خط تیره
            safeName = Regex.Replace(safeName, @"\s+", "-");

            // اضافه کردن timestamp برای یکتا بودن
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var extension = Path.GetExtension(safeName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(safeName);

            return $"{nameWithoutExtension}_{timestamp}{extension}";
        }
    }
}