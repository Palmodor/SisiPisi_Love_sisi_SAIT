namespace LoveSushiPMR.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal BonusUsed { get; set; }
        public decimal FinalAmount { get; set; }
        public string? Comment { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public DateTime? ActualDeliveryTime { get; set; }
        public int UtensilsCount { get; set; } = 1;
        public string? RejectionReason { get; set; }
        
        // Foreign Keys
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int DeliveryAddressId { get; set; }
        public DeliveryAddress DeliveryAddress { get; set; } = null!;
        
        public int? CourierId { get; set; }
        public Courier? Courier { get; set; }
        
        public int PaymentId { get; set; }
        public Payment Payment { get; set; } = null!;
        
        public int? PromoCodeId { get; set; }
        public PromoCode? PromoCode { get; set; }
        
        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }

    public enum OrderStatus
    {
        Pending,           // Ожидает подтверждения
        Confirmed,         // Подтверждён
        Preparing,         // Готовится
        ReadyForDelivery,  // Готов к доставке
        OnDelivery,        // В пути
        Delivered,         // Доставлен
        Cancelled,         // Отменён
        Rejected           // Отклонён
    }
}
