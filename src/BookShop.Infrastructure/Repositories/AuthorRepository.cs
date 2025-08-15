using BookShop.Core.Interfaces;
using BookShop.Core.Models;
using BookShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop.Infrastructure.Repositories
{
    public class AuthorRepository : GenericRepository<Author>, IAuthorRepository
    {
        public AuthorRepository(BookShopDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Author>> GetActiveAuthorsAsync()
        {
            return await _dbContext.Authors
                .Where(a => a.IsActive)
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Author>> GetAllAsync()
        {
            return await _dbContext.Authors
                .OrderBy(a => a.LastName)
                .ThenBy(a => a.FirstName)
                .ToListAsync();
        }
    }
}
