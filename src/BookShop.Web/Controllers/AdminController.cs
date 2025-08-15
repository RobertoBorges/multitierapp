using BookShop.Application.Interfaces;
using BookShop.Core.Models;
using BookShop.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Web.Controllers
{
    [Authorize]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;

        public AdminController(
            IBookService bookService,
            IAuthorService authorService,
            ICategoryService categoryService,
            ILogger<AdminController> logger,
            IConfiguration configuration)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _authorService = authorService ?? throw new ArgumentNullException(nameof(authorService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading admin dashboard");
                
                var books = await _bookService.GetAllBooksAsync();
                
                // Set application name from configuration
                ViewBag.ApplicationName = _configuration["BookShopSettings:ApplicationName"] ?? "BookShop";
                
                return View(BookViewModel.FromBooks(books));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return View("Error");
            }
        }

        [HttpGet("books")]
        public async Task<IActionResult> ManageBooks()
        {
            try
            {
                _logger.LogInformation("Loading book management page");
                
                var books = await _bookService.GetAllBooksAsync();
                
                // Set application name from configuration
                ViewBag.ApplicationName = _configuration["BookShopSettings:ApplicationName"] ?? "BookShop";
                
                return View(BookViewModel.FromBooks(books));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book management page");
                return View("Error");
            }
        }

        [HttpGet("books/create")]
        public async Task<IActionResult> CreateBook()
        {
            try
            {
                _logger.LogInformation("Loading book creation page");
                
                // Get all authors and categories for dropdowns
                var authors = await _authorService.GetActiveAuthorsAsync();
                var categories = await _categoryService.GetActiveCategoriesAsync();
                
                ViewBag.Authors = new SelectList(authors, "AuthorId", "FullName");
                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
                
                // Set application name from configuration
                ViewBag.ApplicationName = _configuration["BookShopSettings:ApplicationName"] ?? "BookShop";
                
                return View(new BookViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading book creation page");
                return View("Error");
            }
        }

        [HttpPost("books/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(BookViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Creating new book: {Title}", viewModel.Title);
                    
                    // Map ViewModel to Domain Model
                    var book = new Book
                    {
                        Title = viewModel.Title,
                        ISBN = viewModel.ISBN,
                        Description = viewModel.Description,
                        Price = viewModel.Price,
                        StockQuantity = viewModel.StockQuantity,
                        PublicationDate = viewModel.PublicationDate,
                        ImageUrl = viewModel.ImageUrl,
                        IsActive = true,
                        AuthorId = viewModel.AuthorId,
                        CategoryId = viewModel.CategoryId
                    };
                    
                    await _bookService.AddBookAsync(book);
                    
                    return RedirectToAction(nameof(ManageBooks));
                }
                
                // If model state is invalid, reload dropdowns and return view
                var authors = await _authorService.GetActiveAuthorsAsync();
                var categories = await _categoryService.GetActiveCategoriesAsync();
                
                ViewBag.Authors = new SelectList(authors, "AuthorId", "FullName");
                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
                
                // Set application name from configuration
                ViewBag.ApplicationName = _configuration["BookShopSettings:ApplicationName"] ?? "BookShop";
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book: {Title}", viewModel.Title);
                ModelState.AddModelError("", "An error occurred while creating the book.");
                
                // Reload dropdowns and return view
                var authors = await _authorService.GetActiveAuthorsAsync();
                var categories = await _categoryService.GetActiveCategoriesAsync();
                
                ViewBag.Authors = new SelectList(authors, "AuthorId", "FullName");
                ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
                
                return View(viewModel);
            }
        }
    }
}
