using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace serena.Admin
{
    public partial class ReportsPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                string tab = (Request.QueryString["tab"] ?? "product").ToLowerInvariant();

                var panProduct = Find<Panel>("panProduct");
                var panOrder = Find<Panel>("panOrder");
                if (panProduct != null) panProduct.Visible = (tab != "order");
                if (panOrder != null) panOrder.Visible = (tab == "order");

                var prod = Find<HtmlAnchor>("lnkTabProduct");
                var ord = Find<HtmlAnchor>("lnkTabOrder");
                
                string baseTabCls = "px-6 py-4 text-xs uppercase tracking-widest font-bold flex items-center gap-3 transition-all hover:bg-off-white group border-t border-gray-50";
                string firstTabCls = "px-6 py-4 text-xs uppercase tracking-widest font-bold flex items-center gap-3 transition-all hover:bg-off-white group";
                
                if (prod != null) prod.Attributes["class"] = firstTabCls + ((panProduct != null && panProduct.Visible) ? " active-tab" : "");
                if (ord != null) ord.Attributes["class"] = baseTabCls + ((panOrder != null && panOrder.Visible) ? " active-tab" : "");

                if (panProduct != null && panProduct.Visible)
                {
                    var lg = Find<Literal>("litProdGenerated");
                    if (lg != null) lg.Text = DateTime.Now.ToString("dd MMM, yyyy · HH:mm");
                }
                else
                {
                    var lg = Find<Literal>("litOrderGenerated");
                    if (lg != null) lg.Text = DateTime.Now.ToString("dd MMM, yyyy · HH:mm");

                    var ym = Find<TextBox>("txtMonth"); if (ym != null && string.IsNullOrEmpty(ym.Text)) ym.Text = DateTime.Now.ToString("yyyy-MM");
                    var yy = Find<TextBox>("txtYear"); if (yy != null && string.IsNullOrEmpty(yy.Text)) yy.Text = DateTime.Now.Year.ToString();
                }
            }
        }

        // ---------------- PRODUCT REPORT ----------------
        protected void btnProdGet_Click(object sender, EventArgs e)
        {
            var lbl = Find<Label>("lblMsg");
            var litGen = Find<Literal>("litProdGenerated");
            var litSum = Find<Literal>("litProdSummary");
            var lit = Find<Literal>("litProdRows");

            try
            {
                if (litGen != null) litGen.Text = DateTime.Now.ToString("dd MMM, yyyy · HH:mm");

                var dt = Db.Query("SELECT id, name, stock FROM products ORDER BY name ASC;");

                var sb = new StringBuilder();
                int totalSkus = dt.Rows.Count;
                long totalStock = 0;

                if (totalSkus == 0)
                {
                    sb.Append("<tr><td colspan='3' class='px-8 py-12 text-center text-gray-400 text-xs italic'>The collection repository currently houses no records.</td></tr>");
                }
                else
                {
                    int i = 0;
                    foreach (DataRow r in dt.Rows)
                    {
                        i++;
                        string name = Html(r["name"]);
                        int stock = r["stock"] != DBNull.Value ? Convert.ToInt32(r["stock"]) : 0;
                        totalStock += stock;

                        sb.Append("<tr class='hover:bg-off-white/30 transition-colors'>");
                        sb.Append("<td class='px-8 py-4 text-[10px] uppercase tracking-widest font-bold text-gray-400'>").Append(i.ToString("D2")).Append("</td>");
                        sb.Append("<td class='px-8 py-4 text-sm font-serif text-text-dark font-bold'>").Append(string.IsNullOrEmpty(name) ? "—" : name).Append("</td>");
                        sb.Append("<td class='px-8 py-4 text-right font-bold text-text-dark text-sm'>").Append(stock.ToString("N0")).Append("</td>");
                        sb.Append("</tr>");
                    }
                }
                if (lit != null) lit.Text = sb.ToString();

                if (litSum != null)
                {
                    litSum.Text = string.Format(@"
                        <div class='grid grid-cols-1 md:grid-cols-2 gap-4'>
                            <div class='bg-off-white/50 p-4 border border-gray-100'>
                                <span class='text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1'>Archetype Magnitude</span>
                                <span class='text-lg font-bold text-text-dark'>{0:N0} SKUs</span>
                            </div>
                            <div class='bg-off-white/50 p-4 border border-gray-100'>
                                <span class='text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1'>Cubic Volume Balance</span>
                                <span class='text-lg font-bold text-text-dark'>{1:N0} Units</span>
                            </div>
                        </div>", totalSkus, totalStock);
                }
            }
            catch (Exception ex)
            {
                if (lit != null) lit.Text = "<tr><td colspan='3' class='px-8 py-12 text-center text-gray-400 text-xs italic'>Collection assessment interrupted.</td></tr>";
                if (lbl != null) { lbl.Text = "System error: " + HttpUtility.HtmlEncode(ex.Message); lbl.Visible = true; }
            }
        }

        protected void btnProdPrint_Click(object sender, EventArgs e)
        {
            string url = ResolveUrl("~/Admin/ReportProduct.aspx");
            EmitPrint(url);
        }

        // ---------------- ORDER REVENUE REPORT ----------------
        protected void btnOrderGet_Click(object sender, EventArgs e)
        {
            var lbl = Find<Label>("lblMsg");
            var litGen = Find<Literal>("litOrderGenerated");
            var litPeriod = Find<Literal>("litOrderPeriod");
            var litSum = Find<Literal>("litOrderSummary");
            var lit = Find<Literal>("litOrderRows");

            try
            {
                if (litGen != null) litGen.Text = DateTime.Now.ToString("dd MMM, yyyy · HH:mm");

                string type = GetRbl("rblType"); if (string.IsNullOrEmpty(type)) type = "range";

                DateTime from, toExclusive;
                string periodLabel;
                if (!ResolvePeriod(type, out from, out toExclusive, out periodLabel))
                {
                    if (lit != null) lit.Text = "";
                    if (litSum != null) litSum.Text = "";
                    if (litPeriod != null) litPeriod.Text = "";
                    if (lbl != null) { lbl.Text = "Please define a valid temporal scope."; lbl.Visible = true; }
                    return;
                }
                if (litPeriod != null) litPeriod.Text = periodLabel;

                var dt = Db.Query(@"
SELECT o.id, o.order_code, o.total_amount, o.total_qty, o.payment, o.order_date,
       COALESCE(o.ship_name, m.full_name) AS customer_name
FROM orders o
LEFT JOIN members m ON m.id = o.member_id
WHERE LOWER(COALESCE(o.status,'')) IN ('delivered', 'completed')
  AND o.order_date >= @from AND o.order_date < @to
ORDER BY o.order_date DESC;",
                    Db.P("@from", from), Db.P("@to", toExclusive));

                var sb = new StringBuilder();
                int count = 0;
                long sumQty = 0;
                decimal sumAmt = 0m;

                if (dt.Rows.Count == 0)
                {
                    sb.Append("<tr><td colspan='6' class='px-8 py-12 text-center text-gray-400 text-xs italic'>No fiscal activity documented within this temporal scope.</td></tr>");
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

                        sb.Append("<tr class='hover:bg-off-white/30 transition-colors'>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-400'>").Append(i.ToString("D2")).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-text-dark'>#").Append(code).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-sm font-serif text-text-dark font-bold'>").Append(cust).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-400'>").Append(payment).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-right font-bold text-text-dark text-sm'>RS ").Append(amt.ToString("N2")).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-primary'>").Append(od == DateTime.MinValue ? "—" : od.ToString("dd MMM, yyyy")).Append("</td>");
                        sb.Append("</tr>");
                    }
                }

                if (lit != null) lit.Text = sb.ToString();
                if (litSum != null)
                {
                    litSum.Text = string.Format(@"
                        <div class='grid grid-cols-1 md:grid-cols-3 gap-6'>
                            <div class='flex flex-col'>
                                <span class='text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1'>Transaction Magnitude</span>
                                <span class='text-lg font-bold text-text-dark'>{0:N0} Orders</span>
                            </div>
                            <div class='flex flex-col'>
                                <span class='text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1'>Acquisition Volume</span>
                                <span class='text-lg font-bold text-text-dark'>{1:N0} Items</span>
                            </div>
                            <div class='flex flex-col'>
                                <span class='text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1'>Fiscal Magnitude</span>
                                <span class='text-lg font-bold text-primary'>RS {2:N2}</span>
                            </div>
                        </div>", count, sumQty, sumAmt);
                }
            }
            catch (Exception ex)
            {
                if (lbl != null) { lbl.Text = "Fiscal assessment disrupted: " + HttpUtility.HtmlEncode(ex.Message); lbl.Visible = true; }
            }
        }

        protected void btnOrderPrint_Click(object sender, EventArgs e)
        {
            string type = GetRbl("rblType"); if (string.IsNullOrEmpty(type)) type = "range";

            DateTime from, toExclusive;
            string periodLabel;
            if (!ResolvePeriod(type, out from, out toExclusive, out periodLabel))
            {
                var lbl = Find<Label>("lblMsg");
                if (lbl != null) { lbl.Text = "Please define a valid temporal scope for export."; lbl.Visible = true; }
                return;
            }

            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["type"] = type;
            if (type == "range")
            {
                qs["from"] = from.ToString("yyyy-MM-dd");
                qs["to"] = toExclusive.AddDays(-1).ToString("yyyy-MM-dd");
            }
            else if (type == "monthly")
            {
                qs["month"] = from.ToString("yyyy-MM");
            }
            else
            {
                qs["year"] = from.Year.ToString();
            }

            string url = ResolveUrl("~/Admin/ReportOrder.aspx?" + qs.ToString());
            EmitPrint(url);
        }

        // ---------------- helpers ----------------
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

        private static string Html(object o) { return HttpUtility.HtmlEncode(Convert.ToString(o) ?? ""); }

        private string GetRbl(string id)
        {
            var r = Find<RadioButtonList>(id);
            return r != null ? (r.SelectedValue ?? "") : "";
        }

        private bool ResolvePeriod(string type, out DateTime from, out DateTime toExclusive, out string label)
        {
            from = DateTime.MinValue; toExclusive = DateTime.MinValue; label = "";
            type = (type ?? "").ToLowerInvariant();

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
                label = f.ToString("dd MMM yyyy") + " — " + t.ToString("dd MMM yyyy");
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
                label = m.ToString("MMMM yyyy");
                return true;
            }
            if (type == "yearly")
            {
                var ty = Find<TextBox>("txtYear");
                int y;
                if (ty == null || !int.TryParse(ty.Text, out y) || y < 1900 || y > 9999) return false;
                from = new DateTime(y, 1, 1);
                toExclusive = from.AddYears(1);
                label = "Annual Briefing " + y.ToString();
                return true;
            }
            return false;
        }

        private void EmitPrint(string url)
        {
            string safe = (url ?? "").Replace("\\", "\\\\").Replace("'", "\\'");
            string js = "printTemplate('" + safe + "');";
            ClientScript.RegisterStartupScript(this.GetType(), "print_" + Guid.NewGuid().ToString("N"), js, true);
        }
    }
}
