using Microsoft.EntityFrameworkCore;
using AlaSaree3.Data;
using AlaSaree3.Models;
using AlaSaree3.Services.Interfaces;
using AlaSaree3.ViewModels.Cart;

namespace AlaSaree3.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartAsync(string customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Seller)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartViewModel> GetCartByCustomerIdAsync(string customerId)
        {
            var cart = await GetOrCreateCartAsync(customerId);

            var items = cart.Items.Select(item => new CartItemViewModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                ProductImageUrl = item.Product.ImageUrl,
                UnitPrice = item.Product.Price, // Always fresh DB price
                Quantity = item.Quantity,
                AvailableStock = item.Product.AvailableQuantity,
                SellerName = item.Product.Seller?.FullName ?? "AlaSaree3 Seller"
            }).ToList();

            return new CartViewModel
            {
                Items = items
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> AddToCartAsync(string customerId, int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return (false, "Quantity must be at least 1.");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return (false, "Product not found.");
            }

            if (product.AvailableQuantity <= 0)
            {
                return (false, "Sorry, this product is currently out of stock.");
            }

            var cart = await GetOrCreateCartAsync(customerId);
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            int totalRequested = quantity;
            if (existingItem != null)
            {
                totalRequested += existingItem.Quantity;
            }

            if (totalRequested > product.AvailableQuantity)
            {
                return (false, $"Cannot add {quantity} more. Only {product.AvailableQuantity} items are available in stock.");
            }

            if (existingItem != null)
            {
                existingItem.Quantity = totalRequested;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateQuantityAsync(string customerId, int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null || cartItem.Cart.CustomerId != customerId)
            {
                return (false, "Cart item not found or unauthorized.");
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                return (true, null);
            }

            if (quantity > cartItem.Product.AvailableQuantity)
            {
                return (false, $"Only {cartItem.Product.AvailableQuantity} units are available in stock.");
            }

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RemoveFromCartAsync(string customerId, int cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null || cartItem.Cart.CustomerId != customerId)
            {
                return (false, "Cart item not found or unauthorized.");
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> ClearCartAsync(string customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart != null && cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<int> GetCartItemCountAsync(string customerId)
        {
            return await _context.CartItems
                .Where(ci => ci.Cart.CustomerId == customerId)
                .SumAsync(ci => ci.Quantity);
        }
    }
}
