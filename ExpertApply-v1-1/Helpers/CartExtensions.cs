using System.Text.Json;
using Rexplor.Models;
using Microsoft.AspNetCore.Http;

namespace Rexplor.Extensions
{
    public static class CartExtensions
    {
        public static bool IsInCart(this HttpContext httpContext, int fileId)
        {
            var cartJson = httpContext.Session.GetString("ShoppingCart");
            if (string.IsNullOrEmpty(cartJson))
                return false;

            var cart = JsonSerializer.Deserialize<List<ShoppingCartItem>>(cartJson);
            return cart?.Any(item => item.DataFileId == fileId) ?? false;
        }
    }
}