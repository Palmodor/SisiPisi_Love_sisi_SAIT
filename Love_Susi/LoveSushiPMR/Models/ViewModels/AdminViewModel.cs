using LoveSushiPMR.Models.Entities;

namespace LoveSushiPMR.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalOrdersToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int TotalUsers { get; set; }
        public int PendingOrders { get; set; }
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();
        public List<PopularDishViewModel> PopularDishes { get; set; } = new List<PopularDishViewModel>();
    }

    public class RecentOrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string StatusText { get; set; } = string.Empty; // Русское название статуса
        public string StatusKey { get; set; } = string.Empty; // Ключ для класса badge
        public DateTime OrderDate { get; set; }
    }

    public class PopularDishViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DishEditViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int WeightGrams { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsPopular { get; set; }
        public bool IsNew { get; set; }
        public int CategoryId { get; set; }
        public List<CategorySelectViewModel> Categories { get; set; } = new List<CategorySelectViewModel>();
    }

    public class CategorySelectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class OrderManagementViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
        public decimal TotalAmount { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string StatusKey { get; set; } = string.Empty; // Ключ статуса (Pending, Confirmed и т.д.)
        public DateTime OrderDate { get; set; }
        public List<string> AvailableStatuses { get; set; } = new List<string>();
        public List<CourierSelectViewModel> AvailableCouriers { get; set; } = new List<CourierSelectViewModel>();
        public int? AssignedCourierId { get; set; }
    }

    public class CourierSelectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class UserManagementViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public int OrdersCount { get; set; }
        public bool IsLocked { get; set; }
    }

    public class CourierEditViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public CourierStatus Status { get; set; } = CourierStatus.Available;
    }

    public class PromoCodeEditViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddMonths(1);
        public int? MaxUsageCount { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
