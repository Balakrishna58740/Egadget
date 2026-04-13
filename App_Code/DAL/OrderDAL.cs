using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using serena.Entities;

namespace serena.DAL
{
    public class OrderDAL
    {
        public int Insert(Order o)
        {
            string sql = @"
                INSERT INTO orders (order_code, member_id, ship_name, ship_phone, status, total_qty, total_amount, payment)
                VALUES (@oc, @mid, @sn, @sp, @st, @tq, @ta, @pay);
                SELECT SCOPE_IDENTITY();";

            return Convert.ToInt32(Db.Scalar<object>(sql,
                Db.P("oc", o.OrderCode),
                Db.P("mid", o.MemberId),
                Db.P("sn", o.ShipName),
                Db.P("sp", o.ShipPhone),
                Db.P("st", o.Status),
                Db.P("tq", o.TotalQty),
                Db.P("ta", o.TotalAmount),
                Db.P("pay", o.Payment)
            ));
        }

        public void InsertItem(OrderItem item)
        {
            string sql = @"
                INSERT INTO order_items (order_id, product_id, quantity, amount)
                VALUES (@oid, @pid, @qty, @amt)";

            Db.Execute(sql,
                Db.P("oid", item.OrderId),
                Db.P("pid", item.ProductId),
                Db.P("qty", item.Quantity),
                Db.P("amt", item.Amount)
            );
        }

        public List<Order> GetByMemberId(int memberId)
        {
            string sql = "SELECT * FROM orders WHERE member_id = @mid ORDER BY order_date DESC";
            DataTable dt = Db.Query(sql, Db.P("mid", memberId));
            var list = new List<Order>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(MapRow(r));
            }
            return list;
        }

        private Order MapRow(DataRow r)
        {
            return new Order
            {
                Id = Convert.ToInt32(r["id"]),
                OrderCode = Convert.ToString(r["order_code"]),
                MemberId = Convert.ToInt32(r["member_id"]),
                ShipName = Convert.ToString(r["ship_name"]),
                ShipPhone = Convert.ToString(r["ship_phone"]),
                Status = Convert.ToString(r["status"]),
                TotalQty = Convert.ToInt32(r["total_qty"]),
                TotalAmount = Convert.ToDecimal(r["total_amount"]),
                Payment = Convert.ToString(r["payment"]),
                OrderDate = Convert.ToDateTime(r["order_date"]),
                CreatedAt = Convert.ToDateTime(r["created_at"]),
                UpdatedAt = Convert.ToDateTime(r["updated_at"])
            };
        }
    }
}
