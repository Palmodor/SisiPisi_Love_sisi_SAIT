namespace LoveSushiPMR.Models.Entities
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public OrderStatus PreviousStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? ChangedBy { get; set; }
        public string? Comment { get; set; }
        
        // Foreign Key
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
