using BookShop.Core.Interfaces;
using BookShop.Core.Models;
using BookShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Infrastructure.Repositories
{
    public class BookRepository : GenericRepository<Book>, IBookRepository
    {
        public BookRepository(BookShopDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Book>> GetBooksByCategoryAsync(int categoryId)
        {
            return await _dbContext.Books
                .Where(b => b.CategoryId == categoryId && b.IsActive)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId)
        {
            return await _dbContext.Books
                .Where(b => b.AuthorId == authorId && b.IsActive)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetFeaturedBooksAsync(int count)
        {
            return await _dbContext.Books
                .Where(b => b.IsActive)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetAllAsync();

            return await _dbContext.Books
                .Where(b => (b.Title.Contains(searchTerm) || 
                             b.Description.Contains(searchTerm) || 
                             b.ISBN.Contains(searchTerm) || 
                             b.Author.FirstName.Contains(searchTerm) || 
                             b.Author.LastName.Contains(searchTerm) ||
                             b.Category.Name.Contains(searchTerm)) && 
                             b.IsActive)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        // Override to include related entities
        public override async Task<Book> GetByIdAsync(int id)
        {
            return await _dbContext.Books
                .Where(b => b.BookId == id)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync();
        }

        public override async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _dbContext.Books
                .Where(b => b.IsActive)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByIdsAsync(IEnumerable<int> ids)
        {
            return await _dbContext.Books
                .Where(b => ids.Contains(b.BookId) && b.IsActive)
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksAsync(int? categoryId = null, int? authorId = null, string searchTerm = null, int page = 1, int pageSize = 12)
        {
            var query = _dbContext.Books.Where(b => b.IsActive);

            // Apply filters
            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            if (authorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == authorId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => 
                    b.Title.Contains(searchTerm) || 
                    b.Description.Contains(searchTerm) || 
                    b.ISBN.Contains(searchTerm) || 
                    b.Author.FirstName.Contains(searchTerm) || 
                    b.Author.LastName.Contains(searchTerm) || 
                    b.Category.Name.Contains(searchTerm));
            }

            // Calculate pagination
            var skip = (page - 1) * pageSize;

            // Return the paginated results with includes
            return await query
                .Include(b => b.Author)
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetBooksCountAsync(int? categoryId = null, int? authorId = null, string searchTerm = null)
        {
            var query = _dbContext.Books.Where(b => b.IsActive);

            // Apply filters
            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            if (authorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == authorId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => 
                    b.Title.Contains(searchTerm) || 
                    b.Description.Contains(searchTerm) || 
                    b.ISBN.Contains(searchTerm) || 
                    b.Author.FirstName.Contains(searchTerm) || 
                    b.Author.LastName.Contains(searchTerm) || 
                    b.Category.Name.Contains(searchTerm));
            }

            // Return the count
            return await query.CountAsync();
        }
    }
}
