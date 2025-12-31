using Rexplor.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rexplor.ViewModels
{
    public class CartIndexViewModel
    {
        public List<ShoppingCartItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public string DiscountCode { get; set; }
        public int? DiscountPercent { get; set; }

        [NotMapped]
        public bool HasDiscount => DiscountAmount > 0;
    }
}