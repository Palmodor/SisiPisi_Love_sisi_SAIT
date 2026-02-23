using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LoveSushiPMR.Data;
using LoveSushiPMR.Models.Entities;
using LoveSushiPMR.Models.ViewModels;

namespace LoveSushiPMR.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();

            // Получаем корзину
            var cartItems = await _context.CartItems
                .Include(c => c.Dish)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Ваша корзина пуста";
                return RedirectToAction("Index", "Cart");
            }

            var cart = new CartViewModel
            {
                Items = cartItems.Select(c => new CartItemViewModel
                {
                    DishId = c.DishId,
                    DishName = c.Dish.Name,
                    ImageUrl = c.Dish.ImageUrl,
                    UnitPrice = c.Dish.Price,
                    Quantity = c.Quantity,
                    WeightGrams = c.Dish.WeightGrams
                }).ToList(),
                TotalAmount = cartItems.Sum(c => c.Dish.Price * c.Quantity),
                TotalItems = cartItems.Sum(c => c.Quantity)
            };

            var addresses = await _context.DeliveryAddresses
                .Include(da => da.DeliveryZone)
                .Where(da => da.UserId == userId)
                .ToListAsync();

            var bonuses = await _context.BonusAccounts
                .Where(ba => ba.UserId == userId)
                .Select(ba => ba.Balance)
                .FirstOrDefaultAsync();

            var viewModel = new CheckoutViewModel
            {
                Cart = cart,
                DeliveryPrice = cart.TotalAmount >= 1000 ? 0 : 100,
                SavedAddresses = addresses.Select(a => new DeliveryAddressViewModel
                {
                    Id = a.Id,
                    FullAddress = $"{a.City}, {a.Street}, д. {a.House}" + 
                        (string.IsNullOrEmpty(a.Apartment) ? "" : $", кв. {a.Apartment}"),
                    IsDefault = a.IsDefault
                }).ToList(),
                AvailableBonuses = bonuses
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = GetUserId();

            // Проверяем, что адрес существует и принадлежит пользователю
            var addressExists = await _context.DeliveryAddresses
                .AnyAsync(da => da.Id == model.DeliveryAddressId && da.UserId == userId);
            
            if (!addressExists)
            {
                TempData["Error"] = "Выберите существующий адрес доставки или добавьте новый в профиле";
                return await Checkout();
            }

            if (!ModelState.IsValid)
            {
                return await Checkout();
            }

            // Получаем корзину
            var cartItems = await _context.CartItems
                .Include(c => c.Dish)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Ваша корзина пуста";
                return RedirectToAction("Index", "Cart");
            }

            // Проверяем промокод
            PromoCode? promoCode = null;
            if (!string.IsNullOrWhiteSpace(model.PromoCode))
            {
                promoCode = await _context.PromoCodes
                    .FirstOrDefaultAsync(pc => pc.Code == model.PromoCode.ToUpper() && 
                        pc.IsActive && 
                        pc.ValidUntil >= DateTime.UtcNow &&
                        (pc.MaxUsageCount == null || pc.CurrentUsageCount < pc.MaxUsageCount));
            }

            // Рассчитываем суммы
            var totalAmount = cartItems.Sum(c => c.Dish.Price * c.Quantity);
            var deliveryPrice = totalAmount >= 1000 ? 0 : 100;
            
            decimal discountAmount = 0;
            if (promoCode != null)
            {
                discountAmount = totalAmount * (promoCode.DiscountPercent / 100);
                if (promoCode.MaxDiscountAmount.HasValue && discountAmount > promoCode.MaxDiscountAmount.Value)
                    discountAmount = promoCode.MaxDiscountAmount.Value;
            }

            var bonusUsed = Math.Min(model.BonusAmount ?? 0, totalAmount - discountAmount);
            
            // Проверяем бонусы пользователя
            var userBonus = await _context.BonusAccounts.FirstOrDefaultAsync(b => b.UserId == userId);
            if (userBonus != null && bonusUsed > userBonus.Balance)
                bonusUsed = userBonus.Balance;

            var finalAmount = totalAmount + deliveryPrice - discountAmount - bonusUsed;

            // Создаём заказ
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                DeliveryAddressId = model.DeliveryAddressId,
                PaymentId = await CreatePayment(finalAmount, model.PaymentMethod),
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                BonusUsed = bonusUsed,
                FinalAmount = finalAmount,
                Comment = model.Comment,
                Status = OrderStatus.Pending,
                UtensilsCount = model.UtensilsCount,
                PromoCodeId = promoCode?.Id,
                OrderDate = DateTime.UtcNow,
                EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(45)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Добавляем позиции заказа
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    DishId = item.DishId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Dish.Price,
                    TotalPrice = item.Dish.Price * item.Quantity
                };
                _context.OrderItems.Add(orderItem);
            }

            // Обновляем бонусы
            if (userBonus != null)
            {
                userBonus.Balance -= bonusUsed;
                // Начисляем бонусы за заказ (5%)
                userBonus.Balance += finalAmount * 0.05m;
                userBonus.LastUpdated = DateTime.UtcNow;
            }

            // Обновляем промокод
            if (promoCode != null)
            {
                promoCode.CurrentUsageCount++;
            }

            // Очищаем корзину
            _context.CartItems.RemoveRange(cartItems);

            // Добавляем историю статуса
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                PreviousStatus = OrderStatus.Pending,
                NewStatus = OrderStatus.Pending,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = "System"
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Заказ {order.OrderNumber} успешно оформлен!";
            return RedirectToAction("Details", new { id = order.Id });
        }

        public async Task<IActionResult> History()
        {
            var userId = GetUserId();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    DiscountAmount = o.DiscountAmount,
                    FinalAmount = o.FinalAmount,
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        DishName = oi.Dish.Name,
                        ImageUrl = oi.Dish.ImageUrl,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                })
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .Include(o => o.DeliveryAddress)
                    .ThenInclude(da => da.DeliveryZone)
                .Include(o => o.Courier)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            var viewModel = new OrderViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.FinalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    DishName = oi.Dish.Name,
                    ImageUrl = oi.Dish.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };

            ViewBag.DeliveryAddress = $"{order.DeliveryAddress.City}, {order.DeliveryAddress.Street}, д. {order.DeliveryAddress.House}";
            ViewBag.EstimatedDelivery = order.EstimatedDeliveryTime;
            ViewBag.Courier = order.Courier;

            return View(viewModel);
        }

        private string GenerateOrderNumber()
        {
            return $"LS-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        private async Task<int> CreatePayment(decimal amount, PaymentMethod method)
        {
            var payment = new Payment
            {
                Amount = amount,
                Method = method,
                Status = method == PaymentMethod.Cash ? PaymentStatus.Pending : PaymentStatus.Completed,
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment.Id;
        }
    }
}
