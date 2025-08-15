using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BookShop.Web.Models;
using BookShop.Application.Interfaces;
using BookShop.Web.Models.ViewModels;

namespace BookShop.Web.Controllers;

public class HomeController : Controller
{
    private readonly IBookService _bookService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(
        IBookService bookService,
        ICategoryService categoryService,
        ILogger<HomeController> logger,
        IConfiguration configuration)
    {
        _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("Loading home page");
            
            // Get featured books for the home page
            var featuredCount = 6;
            var featuredBooks = await _bookService.GetFeaturedBooksAsync(featuredCount);
            
            // Get all active categories for the navigation menu
            var categories = await _categoryService.GetActiveCategoriesAsync();
            
            // Store categories in ViewBag for the navigation menu
            ViewBag.Categories = categories;
            
            // Set application name from configuration
            ViewBag.ApplicationName = _configuration["BookShopSettings:ApplicationName"] ?? "BookShop";
            
            return View(BookViewModel.FromBooks(featuredBooks));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page");
            return View("Error");
        }
    }

    public async Task<IActionResult> About()
    {
        // Get all active categories for the navigation menu
        var categories = await _categoryService.GetActiveCategoriesAsync();
        
        // Store categories in ViewBag for the navigation menu
        ViewBag.Categories = categories;
        
        ViewBag.ApplicationName = _configuration["BookShopSettings:ApplicationName"] ?? "BookShop";
        
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
