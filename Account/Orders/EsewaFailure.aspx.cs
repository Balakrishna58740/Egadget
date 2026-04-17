using System;
using System.Data;
using System.Web.UI;

namespace serena.Site.Account.Orders
{
    public partial class EsewaFailurePage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string orderCode = (Request.QueryString["code"] ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(orderCode)) return;

            try
            {
                EnsurePaymentTransactionsTable();
                EnsureOrderLogsTable();

                var dt = Db.Query("SELECT TOP 1 id, total_amount, payment FROM dbo.orders WHERE order_code=@code", Db.P("@code", orderCode));
                if (dt == null || dt.Rows.Count == 0) return;

                int orderId = 0;
                int.TryParse(Convert.ToString(dt.Rows[0]["id"]), out orderId);
                decimal amount = 0m;
                decimal.TryParse(Convert.ToString(dt.Rows[0]["total_amount"]), out amount);
                string method = Convert.ToString(dt.Rows[0]["payment"] ?? "eSewa");

                if (orderId <= 0) return;

                Db.Execute(@"INSERT INTO dbo.payment_transactions(order_id, order_code, payment_method, transaction_ref, provider_status, amount, raw_response, created_at, updated_at)
VALUES (@oid, @code, @method, @ref, 'failed', @amt, @raw, GETDATE(), GETDATE())",
                    Db.P("@oid", orderId),
                    Db.P("@code", orderCode),
                    Db.P("@method", method),
                    Db.P("@ref", orderCode),
                    Db.P("@amt", amount),
                    Db.P("@raw", "esewa_failure_callback"));

                int anyAdminId = 0;
                try { anyAdminId = Db.Scalar<int>("SELECT TOP 1 id FROM dbo.admins ORDER BY id ASC"); } catch { }
                if (anyAdminId > 0)
                {
                    Db.Execute("INSERT INTO dbo.order_logs(order_id, status, admin_id, created_at, updated_at) VALUES (@oid, 'payment_failed', @aid, GETDATE(), GETDATE())",
                        Db.P("@oid", orderId), Db.P("@aid", anyAdminId));
                }
            }
            catch
            {
                // no-op
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

        private void EnsureOrderLogsTable()
        {
            Db.Execute(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='order_logs' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.order_logs(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    order_id INT NOT NULL,
    [status] VARCHAR(50) NOT NULL,
    admin_id INT NOT NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_order_logs_orders')
  ALTER TABLE dbo.order_logs WITH CHECK
    ADD CONSTRAINT FK_order_logs_orders FOREIGN KEY(order_id) REFERENCES dbo.orders(id);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_order_logs_admins')
  ALTER TABLE dbo.order_logs WITH CHECK
    ADD CONSTRAINT FK_order_logs_admins FOREIGN KEY(admin_id) REFERENCES dbo.admins(id);
");
        }
    }
}
