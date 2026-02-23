using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoveSushiPMR.Data;
using LoveSushiPMR.Models.Entities;
using LoveSushiPMR.Models.ViewModels;

namespace LoveSushiPMR.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var ordersToday = await _context.Orders
                .Where(o => o.OrderDate.Date == today)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalOrdersToday = ordersToday.Count,
                RevenueToday = ordersToday.Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Rejected).Sum(o => o.FinalAmount),
                TotalUsers = await _context.Users.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed),
                
                RecentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .Select(o => new RecentOrderViewModel
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        CustomerName = o.User.Name,
                        TotalAmount = o.FinalAmount,
                        StatusText = GetOrderStatusName(o.Status),
                        StatusKey = o.Status.ToString(),
                        OrderDate = o.OrderDate
                    })
                    .ToListAsync(),

                PopularDishes = await _context.OrderItems
                    .Include(oi => oi.Dish)
                    .GroupBy(oi => new { oi.DishId, oi.Dish.Name })
                    .Select(g => new PopularDishViewModel
                    {
                        Id = g.Key.DishId,
                        Name = g.Key.Name,
                        OrderCount = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.TotalPrice)
                    })
                    .OrderByDescending(d => d.OrderCount)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // Управление блюдами
        public async Task<IActionResult> Dishes()
        {
            var dishes = await _context.Dishes
                .Include(d => d.Category)
                .OrderBy(d => d.Category.SortOrder)
                .ThenBy(d => d.Name)
                .Select(d => new DishViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    Price = d.Price,
                    WeightGrams = d.WeightGrams,
                    ImageUrl = d.ImageUrl,
                    IsAvailable = d.IsAvailable,
                    IsPopular = d.IsPopular,
                    IsNew = d.IsNew,
                    CategoryId = d.CategoryId,
                    CategoryName = d.Category.Name
                })
                .ToListAsync();

            return View(dishes);
        }

        [HttpGet]
        public async Task<IActionResult> CreateDish()
        {
            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            var viewModel = new DishEditViewModel
            {
                Categories = categories.Select(c => new CategorySelectViewModel { Id = c.Id, Name = c.Name }).ToList(),
                IsAvailable = true
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDish(DishEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                model.Categories = categories.Select(c => new CategorySelectViewModel { Id = c.Id, Name = c.Name }).ToList();
                return View(model);
            }

            string? imagePath = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                imagePath = await SaveImage(model.ImageFile);
            }

            var dish = new Dish
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                WeightGrams = model.WeightGrams,
                CategoryId = model.CategoryId,
                IsAvailable = model.IsAvailable,
                IsPopular = model.IsPopular,
                IsNew = model.IsNew,
                ImageUrl = imagePath
            };

            _context.Dishes.Add(dish);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Блюдо успешно добавлено!";
            return RedirectToAction(nameof(Dishes));
        }

        [HttpGet]
        public async Task<IActionResult> EditDish(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null) return NotFound();

            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

            var viewModel = new DishEditViewModel
            {
                Id = dish.Id,
                Name = dish.Name,
                Description = dish.Description,
                Price = dish.Price,
                WeightGrams = dish.WeightGrams,
                CategoryId = dish.CategoryId,
                ImageUrl = dish.ImageUrl,
                IsAvailable = dish.IsAvailable,
                IsPopular = dish.IsPopular,
                IsNew = dish.IsNew,
                Categories = categories.Select(c => new CategorySelectViewModel { Id = c.Id, Name = c.Name }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDish(int id, DishEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                model.Categories = categories.Select(c => new CategorySelectViewModel { Id = c.Id, Name = c.Name }).ToList();
                return View(model);
            }

            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null) return NotFound();

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                dish.ImageUrl = await SaveImage(model.ImageFile);
            }

            dish.Name = model.Name;
            dish.Description = model.Description;
            dish.Price = model.Price;
            dish.WeightGrams = model.WeightGrams;
            dish.CategoryId = model.CategoryId;
            dish.IsAvailable = model.IsAvailable;
            dish.IsPopular = model.IsPopular;
            dish.IsNew = model.IsNew;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Блюдо успешно обновлено!";
            return RedirectToAction(nameof(Dishes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDish(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish != null)
            {
                _context.Dishes.Remove(dish);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Блюдо удалено!";
            }
            return RedirectToAction(nameof(Dishes));
        }

        // Управление заказами
        public async Task<IActionResult> Orders(string? status = null)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Courier)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
            {
                query = query.Where(o => o.Status == orderStatus);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderManagementViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User.Name,
                    Phone = o.User.Phone,
                    TotalAmount = o.FinalAmount,
                    CurrentStatus = GetOrderStatusName(o.Status),
                    StatusKey = o.Status.ToString(),
                    OrderDate = o.OrderDate,
                    AssignedCourierId = o.CourierId
                })
                .ToListAsync();

            // Создаём сопоставление между английскими названиями статусов и русскими
            var statusMappings = Enum.GetValues<OrderStatus>()
                .Select(s => new { EnglishName = s.ToString(), RussianName = GetOrderStatusName(s) })
                .ToList();
            
            ViewBag.StatusMappings = statusMappings;
            ViewBag.SelectedStatus = status;

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Dish)
                .Include(o => o.DeliveryAddress)
                    .ThenInclude(da => da.DeliveryZone)
                .Include(o => o.Courier)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var couriers = await _context.Couriers.Where(c => c.Status == CourierStatus.Available || c.Id == order.CourierId).ToListAsync();

            var viewModel = new OrderManagementViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.User.Name,
                Phone = order.User.Phone,
                Address = $"{order.DeliveryAddress.City}, {order.DeliveryAddress.Street}, д. {order.DeliveryAddress.House}" +
                    (string.IsNullOrEmpty(order.DeliveryAddress.Apartment) ? "" : $", кв. {order.DeliveryAddress.Apartment}"),
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    DishName = oi.Dish.Name,
                    ImageUrl = oi.Dish.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList(),
                TotalAmount = order.FinalAmount,
                CurrentStatus = GetOrderStatusName(order.Status),
                StatusKey = order.Status.ToString(),
                OrderDate = order.OrderDate,
                AssignedCourierId = order.CourierId,
                AvailableCouriers = couriers.Select(c => new CourierSelectViewModel
                {
                    Id = c.Id,
                    Name = c.FullName,
                    Status = GetCourierStatusName(c.Status)
                }).ToList(),
                AvailableStatuses = Enum.GetNames(typeof(OrderStatus)).ToList()
            };

            return View(viewModel);
        }

        // Вспомогательный метод для получения названия статуса заказа
        private static string GetOrderStatusName(OrderStatus status) => status switch
        {
            OrderStatus.Pending => "Ожидает подтверждения",
            OrderStatus.Confirmed => "Подтверждён",
            OrderStatus.Preparing => "Готовится",
            OrderStatus.ReadyForDelivery => "Готов к доставке",
            OrderStatus.OnDelivery => "В пути",
            OrderStatus.Delivered => "Доставлен",
            OrderStatus.Cancelled => "Отменён",
            OrderStatus.Rejected => "Отклонён",
            _ => "Неизвестно"
        };

        // Вспомогательный метод для получения названия статуса курьера
        private static string GetCourierStatusName(CourierStatus status) => status switch
        {
            CourierStatus.Available => "Свободен",
            CourierStatus.OnDelivery => "На доставке",
            CourierStatus.Offline => "Оффлайн",
            _ => "Неизвестно"
        };

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus, string? comment = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            if (Enum.TryParse<OrderStatus>(newStatus, out var status))
            {
                var oldStatus = order.Status;
                order.Status = status;

                // Обновляем статус курьера при доставке
                if (status == OrderStatus.OnDelivery && order.CourierId.HasValue)
                {
                    var courier = await _context.Couriers.FindAsync(order.CourierId.Value);
                    if (courier != null) courier.Status = CourierStatus.OnDelivery;
                }
                else if (status == OrderStatus.Delivered && order.CourierId.HasValue)
                {
                    var courier = await _context.Couriers.FindAsync(order.CourierId.Value);
                    if (courier != null) courier.Status = CourierStatus.Available;
                    order.ActualDeliveryTime = DateTime.UtcNow;
                }

                _context.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    PreviousStatus = oldStatus,
                    NewStatus = status,
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = User.Identity?.Name ?? "Admin",
                    Comment = comment
                });

                await _context.SaveChangesAsync();
                TempData["Success"] = "Статус заказа обновлён!";
            }

            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCourier(int orderId, int courierId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.CourierId = courierId;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Курьер назначен!";
            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        // Управление пользователями
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.RegistrationDate)
                .Select(u => new UserManagementViewModel
                {
                    Id = u.Id,
                    Name = u.Name ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Phone = u.Phone ?? string.Empty,
                    Role = u.Role != null ? u.Role.Name : "Пользователь",
                    RegistrationDate = u.RegistrationDate,
                    OrdersCount = u.Orders.Count
                })
                .ToListAsync();

            return View(users);
        }

        // Управление категориями
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View(new Category { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Категория успешно создана!";
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            if (id != category.Id) return NotFound();
            
            if (ModelState.IsValid)
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Категория успешно обновлена!";
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // Проверяем, есть ли блюда в категории
                var hasDishes = await _context.Dishes.AnyAsync(d => d.CategoryId == id);
                if (hasDishes)
                {
                    TempData["Error"] = "Нельзя удалить категорию, содержащую блюда!";
                    return RedirectToAction(nameof(Categories));
                }
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Категория удалена!";
            }
            return RedirectToAction(nameof(Categories));
        }

        // Управление курьерами
        public async Task<IActionResult> Couriers()
        {
            var couriers = await _context.Couriers
                .Include(c => c.Orders.Where(o => o.Status == OrderStatus.OnDelivery))
                .ToListAsync();
            return View(couriers);
        }

        [HttpGet]
        public IActionResult CreateCourier()
        {
            return View(new CourierEditViewModel { Status = CourierStatus.Available });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourier(CourierEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var courier = new Courier
            {
                FullName = model.FullName,
                Phone = model.Phone,
                Status = model.Status
            };

            _context.Couriers.Add(courier);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Курьер успешно добавлен!";
            return RedirectToAction(nameof(Couriers));
        }

        [HttpGet]
        public async Task<IActionResult> EditCourier(int id)
        {
            var courier = await _context.Couriers.FindAsync(id);
            if (courier == null) return NotFound();

            var viewModel = new CourierEditViewModel
            {
                Id = courier.Id,
                FullName = courier.FullName,
                Phone = courier.Phone,
                Status = courier.Status
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourier(int id, CourierEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var courier = await _context.Couriers.FindAsync(id);
            if (courier == null) return NotFound();

            courier.FullName = model.FullName;
            courier.Phone = model.Phone;
            courier.Status = model.Status;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Курьер успешно обновлён!";
            return RedirectToAction(nameof(Couriers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourier(int id)
        {
            var courier = await _context.Couriers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (courier != null)
            {
                // Проверяем, есть ли активные заказы у курьера
                var hasActiveOrders = courier.Orders.Any(o => o.Status == OrderStatus.OnDelivery || o.Status == OrderStatus.ReadyForDelivery);
                if (hasActiveOrders)
                {
                    TempData["Error"] = "Нельзя удалить курьера с активными заказами!";
                    return RedirectToAction(nameof(Couriers));
                }

                _context.Couriers.Remove(courier);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Курьер удалён!";
            }
            return RedirectToAction(nameof(Couriers));
        }

        // Управление промокодами
        public async Task<IActionResult> PromoCodes()
        {
            var promoCodes = await _context.PromoCodes
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            return View(promoCodes);
        }

        [HttpGet]
        public IActionResult CreatePromoCode()
        {
            return View(new PromoCodeEditViewModel 
            { 
                ValidUntil = DateTime.UtcNow.AddMonths(1),
                IsActive = true 
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePromoCode(PromoCodeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Проверяем, не существует ли уже такой промокод
            if (await _context.PromoCodes.AnyAsync(p => p.Code == model.Code))
            {
                ModelState.AddModelError("Code", "Промокод с таким кодом уже существует!");
                return View(model);
            }

            var promoCode = new PromoCode
            {
                Code = model.Code.ToUpper(),
                DiscountPercent = model.DiscountPercent,
                MaxDiscountAmount = model.MaxDiscountAmount,
                MinOrderAmount = model.MinOrderAmount,
                ValidUntil = EnsureUtc(model.ValidUntil),
                MaxUsageCount = model.MaxUsageCount,
                CurrentUsageCount = 0,
                IsActive = model.IsActive
            };

            _context.PromoCodes.Add(promoCode);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Промокод успешно создан!";
            return RedirectToAction(nameof(PromoCodes));
        }

        [HttpGet]
        public async Task<IActionResult> EditPromoCode(int id)
        {
            var promoCode = await _context.PromoCodes.FindAsync(id);
            if (promoCode == null) return NotFound();

            var viewModel = new PromoCodeEditViewModel
            {
                Id = promoCode.Id,
                Code = promoCode.Code,
                DiscountPercent = promoCode.DiscountPercent,
                MaxDiscountAmount = promoCode.MaxDiscountAmount,
                MinOrderAmount = promoCode.MinOrderAmount,
                ValidUntil = promoCode.ValidUntil,
                MaxUsageCount = promoCode.MaxUsageCount,
                IsActive = promoCode.IsActive
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPromoCode(int id, PromoCodeEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Проверяем, не существует ли уже такой промокод (кроме текущего)
            if (await _context.PromoCodes.AnyAsync(p => p.Code == model.Code && p.Id != id))
            {
                ModelState.AddModelError("Code", "Промокод с таким кодом уже существует!");
                return View(model);
            }

            var promoCode = await _context.PromoCodes.FindAsync(id);
            if (promoCode == null) return NotFound();

            promoCode.Code = model.Code.ToUpper();
            promoCode.DiscountPercent = model.DiscountPercent;
            promoCode.MaxDiscountAmount = model.MaxDiscountAmount;
            promoCode.MinOrderAmount = model.MinOrderAmount;
            promoCode.ValidUntil = EnsureUtc(model.ValidUntil);
            promoCode.MaxUsageCount = model.MaxUsageCount;
            promoCode.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Промокод успешно обновлён!";
            return RedirectToAction(nameof(PromoCodes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePromoCode(int id)
        {
            var promoCode = await _context.PromoCodes.FindAsync(id);
            if (promoCode != null)
            {
                _context.PromoCodes.Remove(promoCode);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Промокод удалён!";
            }
            return RedirectToAction(nameof(PromoCodes));
        }

        // Управление акциями
        public async Task<IActionResult> Promotions()
        {
            var promotions = await _context.Promotions
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            return View(promotions);
        }

        [HttpGet]
        public IActionResult CreatePromotion()
        {
            return View(new Promotion { IsActive = true, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePromotion(Promotion promotion, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    promotion.ImageUrl = await SaveImage(imageFile, "promos");
                }
                
                // Преобразуем даты в UTC для PostgreSQL
                promotion.StartDate = EnsureUtc(promotion.StartDate);
                promotion.EndDate = EnsureUtc(promotion.EndDate);

                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Акция успешно создана!";
                return RedirectToAction(nameof(Promotions));
            }
            return View(promotion);
        }

        [HttpGet]
        public async Task<IActionResult> EditPromotion(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return NotFound();
            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPromotion(int id, Promotion promotion, IFormFile? imageFile)
        {
            if (id != promotion.Id) return NotFound();
            
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    promotion.ImageUrl = await SaveImage(imageFile, "promos");
                }
                
                // Преобразуем даты в UTC для PostgreSQL
                promotion.StartDate = EnsureUtc(promotion.StartDate);
                promotion.EndDate = EnsureUtc(promotion.EndDate);

                _context.Update(promotion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Акция успешно обновлена!";
                return RedirectToAction(nameof(Promotions));
            }
            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Акция удалена!";
            }
            return RedirectToAction(nameof(Promotions));
        }

        private DateTime EnsureUtc(DateTime dateTime)
        {
            return dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            };
        }

        private async Task<string> SaveImage(IFormFile imageFile, string folder = "dishes")
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", folder);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/{folder}/{uniqueFileName}";
        }
    }
}
