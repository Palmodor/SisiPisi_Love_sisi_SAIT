using LoveSushiPMR.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace LoveSushiPMR.Models.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string StatusText => GetStatusText(Status);
        public string StatusClass => GetStatusClass(Status);
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();

        private string GetStatusText(OrderStatus status)
        {
            return status switch
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
        }

        private string GetStatusClass(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "badge-warning",
                OrderStatus.Confirmed => "badge-info",
                OrderStatus.Preparing => "badge-primary",
                OrderStatus.ReadyForDelivery => "badge-info",
                OrderStatus.OnDelivery => "badge-primary",
                OrderStatus.Delivered => "badge-success",
                OrderStatus.Cancelled => "badge-secondary",
                OrderStatus.Rejected => "badge-danger",
                _ => "badge-secondary"
            };
        }
    }

    public class OrderItemViewModel
    {
        public string DishName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Выберите адрес доставки")]
        [Display(Name = "Адрес доставки")]
        public int DeliveryAddressId { get; set; }

        [Display(Name = "Способ оплаты")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Display(Name = "Промокод")]
        public string? PromoCode { get; set; }

        [Display(Name = "Использовать бонусы")]
        public decimal? BonusAmount { get; set; }

        [Display(Name = "Количество приборов")]
        public int UtensilsCount { get; set; } = 1;

        [Display(Name = "Комментарий к заказу")]
        public string? Comment { get; set; }

        public List<DeliveryAddressViewModel> SavedAddresses { get; set; } = new List<DeliveryAddressViewModel>();
        public CartViewModel Cart { get; set; } = new CartViewModel();
        public decimal AvailableBonuses { get; set; }
        public decimal DeliveryPrice { get; set; }
    }

    public class DeliveryAddressViewModel
    {
        public int Id { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}
