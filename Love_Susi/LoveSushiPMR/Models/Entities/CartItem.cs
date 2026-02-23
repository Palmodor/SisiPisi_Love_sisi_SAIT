namespace LoveSushiPMR.Models.Entities
{
    // Временная корзина (сессия/база данных)
    public class CartItem
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign Keys
        public int DishId { get; set; }
        public Dish Dish { get; set; } = null!;
        
        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}
