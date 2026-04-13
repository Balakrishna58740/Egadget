using System;
using System.Collections.Generic;

namespace serena.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public int MemberId { get; set; }
        public string ShipName { get; set; }
        public string ShipPhone { get; set; }
        public string Status { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalAmount { get; set; }
        public string Payment { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public List<OrderItem> Items { get; set; }

        public Order() 
        {
            Items = new List<OrderItem>();
        }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public OrderItem() { }
    }
}
