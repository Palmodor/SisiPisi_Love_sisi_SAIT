namespace LoveSushiPMR.Models.ViewModels
{
    public class MenuViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public List<DishViewModel> Dishes { get; set; } = new List<DishViewModel>();
        public int? SelectedCategoryId { get; set; }
        public string? SearchQuery { get; set; }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public int DishCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class DishViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int WeightGrams { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsPopular { get; set; }
        public bool IsNew { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public double? AverageRating { get; set; }
        public int ReviewsCount { get; set; }
    }

    public class DishDetailViewModel
    {
        public DishViewModel Dish { get; set; } = new DishViewModel();
        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
        public List<DishViewModel> SimilarDishes { get; set; } = new List<DishViewModel>();
    }

    public class ReviewViewModel
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
