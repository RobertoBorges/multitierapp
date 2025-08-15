using BookShop.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Core.Interfaces
{
    public interface IAuthorRepository : IGenericRepository<Author>
    {
        Task<IEnumerable<Author>> GetActiveAuthorsAsync();
    }
}
