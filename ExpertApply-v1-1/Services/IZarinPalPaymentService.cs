// Services/IZarinPalService.cs
using Rexplor.Models;

namespace Rexplor.Services
{
    public interface IZarinPalService
    {
        Task<PaymentRequestResult> RequestPaymentAsync(decimal amount, string description, int orderId);
        Task<PaymentVerificationResult> VerifyPaymentAsync(string authority, decimal amount);
    }
}