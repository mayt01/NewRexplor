// Models/PaymentModels.cs
namespace Rexplor.Models
{
    public class PaymentRequestResult
    {
        public bool IsSuccess { get; set; }
        public string Authority { get; set; }
        public string GatewayUrl { get; set; }
        public string Message { get; set; }
    }

    public class PaymentVerificationResult
    {
        public bool IsSuccess { get; set; }
        public long RefId { get; set; }
        public string Message { get; set; }
    }
}