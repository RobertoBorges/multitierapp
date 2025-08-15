using BookShop.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Core.Interfaces
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        Task<IEnumerable<Book>> GetBooksByCategoryAsync(int categoryId);
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId);
        Task<IEnumerable<Book>> GetFeaturedBooksAsync(int count);
        Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
        Task<IEnumerable<Book>> GetBooksByIdsAsync(IEnumerable<int> ids);
        Task<IEnumerable<Book>> GetBooksAsync(int? categoryId = null, int? authorId = null, string searchTerm = null, int page = 1, int pageSize = 12);
        Task<int> GetBooksCountAsync(int? categoryId = null, int? authorId = null, string searchTerm = null);
    }
}
