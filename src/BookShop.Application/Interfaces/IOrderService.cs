using BookShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Application.Interfaces
{
    public interface IOrderService
    {
        Task<Order> GetOrderAsync(int id);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetCustomerOrdersAsync(int customerId);
        Task<Order> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int id);
    }
}
