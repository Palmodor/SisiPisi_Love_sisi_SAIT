namespace LoveSushiPMR.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public decimal DeliveryPrice { get; set; }
        public decimal FinalAmount => TotalAmount + DeliveryPrice;
    }

    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public int WeightGrams { get; set; }
    }
}
