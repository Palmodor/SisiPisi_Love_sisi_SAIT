namespace LoveSushiPMR.Models.Entities
{
    public class Dish
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int WeightGrams { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsPopular { get; set; } = false;
        public bool IsNew { get; set; } = false;
        
        // Foreign Key
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        
        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<PromotionDish> PromotionDishes { get; set; } = new List<PromotionDish>();
    }
}
