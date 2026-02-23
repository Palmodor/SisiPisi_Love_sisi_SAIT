namespace LoveSushiPMR.Models.Entities
{
    public class Courier
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public CourierStatus Status { get; set; } = CourierStatus.Available;
        public string Phone { get; set; } = string.Empty;
        
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public enum CourierStatus
    {
        Available,
        OnDelivery,
        Offline
    }
}
