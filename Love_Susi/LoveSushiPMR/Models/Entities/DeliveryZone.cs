namespace LoveSushiPMR.Models.Entities
{
    public class DeliveryZone
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DeliveryPrice { get; set; }
        public int MinDeliveryTimeMinutes { get; set; }
        public int MaxDeliveryTimeMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ICollection<DeliveryAddress> DeliveryAddresses { get; set; } = new List<DeliveryAddress>();
    }
}
