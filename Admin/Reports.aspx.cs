using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace serena.Admin
{
    public partial class ReportsPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                SetDefaults();
                RenderDashboard();
            }
        }

        protected void btnExecuteFilter_Click(object sender, EventArgs e)
        {
            RenderDashboard();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            SetDefaults();
            RenderDashboard();
        }

        private void SetDefaults()
        {
            var txtFrom = Find<TextBox>("txtFrom");
            var txtTo = Find<TextBox>("txtTo");
            var txtMonth = Find<TextBox>("txtMonth");
            var txtYear = Find<TextBox>("txtYear");
            var rblType = Find<RadioButtonList>("rblType");

            var today = DateTime.Today;
            if (txtFrom != null) txtFrom.Text = today.AddDays(-29).ToString("yyyy-MM-dd");
            if (txtTo != null) txtTo.Text = today.ToString("yyyy-MM-dd");
            if (txtMonth != null) txtMonth.Text = today.ToString("yyyy-MM");
            if (txtYear != null) txtYear.Text = today.Year.ToString();
            if (rblType != null) rblType.SelectedValue = "range";
        }

        private void RenderDashboard()
        {
            var lbl = Find<Label>("lblMsg");
            if (lbl != null)
            {
                lbl.Text = string.Empty;
                lbl.CssClass = "rp-alert d-none";
            }

            DateTime from;
            DateTime toExclusive;
            string periodLabel;
            if (!ResolvePeriod(out from, out toExclusive, out periodLabel))
            {
                ShowAlert("Please enter valid filter values.", false);
                return;
            }

            SetLit("litPeriod", Html(periodLabel));

            try
            {
                var dt = Db.Query(@"
SELECT o.id, o.order_code, o.total_amount, o.total_qty, o.payment, o.order_date,
       COALESCE(o.ship_name, m.full_name) AS customer_name
FROM orders o
LEFT JOIN members m ON m.id = o.member_id
WHERE o.order_date >= @from AND o.order_date < @to
ORDER BY o.order_date DESC;",
                    Db.P("@from", from), Db.P("@to", toExclusive));

                BuildMetricsAndRows(dt);
            }
            catch (Exception ex)
            {
                ShowAlert("Unable to load reports: " + ex.Message, false);
            }
        }

        private void BuildMetricsAndRows(DataTable dt)
        {
            BuildMetricsAndRows(dt, false);
        }

        private void BuildMetricsAndRows(DataTable dt, bool isDemo)
        {
            var rows = new StringBuilder();
            var revenueByDate = new SortedDictionary<DateTime, decimal>();
            var paymentMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                { "Cash On Delivery", 0m },
                { "eSewa", 0m }
            };

            int totalOrders = 0;
            long totalItems = 0;
            decimal totalRevenue = 0m;

            if (dt == null || dt.Rows.Count == 0)
            {
                if (!isDemo)
                {
                    BuildMetricsAndRows(CreateDemoTransactions(), true);
                    ShowAlert("No report records found. Showing demo data for visualization testing.", true);
                    return;
                }

                rows.Append("<tr><td colspan='6' class='rp-empty'>No transactions found.</td></tr>");
                SetLit("litRows", rows.ToString());
                SetLit("litTotalOrders", "0");
                SetLit("litTotalItems", "0");
                SetLit("litRevenue", "0.00");
                SetLit("litAov", "0.00");
                SetHidden("hidLineLabels", "[]");
                SetHidden("hidLineData", "[]");
                SetHidden("hidPaymentLabels", "[]");
                SetHidden("hidPaymentData", "[]");
                return;
            }

            int index = 0;
            foreach (DataRow r in dt.Rows)
            {
                index++;

                string code = Convert.ToString(r["order_code"] ?? "");
                string customer = Convert.ToString(r["customer_name"] ?? "");
                string payment = Convert.ToString(r["payment"] ?? "");
                DateTime date = r["order_date"] != DBNull.Value ? Convert.ToDateTime(r["order_date"]) : DateTime.MinValue;
                int qty = r["total_qty"] != DBNull.Value ? Convert.ToInt32(r["total_qty"]) : 0;
                decimal amount = r["total_amount"] != DBNull.Value ? Convert.ToDecimal(r["total_amount"]) : 0m;

                totalOrders++;
                totalItems += qty;
                totalRevenue += amount;

                if (date != DateTime.MinValue)
                {
                    var key = date.Date;
                    decimal prev;
                    revenueByDate.TryGetValue(key, out prev);
                    revenueByDate[key] = prev + amount;
                }

                var paymentKey = NormalizePaymentForChart(payment);
                if (!string.IsNullOrEmpty(paymentKey))
                {
                    paymentMap[paymentKey] += amount;
                }

                rows.Append("<tr>");
                rows.Append("<td>").Append(index).Append("</td>");
                rows.Append("<td>#").Append(Html(code)).Append("</td>");
                rows.Append("<td>").Append(string.IsNullOrWhiteSpace(customer) ? "-" : Html(customer)).Append("</td>");
                rows.Append("<td>").Append(string.IsNullOrWhiteSpace(payment) ? "-" : Html(payment)).Append("</td>");
                rows.Append("<td class='right'>RS ").Append(amount.ToString("N2")).Append("</td>");
                rows.Append("<td>").Append(date == DateTime.MinValue ? "-" : date.ToString("yyyy-MM-dd HH:mm")).Append("</td>");
                rows.Append("</tr>");
            }

            var aov = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

            SetLit("litRows", rows.ToString());
            SetLit("litTotalOrders", totalOrders.ToString("N0"));
            SetLit("litTotalItems", totalItems.ToString("N0"));
            SetLit("litRevenue", totalRevenue.ToString("N2"));
            SetLit("litAov", aov.ToString("N2"));

            SetHidden("hidLineLabels", ToJsStringArray(revenueByDate.Keys));
            SetHidden("hidLineData", ToJsDecimalArray(revenueByDate.Values));
            SetHidden("hidPaymentLabels", ToJsStringArray(paymentMap.Keys));
            SetHidden("hidPaymentData", ToJsDecimalArray(paymentMap.Values));
        }

        private static DataTable CreateDemoTransactions()
        {
            var dt = new DataTable();
            dt.Columns.Add("order_code", typeof(string));
            dt.Columns.Add("customer_name", typeof(string));
            dt.Columns.Add("payment", typeof(string));
            dt.Columns.Add("order_date", typeof(DateTime));
            dt.Columns.Add("total_qty", typeof(int));
            dt.Columns.Add("total_amount", typeof(decimal));

            var today = DateTime.Today;
            dt.Rows.Add("EG-1001", "Aarav Sharma", "Cash On Delivery", today.AddDays(-6).AddHours(10), 2, 15400m);
            dt.Rows.Add("EG-1002", "Emma Smith", "eSewa", today.AddDays(-5).AddHours(11), 1, 8200m);
            dt.Rows.Add("EG-1003", "Noah Joshi", "eSewa", today.AddDays(-4).AddHours(14), 3, 27600m);
            dt.Rows.Add("EG-1004", "Olivia Karki", "Cash On Delivery", today.AddDays(-3).AddHours(15), 1, 6900m);
            dt.Rows.Add("EG-1005", "Liam Thapa", "eSewa", today.AddDays(-2).AddHours(12), 2, 13300m);
            dt.Rows.Add("EG-1006", "Sophia Rana", "Cash On Delivery", today.AddDays(-1).AddHours(17), 4, 34500m);
            dt.Rows.Add("EG-1007", "Mason Adhikari", "eSewa", today.AddHours(9), 1, 5600m);

            return dt;
        }

        private static string NormalizePaymentForChart(string payment)
        {
            var p = (payment ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(p)) return null;
            if (p.Contains("esewa")) return "eSewa";
            if (p == "cod" || p.Contains("cash on delivery") || p.Contains("cashondelivery")) return "Cash On Delivery";
            return null;
        }

        private T Find<T>(string id) where T : Control
        {
            var ph = Master != null ? Master.FindControl("MainContent") : null;
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

        private void SetLit(string id, string text)
        {
            var lit = Find<Literal>(id);
            if (lit != null) lit.Text = text ?? "";
        }

        private void SetHidden(string id, string value)
        {
            var h = Find<HiddenField>(id);
            if (h != null) h.Value = value ?? "";
        }

        private void ShowAlert(string message, bool ok)
        {
            var lbl = Find<Label>("lblMsg");
            if (lbl == null) return;
            lbl.Text = Html(message);
            lbl.CssClass = ok ? "rp-alert rp-alert-success" : "rp-alert rp-alert-danger";
        }

        private bool ResolvePeriod(out DateTime from, out DateTime toExclusive, out string label)
        {
            from = DateTime.MinValue; toExclusive = DateTime.MinValue; label = "";

            string type = "range";
            var rbl = Find<RadioButtonList>("rblType");
            if (rbl != null && !string.IsNullOrWhiteSpace(rbl.SelectedValue)) type = rbl.SelectedValue.Trim().ToLowerInvariant();

            if (type == "range")
            {
                var tf = Find<TextBox>("txtFrom");
                var tt = Find<TextBox>("txtTo");
                DateTime f, t;
                if (tf == null || tt == null) return false;
                if (!DateTime.TryParse(tf.Text, out f)) return false;
                if (!DateTime.TryParse(tt.Text, out t)) return false;
                if (t < f) { var tmp = f; f = t; t = tmp; }
                from = f.Date;
                toExclusive = t.Date.AddDays(1);
                label = f.ToString("dd MMM yyyy") + " to " + t.ToString("dd MMM yyyy");
                return true;
            }
            if (type == "monthly")
            {
                var tm = Find<TextBox>("txtMonth");
                DateTime m;
                if (tm == null || string.IsNullOrWhiteSpace(tm.Text)) return false;
                if (!DateTime.TryParseExact(tm.Text, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out m))
                    return false;
                from = new DateTime(m.Year, m.Month, 1);
                toExclusive = from.AddMonths(1);
                label = m.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
                return true;
            }
            if (type == "yearly")
            {
                var ty = Find<TextBox>("txtYear");
                int y;
                if (ty == null || !int.TryParse(ty.Text, out y) || y < 1900 || y > 9999) return false;
                from = new DateTime(y, 1, 1);
                toExclusive = from.AddYears(1);
                label = "Year " + y.ToString();
                return true;
            }
            return false;
        }

        private static string Html(object o)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(o) ?? "");
        }

        private static string ToJsStringArray(IEnumerable<string> values)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            bool first = true;
            foreach (var value in values)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append("\"").Append(HttpUtility.JavaScriptStringEncode(value ?? "")).Append("\"");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private static string ToJsStringArray(IEnumerable<DateTime> values)
        {
            var list = new List<string>();
            foreach (var d in values) list.Add(d.ToString("yyyy-MM-dd"));
            return ToJsStringArray(list);
        }

        private static string ToJsDecimalArray(IEnumerable<decimal> values)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            bool first = true;
            foreach (var value in values)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append(value.ToString("0.##", CultureInfo.InvariantCulture));
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
