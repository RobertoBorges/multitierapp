using BookShop.Application.Interfaces;
using BookShop.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ICategoryService _categoryService;
        private readonly IAuthorService _authorService;
        private readonly ILogger<CartController> _logger;
        
        private const string CartSessionKey = "ShoppingCart";

        public CartController(
            IBookService bookService,
            ICategoryService categoryService,
            IAuthorService authorService,
            ILogger<CartController> logger)
        {
            _bookService = bookService;
            _categoryService = categoryService;
            _authorService = authorService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var cart = GetCartFromSession();
            
            if (cart.Count == 0)
            {
                return View(new CartViewModel 
                { 
                    Items = new List<CartItemViewModel>(),
                    TotalPrice = 0
                });
            }
            
            var bookIds = cart.Select(i => i.BookId).ToList();
            var books = await _bookService.GetBooksByIdsAsync(bookIds);
            var authors = await _authorService.GetAllAuthorsAsync();
            
            var cartItems = new List<CartItemViewModel>();
            decimal totalPrice = 0;
            
            foreach (var book in books)
            {
                var cartItem = cart.First(i => i.BookId == book.BookId);
                var authorName = authors.FirstOrDefault(a => a.AuthorId == book.AuthorId)?.FullName ?? "Unknown Author";
                
                var item = new CartItemViewModel
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    AuthorName = authorName,
                    Price = book.Price,
                    Quantity = cartItem.Quantity,
                    ImageUrl = !string.IsNullOrEmpty(book.ImageUrl) ? book.ImageUrl : "no-image.jpg",
                    Subtotal = book.Price * cartItem.Quantity
                };
                
                cartItems.Add(item);
                totalPrice += item.Subtotal;
            }
            
            var viewModel = new CartViewModel
            {
                Items = cartItems,
                TotalPrice = totalPrice
            };
            
            return View(viewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
        {
            var book = await _bookService.GetBookByIdAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }
            
            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(i => i.BookId == bookId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemModel { BookId = bookId, Quantity = quantity });
            }
            
            SaveCartToSession(cart);
            
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public IActionResult UpdateQuantity(int bookId, int quantity)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(i => i.BookId == bookId);
            
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    cart.Remove(item);
                }
                
                SaveCartToSession(cart);
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public IActionResult RemoveFromCart(int bookId)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(i => i.BookId == bookId);
            
            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        [HttpGet]
        [Route("/api/cart/count")]
        public IActionResult GetCartCount()
        {
            var cart = GetCartFromSession();
            int count = cart.Sum(i => i.Quantity);
            
            return Json(new { count });
        }
        
        [HttpPost]
        [Route("/api/cart")]
        public async Task<IActionResult> ApiAddToCart([FromBody] CartItemModel model)
        {
            var book = await _bookService.GetBookByIdAsync(model.BookId);
            if (book == null)
            {
                return NotFound();
            }
            
            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(i => i.BookId == model.BookId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += model.Quantity;
            }
            else
            {
                cart.Add(new CartItemModel { BookId = model.BookId, Quantity = model.Quantity });
            }
            
            SaveCartToSession(cart);
            
            return Ok(new { success = true });
        }
        
        private List<CartItemModel> GetCartFromSession()
        {
            var cart = HttpContext.Session.Get<List<CartItemModel>>(CartSessionKey);
            return cart ?? new List<CartItemModel>();
        }
        
        private void SaveCartToSession(List<CartItemModel> cart)
        {
            HttpContext.Session.Set(CartSessionKey, cart);
        }
    }
    
    public class CartItemModel
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
    }
}
