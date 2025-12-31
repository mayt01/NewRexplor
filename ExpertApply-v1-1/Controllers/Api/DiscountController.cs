using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rexplor.Services;
using System.Security.Claims;

namespace Rexplor.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateDiscount([FromBody] ValidateDiscountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { isValid = false, message = "کد تخفیف الزامی است" });
            }

            // گرفتن userId اگر کاربر لاگین کرده باشد
            string userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            var result = await _discountService.ValidateDiscountAsync(
                request.Code,
                request.FileId,
                request.OriginalAmount,
                userId);

            return Ok(result);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveDiscounts()
        {
            var discounts = await _discountService.GetActiveDiscountsAsync();
            return Ok(discounts);
        }
    }

    public class ValidateDiscountRequest
    {
        public string Code { get; set; }
        public int? FileId { get; set; }
        public decimal OriginalAmount { get; set; }
    }
}