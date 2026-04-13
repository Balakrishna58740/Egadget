using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace serena.Admin
{
    public partial class Dashboard : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (IsPostBack) return;

            // find literals without needing a designer file
            var litOrdersToday = Find<Literal>("litOrdersToday");
            var litRevenueToday = Find<Literal>("litRevenueToday");
            var litPending = Find<Literal>("litPending");
            var litDelivering = Find<Literal>("litDelivering");
            var litLatestOrders = Find<Literal>("litLatestOrders");
            var litLowStock = Find<Literal>("litLowStock");

            // 1) KPIs
            DateTime today = DateTime.Today;

            int ordersToday = Db.Scalar<int>(
                "SELECT COUNT(*) FROM orders WHERE CONVERT(date, order_date)=CONVERT(date, @d) AND status <> 'cancel'",
                Db.P("@d", today, SqlDbType.DateTime2));

            decimal revenueToday = Db.Scalar<decimal>(
                "SELECT ISNULL(SUM(total_amount),0) FROM orders WHERE CONVERT(date, order_date)=CONVERT(date, @d) AND status NOT IN ('cancel', 'rejected')",
                Db.P("@d", today, SqlDbType.DateTime2));

            int pending = Db.Scalar<int>("SELECT COUNT(*) FROM orders WHERE LOWER(status)='pending'");
            int delivering = Db.Scalar<int>("SELECT COUNT(*) FROM orders WHERE LOWER(status)='delivering'");

            if (litOrdersToday != null) litOrdersToday.Text = ordersToday.ToString();
            if (litRevenueToday != null) litRevenueToday.Text = "RS " + revenueToday.ToString("N2");
            if (litPending != null) litPending.Text = pending.ToString();
            if (litDelivering != null) litDelivering.Text = delivering.ToString();

            // 2) Latest orders
            var latest = Db.Query(@"
SELECT TOP 5 o.id, o.order_code, o.order_date, o.status, o.total_amount,
       m.full_name
FROM orders o
LEFT JOIN members m ON m.id = o.member_id
ORDER BY o.order_date DESC;");

            if (litLatestOrders != null)
            {
                var sb = new StringBuilder();
                if (latest.Rows.Count == 0)
                {
                    sb.Append("<tr><td colspan='6' class='px-8 py-10 text-center text-gray-400 text-xs italic'>No transactions recorded yet in the ledger.</td></tr>");
                }
                else
                {
                    foreach (DataRow r in latest.Rows)
                    {
                        string code = Html(r["order_code"]);
                        string name = Html(r["full_name"]);
                        string date = Convert.ToDateTime(r["order_date"]).ToString("dd MMM, yyyy");
                        string statusRaw = (Convert.ToString(r["status"]) ?? "").ToLower();
                        string amt = "RS " + Convert.ToDecimal(r["total_amount"]).ToString("N2");

                        string statusClass = "border-gray-200 text-gray-400";
                        if (statusRaw == "pending") statusClass = "border-orange-200 text-orange-400";
                        else if (statusRaw == "paid" || statusRaw == "accepted") statusClass = "border-green-200 text-green-500";
                        else if (statusRaw == "delivering") statusClass = "border-blue-200 text-blue-400";
                        else if (statusRaw == "completed" || statusRaw == "delivered") statusClass = "border-primary text-primary";
                        else if (statusRaw == "cancel" || statusRaw == "rejected") statusClass = "border-red-200 text-red-500";

                        sb.Append("<tr class='hover:bg-off-white/30 transition-colors'>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-text-dark'>#").Append(code).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-sm font-serif font-bold text-text-dark'>").Append(string.IsNullOrEmpty(name) ? "<span class='text-gray-300 font-normal italic'>Beneficiary Unknown</span>" : name).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-400'>").Append(date).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-right font-bold text-text-dark text-sm'>").Append(amt).Append("</td>");
                        sb.Append("<td class='px-8 py-5'>");
                        sb.Append("<span class='text-[8px] uppercase tracking-widest font-bold px-3 py-1 border ").Append(statusClass).Append("'>").Append(Html(statusRaw)).Append("</span>");
                        sb.Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-right'>");
                        sb.Append("<a class='text-primary hover:underline text-[10px] uppercase tracking-widest font-bold' href='")
                          .Append(ResolveUrl("~/Admin/OrderView.aspx?id=" + r["id"]))
                          .Append("'>Inspect</a>");
                        sb.Append("</td>");
                        sb.Append("</tr>");
                    }
                }
                litLatestOrders.Text = sb.ToString();
            }

            // 3) Low stock
            var low = Db.Query(@"
SELECT TOP 5 id, name, stock
FROM products
WHERE stock <= 10
ORDER BY stock ASC, name ASC;");

            if (litLowStock != null)
            {
                var sb = new StringBuilder();
                if (low.Rows.Count == 0)
                {
                    sb.Append("<tr><td colspan='3' class='px-8 py-10 text-center text-gray-400 text-xs italic'>All collection inventory levels are optimal.</td></tr>");
                }
                else
                {
                    foreach (DataRow r in low.Rows)
                    {
                        string name = Html(r["name"]);
                        int stock = Convert.ToInt32(r["stock"]);
                        string stockColor = stock <= 0 ? "text-red-500" : (stock <= 3 ? "text-orange-400" : "text-amber-500");

                        sb.Append("<tr class='hover:bg-off-white/30 transition-colors'>");
                        sb.Append("<td class='px-8 py-4 font-serif text-sm font-bold text-text-dark'>").Append(name).Append("</td>");
                        sb.Append("<td class='px-8 py-4 text-right font-bold ").Append(stockColor).Append("'>").Append(stock).Append("</td>");
                        sb.Append("<td class='px-8 py-4 text-right'>");
                        sb.Append("<a class='text-primary hover:underline text-[10px] uppercase tracking-widest font-bold' href='")
                          .Append(ResolveUrl("~/Admin/Products.aspx?edit=" + r["id"]))
                          .Append("'>Update</a>");
                        sb.Append("</td>");
                        sb.Append("</tr>");
                    }
                }
                litLowStock.Text = sb.ToString();
            }
        }

        // helpers
        private T Find<T>(string id) where T : Control
        {
            var ph = Master.FindControl("MainContent");
            return ph != null ? FindControlRecursive<T>(ph, id) : null;
        }

        private static T FindControlRecursive<T>(Control root, string id) where T : Control
        {
            if (root == null) return null;
            var c = root.FindControl(id) as T;
            if (c != null) return c;
            foreach (Control child in root.Controls)
            {
                var found = FindControlRecursive<T>(child, id);
                if (found != null) return found;
            }
            return null;
        }

        private static string Html(object o)
        {
            return System.Web.HttpUtility.HtmlEncode(Convert.ToString(o) ?? "");
        }
    }
}
