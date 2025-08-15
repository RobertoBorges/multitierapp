using BookShop.Application.Interfaces;
using BookShop.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksApiController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ICategoryService _categoryService;
        private readonly IAuthorService _authorService;

        public BooksApiController(
            IBookService bookService,
            ICategoryService categoryService,
            IAuthorService authorService)
        {
            _bookService = bookService;
            _categoryService = categoryService;
            _authorService = authorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookViewModel>>> GetBooks(
            [FromQuery] int? categoryId = null,
            [FromQuery] int? authorId = null,
            [FromQuery] string searchTerm = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            var books = await _bookService.GetBooksAsync(
                categoryId, 
                authorId, 
                searchTerm, 
                page, 
                pageSize);

            var categories = await _categoryService.GetAllCategoriesAsync();
            var authors = await _authorService.GetAllAuthorsAsync();

            var viewModels = books.Select(b => new BookViewModel
            {
                BookId = b.BookId,
                Title = b.Title,
                Description = b.Description,
                ISBN = b.ISBN,
                Price = b.Price,
                PublicationDate = b.PublicationDate,
                AuthorId = b.AuthorId,
                CategoryId = b.CategoryId,
                AuthorName = authors.FirstOrDefault(a => a.AuthorId == b.AuthorId)?.FullName,
                CategoryName = categories.FirstOrDefault(c => c.CategoryId == b.CategoryId)?.Name,
                ImageUrl = !string.IsNullOrEmpty(b.ImageUrl) ? b.ImageUrl : "no-image.jpg",
                StockQuantity = b.StockQuantity
            });

            return Ok(viewModels);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookViewModel>> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var author = await _authorService.GetAuthorByIdAsync(book.AuthorId);
            var category = await _categoryService.GetCategoryByIdAsync(book.CategoryId);

            var viewModel = new BookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                ISBN = book.ISBN,
                Price = book.Price,
                PublicationDate = book.PublicationDate,
                AuthorId = book.AuthorId,
                CategoryId = book.CategoryId,
                AuthorName = author?.FullName,
                CategoryName = category?.Name,
                ImageUrl = !string.IsNullOrEmpty(book.ImageUrl) ? book.ImageUrl : "no-image.jpg",
                StockQuantity = book.StockQuantity
            };

            return Ok(viewModel);
        }
    }
}
