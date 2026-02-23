using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoveSushiPMR.Data;
using LoveSushiPMR.Models.ViewModels;

namespace LoveSushiPMR.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search)
        {
            var query = _context.Dishes
                .Include(d => d.Category)
                .Where(d => d.IsAvailable)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Name.Contains(search) || d.Description.Contains(search));
            }

            var viewModel = new MenuViewModel
            {
                Categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new CategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        IconUrl = c.IconUrl,
                        DishCount = c.Dishes.Count(d => d.IsAvailable),
                        IsActive = c.Id == categoryId
                    })
                    .ToListAsync(),

                Dishes = await query
                    .OrderBy(d => d.Category.SortOrder)
                    .ThenBy(d => d.Name)
                    .Select(d => new DishViewModel
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        Price = d.Price,
                        WeightGrams = d.WeightGrams,
                        ImageUrl = d.ImageUrl,
                        IsAvailable = d.IsAvailable,
                        IsPopular = d.IsPopular,
                        IsNew = d.IsNew,
                        CategoryId = d.CategoryId,
                        CategoryName = d.Category.Name
                    })
                    .ToListAsync(),

                SelectedCategoryId = categoryId,
                SearchQuery = search
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var dish = await _context.Dishes
                .Include(d => d.Category)
                .Include(d => d.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dish == null)
                return NotFound();

            var similarDishes = await _context.Dishes
                .Where(d => d.CategoryId == dish.CategoryId && d.Id != id && d.IsAvailable)
                .Take(4)
                .Select(d => new DishViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    Price = d.Price,
                    WeightGrams = d.WeightGrams,
                    ImageUrl = d.ImageUrl,
                    IsAvailable = d.IsAvailable,
                    IsPopular = d.IsPopular,
                    IsNew = d.IsNew,
                    CategoryId = d.CategoryId,
                    CategoryName = d.Category.Name
                })
                .ToListAsync();

            var viewModel = new DishDetailViewModel
            {
                Dish = new DishViewModel
                {
                    Id = dish.Id,
                    Name = dish.Name,
                    Description = dish.Description,
                    Price = dish.Price,
                    WeightGrams = dish.WeightGrams,
                    ImageUrl = dish.ImageUrl,
                    IsAvailable = dish.IsAvailable,
                    IsPopular = dish.IsPopular,
                    IsNew = dish.IsNew,
                    CategoryId = dish.CategoryId,
                    CategoryName = dish.Category.Name,
                    AverageRating = dish.Reviews.Any() ? dish.Reviews.Average(r => r.Rating) : null,
                    ReviewsCount = dish.Reviews.Count
                },
                Reviews = dish.Reviews
                    .Where(r => r.IsVisible)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewViewModel
                    {
                        Id = r.Id,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        UserName = r.User.Name
                    })
                    .ToList(),
                SimilarDishes = similarDishes
            };

            return View(viewModel);
        }

        public IActionResult Category(int id)
        {
            return RedirectToAction(nameof(Index), new { categoryId = id });
        }
    }
}
