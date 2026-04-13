using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace serena.Site.Account
{
    public partial class CheckoutPage : Page
    {
        private class CartItem
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Qty { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RequireLogin();

            if (!IsPostBack)
            {
                if (CartIsEmpty())
                {
                    phEmptyCart.Visible = true;
                    phCheckout.Visible = false;
                    return;
                }

                phEmptyCart.Visible = false;
                phCheckout.Visible = true;

                BindCartSummary();
                PrefillDefaultAddress();
                BindPaymentMethods();
            }
        }

        private void RequireLogin()
        {
            if (EnsureMemberSession()) return;

            string returnUrl = Server.UrlEncode(Request.RawUrl);
            Response.Redirect("~/Account/Login.aspx?returnUrl=" + returnUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private bool EnsureMemberSession()
        {
            if (Session["MEMBER_ID"] != null)
                return true;

            try
            {
                var ck = Request != null ? Request.Cookies["MemberToken"] : null;
                if (ck == null || string.IsNullOrEmpty(ck.Value))
                    return false;

                int memberId = Db.Scalar<int>(
                    "SELECT id FROM dbo.members WHERE persistent_token = @t AND token_expires > GETUTCDATE()",
                    Db.P("@t", ck.Value));

                if (memberId > 0)
                {
                    Session["MEMBER_ID"] = memberId;
                    return true;
                }
            }
            catch
            {
                // fall through to login redirect
            }

            return false;
        }

        private void BindCartSummary()
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                phEmptyCart.Visible = true;
                phCheckout.Visible = false;
                return;
            }

            int totalQty = cart.Sum(x => x.Qty);
            decimal subTotal = cart.Sum(x => x.Price * x.Qty);

            litItemsCount.Text = totalQty.ToString(CultureInfo.InvariantCulture);
            litSubtotal.Text = subTotal.ToString("N2");

            var sb = new StringBuilder();
            sb.Append("<div class='table-responsive'><table class='table table-sm align-middle mb-0'>");
            sb.Append("<thead><tr><th>Product</th><th class='text-end'>Price</th><th class='text-center'>Qty</th><th class='text-end'>Total</th></tr></thead><tbody>");
            foreach (var it in cart)
            {
                decimal line = it.Price * it.Qty;
                sb.Append("<tr>");
                sb.AppendFormat("<td>{0}</td>", Server.HtmlEncode(it.Name));
                sb.AppendFormat("<td class='text-end'>RS {0}</td>", it.Price.ToString("N2"));
                sb.AppendFormat("<td class='text-center'>{0}</td>", it.Qty);
                sb.AppendFormat("<td class='text-end'>RS {0}</td>", line.ToString("N2"));
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table></div>");
            litCartTable.Text = sb.ToString();
        }

        private void PrefillDefaultAddress()
        {
            try
            {
                int memberId = Convert.ToInt32(Session["MEMBER_ID"]);
                DataTable dt = Db.Query(@"
                    SELECT TOP 1
                        m.full_name,
                        a.address, a.township, a.postal_code, a.city, a.state, a.country
                    FROM dbo.members m
                    OUTER APPLY (
                        SELECT TOP 1 *
                        FROM dbo.member_addresses
                        WHERE member_id = @mid AND is_default = 1
                        ORDER BY id DESC
                    ) a
                    WHERE m.id = @mid",
                    Db.P("@mid", memberId)
                );

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtShipName.Value = ((row["full_name"] ?? "").ToString()).Trim();
                    txtAddress.Value = (row["address"] ?? "").ToString();
                    txtTownship.Value = (row["township"] ?? "").ToString();
                    txtPostal.Value = (row["postal_code"] ?? "").ToString();
                    txtCity.Value = (row["city"] ?? "").ToString();
                    txtState.Value = (row["state"] ?? "").ToString();
                    txtCountry.Value = (row["country"] ?? "").ToString();
                }
            }
            catch { /* allow manual entry */ }
        }

        private void BindPaymentMethods()
        {
            try
            {
                DataTable methods = Db.Query("SELECT id, name FROM dbo.payment_methods WHERE is_use = 1 ORDER BY name ASC");
                rblPayment.DataSource = methods;
                rblPayment.DataTextField = "name";
                rblPayment.DataValueField = "id";
                rblPayment.DataBind();
            }
            catch
            {
                BindFallbackPaymentMethods();
            }

            if (rblPayment.Items.Count > 0 && rblPayment.SelectedIndex < 0)
                rblPayment.SelectedIndex = 0;
        }

        private void BindFallbackPaymentMethods()
        {
            rblPayment.Items.Clear();
            rblPayment.Items.Add(new ListItem("Cash On Delivery", "Cash On Delivery"));
            rblPayment.Items.Add(new ListItem("Card", "Card"));
            rblPayment.Items.Add(new ListItem("Bank", "Bank"));
            rblPayment.Items.Add(new ListItem("eSewa", "eSewa"));
        }

        private void EnsureCheckoutTables()
        {
            Db.Execute(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='order_addresses' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.order_addresses(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    order_id INT NOT NULL,
    [address] VARCHAR(MAX) NULL,
    township VARCHAR(255) NULL,
    postal_code VARCHAR(20) NULL,
    city VARCHAR(100) NULL,
    [state] VARCHAR(100) NULL,
    country VARCHAR(100) NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_order_addresses_orders')
  ALTER TABLE dbo.order_addresses WITH CHECK
    ADD CONSTRAINT FK_order_addresses_orders FOREIGN KEY(order_id) REFERENCES dbo.orders(id);

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

        protected void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            RequireLogin();
            if (CartIsEmpty()) { ShowAlert("Your cart is empty.", "warning"); return; }
            if (string.IsNullOrWhiteSpace(txtShipName.Value)) { ShowAlert("Recipient name is required.", "danger"); return; }
            if (string.IsNullOrWhiteSpace(txtAddress.Value)) { ShowAlert("Shipping address is required.", "danger"); return; }
            if (string.IsNullOrWhiteSpace(txtCountry.Value)) { ShowAlert("Country is required.", "danger"); return; }
            if (rblPayment.Items.Count == 0 || rblPayment.SelectedItem == null) { ShowAlert("Please select a payment method.", "danger"); return; }

            var cart = GetCart();
            int memberId = Convert.ToInt32(Session["MEMBER_ID"]);
            int totalQty = cart.Sum(x => x.Qty);
            decimal subTotal = cart.Sum(x => x.Price * x.Qty);
            string paymentName = rblPayment.SelectedItem.Text;
            string orderCode = GenerateUniqueOrderCode();

            int orderId = 0;
            var updatedProducts = new List<Tuple<int, int>>();
            string successUrl = null;

            try
            {
                EnsureCheckoutTables();

                // Precheck stock
                foreach (var it in cart)
                {
                    int stock = Db.Scalar<int>("SELECT stock FROM dbo.products WHERE id=@pid", Db.P("@pid", it.ProductId));
                    if (stock < it.Qty) throw new Exception("Insufficient stock for " + it.Name);
                }

                // 1) Header
                orderId = Db.Scalar<int>(@"
INSERT INTO dbo.orders(order_code, member_id, ship_name, ship_phone, status,
                       total_qty, total_amount, payment)
OUTPUT INSERTED.id
VALUES (@code, @mid, @sname, @sphone, 'pending',
        @tqty, @tamt, @payment)",
                    Db.P("@code", orderCode),
                    Db.P("@mid", memberId),
                    Db.P("@sname", (txtShipName.Value ?? "").Trim()),
                    Db.P("@sphone", (txtShipPhone.Value ?? "").Trim()),
                    Db.P("@tqty", totalQty),
                    Db.P("@tamt", subTotal),
                    Db.P("@payment", paymentName)
                );
                if (orderId <= 0) throw new Exception("Order was not created.");

                // 2) Items
                foreach (var it in cart)
                {
                    Db.Execute(@"
INSERT INTO dbo.order_items(order_id, product_id, quantity, amount)
VALUES (@oid, @pid, @qty, @amt)",
                        Db.P("@oid", orderId),
                        Db.P("@pid", it.ProductId),
                        Db.P("@qty", it.Qty),
                        Db.P("@amt", it.Price * it.Qty)
                    );
                }

                // 3) Address
                Db.Execute(@"
INSERT INTO dbo.order_addresses(order_id, address, township, postal_code, city, state, country)
VALUES (@oid, @addr, @town, @zip, @city, @state, @country)",
                    Db.P("@oid", orderId),
                    Db.P("@addr", (txtAddress.Value ?? "").Trim()),
                    Db.P("@town", (txtTownship.Value ?? "").Trim()),
                    Db.P("@zip", (txtPostal.Value ?? "").Trim()),
                    Db.P("@city", (txtCity.Value ?? "").Trim()),
                    Db.P("@state", (txtState.Value ?? "").Trim()),
                    Db.P("@country", (txtCountry.Value ?? "").Trim())
                );

                // 4) First log (optional, only if an admin exists)
                int anyAdminId = 0;
                try { anyAdminId = Db.Scalar<int>("SELECT TOP 1 id FROM dbo.admins ORDER BY id ASC"); } catch { }
                if (anyAdminId > 0)
                {
                    Db.Execute("INSERT INTO dbo.order_logs(order_id, status, admin_id) VALUES (@oid, 'pending', @aid)",
                        Db.P("@oid", orderId), Db.P("@aid", anyAdminId));
                }

                // 5) Decrement stock
                foreach (var it in cart)
                {
                    Db.Execute("UPDATE dbo.products SET stock = stock - @q WHERE id=@pid",
                        Db.P("@q", it.Qty), Db.P("@pid", it.ProductId));
                    updatedProducts.Add(Tuple.Create(it.ProductId, it.Qty));
                }

                // Prepare redirect AFTER try so we don't catch ThreadAbortException.
                string memberName = null;
                string memberEmail = null;
                try
                {
                    var memberDt = Db.Query("SELECT TOP 1 full_name, email FROM dbo.members WHERE id=@id", Db.P("@id", memberId));
                    if (memberDt != null && memberDt.Rows.Count > 0)
                    {
                        memberName = Convert.ToString(memberDt.Rows[0]["full_name"]);
                        memberEmail = Convert.ToString(memberDt.Rows[0]["email"]);
                    }
                }
                catch { }

                NotificationService.NotifyAdminOrderPlaced(orderId, orderCode, subTotal, paymentName);
                NotificationService.NotifyMemberOrderPlaced(memberId, orderId, orderCode, subTotal);
                EmailService.SendStoreOrderAlert(orderCode, subTotal, paymentName);
                EmailService.SendCustomerOrderConfirmation(memberEmail, memberName, orderCode, subTotal, paymentName);

                ClearCart();
                
                if (paymentName.Equals("eSewa", StringComparison.OrdinalIgnoreCase))
                {
                    successUrl = "~/Account/Orders/InitiateEsewa.aspx?code=" + Server.UrlEncode(orderCode);
                }
                else
                {
                    successUrl = "~/Account/Orders/Receipt.aspx?code=" + Server.UrlEncode(orderCode);
                }
            }
            catch (Exception ex)
            {
                // Compensate
                foreach (var up in updatedProducts)
                {
                    try
                    {
                        Db.Execute("UPDATE dbo.products SET stock = stock + @q WHERE id=@pid",
                            Db.P("@q", up.Item2), Db.P("@pid", up.Item1));
                    }
                    catch { }
                }
                if (orderId > 0)
                {
                    try { Db.Execute("DELETE FROM dbo.order_logs      WHERE order_id=@oid", Db.P("@oid", orderId)); } catch { }
                    try { Db.Execute("DELETE FROM dbo.order_addresses WHERE order_id=@oid", Db.P("@oid", orderId)); } catch { }
                    try { Db.Execute("DELETE FROM dbo.order_items     WHERE order_id=@oid", Db.P("@oid", orderId)); } catch { }
                    try { Db.Execute("DELETE FROM dbo.orders          WHERE id=@oid", Db.P("@oid", orderId)); } catch { }
                }

                ShowAlert("Order failed: " + Server.HtmlEncode(ex.Message), "danger");
                return;
            }

            // Do the redirect here, OUTSIDE the try/catch, without aborting the thread.
            if (!string.IsNullOrEmpty(successUrl))
            {
                Response.Redirect(successUrl, false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }


        // -------- Helpers --------
        private List<CartItem> GetCart()
        {
            var results = new List<CartItem>();
            object raw = Session["CART_DICT"];
            if (raw == null) return results;

            var dictInt = raw as Dictionary<int, int>;
            if (dictInt != null) { foreach (var kv in dictInt) AppendItemFromDb(results, kv.Key, kv.Value); return results; }

            var ht = raw as Hashtable;
            if (ht != null) { foreach (DictionaryEntry de in ht) AppendItemFromDb(results, Convert.ToInt32(de.Key), Convert.ToInt32(de.Value)); return results; }

            var list = raw as List<CartItem>;
            if (list != null) return list;

            return results;
        }

        private void AppendItemFromDb(List<CartItem> list, int productId, int qty)
        {
            DataTable dt = Db.Query("SELECT TOP 1 name, price FROM dbo.products WHERE id=@pid", Db.P("@pid", productId));
            string name = (dt != null && dt.Rows.Count > 0) ? (dt.Rows[0]["name"] + "") : ("Product #" + productId);
            decimal price = 0m;
            if (dt != null && dt.Rows.Count > 0) decimal.TryParse((dt.Rows[0]["price"] + ""), out price);
            list.Add(new CartItem { ProductId = productId, Name = name, Price = price, Qty = qty });
        }

        private bool CartIsEmpty() { return GetCart().Count == 0; }
        private void ClearCart()
        {
            Session.Remove("CART_DICT");
            Session["CartQty"] = 0;   // avoid any stale reads elsewhere
        }

        private string GenerateUniqueOrderCode()
        {
            // avoids UNIQUE constraint violations on UQ_orders_order_code
            for (int i = 0; i < 6; i++)
            {
                string code = "SS-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(10000, 99999);
                int exists = 0;
                try { exists = Db.Scalar<int>("SELECT COUNT(1) FROM dbo.orders WHERE order_code=@c", Db.P("@c", code)); }
                catch { }
                if (exists == 0) return code;
            }
            // last resort
            return "SS-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private void ShowAlert(string message, string type)
        {
            alertMsg.Visible = true;
            var sb = new StringBuilder();
            sb.Append("<div class='alert alert-").Append(type).Append(" alert-dismissible fade show' role='alert'>");
            sb.Append(message);
            sb.Append("<button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close'></button></div>");
            alertMsg.InnerHtml = sb.ToString();
        }
    }
}
