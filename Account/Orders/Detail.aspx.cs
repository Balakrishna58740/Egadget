using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace serena.Site.Account.Orders
{
    public partial class OrderDetailPage : Page
    {
        private int _orderId;
        private string _orderCode = "";

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int memberId = GetMemberId();
            if (memberId <= 0)
            {
                string ru = HttpUtility.UrlEncode(Request.RawUrl);
                Response.Redirect("~/Account/Login.aspx?returnUrl=" + ru, true);
                return;
            }

            if (!IsPostBack)
            {
                LoadOrder(memberId);
            }
        }

        private int GetMemberId()
        {
            try { return Convert.ToInt32(Session["MEMBER_ID"] ?? 0); } catch { return 0; }
        }

        private void LoadOrder(int memberId)
        {
            string code = (Request["code"] ?? "").Trim();
            if (string.IsNullOrEmpty(code)) { ShowAlert("Order code is missing.", "warning"); return; }

            var dt = Db.Query(@"SELECT TOP 1 * FROM dbo.orders WHERE order_code=@c AND member_id=@m;",
                new SqlParameter("@c", code), new SqlParameter("@m", memberId));
            if (dt == null || dt.Rows.Count == 0) { ShowAlert("Order not found.", "warning"); return; }

            var o = dt.Rows[0];
            _orderId = SafeInt(o["id"]);
            _orderCode = Convert.ToString(o["order_code"]);

            // Buttons
            var litOrderCode = Find<Literal>("litOrderCode");
            var lnkReceipt = Find<HyperLink>("lnkReceipt");
            var lblStatus = Find<HtmlGenericControl>("lblStatus");
            var litPayment = Find<Literal>("litPayment");
            var litOrderDate = Find<Literal>("litOrderDate");
            var litTotalQty = Find<Literal>("litTotalQty");
            var litTotalAmt = Find<Literal>("litTotalAmt");
            var litShipName = Find<Literal>("litShipName");
            var litShipPhone = Find<Literal>("litShipPhone");
            var litShipAddr = Find<Literal>("litShipAddr");
            var litItemsTable = Find<Literal>("litItemsTable");
            var rptLogs = Find<Repeater>("rptLogs");
            var phNoLogs = Find<PlaceHolder>("phNoLogs");
            var alertMsg = Find<HtmlGenericControl>("alertMsg");

            if (litOrderCode != null) litOrderCode.Text = Server.HtmlEncode(_orderCode);
            if (lnkReceipt != null) lnkReceipt.NavigateUrl = ResolveUrl("~/Account/Orders/Receipt.aspx?code=" + HttpUtility.UrlEncode(_orderCode));
            // NO: lnkReceipt.Target / rel

            // Summary (uppercase status)
            string status = NormalizeStatus(Convert.ToString(o["status"]));
            string statusUpper = Upper(status);
            if (lblStatus != null)
            {
                lblStatus.InnerText = statusUpper;
                lblStatus.Attributes["class"] = "badge rounded-pill " + StatusBadgeCss(status);
            }

            if (litPayment != null) litPayment.Text = Server.HtmlEncode(Convert.ToString(o["payment"]));
            if (litOrderDate != null) litOrderDate.Text = FmtDate(o["order_date"]);
            if (litTotalQty != null) litTotalQty.Text = SafeInt(o["total_qty"]).ToString(CultureInfo.InvariantCulture);
            if (litTotalAmt != null) litTotalAmt.Text = SafeDec(o["total_amount"]).ToString("N2");

            // Shipping
            var addrDt = Db.Query("SELECT TOP 1 * FROM dbo.order_addresses WHERE order_id=@id ORDER BY id DESC;",
                new SqlParameter("@id", _orderId));
            if (addrDt != null && addrDt.Rows.Count > 0)
            {
                var a = addrDt.Rows[0];
                if (litShipName != null) litShipName.Text = Server.HtmlEncode(Convert.ToString(o["ship_name"]));
                if (litShipPhone != null) litShipPhone.Text = Server.HtmlEncode(Convert.ToString(o["ship_phone"]));
                var sb = new StringBuilder();
                string line1 = SafeStr(a["address"]);
                string line2 = (SafeStr(a["township"]) + " " + SafeStr(a["postal_code"]) + " " + SafeStr(a["city"])).Trim();
                string line3 = SafeStr(a["state"]);
                string line4 = SafeStr(a["country"]);
                if (line1.Length > 0) sb.Append(Server.HtmlEncode(line1) + "<br/>");
                if (line2.Length > 0) sb.Append(Server.HtmlEncode(line2) + "<br/>");
                if (line3.Length > 0) sb.Append(Server.HtmlEncode(line3) + "<br/>");
                if (line4.Length > 0) sb.Append(Server.HtmlEncode(line4));
                if (litShipAddr != null) litShipAddr.Text = sb.ToString();
            }
            else
            {
                if (litShipName != null) litShipName.Text = Server.HtmlEncode(Convert.ToString(o["ship_name"]));
                if (litShipPhone != null) litShipPhone.Text = Server.HtmlEncode(Convert.ToString(o["ship_phone"]));
                if (litShipAddr != null) litShipAddr.Text = "-";
            }

            // Items (dark)
            var items = Db.Query(@"
SELECT oi.product_id, oi.quantity, oi.amount, p.name
FROM dbo.order_items oi
LEFT JOIN dbo.products p ON p.id = oi.product_id
WHERE oi.order_id=@oid
ORDER BY oi.id ASC;",
                new SqlParameter("@oid", _orderId)) ?? new DataTable();

            var sbTable = new StringBuilder();
            sbTable.Append("<table class='w-full text-left'>");
            sbTable.Append("<thead><tr><th class='py-4'>Product</th><th class='py-4 text-center'>Qty</th><th class='py-4 text-right'>Line Total</th></tr></thead><tbody>");
            foreach (DataRow r in items.Rows)
            {
                int qty = SafeInt(r["quantity"]);
                decimal line = SafeDec(r["amount"]); // amount column actually stores the line subtotal in this BLL

                sbTable.Append("<tr class='border-b border-gray-50'>");
                sbTable.AppendFormat("<td class='py-4'>{0}</td>", Server.HtmlEncode(SafeStr(r["name"], "Product #" + SafeStr(r["product_id"]))));
                sbTable.AppendFormat("<td class='py-4 text-center'>{0}</td>", qty);
                sbTable.AppendFormat("<td class='py-4 text-right font-bold'>RS {0}</td>", line.ToString("N2"));
                sbTable.Append("</tr>");
            }
            sbTable.Append("</tbody></table>");
            if (litItemsTable != null) litItemsTable.Text = sbTable.ToString();

            // Activity
            var logs = Db.Query(@"SELECT status, created_at FROM dbo.order_logs WHERE order_id=@id ORDER BY id ASC;",
                new SqlParameter("@id", _orderId));
            if (logs != null && logs.Rows.Count > 0)
            {
                if (rptLogs != null)
                {
                    rptLogs.DataSource = logs;
                    rptLogs.DataBind();
                }
                if (phNoLogs != null) phNoLogs.Visible = false;
            }
            else
            {
                if (phNoLogs != null) phNoLogs.Visible = true;
            }
        }

        /* Helpers */
        private void ShowAlert(string message, string type)
        {
            var alertMsg = Find<HtmlGenericControl>("alertMsg");
            if (alertMsg == null) return;
            alertMsg.InnerHtml =
                "<div class='alert alert-" + (type ?? "info") + " alert-dismissible fade show' role='alert'>" +
                Server.HtmlEncode(message ?? "") +
                "<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>" +
                "</div>";
        }

        private T Find<T>(string id) where T : Control
        {
            var root = Master != null ? Master.FindControl("MainContent") : null;
            return root != null ? FindRecursive<T>(root, id) : null;
        }

        private static T FindRecursive<T>(Control root, string id) where T : Control
        {
            if (root == null) return null;
            var found = root.FindControl(id) as T;
            if (found != null) return found;
            foreach (Control child in root.Controls)
            {
                var nested = FindRecursive<T>(child, id);
                if (nested != null) return nested;
            }
            return null;
        }

        public string FmtDate(object o) { try { return Convert.ToDateTime(o).ToString("yyyy-MM-dd HH:mm"); } catch { return "-"; } }
        public string Upper(object s)
        {
            var t = NormalizeStatus(Convert.ToString(s));
            return string.IsNullOrEmpty(t) ? "" : t.ToUpperInvariant();
        }

        private static string NormalizeStatus(string s)
        {
            string v = (s ?? "").Trim().ToLowerInvariant();
            if (v == "paid") return "accepted";
            if (v == "delivering") return "inprocess";
            if (v == "completed") return "delivered";
            return v;
        }

        private string StatusBadgeCss(string statusObj)
        {
            string s = (Convert.ToString(statusObj) ?? "").ToLowerInvariant();
            if (s.IndexOf("pending") >= 0) return "bg-warning text-dark";
            if (s.IndexOf("accepted") >= 0 || s.IndexOf("paid") >= 0) return "bg-info";
            if (s.IndexOf("inprocess") >= 0 || s.IndexOf("delivering") >= 0) return "bg-primary";
            if (s.IndexOf("delivered") >= 0 || s.IndexOf("completed") >= 0) return "bg-success";
            if (s.IndexOf("canceled") >= 0) return "bg-danger";
            return "bg-light text-dark";
        }

        private int SafeInt(object o) { try { return Convert.ToInt32(o); } catch { return 0; } }
        private decimal SafeDec(object o)
        {
            decimal d;
            if (decimal.TryParse(Convert.ToString(o), NumberStyles.Any, CultureInfo.InvariantCulture, out d)) return d;
            if (decimal.TryParse(Convert.ToString(o), NumberStyles.Any, CultureInfo.CurrentCulture, out d)) return d;
            return 0m;
        }
        private string SafeStr(object o, string fallback = "") { string s = Convert.ToString(o); return string.IsNullOrEmpty(s) ? fallback : s; }
    }
}
