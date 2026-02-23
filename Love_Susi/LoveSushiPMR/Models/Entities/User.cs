namespace LoveSushiPMR.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public bool EmailConfirmed { get; set; } = true;
        
        // Foreign Keys
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        
        // Navigation properties
        public BonusAccount? BonusAccount { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<DeliveryAddress> DeliveryAddresses { get; set; } = new List<DeliveryAddress>();
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
