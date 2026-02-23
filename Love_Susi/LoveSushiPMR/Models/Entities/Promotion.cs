namespace LoveSushiPMR.Models.Entities
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Conditions { get; set; }
        public decimal? DiscountPercent { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ICollection<PromotionDish> PromotionDishes { get; set; } = new List<PromotionDish>();
    }

    public class PromotionDish
    {
        public int PromotionId { get; set; }
        public Promotion Promotion { get; set; } = null!;
        
        public int DishId { get; set; }
        public Dish Dish { get; set; } = null!;
    }
}
