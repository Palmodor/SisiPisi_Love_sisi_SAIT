namespace LoveSushiPMR.Models.Entities
{
    public class PromoCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddMonths(1);
        public int? MaxUsageCount { get; set; }
        public int CurrentUsageCount { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
