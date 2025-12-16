// Services/ZarinPalService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Rexplor.Models;
using System.ServiceModel;
using ZarinpalService;

namespace Rexplor.Services
{
    public class ZarinPalService : IZarinPalService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _merchantId;
        private readonly string _siteBaseUrl;

        public ZarinPalService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _merchantId = _config["ZarinPal:MerchantId"];
            _siteBaseUrl = _config["ZarinPal:SiteBaseUrl"] ?? "https://rexplor.ir";
        }

        // 🔧 ساخت Callback URL هوشمند
        private string BuildCallbackUrl(int orderId)
        {
            var context = _httpContextAccessor.HttpContext;

            // همیشه از آدرس سایت واقعی استفاده می‌کنیم
            // زیرا زرین‌پال باید بتواند به آن دسترسی داشته باشد
            return $"{_siteBaseUrl}/Orders/VerifyPayment?id={orderId}";
        }

        public async Task<PaymentRequestResult> RequestPaymentAsync(decimal amount, string description, int orderId)
        {
            try
            {
                // ۱. ساخت Callback URL
                var callbackUrl = BuildCallbackUrl(orderId);
                Console.WriteLine($"🌐 Callback URL ساخته شد: {callbackUrl}");

                // ۲. تبدیل تومان به ریال
                var amountInRials = (int)(amount);
                //var amountInRials = (int)(amount * 10);

                // ۳. اتصال به سرویس زرین‌پال
                var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                var endpoint = new EndpointAddress("https://www.zarinpal.com/pg/services/WebGate/service");
                var client = new PaymentGatewayImplementationServicePortTypeClient(binding, endpoint);

                // ۴. ارسال درخواست پرداخت
                var response = await client.PaymentRequestAsync(
                    _merchantId,
                    amountInRials,
                    description,
                    "", // ایمیل (اختیاری)
                    "", // موبایل (اختیاری)
                    callbackUrl
                );

                // ۵. بررسی پاسخ
                var status = response.Body?.Status;
                var authority = response.Body?.Authority;

                Console.WriteLine($"🔄 پاسخ زرین‌پال: Status={status}, Authority={authority}");

                if (status == 100 && !string.IsNullOrEmpty(authority))
                {
                    return new PaymentRequestResult
                    {
                        IsSuccess = true,
                        Authority = authority,
                        GatewayUrl = $"https://www.zarinpal.com/pg/StartPay/{authority}",
                        Message = "درخواست پرداخت موفق"
                    };
                }

                return new PaymentRequestResult
                {
                    IsSuccess = false,
                    Message = $"خطا از سمت زرین‌پال. کد: {status}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 خطا در RequestPaymentAsync: {ex.Message}");
                return new PaymentRequestResult
                {
                    IsSuccess = false,
                    Message = $"خطا در ارتباط با درگاه: {ex.Message}"
                };
            }
        }

        public async Task<PaymentVerificationResult> VerifyPaymentAsync(string authority, decimal amount)
        {
            try
            {
                // ۱. تبدیل تومان به ریال
                var amountInRials = (int)(amount);
                //var amountInRials = (int)(amount * 10);

                // ۲. اتصال به سرویس زرین‌پال
                var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                var endpoint = new EndpointAddress("https://www.zarinpal.com/pg/services/WebGate/service");
                var client = new PaymentGatewayImplementationServicePortTypeClient(binding, endpoint);

                // ۳. ارسال درخواست تأیید
                var response = await client.PaymentVerificationAsync(
                    _merchantId,
                    authority,
                    amountInRials
                );

                // ۴. بررسی پاسخ
                var status = response.Body?.Status;
                var refId = response.Body?.RefID;

                Console.WriteLine($"🔄 تأیید پرداخت: Status={status}, RefID={refId}");

                if (status == 100 || status == 101)
                {
                    return new PaymentVerificationResult
                    {
                        IsSuccess = true,
                        RefId = refId ?? 0,
                        Message = "پرداخت موفق"
                    };
                }

                return new PaymentVerificationResult
                {
                    IsSuccess = false,
                    Message = $"تراکنش ناموفق. کد: {status}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 خطا در VerifyPaymentAsync: {ex.Message}");
                return new PaymentVerificationResult
                {
                    IsSuccess = false,
                    Message = $"خطا در تأیید پرداخت: {ex.Message}"
                };
            }
        }
    }
}