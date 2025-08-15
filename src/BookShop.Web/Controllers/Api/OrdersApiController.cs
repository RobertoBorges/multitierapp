using BookShop.Application.Interfaces;
using BookShop.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookShop.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersApiController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersApiController> _logger;

        public OrdersApiController(IOrderService orderService, ILogger<OrdersApiController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Order>>> GetUserOrders()
        {
            var userId = User.Identity.Name;
            _logger.LogInformation("Getting orders for user {UserId}", userId);
            
            var orders = await _orderService.GetCustomerOrdersAsync(int.Parse(userId));
            return Ok(orders);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var userId = User.Identity.Name;
            var order = await _orderService.GetOrderAsync(id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            // Ensure the user owns the order or is an admin
            if (order.CustomerId != int.Parse(userId) && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            
            return Ok(order);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Set the customer ID to the current user
            order.CustomerId = User.Identity?.Name != null ? int.Parse(User.Identity.Name) : 0;
            
            var createdOrder = await _orderService.CreateOrderAsync(order);
            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
        }
    }
}
