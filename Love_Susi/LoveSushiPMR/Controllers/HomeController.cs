using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoveSushiPMR.Data;
using LoveSushiPMR.Models;
using LoveSushiPMR.Models.Entities;
using LoveSushiPMR.Models.ViewModels;

namespace LoveSushiPMR.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeViewModel
        {
            PopularDishes = await _context.Dishes
                .Where(d => d.IsPopular && d.IsAvailable)
                .OrderByDescending(d => d.Id)
                .Take(8)
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

            NewDishes = await _context.Dishes
                .Where(d => d.IsNew && d.IsAvailable)
                .OrderByDescending(d => d.Id)
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
                .ToListAsync(),

            Promotions = await _context.Promotions
                .Where(p => p.IsActive && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                .OrderByDescending(p => p.Id)
                .Take(3)
                .ToListAsync(),

            Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    IconUrl = c.IconUrl,
                    DishCount = c.Dishes.Count(d => d.IsAvailable)
                })
                .ToListAsync()
        };

        return View(viewModel);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contacts()
    {
        return View();
    }

    public IActionResult Delivery()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
