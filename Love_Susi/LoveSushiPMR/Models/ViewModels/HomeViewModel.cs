using LoveSushiPMR.Models.Entities;

namespace LoveSushiPMR.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<DishViewModel> PopularDishes { get; set; } = new List<DishViewModel>();
        public List<DishViewModel> NewDishes { get; set; } = new List<DishViewModel>();
        public List<Promotion> Promotions { get; set; } = new List<Promotion>();
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }
}
