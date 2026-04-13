using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace serena.Site.Account.Orders
{
    public partial class DownloadPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int memberId = GetMemberId();
            if (memberId <= 0) { WriteError(401, "Please sign in to download this receipt."); return; }

            int orderId; string orderCode; DataRow order;
            if (!TryGetOrder(memberId, out order, out orderId, out orderCode))
            { WriteError(404, "Order not found or you do not have access."); return; }

            var addr = Db.Query("SELECT TOP 1 * FROM dbo.order_addresses WHERE order_id=@oid ORDER BY id DESC",
                                Db.P("@oid", orderId));
            var items = Db.Query(@"
SELECT oi.product_id, oi.quantity, oi.amount, p.name
FROM dbo.order_items oi
LEFT JOIN dbo.products p ON p.id = oi.product_id
WHERE oi.order_id=@oid
ORDER BY oi.id ASC", Db.P("@oid", orderId)) ?? new DataTable();

            bool autoPrint = IsAutoPrint();

            string html = BuildHtml(order, addr, items, autoPrint);

            string namePart = !string.IsNullOrEmpty(orderCode) ? orderCode : ("ID-" + orderId);
            string fileName = "eGadgetHub-Receipt-" + SafeFileName(namePart) + ".html";

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "text/html; charset=utf-8";

            // Only force download when NOT printing
            if (!autoPrint)
            {
                Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentEncoding = Encoding.UTF8;
            Response.Write(html);
            Response.End();

        }

        private bool IsAutoPrint()
        {
            string p = (Request["print"] ?? "").Trim().ToLowerInvariant();
            return p == "1" || p == "true" || p == "yes";
        }

        private int GetMemberId()
        {
            try { return Convert.ToInt32(Session["MEMBER_ID"] ?? 0); } catch { return 0; }
        }

        private string QS(string key) { string v = Request.QueryString[key]; return (v ?? "").Trim(); }

        private bool TryGetOrder(int memberId, out DataRow orderRow, out int orderId, out string orderCode)
        {
            orderRow = null; orderId = 0; orderCode = "";

            string code = QS("code");
            int id = 0; int.TryParse(QS("id"), out id);

            DataTable dt = null;

            if (!string.IsNullOrEmpty(code))
                dt = Db.Query("SELECT TOP 1 * FROM dbo.orders WHERE order_code=@c AND member_id=@m",
                              Db.P("@c", code), Db.P("@m", memberId));

            if ((dt == null || dt.Rows.Count == 0) && id > 0)
                dt = Db.Query("SELECT TOP 1 * FROM dbo.orders WHERE id=@id AND member_id=@m",
                              Db.P("@id", id), Db.P("@m", memberId));

            if (dt == null || dt.Rows.Count == 0) return false;

            orderRow = dt.Rows[0];
            orderId = SafeInt(orderRow["id"]);
            orderCode = Convert.ToString(orderRow["order_code"]);
            return true;
        }

        private string BuildHtml(DataRow o, DataTable addrDt, DataTable items, bool autoPrint)
        {
            string orderCode = HtmlEnc(Convert.ToString(o["order_code"]));
            string status = HtmlEnc(Convert.ToString(o["status"]));
            string payment = HtmlEnc(Convert.ToString(o["payment"]));
            string paymentAdvice = BuildPaymentAdviceHtml(Convert.ToString(o["payment"]));
            int totalQtyH = SafeInt(o["total_qty"]);
            decimal totalAmtH = SafeDec(o["total_amount"]);
            string orderDate = SafeDate(o["order_date"]);

            string shipName = HtmlEnc(Convert.ToString(o["ship_name"]));
            string shipPhone = HtmlEnc(Convert.ToString(o["ship_phone"]));
            string shipAddrHtml = "";
            if (addrDt != null && addrDt.Rows.Count > 0)
            {
                var a = addrDt.Rows[0];
                var sbAddr = new StringBuilder();
                string line1 = SafeStr(a["address"]);
                string line2 = (SafeStr(a["township"]) + " " + SafeStr(a["postal_code"]) + " " + SafeStr(a["city"])).Trim();
                string line3 = SafeStr(a["state"]);
                string line4 = SafeStr(a["country"]);
                if (line1.Length > 0) sbAddr.Append(HtmlEnc(line1) + "<br/>");
                if (line2.Trim().Length > 0) sbAddr.Append(HtmlEnc(line2.Trim()) + "<br/>");
                if (line3.Length > 0) sbAddr.Append(HtmlEnc(line3) + "<br/>");
                if (line4.Length > 0) sbAddr.Append(HtmlEnc(line4));
                shipAddrHtml = sbAddr.ToString();
            }

            int totalQty = 0; decimal totalAmt = 0m;
            var sbItems = new StringBuilder();
            sbItems.Append("<div class='table'><table>");
            sbItems.Append("<thead><tr><th>Product</th><th class='c'>Qty</th><th class='r'>Line Total</th></tr></thead><tbody>");
            foreach (DataRow r in items.Rows)
            {
                int qty = SafeInt(r["quantity"]);
                decimal unit = SafeDec(r["amount"]);
                decimal line = qty * unit;
                totalQty += qty; totalAmt += line;

                string name = SafeStr(r["name"], "Product #" + SafeStr(r["product_id"]));
                sbItems.Append("<tr>");
                sbItems.AppendFormat("<td>{0}</td>", HtmlEnc(name));
                sbItems.AppendFormat("<td class='c'>{0}</td>", qty);
                sbItems.AppendFormat("<td class='r'>RS {0}</td>", line.ToString("N2"));
                sbItems.Append("</tr>");
            }
            sbItems.Append("</tbody></table></div>");

            int finalQty = (totalQtyH > 0 ? totalQtyH : totalQty);
            decimal finalAmt = (totalAmtH > 0m ? totalAmtH : totalAmt);

            var html = new StringBuilder();
            html.Append("<!doctype html><html><head><meta charset='utf-8'/>");
            html.Append("<title>Receipt - " + orderCode + "</title>");
            html.Append(@"
<style>
:root{--bg:#ffffff;--ink:#111827;--muted:#6b7280;--card:#f7f9fc;--line:#e5e7eb;}
body{margin:24px;font-family:system-ui,-apple-system,Segoe UI,Roboto,Ubuntu,Cantarell,'Helvetica Neue',Arial,'Noto Sans',sans-serif;color:var(--ink);background:var(--bg);}
h1,h2,h3{margin:0 0 6px;}
.small{font-size:12px;color:var(--muted);}
.card{border:1px solid var(--line);background:var(--card);border-radius:10px;padding:16px;}
.grid{display:grid;grid-template-columns:1fr 1fr;gap:16px;}
@media(max-width:800px){.grid{grid-template-columns:1fr;}}
dl{margin:0;display:grid;grid-template-columns:120px 1fr;grid-row-gap:8px;}
dt{font-weight:600;color:#374151;}
dd{margin:0;}
.table table{width:100%;border-collapse:collapse;}
.table th,.table td{padding:10px;border-top:1px solid var(--line);}
.table thead th{border-top:0;background:#fafafa;font-weight:600;text-align:left;}
.table .r{text-align:right;}
.table .c{text-align:center;}
.kbd{font-family:ui-monospace, SFMono-Regular, Menlo, Consolas, 'Liberation Mono','Courier New', monospace;background:#f1f5f9;border:1px solid var(--line);border-radius:6px;padding:2px 6px;}
.hr{height:1px;background:var(--line);margin:18px 0;}
@media print {
  body{margin:0;}
}
</style>");
            if (autoPrint)
            {
                html.Append("<script>window.addEventListener('load',function(){try{window.print();}catch(e){}});</script>");
            }
            html.Append("</head><body>");

            html.Append("<h2>Receipt</h2>");
            html.Append("<div class='small'>Order <span class='kbd'>" + orderCode + "</span></div>");
            html.Append("<div class='hr'></div>");

            html.Append("<div class='grid'>");
            html.Append("<div class='card'><h3>Summary</h3><dl>");
            html.Append("<dt>Order Date</dt><dd>" + HtmlEnc(orderDate) + "</dd>");
            html.Append("<dt>Payment</dt><dd>" + payment + "</dd>");
            html.Append("<dt>Total Qty</dt><dd>" + finalQty.ToString(CultureInfo.InvariantCulture) + "</dd>");
            html.Append("<dt>Total Amount</dt><dd>RS " + finalAmt.ToString("N2") + "</dd>");
            html.Append("<dt>Note</dt><dd>" + paymentAdvice + "</dd>");
            html.Append("</dl></div>");

            html.Append("<div class='card'><h3>Shipping</h3><dl>");
            html.Append("<dt>Recipient</dt><dd>" + shipName + "</dd>");
            html.Append("<dt>Phone</dt><dd>" + shipPhone + "</dd>");
            html.Append("<dt>Address</dt><dd>" + (shipAddrHtml.Length > 0 ? shipAddrHtml : "-") + "</dd>");
            html.Append("</dl></div>");
            html.Append("</div>");

            html.Append("<div class='hr'></div>");
            html.Append("<h3>Items</h3>");
            html.Append(sbItems.ToString());

            html.Append("</body></html>");
            return html.ToString();
        }

        // helpers
        private string HtmlEnc(string s) { return HttpUtility.HtmlEncode(s ?? ""); }
        private int SafeInt(object o) { int v; return int.TryParse(Convert.ToString(o), out v) ? v : 0; }
        private decimal SafeDec(object o)
        {
            decimal d;
            if (decimal.TryParse(Convert.ToString(o), NumberStyles.Any, CultureInfo.InvariantCulture, out d)) return d;
            if (decimal.TryParse(Convert.ToString(o), NumberStyles.Any, CultureInfo.CurrentCulture, out d)) return d;
            return 0m;
        }
        private string SafeStr(object o, string fallback = "") { var s = Convert.ToString(o); return string.IsNullOrEmpty(s) ? fallback : s; }
        private string SafeDate(object o) { DateTime dt; return DateTime.TryParse(Convert.ToString(o), out dt) ? dt.ToString("yyyy-MM-dd HH:mm") : "-"; }
        private string BuildPaymentAdviceHtml(string payment)
        {
            string p = (payment ?? string.Empty).Trim().ToLowerInvariant();
            if (p == "cash on delivery") return HtmlEnc("Pay cash to the delivery rider when the order is delivered.");
            if (p == "card") return HtmlEnc("Card payments are recorded for verification. Keep your card available if the delivery team needs to confirm the payment.");
            if (p == "bank") return HtmlEnc("Transfer the payment to the bank account shared by the store and keep the receipt until verified.");
            if (p == "esewa") return HtmlEnc("Your eSewa payment will be verified after redirect completion.");
            return HtmlEnc("Payment will be processed according to the selected method.");
        }
        private string SafeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Receipt";
            name = name.Trim();
            name = Regex.Replace(name, @"[^\w\-]+", "_");
            if (name.Length > 80) name = name.Substring(0, 80);
            return name.Length == 0 ? "Receipt" : name;
        }

        private void WriteError(int status, string message)
        {
            Response.Clear();
            Response.StatusCode = status;
            Response.ContentType = "text/plain; charset=utf-8";
            Response.Write(message ?? "Error");
            Response.End();
        }
    }
}
