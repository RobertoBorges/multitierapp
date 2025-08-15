using System.Collections.Generic;

namespace BookShop.Web.Models.ViewModels
{
    public class BookListViewModel
    {
        public IEnumerable<BookViewModel> Books { get; set; } = new List<BookViewModel>();
        public string? CurrentCategory { get; set; }
        public string? CurrentAuthor { get; set; }
        public string? SearchTerm { get; set; }
        public PagingInfo PagingInfo { get; set; } = new PagingInfo();
    }
    
    public class PagingInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; } = 12; // Default from old web.config
        public int CurrentPage { get; set; } = 1;
        public int TotalPages => (TotalItems / ItemsPerPage) + (TotalItems % ItemsPerPage > 0 ? 1 : 0);
    }
}
