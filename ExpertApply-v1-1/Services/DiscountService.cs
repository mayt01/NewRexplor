using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;
using System.Security.Claims;

namespace Rexplor.Services
{
    public interface IDiscountService
    {
        Task<DiscountValidationResult> ValidateDiscountAsync(string code, int? fileId, decimal originalAmount, string userId = null);
        Task<Discount> GetDiscountByCodeAsync(string code);
        Task<bool> UseDiscountAsync(string code, string userId, int? fileId = null, int? orderId = null);
        Task<List<Discount>> GetActiveDiscountsAsync();
        Task<Discount> CreateDiscountAsync(Discount discount);
        Task<bool> UpdateDiscountAsync(Discount discount);
        Task<bool> DeleteDiscountAsync(int id);
    }

    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;

        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DiscountValidationResult> ValidateDiscountAsync(
            string code,
            int? fileId,
            decimal originalAmount,
            string userId = null)
        {
            var result = new DiscountValidationResult
            {
                IsValid = false,
                Message = "کد تخفیف معتبر نیست"
            };

            // پیدا کردن تخفیف
            var discount = await _context.Discounts
                .Include(d => d.FileDiscounts)
                .FirstOrDefaultAsync(d => d.Code.ToUpper() == code.ToUpper());

            if (discount == null)
            {
                result.Message = "کد تخفیف یافت نشد";
                return result;
            }

            // بررسی وضعیت فعال
            if (!discount.IsActive)
            {
                result.Message = "این کد تخفیف غیرفعال است";
                return result;
            }

            // بررسی تاریخ اعتبار
            if (DateTime.Now < discount.StartDate)
            {
                result.Message = $"این کد تخفیف از {discount.StartDate.ToString("yyyy/MM/dd")} فعال می‌شود";
                return result;
            }

            if (DateTime.Now > discount.EndDate)
            {
                result.Message = $"این کد تخفیف در {discount.EndDate.ToString("yyyy/MM/dd")} منقضی شده است";
                return result;
            }

            // بررسی محدودیت استفاده
            if (discount.UsageLimit.HasValue && discount.UsedCount >= discount.UsageLimit.Value)
            {
                result.Message = "سقف استفاده از این کد تخفیف تکمیل شده است";
                return result;
            }

            // بررسی حداقل مبلغ خرید
            if (discount.MinPurchaseAmount.HasValue && originalAmount < discount.MinPurchaseAmount.Value)
            {
                result.Message = $"حداقل مبلغ خرید برای این کد تخفیف {discount.MinPurchaseAmount.Value:N0} تومان است";
                return result;
            }

            // بررسی اعتبار برای فایل خاص
            if (!discount.IsForAllFiles && fileId.HasValue)
            {
                var isValidForFile = await _context.FileDiscounts
                    .AnyAsync(fd => fd.DiscountId == discount.Id && fd.FileId == fileId.Value);

                if (!isValidForFile)
                {
                    result.Message = "این کد تخفیف برای این فایل معتبر نیست";
                    return result;
                }
            }

            // بررسی کاربر (اگر userId ارسال شده)
            if (!string.IsNullOrEmpty(userId))
            {
                var hasUsedBefore = await _context.DiscountUsages
                    .AnyAsync(du => du.UserId == userId && du.DiscountId == discount.Id);

                if (hasUsedBefore)
                {
                    result.Message = "شما قبلاً از این کد تخفیف استفاده کرده‌اید";
                    return result;
                }
            }

            // محاسبه تخفیف
            var discountAmount = discount.CalculateDiscount(originalAmount);
            var finalAmount = originalAmount - discountAmount;

            // همه چیز OK
            result.IsValid = true;
            result.Message = "کد تخفیف معتبر است";
            result.DiscountPercent = discount.DiscountPercent;
            result.DiscountAmount = discountAmount;
            result.FinalAmount = finalAmount;
            result.MaxDiscountAmount = discount.MaxDiscountAmount;
            result.Code = discount.Code;
            result.DiscountId = discount.Id;

            return result;
        }

        public async Task<Discount> GetDiscountByCodeAsync(string code)
        {
            return await _context.Discounts
                .Include(d => d.FileDiscounts)
                .FirstOrDefaultAsync(d => d.Code.ToUpper() == code.ToUpper());
        }

        //public async Task<bool> UseDiscountAsync(string code, string userId, int? fileId = null, int? orderId = null)
        //{
        //    var discount = await GetDiscountByCodeAsync(code);
        //    if (discount == null || !discount.IsAvailable) return false;

        //    // افزایش تعداد استفاده
        //    discount.UsedCount++;
        //    _context.Discounts.Update(discount);

        //    // ثبت استفاده
        //    var usage = new DiscountUsage
        //    {
        //        UserId = userId,
        //        DiscountId = discount.Id,
        //        FileId = fileId,
        //        OrderId = orderId,
        //        UsedAt = DateTime.Now
        //    };

        //    _context.DiscountUsages.Add(usage);

        //    await _context.SaveChangesAsync();
        //    return true;
        //}

        public async Task<bool> UseDiscountAsync(string code, string userId, int? fileId = null, int? orderId = null)
        {
            var discount = await GetDiscountByCodeAsync(code);
            if (discount == null || !discount.IsAvailable) return false;

            // افزایش تعداد استفاده
            discount.UsedCount++;
            _context.Discounts.Update(discount);

            // ثبت استفاده
            var usage = new DiscountUsage
            {
                UserId = userId,
                DiscountId = discount.Id,
                FileId = fileId,
                OrderId = orderId,
                UsedAt = DateTime.Now
            };

            _context.DiscountUsages.Add(usage);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Discount>> GetActiveDiscountsAsync()
        {
            return await _context.Discounts
                .Where(d => d.IsActive && d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<Discount> CreateDiscountAsync(Discount discount)
        {
            discount.Code = discount.Code.ToUpper();
            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();
            return discount;
        }

        public async Task<bool> UpdateDiscountAsync(Discount discount)
        {
            discount.Code = discount.Code.ToUpper();
            _context.Discounts.Update(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDiscountAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return false;

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class DiscountValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public int DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public string Code { get; set; }
        public int? DiscountId { get; set; }
    }
}