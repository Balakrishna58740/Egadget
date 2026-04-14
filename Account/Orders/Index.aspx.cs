using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace serena.Site.Account.Orders
{
    public partial class OrderIndexPage : Page
    {
        private static readonly string[] Allowed = new[] { "all", "pending", "accepted", "inprocess", "delivered", "canceled" };

        private string _status;
        private string _codeLike;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int memberId = GetMemberId();
            if (memberId <= 0)
            {
                Response.Redirect("~/Account/Login.aspx?returnUrl=" + HttpUtility.UrlEncode(Request.RawUrl), true);
                return;
            }

            _status = CanonicalStatus(Request.QueryString["status"]);
            _codeLike = (Request.QueryString["q"] ?? "").Trim();

            // If searching by code, always normalize to ALL so UI matches data
            if (!string.IsNullOrEmpty(_codeLike) &&
                !string.Equals(_status, "all", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(ResolveUrl("~/Account/Orders/Index.aspx?status=all&q=" +
                    HttpUtility.UrlEncode(_codeLike) + "&page=1"), true);
                return;
            }

            if (!IsPostBack)
            {
                var txtCode = Find<TextBox>("txtCode");
                var lnkClear = Find<HyperLink>("lnkClear");
                if (txtCode != null) txtCode.Text = _codeLike; // <-- don't overwrite on postback
                if (lnkClear != null) lnkClear.NavigateUrl = ResolveUrl("~/Account/Orders/Index.aspx?status=all&page=1");
            }

            BindStatusBoard(memberId);
            BindOrders(memberId);
        }

        private int GetMemberId() { try { return Convert.ToInt32(Session["MEMBER_ID"] ?? 0); } catch { return 0; } }
        private string CanonicalStatus(string s)
        {
            string v = (s ?? "").Trim().ToLowerInvariant();
            foreach (var a in Allowed) if (a == v) return v;
            return "all";
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            // Grab the posted value (most reliable)
            var txtCode = Find<TextBox>("txtCode");
            string q = (txtCode != null ? (Request.Form[txtCode.UniqueID] ?? txtCode.Text ?? "") : "").Trim();

            // Always switch to ALL when searching by code, and start at page 1
            Response.Redirect(ResolveUrl(
                "~/Account/Orders/Index.aspx?status=all&q=" + HttpUtility.UrlEncode(q) + "&page=1"
            ), true);
        }


        /* ---------- Dashboard ---------- */
        private void BindStatusBoard(int memberId)
        {
            int total = Db.Scalar<int>("SELECT COUNT(*) FROM dbo.orders WHERE member_id=@m;", Db.P("@m", memberId));

            var dt = Db.Query(@"
SELECT
  SUM(CASE WHEN LOWER(LTRIM(RTRIM(status)))='pending'    THEN 1 ELSE 0 END) AS Pending,
  SUM(CASE WHEN LOWER(LTRIM(RTRIM(status))) IN ('accepted','paid','processing') THEN 1 ELSE 0 END) AS Accepted,
  SUM(CASE WHEN LOWER(LTRIM(RTRIM(status))) IN ('inprocess','delivering') THEN 1 ELSE 0 END) AS InProcess,
  SUM(CASE WHEN LOWER(LTRIM(RTRIM(status))) IN ('delivered','completed') THEN 1 ELSE 0 END) AS Delivered,
  SUM(CASE WHEN LOWER(LTRIM(RTRIM(status)))='canceled'   THEN 1 ELSE 0 END) AS Canceled
FROM dbo.orders WHERE member_id=@m;", new SqlParameter("@m", memberId));

            int cPending = 0, cAccepted = 0, cInProcess = 0, cDelivered = 0, cCanceled = 0;
            if (dt != null && dt.Rows.Count > 0)
            {
                var r = dt.Rows[0];
                cPending = SafeInt(r["Pending"]);
                cAccepted = SafeInt(r["Accepted"]);
                cInProcess = SafeInt(r["InProcess"]);
                cDelivered = SafeInt(r["Delivered"]);
                cCanceled = SafeInt(r["Canceled"]);
            }

            var view = new DataTable();
            view.Columns.Add("LabelUpper", typeof(string));
            view.Columns.Add("Count", typeof(int));
            view.Columns.Add("Href", typeof(string));
            view.Columns.Add("CssClass", typeof(string));
            view.Columns.Add("BadgeClass", typeof(string));

            string baseUrl = ResolveUrl("~/Account/Orders/Index.aspx");
            string qparam = string.IsNullOrEmpty(_codeLike) ? "" : "&q=" + HttpUtility.UrlEncode(_codeLike);
            string page1 = "&page=1";

            AddTile(view, "all", "ALL", total, baseUrl + "?status=all" + qparam + page1, IsActive("all"));
            AddTile(view, "pending", "PENDING", cPending, baseUrl + "?status=pending" + qparam + page1, IsActive("pending"));
            AddTile(view, "accepted", "ACCEPTED", cAccepted, baseUrl + "?status=accepted" + qparam + page1, IsActive("accepted"));
            AddTile(view, "inprocess", "IN PROCESS", cInProcess, baseUrl + "?status=inprocess" + qparam + page1, IsActive("inprocess"));
            AddTile(view, "delivered", "DELIVERED", cDelivered, baseUrl + "?status=delivered" + qparam + page1, IsActive("delivered"));
            AddTile(view, "canceled", "CANCELED", cCanceled, baseUrl + "?status=canceled" + qparam + page1, IsActive("canceled"));

            var rptStatus = Find<Repeater>("rptStatus");
            if (rptStatus != null)
            {
                rptStatus.DataSource = view;
                rptStatus.DataBind();
            }
        }

        private void AddTile(DataTable v, string key, string labelUpper, int count, string href, bool active)
        {
            string btn = "btn btn-sm d-inline-flex align-items-center gap-2 ";
            switch ((key ?? "").ToLowerInvariant())
            {
                case "pending": btn += active ? "btn-warning text-dark" : "btn-outline-warning"; break;
                case "accepted": btn += active ? "btn-info" : "btn-outline-info"; break;
                case "inprocess": btn += active ? "btn-primary" : "btn-outline-primary"; break;
                case "delivered": btn += active ? "btn-success" : "btn-outline-success"; break;
                case "canceled": btn += active ? "btn-danger" : "btn-outline-danger"; break;
                default: btn += active ? "btn-dark" : "btn-outline-dark"; break; // ALL
            }

            var row = v.NewRow();
            row["LabelUpper"] = labelUpper;
            row["Count"] = count;
            row["Href"] = href;
            row["CssClass"] = btn;
            row["BadgeClass"] = active ? "badge rounded-pill bg-light text-dark"
                                       : "badge rounded-pill bg-secondary";
            v.Rows.Add(row);
        }

        private bool IsActive(string st)
        {
            string a = _status ?? "all";
            return string.Equals(a, st, StringComparison.OrdinalIgnoreCase);
        }

        /* ---------- Data bind (ListView) ---------- */
        private void BindOrders(int memberId)
        {
            string sql = @"
SELECT id, order_code, status, payment, total_qty, total_amount, order_date
FROM dbo.orders WHERE member_id=@m";

            var ps = new System.Collections.Generic.List<SqlParameter> { new SqlParameter("@m", memberId) };

            if (!string.Equals(_status, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(_status, "accepted", StringComparison.OrdinalIgnoreCase))
                    sql += " AND LOWER(LTRIM(RTRIM(status))) IN ('accepted','paid','processing')";
                else if (string.Equals(_status, "inprocess", StringComparison.OrdinalIgnoreCase))
                    sql += " AND LOWER(LTRIM(RTRIM(status))) IN ('inprocess','delivering')";
                else if (string.Equals(_status, "delivered", StringComparison.OrdinalIgnoreCase))
                    sql += " AND LOWER(LTRIM(RTRIM(status))) IN ('delivered','completed')";
                else
                {
                    sql += " AND LOWER(LTRIM(RTRIM(status)))=@st";
                    ps.Add(new SqlParameter("@st", _status));
                }
            }
            if (!string.IsNullOrEmpty(_codeLike))
            {
                sql += " AND order_code LIKE @q";
                ps.Add(new SqlParameter("@q", "%" + _codeLike + "%"));
            }
            sql += " ORDER BY order_date DESC, id DESC;";

            var dt = Db.Query(sql, ps.ToArray()) ?? new DataTable();

            var phEmpty = Find<PlaceHolder>("phEmpty");
            var lvOrders = Find<ListView>("lvOrders");
            if (phEmpty != null) phEmpty.Visible = (dt.Rows.Count == 0);
            if (lvOrders != null)
            {
                lvOrders.DataSource = dt;
                lvOrders.DataBind();
            }
        }

        /* ---------- Helpers used by markup ---------- */
        public string FmtDate(object o) { try { return Convert.ToDateTime(o).ToString("yyyy-MM-dd HH:mm"); } catch { return "-"; } }
        public string UrlForCode(object codeObj)
        {
            string code = Convert.ToString(codeObj);
            return string.IsNullOrEmpty(code) ? "#" : ResolveUrl("~/Account/Orders/Detail.aspx?code=" + HttpUtility.UrlEncode(code));
        }
        public string Upper(object s)
        {
            string t = (Convert.ToString(s) ?? "").Trim().ToLowerInvariant();
            if (t == "paid") t = "accepted";
            else if (t == "delivering") t = "inprocess";
            else if (t == "completed") t = "delivered";
            return string.IsNullOrEmpty(t) ? "" : t.ToUpperInvariant();
        }
        public string StatusBadgeCss(object statusObj)
        {
            string s = (Convert.ToString(statusObj) ?? "").ToLowerInvariant();
            string cls = "badge rounded-pill ";
            if (s.Contains("pending")) cls += "bg-warning text-dark";
            else if (s.Contains("accepted") || s.Contains("paid")) cls += "bg-info";
            else if (s.Contains("inprocess") || s.Contains("delivering")) cls += "bg-primary";
            else if (s.Contains("delivered") || s.Contains("completed")) cls += "bg-success";
            else if (s.Contains("canceled")) cls += "bg-danger";
            else cls += "bg-light text-dark";
            return cls;
        }

        // Row number using DataPager's current start index
        public int GetRowNoLV(object containerObj)
        {
            var item = containerObj as ListViewDataItem;
            int idx = (item != null) ? item.DataItemIndex : 0;
            var pager = Find<DataPager>("pager");
            return (pager != null ? pager.StartRowIndex : 0) + idx + 1; // pager is the DataPager in the .aspx
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

        private int SafeInt(object o) { try { return Convert.ToInt32(o); } catch { return 0; } }
    }
}
