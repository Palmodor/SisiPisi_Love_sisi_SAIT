namespace LoveSushiPMR.Models.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        
        // Foreign Keys
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int DishId { get; set; }
        public Dish Dish { get; set; } = null!;
        
        public int? OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
