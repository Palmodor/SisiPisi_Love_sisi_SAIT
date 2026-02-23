using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LoveSushiPMR.Data;
using LoveSushiPMR.Models.Entities;
using LoveSushiPMR.Models.ViewModels;

namespace LoveSushiPMR.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetSessionId()
        {
            var sessionId = HttpContext.Session.GetString("CartSessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartSessionId", sessionId);
            }
            return sessionId;
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await GetCartViewModel();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int dishId, int quantity = 1)
        {
            var dish = await _context.Dishes.FindAsync(dishId);
            if (dish == null || !dish.IsAvailable)
                return Json(new { success = false, message = "Блюдо не найдено или недоступно" });

            var sessionId = GetSessionId();
            var userId = GetUserId();

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.DishId == dishId && 
                    (userId.HasValue ? c.UserId == userId : c.SessionId == sessionId));

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    DishId = dishId,
                    Quantity = quantity,
                    SessionId = sessionId,
                    UserId = userId,
                    AddedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            var cart = await GetCartViewModel();
            return Json(new { success = true, totalItems = cart.TotalItems, totalAmount = cart.TotalAmount });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (quantity < 1)
                return await RemoveFromCart(cartItemId);

            var sessionId = GetSessionId();
            var userId = GetUserId();

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && 
                    (userId.HasValue ? c.UserId == userId : c.SessionId == sessionId));

            if (cartItem == null)
                return Json(new { success = false, message = "Товар не найден в корзине" });

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            var cart = await GetCartViewModel();
            return Json(new { 
                success = true, 
                totalItems = cart.TotalItems, 
                totalAmount = cart.TotalAmount,
                itemTotal = cart.Items.FirstOrDefault(i => i.CartItemId == cartItemId)?.TotalPrice ?? 0
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var sessionId = GetSessionId();
            var userId = GetUserId();

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && 
                    (userId.HasValue ? c.UserId == userId : c.SessionId == sessionId));

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            var cart = await GetCartViewModel();
            return Json(new { success = true, totalItems = cart.TotalItems, totalAmount = cart.TotalAmount });
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var sessionId = GetSessionId();
            var userId = GetUserId();

            var cartItems = await _context.CartItems
                .Where(c => userId.HasValue ? c.UserId == userId : c.SessionId == sessionId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var cart = await GetCartViewModel();
            return Json(new { count = cart.TotalItems, total = cart.TotalAmount });
        }

        private async Task<CartViewModel> GetCartViewModel()
        {
            var sessionId = GetSessionId();
            var userId = GetUserId();

            var cartItems = await _context.CartItems
                .Include(c => c.Dish)
                .Where(c => userId.HasValue ? c.UserId == userId : c.SessionId == sessionId)
                .ToListAsync();

            var items = cartItems.Select(c => new CartItemViewModel
            {
                CartItemId = c.Id,
                DishId = c.DishId,
                DishName = c.Dish.Name,
                ImageUrl = c.Dish.ImageUrl,
                UnitPrice = c.Dish.Price,
                Quantity = c.Quantity,
                WeightGrams = c.Dish.WeightGrams
            }).ToList();

            var totalAmount = items.Sum(i => i.TotalPrice);
            var totalItems = items.Sum(i => i.Quantity);

            // Определение зоны доставки и стоимости (упрощённо)
            decimal deliveryPrice = totalAmount >= 1000 ? 0 : 100;

            return new CartViewModel
            {
                Items = items,
                TotalAmount = totalAmount,
                TotalItems = totalItems,
                DeliveryPrice = deliveryPrice
            };
        }
    }
}
