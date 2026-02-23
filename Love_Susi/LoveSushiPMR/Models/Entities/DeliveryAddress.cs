namespace LoveSushiPMR.Models.Entities
{
    public class DeliveryAddress
    {
        public int Id { get; set; }
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string House { get; set; } = string.Empty;
        public string? Apartment { get; set; }
        public string? Entrance { get; set; }
        public string? Floor { get; set; }
        public string? Intercom { get; set; }
        public string? Comment { get; set; }
        public bool IsDefault { get; set; }
        
        // Foreign Keys
        public int? UserId { get; set; }
        public User? User { get; set; }
        
        public int DeliveryZoneId { get; set; }
        public DeliveryZone DeliveryZone { get; set; } = null!;
        
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
