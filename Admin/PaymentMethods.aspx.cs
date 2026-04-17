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
    public partial class PaymentMethodsPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            EnsurePaymentMethodsTableAndSeed();
            EnsurePaymentTransactionsTable();

            if (!IsPostBack)
            {
                int id;
                if (TryGetQueryInt("del", out id))
                {
                    DeleteMethod(id);
                }
                else if (TryGetQueryInt("toggle", out id))
                {
                    ToggleUse(id);
                }
                else if (TryGetQueryInt("edit", out id))
                {
                    LoadForEdit(id);
                }

                BindTable();
                BindTransactions();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var lbl = Find<Label>("lblMsg");
            var hid = Find<HiddenField>("hidId");
            var txt = Find<TextBox>("txtName");
            var chk = Find<CheckBox>("chkUse");

            string name = (txt != null ? txt.Text.Trim() : "");
            bool use = (chk != null ? chk.Checked : false);

            if (string.IsNullOrWhiteSpace(name))
            {
                Show(lbl, "Please enter a name.", false);
                return;
            }

            int id = 0;
            if (hid != null && !string.IsNullOrEmpty(hid.Value)) int.TryParse(hid.Value, out id);

            if (!TryNormalizeSupportedMethod(name, out name))
            {
                Show(lbl, "Only Cash On Delivery and eSewa are supported.", false);
                return;
            }

            try
            {
                int exists = Db.Scalar<int>(
                    id > 0
                        ? "SELECT COUNT(*) FROM dbo.payment_methods WHERE LOWER(name)=LOWER(@n) AND id<>@id"
                        : "SELECT COUNT(*) FROM dbo.payment_methods WHERE LOWER(name)=LOWER(@n)",
                    Db.P("@n", name), Db.P("@id", id));

                if (exists > 0)
                {
                    Show(lbl, "Name already exists.", false);
                    return;
                }

                if (id > 0)
                {
                    Db.Execute(
                        "UPDATE dbo.payment_methods SET name=@n, is_use=@u, updated_at=GETDATE() WHERE id=@id",
                        Db.P("@n", name), Db.P("@u", use), Db.P("@id", id));
                    Show(lbl, "Payment method updated.", true);
                }
                else
                {
                    Db.Execute(
                        "INSERT INTO dbo.payment_methods (name, is_use, created_at, updated_at) VALUES (@n, @u, GETDATE(), GETDATE())",
                        Db.P("@n", name), Db.P("@u", use));
                    Show(lbl, "Payment method added.", true);
                }

                if (hid != null) hid.Value = "";
                if (txt != null) txt.Text = "";
                if (chk != null) chk.Checked = false;

                BindTable();
                BindTransactions();
            }
            catch
            {
                Show(lbl, "Save failed.", false);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Admin/PaymentMethods.aspx");
        }

        private void LoadForEdit(int id)
        {
            using (var con = Db.Open())
            using (var cmd = new SqlCommand("SELECT id, name, is_use FROM dbo.payment_methods WHERE id=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (r.Read())
                    {
                        var hid = Find<HiddenField>("hidId");
                        var txt = Find<TextBox>("txtName");
                        var chk = Find<CheckBox>("chkUse");

                        if (hid != null) hid.Value = Convert.ToString(r["id"]);
                        if (txt != null) txt.Text = Convert.ToString(r["name"]);
                        if (chk != null) chk.Checked = Convert.ToBoolean(r["is_use"]);
                    }
                }
            }
        }

        private void DeleteMethod(int id)
        {
            var lbl = Find<Label>("lblMsg");
            try
            {
                int n = Db.Execute("DELETE FROM dbo.payment_methods WHERE id=@id", Db.P("@id", id));
                if (n > 0) Show(lbl, "Payment method deleted.", true);
                else Show(lbl, "Not found.", false);
            }
            catch
            {
                Show(lbl, "Delete failed.", false);
            }
        }

        private void ToggleUse(int id)
        {
            var lbl = Find<Label>("lblMsg");
            try
            {
                int n = Db.Execute(@"
UPDATE dbo.payment_methods
SET is_use = CASE WHEN is_use=1 THEN 0 ELSE 1 END,
    updated_at=GETDATE()
WHERE id=@id
  AND LOWER(LTRIM(RTRIM(name))) IN ('cash on delivery','esewa')",
                           Db.P("@id", id));

                if (n <= 0)
                    Show(lbl, "Only Cash On Delivery and eSewa can be activated.", false);
            }
            catch
            {
                Show(lbl, "Unable to update method state.", false);
            }
        }

        private void BindTable()
        {
            var lit = Find<Literal>("litRows");
            if (lit == null) return;

            var dt = Db.Query("SELECT id, name, is_use FROM dbo.payment_methods ORDER BY name ASC;");

            var sb = new StringBuilder();
            if (dt.Rows.Count == 0)
            {
                sb.Append("<tr><td colspan='4' class='text-center pay-muted'>No payment methods yet.</td></tr>");
            }
            else
            {
                int i = 0;
                foreach (DataRow r in dt.Rows)
                {
                    i++;
                    int id = Convert.ToInt32(r["id"]);
                    string rawName = Convert.ToString(r["name"] ?? "");
                    string name = Html(rawName);
                    bool use = Convert.ToBoolean(r["is_use"]);
                    bool supported = IsSupportedMethod(rawName);

                    sb.Append("<tr>");
                    sb.Append("<td>").Append(i).Append("</td>");
                    sb.Append("<td>").Append(name).Append("</td>");
                    sb.Append("<td>");
                    sb.Append(!supported
                        ? "<span class='pay-status pay-status-failed'>Unsupported</span>"
                        : (use
                        ? "<span class='pay-status pay-status-active'>Active</span>"
                        : "<span class='pay-status pay-status-inactive'>Inactive</span>"));
                    sb.Append("</td>");
                    sb.Append("<td class='text-end'>");
                    sb.Append("<div class='pay-method-actions'>");
                    sb.Append("<a class='pay-link-btn' href='PaymentMethods.aspx?edit=").Append(id).Append("'>Edit</a>");
                    if (supported)
                    {
                        sb.Append("<a class='pay-link-btn warn' href='PaymentMethods.aspx?toggle=").Append(id).Append("'>");
                        sb.Append(use ? "Disable" : "Enable").Append("</a>");
                    }
                    sb.Append("<a class='pay-link-btn danger' href='PaymentMethods.aspx?del=").Append(id).Append("' ");
                    sb.Append("onclick=\"return confirm('Delete this payment method?');\">Delete</a>");
                    sb.Append("</div>");
                    sb.Append("</td>");
                    sb.Append("</tr>");
                }
            }
            lit.Text = sb.ToString();
        }

        private void BindTransactions()
        {
            var litRows = Find<Literal>("litTxnRows");
            var litTotal = Find<Literal>("litTxnTotal");
            if (litRows == null) return;

            try
            {
                int total = Db.Scalar<int>("SELECT COUNT(*) FROM dbo.payment_transactions;");
                if (litTotal != null) litTotal.Text = total.ToString("N0", CultureInfo.InvariantCulture);

                var dt = Db.Query(@"
SELECT TOP 50
    pt.order_id,
    pt.order_code,
    pt.payment_method,
    pt.transaction_ref,
    pt.provider_status,
    pt.amount,
    COALESCE(pt.updated_at, pt.created_at) AS tx_date,
    COALESCE(NULLIF(o.ship_name,''), m.full_name, '-') AS client_name
FROM dbo.payment_transactions pt
LEFT JOIN dbo.orders o ON o.id = pt.order_id
LEFT JOIN dbo.members m ON m.id = o.member_id
ORDER BY COALESCE(pt.updated_at, pt.created_at) DESC, pt.id DESC;");

                var sb = new StringBuilder();
                if (dt.Rows.Count == 0)
                {
                    sb.Append("<tr><td colspan='8' class='text-center pay-muted'>No payment transactions yet.</td></tr>");
                }
                else
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int orderId = 0;
                        int.TryParse(Convert.ToString(r["order_id"]), out orderId);

                        string orderCode = Html(r["order_code"]);
                        string clientName = Html(r["client_name"]);
                        string method = Html(r["payment_method"]);
                        string txRef = Html(r["transaction_ref"]);
                        string providerStatus = Convert.ToString(r["provider_status"] ?? "");

                        decimal amount = 0m;
                        decimal.TryParse(Convert.ToString(r["amount"]), out amount);

                        string txDate = "-";
                        if (r["tx_date"] != DBNull.Value)
                            txDate = Convert.ToDateTime(r["tx_date"]).ToString("dd MMM yyyy, hh:mm tt", CultureInfo.InvariantCulture);

                        sb.Append("<tr>");
                        sb.Append("<td><span class='pay-code'>").Append(string.IsNullOrWhiteSpace(orderCode) ? "-" : orderCode).Append("</span></td>");
                        sb.Append("<td>").Append(string.IsNullOrWhiteSpace(clientName) ? "-" : clientName).Append("</td>");
                        sb.Append("<td>").Append(string.IsNullOrWhiteSpace(method) ? "-" : method).Append("</td>");
                        sb.Append("<td>").Append(string.IsNullOrWhiteSpace(txRef) ? "-" : txRef).Append("</td>");
                        sb.Append("<td>").Append(RenderProviderStatus(providerStatus)).Append("</td>");
                        sb.Append("<td class='text-right pay-amount'>RS ").Append(amount.ToString("N2", CultureInfo.InvariantCulture)).Append("</td>");
                        sb.Append("<td>").Append(txDate).Append("</td>");
                        sb.Append("<td class='text-right'><div class='pay-txn-actions'>");
                        if (orderId > 0)
                        {
                            sb.Append("<a class='pay-link-btn' href='OrderView.aspx?id=").Append(orderId).Append("'>View Order</a>");
                        }
                        else
                        {
                            sb.Append("<span class='pay-muted'>-</span>");
                        }
                        sb.Append("</div></td>");
                        sb.Append("</tr>");
                    }
                }

                litRows.Text = sb.ToString();
            }
            catch
            {
                if (litTotal != null) litTotal.Text = "0";
                litRows.Text = "<tr><td colspan='8' class='text-center pay-muted'>Unable to load transaction records.</td></tr>";
            }
        }

        private void EnsurePaymentMethodsTableAndSeed()
        {
            try
            {
                Db.Execute(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='payment_methods' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.payment_methods(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] VARCHAR(100) NOT NULL UNIQUE,
    is_use BIT NOT NULL DEFAULT(1),
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name='CK_payment_methods_name' AND parent_object_id = OBJECT_ID('dbo.payment_methods'))
  ALTER TABLE dbo.payment_methods DROP CONSTRAINT CK_payment_methods_name;

DELETE FROM dbo.payment_methods
WHERE LOWER(LTRIM(RTRIM([name]))) NOT IN ('cash on delivery','esewa');

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name='CK_payment_methods_name' AND parent_object_id = OBJECT_ID('dbo.payment_methods'))
  ALTER TABLE dbo.payment_methods WITH CHECK ADD CONSTRAINT CK_payment_methods_name
    CHECK ([name] IN ('Cash On Delivery','eSewa'));

IF NOT EXISTS (SELECT 1 FROM dbo.payment_methods WHERE [name]='Cash On Delivery')
  INSERT INTO dbo.payment_methods([name], is_use) VALUES ('Cash On Delivery', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.payment_methods WHERE [name]='eSewa')
  INSERT INTO dbo.payment_methods([name], is_use) VALUES ('eSewa', 1);
");
            }
            catch
            {
                // best-effort bootstrap
            }
        }

        private void EnsurePaymentTransactionsTable()
        {
            try
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

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_payment_transactions_orders')
  ALTER TABLE dbo.payment_transactions WITH CHECK
    ADD CONSTRAINT FK_payment_transactions_orders FOREIGN KEY(order_id) REFERENCES dbo.orders(id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_payment_transactions_order_id' AND object_id=OBJECT_ID('dbo.payment_transactions'))
  CREATE NONCLUSTERED INDEX IX_payment_transactions_order_id ON dbo.payment_transactions(order_id, id DESC);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_payment_transactions_ref' AND object_id=OBJECT_ID('dbo.payment_transactions'))
  CREATE NONCLUSTERED INDEX IX_payment_transactions_ref ON dbo.payment_transactions(transaction_ref);
");
            }
            catch
            {
                // best-effort bootstrap
            }
        }

        // ------- helpers (designer-free) -------
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

        private static string Html(object o)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(o) ?? "");
        }

        private static bool TryGetQueryInt(string key, out int value)
        {
            value = 0;
            string s = HttpContext.Current.Request.QueryString[key];
            return !string.IsNullOrEmpty(s) && int.TryParse(s, out value);
        }

        private static string RenderProviderStatus(string status)
        {
            string raw = (status ?? string.Empty).Trim();
            string s = raw.ToLowerInvariant();

            string css = "pay-status pay-status-neutral";
            if (s.Contains("complete") || s.Contains("success") || s.Contains("paid")) css = "pay-status pay-status-success";
            else if (s.Contains("pending") || s.Contains("init")) css = "pay-status pay-status-pending";
            else if (s.Contains("fail") || s.Contains("cancel") || s.Contains("error")) css = "pay-status pay-status-failed";

            string text = string.IsNullOrWhiteSpace(raw) ? "Unknown" : HttpUtility.HtmlEncode(raw);
            return "<span class='" + css + "'>" + text + "</span>";
        }

        private static bool TryNormalizeSupportedMethod(string input, out string normalized)
        {
            normalized = null;
            string v = (input ?? string.Empty).Trim().ToLowerInvariant();

            if (v == "cash on delivery" || v == "cashondelivery" || v == "cod")
            {
                normalized = "Cash On Delivery";
                return true;
            }

            if (v == "esewa" || v == "e-sewa" || v == "e sewa")
            {
                normalized = "eSewa";
                return true;
            }

            return false;
        }

        private static bool IsSupportedMethod(string name)
        {
            string v = (name ?? string.Empty).Trim().ToLowerInvariant();
            return v == "cash on delivery" || v == "esewa";
        }

        private void Show(Label lbl, string msg, bool ok)
        {
            if (lbl == null) return;
            lbl.Text = Server.HtmlEncode(msg);
            lbl.CssClass = ok ? "pay-alert alert-success" : "pay-alert alert-danger";
        }
    }
}
