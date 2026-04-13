using System;
using System.Collections.Generic;
using serena.Entities;
using serena.DAL;

namespace serena.BLL
{
    public class OrderBLL
    {
        private OrderDAL _dal = new OrderDAL();
        private ProductDAL _productDal = new ProductDAL();

        public string PlaceOrder(Order o)
        {
            try 
            {
                // Basic validation
                if (o.Items == null || o.Items.Count == 0) return "Order must have at least one item.";

                // Generate Order Code if not present
                if (string.IsNullOrEmpty(o.OrderCode))
                    o.OrderCode = "ORD-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + new Random().Next(100, 999);

                o.Status = "Pending";
                o.OrderDate = DateTime.Now;

                int orderId = _dal.Insert(o);
                if (orderId > 0)
                {
                    foreach (var item in o.Items)
                    {
                        item.OrderId = orderId;
                        _dal.InsertItem(item);
                    }
                    return "SUCCESS";
                }
                return "Failed to save order.";
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        public List<Order> GetMemberOrders(int memberId)
        {
            return _dal.GetByMemberId(memberId);
        }
    }
}
