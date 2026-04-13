using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace serena.Admin
{
    public partial class ReportOrderPrintPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                var lg = Find<Literal>("litGenerated");
                if (lg != null) lg.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                LoadDataFromQuery();
            }
        }

        private void LoadDataFromQuery()
        {
            var lbl = Find<Label>("lblMsg");
            var lper = Find<Literal>("litPeriod");
            var lsum = Find<Literal>("litSummary");
            var lrows = Find<Literal>("litRows");

            try
            {
                string type = (Request.QueryString["type"] ?? "range").Trim().ToLowerInvariant();

                DateTime from, toExclusive;
                string label;
                if (!ResolvePeriodFromQuery(type, out from, out toExclusive, out label))
                {
                    if (lper != null) lper.Text = "";
                    if (lsum != null) lsum.Text = "";
                    if (lrows != null) lrows.Text = "<tr><td colspan='6' class='text-center text-muted py-3'>Missing or invalid period.</td></tr>";
                    Show(lbl, "Missing or invalid period.", false);
                    return;
                }

                if (lper != null) lper.Text = Server.HtmlEncode(label);

                // Delivered only within range
                var dt = Db.Query(@"
SELECT o.id, o.order_code, o.total_amount, o.total_qty, o.payment, o.order_date,
       COALESCE(o.ship_name, m.full_name) AS customer_name
FROM orders o
LEFT JOIN members m ON m.id = o.member_id
WHERE LOWER(COALESCE(o.status,''))='delivered'
  AND o.order_date >= @from AND o.order_date < @to
ORDER BY o.order_date DESC;",
                    Db.P("@from", from), Db.P("@to", toExclusive));

                var sb = new StringBuilder();
                int count = 0;
                long sumQty = 0;
                decimal sumAmt = 0m;

                if (dt.Rows.Count == 0)
                {
                    sb.Append("<tr><td colspan='6' class='text-center text-muted py-3'>No delivered orders in this period.</td></tr>");
                }
                else
                {
                    int i = 0;
                    foreach (DataRow r in dt.Rows)
                    {
                        i++; count++;

                        string code = Html(r["order_code"]);
                        string cust = Html(r["customer_name"]);
                        string payment = Html(r["payment"]);
                        DateTime od = r["order_date"] != DBNull.Value ? Convert.ToDateTime(r["order_date"]) : DateTime.MinValue;
                        int qty = r["total_qty"] != DBNull.Value ? Convert.ToInt32(r["total_qty"]) : 0;
                        decimal amt = r["total_amount"] != DBNull.Value ? Convert.ToDecimal(r["total_amount"]) : 0m;

                        sumQty += qty;
                        sumAmt += amt;

                        sb.Append("<tr>");
                        sb.Append("<td>").Append(i).Append("</td>");
                        sb.Append("<td>").Append(string.IsNullOrEmpty(code) ? "-" : code).Append("</td>");
                        sb.Append("<td>").Append(string.IsNullOrEmpty(cust) ? "-" : cust).Append("</td>");
                        sb.Append("<td>").Append(string.IsNullOrEmpty(payment) ? "-" : payment).Append("</td>");
                        sb.Append("<td class='text-end'>").Append(amt.ToString("N2")).Append("</td>");
                        sb.Append("<td>").Append(od == DateTime.MinValue ? "-" : od.ToString("yyyy-MM-dd HH:mm")).Append("</td>");
                        sb.Append("</tr>");
                    }
                }

                if (lrows != null) lrows.Text = sb.ToString();
                if (lsum != null)
                {
                    lsum.Text = "<div class='small text-muted'>Total Orders: <strong>" + count.ToString("N0") +
                                "</strong> &nbsp;|&nbsp; Total Qty: <strong>" + sumQty.ToString("N0") +
                                "</strong> &nbsp;|&nbsp; Total Revenue: <strong>RS " + sumAmt.ToString("N2") +
                                "</strong></div>";
                }

                Show(lbl, "Loaded " + count + " orders.", true);
            }
            catch (Exception ex)
            {
                Show(lbl, "Error: " + Server.HtmlEncode(ex.Message), false);
            }
        }

        private bool ResolvePeriodFromQuery(string type, out DateTime from, out DateTime toExclusive, out string label)
        {
            from = DateTime.MinValue; toExclusive = DateTime.MinValue; label = "";
            type = (type ?? "").ToLowerInvariant();

            if (type == "range")
            {
                DateTime f, t;
                if (!DateTime.TryParse(Request.QueryString["from"], out f)) return false;
                if (!DateTime.TryParse(Request.QueryString["to"], out t)) return false;
                if (t < f) { var tmp = f; f = t; t = tmp; }

                from = f.Date;
                toExclusive = t.Date.AddDays(1);
                label = f.ToString("yyyy-MM-dd") + " to " + t.ToString("yyyy-MM-dd");
                return true;
            }
            if (type == "monthly")
            {
                string mm = Request.QueryString["month"];
                DateTime m;
                if (string.IsNullOrWhiteSpace(mm) ||
                    !DateTime.TryParseExact(mm, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out m))
                    return false;

                from = new DateTime(m.Year, m.Month, 1);
                toExclusive = from.AddMonths(1);
                label = m.ToString("yyyy-MM");
                return true;
            }
            if (type == "yearly")
            {
                int y;
                if (!int.TryParse(Request.QueryString["year"], out y) || y < 1900 || y > 9999) return false;

                from = new DateTime(y, 1, 1);
                toExclusive = from.AddYears(1);
                label = y.ToString();
                return true;
            }
            return false;
        }

        // helpers
        private T Find<T>(string id) where T : Control
        {
            var ph = Master.FindControl("MainContent");
            return ph != null ? FindRecursive<T>(ph, id) : null;
        }
        private static T FindRecursive<T>(Control root, string id) where T : Control
        {
            if (root == null) return null;
            var c = root.FindControl(id) as T;
            if (c != null) return c;
            foreach (Control child in root.Controls)
            {
                var f = FindRecursive<T>(child, id);
                if (f != null) return f;
            }
            return null;
        }
        private static string Html(object o) { return HttpUtility.HtmlEncode(Convert.ToString(o) ?? ""); }
        private void Show(Label lbl, string msg, bool ok)
        {
            if (lbl == null) return;
            lbl.Text = Server.HtmlEncode(msg);
            lbl.CssClass = ok ? "alert alert-success" : "alert alert-danger";
        }
    }
}
