using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace serena.Site.Account.Orders
{
    public partial class EsewaSuccessPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string dataB64 = Request.QueryString["data"];
            if (string.IsNullOrEmpty(dataB64))
            {
                Response.Redirect("~/Account/Orders/Index.aspx");
                return;
            }

            try
            {
                // Decode data from eSewa v2
                byte[] dataBytes = Convert.FromBase64String(dataB64);
                string decodedJson = Encoding.UTF8.GetString(dataBytes);
                string status = ExtractValue(decodedJson, "status");
                
                // Example JSON: {"transaction_code":"00010OT","status":"COMPLETE","total_amount":"100.0","transaction_uuid":"SS-20240214-12345","product_code":"EPAYTEST","signature":"..."}
                // We should parse it properly. For now, we'll extract the UUID (Order Code).
                
                string orderCode = ExtractValue(decodedJson, "transaction_uuid");
                if (string.IsNullOrEmpty(orderCode)) throw new Exception("Invalid response");

                if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "COMPLETE", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Payment not completed.");

                var litOrderCode = Find<Literal>("litOrderCode");
                var lnkOrder = Find<HyperLink>("lnkOrder");
                if (litOrderCode != null) litOrderCode.Text = orderCode;
                if (lnkOrder != null) lnkOrder.NavigateUrl = "~/Account/Orders/Detail.aspx?code=" + orderCode;

                // Update order status in DB
                Db.Execute("UPDATE dbo.orders SET status = 'processing', updated_at = GETDATE() WHERE order_code = @code AND status = 'pending'",
                            Db.P("@code", orderCode));

                DataTable orderDt = Db.Query(@"
SELECT TOP 1 o.id, o.total_amount, o.payment, m.full_name, m.email
FROM dbo.orders o
LEFT JOIN dbo.members m ON m.id = o.member_id
WHERE o.order_code = @code",
                    Db.P("@code", orderCode));

                int orderId = 0;
                decimal totalAmount = 0m;
                string paymentMethod = "eSewa";
                string customerName = "";
                string customerEmail = "";

                if (orderDt != null && orderDt.Rows.Count > 0)
                {
                    var row = orderDt.Rows[0];
                    int.TryParse(Convert.ToString(row["id"]), out orderId);
                    decimal.TryParse(Convert.ToString(row["total_amount"]), out totalAmount);
                    paymentMethod = Convert.ToString(row["payment"]) ?? paymentMethod;
                    customerName = Convert.ToString(row["full_name"]) ?? "";
                    customerEmail = Convert.ToString(row["email"]) ?? "";
                }
                
                // Log the payment
                int anyAdminId = 0;
                try { anyAdminId = Db.Scalar<int>("SELECT TOP 1 id FROM dbo.admins ORDER BY id ASC"); } catch { }
                
                if (orderId > 0 && anyAdminId > 0)
                {
                    Db.Execute("INSERT INTO dbo.order_logs(order_id, status, admin_id) VALUES (@oid, 'processing', @aid)",
                        Db.P("@oid", orderId), Db.P("@aid", anyAdminId));
                }

                if (!string.IsNullOrWhiteSpace(customerEmail))
                {
                    try { EmailService.SendPaymentConfirmationEmail(customerEmail, customerName, orderCode, totalAmount, paymentMethod); } catch { }
                }

                try { EmailService.SendPaymentReceivedAlert(orderCode, totalAmount, paymentMethod); } catch { }

                try { NotificationService.NotifyMemberOrderStatus(Convert.ToInt32(Session["MEMBER_ID"] ?? 0), orderId, orderCode, "paid"); } catch { }
            }
            catch
            {
                Response.Redirect("~/Account/Orders/EsewaFailure.aspx");
            }
        }

        private string ExtractValue(string json, string key)
        {
            // Primitive JSON parsing for eSewa response without external dependencies
            string search = "\"" + key + "\":\"";
            int start = json.IndexOf(search);
            if (start == -1) return null;
            start += search.Length;
            int end = json.IndexOf("\"", start);
            if (end == -1) return null;
            return json.Substring(start, end - start);
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
    }
}
