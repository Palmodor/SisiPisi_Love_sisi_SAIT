namespace LoveSushiPMR.Models.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        
        // Foreign Keys
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        
        public int DishId { get; set; }
        public Dish Dish { get; set; } = null!;
    }
}
