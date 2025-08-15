using BookShop.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.Web.Models.ViewModels
{
    public class BookViewModel
    {
        public int BookId { get; set; }
        
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;
        
        [Display(Name = "ISBN")]
        public string ISBN { get; set; } = string.Empty;
        
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }
        
        [Display(Name = "In Stock")]
        public int StockQuantity { get; set; }
        
        [Display(Name = "Publication Date")]
        [DisplayFormat(DataFormatString = "{0:MMMM dd, yyyy}")]
        public DateTime PublicationDate { get; set; }
        
        [Display(Name = "Cover Image")]
        public string? ImageUrl { get; set; }
        
        [Display(Name = "Author")]
        public string AuthorName { get; set; } = string.Empty;
        
        [Display(Name = "Category")]
        public string CategoryName { get; set; } = string.Empty;
        
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }
        
        // Helper methods for mapping
        public static BookViewModel FromBook(Book book)
        {
            return new BookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                ISBN = book.ISBN,
                Description = book.Description,
                Price = book.Price,
                StockQuantity = book.StockQuantity,
                PublicationDate = book.PublicationDate,
                ImageUrl = book.ImageUrl,
                AuthorId = book.AuthorId,
                CategoryId = book.CategoryId,
                AuthorName = book.Author?.FullName ?? "Unknown",
                CategoryName = book.Category?.Name ?? "Uncategorized"
            };
        }
        
        public static IEnumerable<BookViewModel> FromBooks(IEnumerable<Book> books)
        {
            var viewModels = new List<BookViewModel>();
            
            foreach (var book in books)
            {
                viewModels.Add(FromBook(book));
            }
            
            return viewModels;
        }
    }
}
