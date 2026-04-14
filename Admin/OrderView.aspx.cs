using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace serena.Admin
{
    public partial class OrderViewPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int id;
            if (!TryGetQueryInt("id", out id) || id <= 0)
            {
                Response.Redirect("~/Admin/Orders.aspx");
                return;
            }

            if (!IsPostBack)
            {
                SetHidden("hidId", id.ToString());
                BindAll(id);
            }
        }

        // -------- Status action buttons --------
        protected void btnAccept_Click(object sender, EventArgs e) { ChangeStatus("accepted"); }
        protected void btnDelivering_Click(object sender, EventArgs e) { ChangeStatus("inprocess"); }
        protected void btnDelivered_Click(object sender, EventArgs e) { ChangeStatus("delivered"); }
        protected void btnCancel_Click(object sender, EventArgs e) { ChangeStatus("canceled"); }

        private void ChangeStatus(string target)
        {
            var lbl = Find<Label>("lblMsg");
            int id = SafeId();
            if (id <= 0) { Show(lbl, "Invalid record reference.", false); return; }

            try
            {
                int adminId = GetAdminId();
                if (adminId <= 0) { Show(lbl, "Authorized session required for transition.", false); return; }

                string current = Db.Scalar<string>("SELECT LOWER(COALESCE(status,'')) FROM orders WHERE id=@id", Db.P("@id", id));
                current = NormalizeStatus(current);
                target = NormalizeStatus(target);

                // Normalize for internal logic if needed, but here we assume DB stores the target directly
                if (!IsAllowedTransition(current, target))
                {
                    Show(lbl, "Unauthorized state transition requested.", false);
                    return;
                }

                // Do everything atomically
                using (var con = Db.Open())
                using (var tx = con.BeginTransaction())
                {
                    // If canceling a PENDING order, add stock back for each item
                    if (current == "pending" && target == "canceled")
                    {
                        using (var cmd = new SqlCommand(@"
UPDATE p
   SET p.stock = p.stock + oi.quantity
FROM products p
INNER JOIN order_items oi ON oi.product_id = p.id
WHERE oi.order_id = @oid;", con, tx))
                        {
                            cmd.Parameters.AddWithValue("@oid", id);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Update order status (stored lowercase) and updated_at
                    using (var cmd = new SqlCommand(
                        "UPDATE orders SET status=@s, updated_at=GETDATE() WHERE id=@id", con, tx))
                    {
                        cmd.Parameters.AddWithValue("@s", target);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }

                    // Insert order log
                    using (var cmd = new SqlCommand(@"
INSERT INTO order_logs (order_id, admin_id, status, created_at, updated_at)
VALUES (@oid, @aid, @st, GETDATE(), GETDATE());", con, tx))
                    {
                        cmd.Parameters.AddWithValue("@oid", id);
                        cmd.Parameters.AddWithValue("@aid", adminId);
                        cmd.Parameters.AddWithValue("@st", target);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }

                try { SendStatusUpdateNotification(id, target); } catch { }

                Show(lbl, "Transition successful. Transaction is now " + target.ToUpperInvariant() + ".", true);
                BindAll(id);
            }
            catch (Exception ex)
            {
                Show(lbl, "Transition failure: " + Server.HtmlEncode(ex.Message), false);
            }
        }

        private void SendStatusUpdateNotification(int orderId, string status)
        {
            var dt = Db.Query(@"
SELECT TOP 1 o.order_code, o.member_id, m.full_name, m.email
FROM dbo.orders o
LEFT JOIN dbo.members m ON m.id = o.member_id
WHERE o.id=@id", Db.P("@id", orderId));

            if (dt == null || dt.Rows.Count == 0) return;

            var r = dt.Rows[0];
            int memberId = 0;
            int.TryParse(Convert.ToString(r["member_id"]), out memberId);

            string orderCode = Convert.ToString(r["order_code"]);
            string name = Convert.ToString(r["full_name"]);
            string email = Convert.ToString(r["email"]);

            NotificationService.NotifyMemberOrderStatus(memberId, orderId, orderCode, status);
            EmailService.SendCustomerStatusUpdate(email, name, orderCode, status);
        }

        private static string NormalizeStatus(string s)
        {
            string v = (s ?? "").Trim().ToLowerInvariant();
            if (v == "paid") return "accepted";
            if (v == "processing") return "accepted";
            if (v == "delivering") return "inprocess";
            if (v == "completed") return "delivered";
            return v;
        }

        private static bool IsAllowedTransition(string current, string target)
        {
            if (current == "pending" && (target == "accepted" || target == "canceled")) return true;
            if (current == "accepted" && target == "inprocess") return true;
            if (current == "inprocess" && target == "delivered") return true;
            return false; // completed/canceled are terminal
        }

        // -------- Bind everything --------
        private void BindAll(int id)
        {
            var lbl = Find<Label>("lblMsg");
            try
            {
                BindHeaderAndDetail(id);
                BindAddress(id);
                BindItems(id);
                BindLogs(id);
                ToggleButtonsForStatus();
            }
            catch (Exception ex)
            {
                Show(lbl, "System error during dossier retrieval: " + Server.HtmlEncode(ex.Message), false);
            }
        }

        private void BindHeaderAndDetail(int id)
        {
            var dt = Db.Query(@"
SELECT 
  o.id, o.order_code, o.payment, o.status, o.total_qty, o.total_amount, o.order_date,
  o.ship_name, o.ship_phone,
  m.full_name, m.email, m.phone
FROM orders o
LEFT JOIN members m ON m.id = o.member_id
WHERE o.id=@id;", Db.P("@id", id));

            if (dt.Rows.Count == 0)
            {
                Response.Redirect("~/Admin/Orders.aspx");
                return;
            }

            var r = dt.Rows[0];

            string code = Convert.ToString(r["order_code"]);
            string status = NormalizeStatus(Convert.ToString(r["status"]));
            string statusUpper = status.Length > 0 ? status.ToUpperInvariant() : "UNKNOWN";

            SetText("litTitleCode", code);
            SetText("litOrderCode", code);

            string od = r["order_date"] != DBNull.Value ? Convert.ToDateTime(r["order_date"]).ToString("dd MMM, yyyy · HH:mm") : "-";
            SetText("litOrderDate", od);

            SetText("litPayment", Convert.ToString(r["payment"] ?? "-"));
            SetText("litTotalQty", Convert.ToString(r["total_qty"] ?? "0"));

            decimal amt = r["total_amount"] != DBNull.Value ? Convert.ToDecimal(r["total_amount"]) : 0m;
            SetText("litTotalAmount", amt.ToString("N2"));

            SetText("litStatusUpper", statusUpper);

            var tx = Db.Query(@"SELECT TOP 1 transaction_ref, provider_status
FROM dbo.payment_transactions
WHERE order_id=@oid
ORDER BY id DESC", Db.P("@oid", id));

            if (tx != null && tx.Rows.Count > 0)
            {
                string txRef = Convert.ToString(tx.Rows[0]["transaction_ref"]);
                string txStatus = Convert.ToString(tx.Rows[0]["provider_status"]);
                SetText("litTxnRef", string.IsNullOrWhiteSpace(txRef) ? "-" : txRef);
                SetText("litTxnStatus", string.IsNullOrWhiteSpace(txStatus) ? "-" : txStatus.ToUpperInvariant());
            }
            else
            {
                SetText("litTxnRef", "-");
                SetText("litTxnStatus", "-");
            }

            var badge = Find<HtmlGenericControl>("badgeStatus");
            if (badge != null)
            {
                string cls = "text-[8px] uppercase tracking-widest font-bold px-3 py-1 border ";
                if (status == "pending") cls += "border-orange-200 text-orange-400";
                else if (status == "accepted") cls += "border-green-200 text-green-500";
                else if (status == "inprocess") cls += "border-blue-200 text-blue-400";
                else if (status == "delivered") cls += "border-primary text-primary";
                else if (status == "canceled") cls += "border-red-200 text-red-500";
                else cls += "border-gray-200 text-gray-300";

                badge.Attributes["class"] = cls;
                badge.InnerText = statusUpper;
            }

            // Customer info (prefer shipping fields; fall back to member profile)
            string custName = Convert.ToString(r["ship_name"]);
            if (string.IsNullOrWhiteSpace(custName)) custName = Convert.ToString(r["full_name"]);
            string custEmail = Convert.ToString(r["email"]);
            string custPhone = Convert.ToString(r["ship_phone"]);
            if (string.IsNullOrWhiteSpace(custPhone)) custPhone = Convert.ToString(r["phone"]);

            SetText("litCustName", string.IsNullOrWhiteSpace(custName) ? "-" : custName);
            SetText("litCustEmail", string.IsNullOrWhiteSpace(custEmail) ? "-" : custEmail);
            SetText("litCustPhone", string.IsNullOrWhiteSpace(custPhone) ? "-" : custPhone);

            ViewState["status"] = status;
        }

        private void BindAddress(int id)
        {
            var dt = Db.Query(@"
SELECT TOP 1 address, township, postal_code, city, state, country
FROM order_addresses
WHERE order_id=@id
ORDER BY id DESC;", Db.P("@id", id));

            string address = "-";
            string township = "-";
            string postal = "-";
            string city = "-";
            string state = "-";
            string country = "-";

            if (dt.Rows.Count > 0)
            {
                var r = dt.Rows[0];
                address = SafeDisplay(r["address"]);
                township = SafeDisplay(r["township"]);
                postal = SafeDisplay(r["postal_code"]);
                city = SafeDisplay(r["city"]);
                state = SafeDisplay(r["state"]);
                country = SafeDisplay(r["country"]);
            }

            SetText("litAddrLine", address);
            SetText("litTownship", township);
            SetText("litPostal", postal);
            SetText("litCity", city);
            SetText("litState", state);
            SetText("litCountry", country);
        }

        private void BindItems(int id)
        {
            var lit = Find<Literal>("litItemRows");
            if (lit == null) return;

            var dt = Db.Query(@"
SELECT 
  oi.id,
  COALESCE(p.name,'(deleted product)') AS product_name,
  oi.quantity,
  oi.amount
FROM order_items oi
LEFT JOIN products p ON p.id = oi.product_id
WHERE oi.order_id=@id
ORDER BY oi.id ASC;", Db.P("@id", id));

            var sb = new StringBuilder();
            if (dt.Rows.Count == 0)
            {
                sb.Append("<tr><td colspan='4' class='px-8 py-12 text-center text-gray-400 text-xs italic'>Manifest contains no items.</td></tr>");
            }
            else
            {
                foreach (DataRow r in dt.Rows)
                {
                    string name = Html(r["product_name"]);
                    int qty = r["quantity"] != DBNull.Value ? Convert.ToInt32(r["quantity"]) : 0;
                    decimal amount = r["amount"] != DBNull.Value ? Convert.ToDecimal(r["amount"]) : 0m;
                    decimal unit = qty > 0 ? amount / qty : 0m;

                    sb.Append("<tr class='hover:bg-off-white/30 transition-colors'>");
                    sb.Append("<td class='px-8 py-5 text-sm font-serif text-text-dark'>").Append(name).Append("</td>");
                    sb.Append("<td class='px-8 py-5 text-right text-[10px] uppercase tracking-widest font-bold text-gray-400'>RS ").Append(unit.ToString("N2")).Append("</td>");
                    sb.Append("<td class='px-8 py-5 text-center font-bold text-text-dark text-sm'>").Append(qty).Append("</td>");
                    sb.Append("<td class='px-8 py-5 text-right font-bold text-primary text-sm'>RS ").Append(amount.ToString("N2")).Append("</td>");
                    sb.Append("</tr>");
                }
            }
            lit.Text = sb.ToString();
        }

        private void BindLogs(int id)
        {
            var lit = Find<Literal>("litLogRows");
            if (lit == null) return;

            var dt = Db.Query(@"
SELECT l.created_at, l.status, a.full_name AS admin_name
FROM order_logs l
LEFT JOIN admins a ON a.id = l.admin_id
WHERE l.order_id=@id
ORDER BY l.created_at DESC;", Db.P("@id", id));

            var sb = new StringBuilder();
            if (dt.Rows.Count == 0)
            {
                sb.Append("<tr><td colspan='3' class='px-8 py-12 text-center text-gray-400 text-xs italic'>Chronicle has no recorded transitions.</td></tr>");
            }
            else
            {
                foreach (DataRow r in dt.Rows)
                {
                    string date = r["created_at"] != DBNull.Value
                        ? Convert.ToDateTime(r["created_at"]).ToString("dd MMM, yyyy · HH:mm")
                        : "-";
                    string by = Html(r["admin_name"]);
                    if (string.IsNullOrEmpty(by)) by = "System Architecture";
                    string st = NormalizeStatus(Convert.ToString(r["status"])).ToUpperInvariant();

                    sb.Append("<tr class='hover:bg-off-white/30 transition-colors'>");
                    sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-400'>").Append(date).Append("</td>");
                    sb.Append("<td class='px-8 py-5 text-sm font-serif text-text-dark'>").Append(by).Append("</td>");
                    sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-primary'>").Append(st).Append("</td>");
                    sb.Append("</tr>");
                }
            }
            lit.Text = sb.ToString();
        }

        private void ToggleButtonsForStatus()
        {
            string status = Convert.ToString(ViewState["status"] ?? "").ToLowerInvariant();

            var bAccept = Find<Button>("btnAccept");
            var bDelivering = Find<Button>("btnDelivering");
            var bDelivered = Find<Button>("btnDelivered");
            var bCancel = Find<Button>("btnCancel");

            if (bAccept != null) bAccept.Visible = false;
            if (bDelivering != null) bDelivering.Visible = false;
            if (bDelivered != null) bDelivered.Visible = false;
            if (bCancel != null) bCancel.Visible = false;

            if (status == "pending")
            {
                if (bAccept != null) bAccept.Visible = true;
                if (bCancel != null) bCancel.Visible = true; // cancel only allowed here
            }
            else if (status == "accepted")
            {
                if (bDelivering != null) bDelivering.Visible = true;
                if (bCancel != null) bCancel.Visible = true;
            }
            else if (status == "inprocess")
            {
                if (bDelivered != null) bDelivered.Visible = true;
            }
            // completed/canceled -> terminal, no buttons
        }

        // -------- helpers --------
        private int SafeId()
        {
            var hid = Find<HiddenField>("hidId");
            int id;
            return (hid != null && int.TryParse(hid.Value, out id)) ? id : 0;
        }

        private static bool TryGetQueryInt(string key, out int value)
        {
            value = 0;
            string s = HttpContext.Current.Request.QueryString[key];
            return !string.IsNullOrEmpty(s) && int.TryParse(s, out value);
        }

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

        private void SetHidden(string id, string v) { var h = Find<HiddenField>(id); if (h != null) h.Value = v ?? ""; }
        private void SetText(string id, string v) { var l = Find<Literal>(id); if (l != null) l.Text = v ?? ""; }

        private static string Html(object o) { return HttpUtility.HtmlEncode(Convert.ToString(o) ?? ""); }
        private static string SafeDisplay(object o)
        {
            var s = Convert.ToString(o);
            return string.IsNullOrWhiteSpace(s) ? "-" : HttpUtility.HtmlEncode(s);
        }

        private void Show(Label lbl, string msg, bool ok)
        {
            if (lbl == null) return;
            lbl.Text = HttpUtility.HtmlEncode(msg);
            lbl.Visible = true;
        }

        private int GetAdminId()
        {
            try
            {
                // 1) Prefer FormsAuth identity
                if (Context != null && Context.User != null &&
                    Context.User.Identity != null && Context.User.Identity.IsAuthenticated)
                {
                    string uname = Context.User.Identity.Name;
                    if (!string.IsNullOrEmpty(uname))
                    {
                        int id = Db.Scalar<int>("SELECT id FROM dbo.admins WHERE username=@u", Db.P("@u", uname));
                        if (id > 0) return id;
                    }
                }

                // 2) Fallback to persistent token cookie
                HttpCookie ck = (Request != null) ? Request.Cookies["AdminToken"] : null;
                if (ck != null && !string.IsNullOrEmpty(ck.Value))
                {
                    int id2 = Db.Scalar<int>(
                        "SELECT id FROM dbo.admins WHERE persistent_token=@t AND token_expires > GETUTCDATE()",
                        Db.P("@t", ck.Value));
                    if (id2 > 0) return id2;
                }
            }
            catch
            {
                // ignore and fall through
            }
            return 0;
        }
    }
}
