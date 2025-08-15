using BookShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Core.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetCustomerOrdersAsync(int customerId);
    }
}
