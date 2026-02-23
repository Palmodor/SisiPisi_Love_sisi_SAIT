namespace LoveSushiPMR.Models.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string? TransactionId { get; set; }
        
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public enum PaymentMethod
    {
        Cash,           // Наличные
        CardOnline,     // Картой онлайн
        CardCourier,    // Картой курьеру
        BonusPoints     // Бонусные баллы
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Refunded
    }
}
