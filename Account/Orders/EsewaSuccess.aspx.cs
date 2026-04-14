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
                string transactionCode = ExtractValue(decodedJson, "transaction_code");
                
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

                // Keep order in pending so admin can explicitly accept the order after payment verification.
                Db.Execute("UPDATE dbo.orders SET updated_at = GETDATE() WHERE order_code = @code",
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
                
                // Do not auto-change order workflow here; admin actions control order status transitions.

                EnsurePaymentTransactionsTable();
                if (orderId > 0)
                {
                    int updated = Db.Execute(@"UPDATE dbo.payment_transactions
SET transaction_ref = @ref,
    provider_status = @st,
    amount = @amt,
    raw_response = @raw,
    updated_at = GETDATE()
WHERE order_id = @oid",
                        Db.P("@ref", string.IsNullOrWhiteSpace(transactionCode) ? orderCode : transactionCode),
                        Db.P("@st", string.IsNullOrWhiteSpace(status) ? "complete" : status),
                        Db.P("@amt", totalAmount),
                        Db.P("@raw", decodedJson),
                        Db.P("@oid", orderId));

                    if (updated <= 0)
                    {
                        Db.Execute(@"INSERT INTO dbo.payment_transactions(order_id, order_code, payment_method, transaction_ref, provider_status, amount, raw_response, created_at, updated_at)
VALUES (@oid, @code, @method, @ref, @st, @amt, @raw, GETDATE(), GETDATE())",
                            Db.P("@oid", orderId),
                            Db.P("@code", orderCode),
                            Db.P("@method", paymentMethod),
                            Db.P("@ref", string.IsNullOrWhiteSpace(transactionCode) ? orderCode : transactionCode),
                            Db.P("@st", string.IsNullOrWhiteSpace(status) ? "complete" : status),
                            Db.P("@amt", totalAmount),
                            Db.P("@raw", decodedJson));
                    }
                }

                if (!string.IsNullOrWhiteSpace(customerEmail))
                {
                    try { global::EmailService.SendPaymentConfirmationEmail(customerEmail, customerName, orderCode, totalAmount, paymentMethod); } catch { }
                }

                try { global::EmailService.SendPaymentReceivedAlert(orderCode, totalAmount, paymentMethod); } catch { }

                try { NotificationService.NotifyMemberOrderStatus(Convert.ToInt32(Session["MEMBER_ID"] ?? 0), orderId, orderCode, "paid"); } catch { }
            }
            catch
            {
                Response.Redirect("~/Account/Orders/EsewaFailure.aspx");
            }
        }

        private void EnsurePaymentTransactionsTable()
        {
            Db.Execute(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='payment_transactions' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.payment_transactions(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    order_id INT NOT NULL,
    order_code VARCHAR(50) NOT NULL,
    payment_method VARCHAR(100) NOT NULL,
    transaction_ref VARCHAR(120) NULL,
    provider_status VARCHAR(50) NULL,
    amount DECIMAL(10,2) NULL,
    raw_response VARCHAR(MAX) NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;
");
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
