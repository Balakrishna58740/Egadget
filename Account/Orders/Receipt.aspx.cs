using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;

namespace serena.Site.Account.Orders
{
    public partial class ReceiptPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RequireLogin();
            if (!IsPostBack) LoadReceipt();
        }

        private void RequireLogin()
        {
            if (Session["MEMBER_ID"] == null)
            {
                string returnUrl = Server.UrlEncode(Request.RawUrl);
                Response.Redirect("~/Account/Login.aspx?returnUrl=" + returnUrl);
            }
        }

        private void LoadReceipt()
        {
            int id = 0; int.TryParse(Request["id"] ?? "0", out id);
            string code = (Request["code"] ?? "").Trim();

            int sessionMid = Convert.ToInt32(Session["MEMBER_ID"]);
            DataTable orderDt = null;

            if (id > 0) orderDt = Db.Query("SELECT TOP 1 * FROM dbo.orders WHERE id=@id", Db.P("@id", id));
            if ((orderDt == null || orderDt.Rows.Count == 0) && !string.IsNullOrEmpty(code))
                orderDt = Db.Query("SELECT TOP 1 * FROM dbo.orders WHERE order_code=@code", Db.P("@code", code));

            if (orderDt == null || orderDt.Rows.Count == 0) { ShowAlert("Order not found.", "warning"); return; }

            var o = orderDt.Rows[0];
            int ownerMid = Convert.ToInt32(o["member_id"]);
            if (ownerMid != sessionMid) { ShowAlert("This order belongs to a different account.", "warning"); return; }

            int orderId = Convert.ToInt32(o["id"]);
            string ocode = Convert.ToString(o["order_code"]);
            string status = NormalizeStatus(Convert.ToString(o["status"]));
            string payment = Convert.ToString(o["payment"]);

            // Buttons
            litOrderCode.Text = Server.HtmlEncode(ocode);
            lnkBackDetail.NavigateUrl = ResolveUrl("~/Account/Orders/Detail.aspx?code=" + HttpUtility.UrlEncode(ocode));
            lnkBackList.NavigateUrl = ResolveUrl("~/Account/Orders/Index.aspx");
            // PRINT => Download.aspx in a new tab; add print=1 to force inline/print
            lnkPrint.NavigateUrl = ResolveUrl(
                "~/Account/Orders/Download.aspx?code=" + HttpUtility.UrlEncode(ocode) + "&print=1"
            );
            lnkPrint.Target = "_blank";
            lnkPrint.Attributes["rel"] = "noopener";

            // Status (uppercase + badge color)
            string statusUpper = Upper(status);
            litOrderStatus.InnerText = statusUpper;
            litOrderStatus.Attributes["class"] = "badge rounded-pill " + StatusBadgeCss(status);

            // Summary
            litPayment.Text = Server.HtmlEncode(payment);
            litPaymentAdvice.Text = BuildPaymentAdvice(payment);
            litOrderDate.Text = FmtDate(o["order_date"]);
            litTotalQty.Text = Convert.ToInt32(o["total_qty"]).ToString(CultureInfo.InvariantCulture);
            litTotalAmt.Text = Convert.ToDecimal(o["total_amount"]).ToString("N2");

            // Shipping
            var addrDt = Db.Query("SELECT TOP 1 * FROM dbo.order_addresses WHERE order_id=@oid", Db.P("@oid", orderId));
            if (addrDt != null && addrDt.Rows.Count > 0)
            {
                var a = addrDt.Rows[0];
                var sb = new StringBuilder();
                string line1 = (a["address"] + "").Trim();
                string line2 = string.Format("{0} {1} {2}", (a["township"] + "").Trim(), (a["postal_code"] + "").Trim(), (a["city"] + "").Trim()).Trim();
                string line3 = (a["state"] + "").Trim();
                string line4 = (a["country"] + "").Trim();
                if (line1 != "") sb.Append(Server.HtmlEncode(line1) + "<br/>");
                if (line2 != "") sb.Append(Server.HtmlEncode(line2) + "<br/>");
                if (line3 != "") sb.Append(Server.HtmlEncode(line3) + "<br/>");
                if (line4 != "") sb.Append(Server.HtmlEncode(line4));
                litShipAddr.Text = sb.ToString();
            }
            litShipName.Text = Server.HtmlEncode(Convert.ToString(o["ship_name"]));
            litShipPhone.Text = Server.HtmlEncode(Convert.ToString(o["ship_phone"]));

            // Items (dark table)
            var items = Db.Query(@"
SELECT oi.product_id, oi.quantity, oi.amount, p.name
FROM dbo.order_items oi
LEFT JOIN dbo.products p ON p.id = oi.product_id
WHERE oi.order_id=@oid
ORDER BY oi.id ASC", Db.P("@oid", orderId));

            var sbItems = new StringBuilder();
            sbItems.Append("<div class='table-responsive'><table class='table table-dark table-hover align-middle mb-0'>");
            sbItems.Append("<thead><tr><th>Product</th><th class='text-center'>Qty</th><th class='text-end'>Line Total</th></tr></thead><tbody>");
            foreach (DataRow r in items.Rows)
            {
                int qty = Convert.ToInt32(r["quantity"]);
                decimal lineTotal = 0m; decimal.TryParse(r["amount"] + "", out lineTotal);

                sbItems.Append("<tr>");
                sbItems.AppendFormat("<td>{0}</td>", Server.HtmlEncode(r["name"] + ""));
                sbItems.AppendFormat("<td class='text-center'>{0}</td>", qty);
                sbItems.AppendFormat("<td class='text-end'>RS {0}</td>", lineTotal.ToString("N2"));
                sbItems.Append("</tr>");
            }
            sbItems.Append("</tbody></table></div>");
            litItemsTable.Text = sbItems.ToString();
        }

        private void ShowAlert(string message, string type)
        {
            alertMsg.Visible = true;
            alertMsg.InnerHtml =
                "<div class='alert alert-" + type + " alert-dismissible fade show' role='alert'>" +
                Server.HtmlEncode(message ?? "") +
                "<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button>" +
                "</div>";
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

        public string FmtDate(object o) { try { return Convert.ToDateTime(o).ToString("yyyy-MM-dd HH:mm"); } catch { return "-"; } }
        public string Upper(object s)
        {
            string t = NormalizeStatus(Convert.ToString(s));
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

        private string BuildPaymentAdvice(string payment)
        {
            string p = (payment ?? string.Empty).Trim().ToLowerInvariant();
            if (p == "cash on delivery")
            {
                return "Pay cash to the delivery rider when your order arrives. Please keep the recipient phone active for delivery confirmation.";
            }

            if (p == "card")
            {
                return "Card payments are recorded for verification. Please keep your card available if the delivery team needs to confirm payment details.";
            }

            if (p == "bank")
            {
                return "Transfer the total amount to the bank account shared by the store and keep the transaction slip until your order is verified.";
            }

            return "Payment will be processed according to the selected method.";
        }
    }
}
