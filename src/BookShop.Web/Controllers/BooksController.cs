using BookShop.Application.Interfaces;
using BookShop.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BookShop.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<BooksController> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _maxBooksPerPage;

        public BooksController(
            IBookService bookService,
            IAuthorService authorService,
            ICategoryService categoryService,
            ILogger<BooksController> logger,
            IConfiguration configuration)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _authorService = authorService ?? throw new ArgumentNullException(nameof(authorService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            // Get the max books per page from configuration
            _maxBooksPerPage = configuration.GetValue("BookShopSettings:MaxBooksPerPage", 12);
        }

        public async Task<IActionResult> Index(int? categoryId = null, int? authorId = null, string searchTerm = null, int page = 1)
        {
            try
            {
                _logger.LogInformation("Retrieving books with filters - CategoryId: {CategoryId}, AuthorId: {AuthorId}, SearchTerm: {SearchTerm}, Page: {Page}",
                    categoryId, authorId, searchTerm, page);

                // Initialize the view model
                var viewModel = new BookListViewModel
                {
                    PagingInfo = new PagingInfo
                    {
                        CurrentPage = page,
                        ItemsPerPage = _maxBooksPerPage
                    },
                    SearchTerm = searchTerm
                };

                // Get books based on filters
                var books = Enumerable.Empty<Core.Models.Book>();
                
                if (categoryId.HasValue)
                {
                    books = await _bookService.GetBooksByCategoryAsync(categoryId.Value);
                    var category = await _categoryService.GetCategoryByIdAsync(categoryId.Value);
                    viewModel.CurrentCategory = category?.Name;
                }
                else if (authorId.HasValue)
                {
                    books = await _bookService.GetBooksByAuthorAsync(authorId.Value);
                    var author = await _authorService.GetAuthorByIdAsync(authorId.Value);
                    viewModel.CurrentAuthor = author?.FullName;
                }
                else if (!string.IsNullOrEmpty(searchTerm))
                {
                    books = await _bookService.SearchBooksAsync(searchTerm);
                }
                else
                {
                    books = await _bookService.GetAllBooksAsync();
                }

                // Calculate total items for pagination
                viewModel.PagingInfo.TotalItems = books.Count();
                
                // Apply pagination (skip + take)
                books = books
                    .Skip((page - 1) * _maxBooksPerPage)
                    .Take(_maxBooksPerPage);
                
                // Convert to view models and set in the ViewModel
                viewModel.Books = BookViewModel.FromBooks(books);

                // Get all active categories for the navigation menu
                var categories = await _categoryService.GetActiveCategoriesAsync();
                ViewBag.Categories = categories;
                
                // Set application name from configuration
                ViewBag.ApplicationName = _configuration["BookShopSettings:ApplicationName"] ?? "BookShop";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books");
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving book details for ID: {BookId}", id);
                
                var book = await _bookService.GetBookByIdAsync(id);
                
                if (book == null)
                {
                    _logger.LogWarning("Book with ID {BookId} not found", id);
                    return NotFound();
                }

                // Get all active categories for the navigation menu
                var categories = await _categoryService.GetActiveCategoriesAsync();
                ViewBag.Categories = categories;
                
                // Set application name from configuration
                ViewBag.ApplicationName = _configuration["BookShopSettings:ApplicationName"] ?? "BookShop";

                return View(BookViewModel.FromBook(book));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book details for ID: {BookId}", id);
                return View("Error");
            }
        }
    }
}
