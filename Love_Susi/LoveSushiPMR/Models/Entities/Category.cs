namespace LoveSushiPMR.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    }
}
