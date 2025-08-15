using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Core.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        
        public int CustomerId { get; set; }
        
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        
        [StringLength(50)]
        public string OrderStatus { get; set; } = "Pending";
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        [StringLength(500)]
        public string? ShippingAddress { get; set; }
        
        public DateTime? ShippedDate { get; set; }
        
        public DateTime? DeliveryDate { get; set; }
        
        // Navigation properties
        public Customer? Customer { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
