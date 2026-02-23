namespace LoveSushiPMR.Models.Entities
{
    public class BonusAccount
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        // Foreign Key
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
