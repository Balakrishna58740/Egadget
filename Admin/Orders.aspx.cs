using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace serena.Admin
{
    public partial class OrdersPage : Page
    {
        private const int PAGE_SIZE = 10;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                SetText("txtCode", Request.QueryString["code"]);
                SetText("txtName", Request.QueryString["name"]);
                SetText("txtFrom", Request.QueryString["from"]);
                SetText("txtTo", Request.QueryString["to"]);

                BindAll();
            }
        }

        // ---- Events ----
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            string code = GetText("txtCode");
            string name = GetText("txtName");
            string from = GetText("txtFrom");
            string to = GetText("txtTo");
            string status = Convert.ToString(Request.QueryString["status"]); // keep selected pill

            var qs = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(code)) qs["code"] = code;
            if (!string.IsNullOrWhiteSpace(name)) qs["name"] = name;
            if (!string.IsNullOrWhiteSpace(from)) qs["from"] = from;
            if (!string.IsNullOrWhiteSpace(to)) qs["to"] = to;
            if (!string.IsNullOrWhiteSpace(status)) qs["status"] = status;
            qs["page"] = "1";

            Response.Redirect("~/Admin/Orders.aspx" + (qs.Count > 0 ? "?" + qs.ToString() : ""));
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Admin/Orders.aspx");
        }

        // ---- Main bind ----
        private void BindAll()
        {
            var litRows = Find<Literal>("litRows");
            var litTotal = Find<Literal>("litTotal");
            var litPills = Find<Literal>("litStatusPills");
            var pagerLit = Find<Literal>("pager");
            var lbl = Find<Label>("lblMsg");
            if (litRows == null) return;

            try
            {
                string code = Request.QueryString["code"];
                string name = Request.QueryString["name"];
                string fromStr = Request.QueryString["from"];
                string toStr = Request.QueryString["to"];
                string status = (Request.QueryString["status"] ?? "").ToLowerInvariant();

                int page = 1;
                int.TryParse(Request.QueryString["page"], out page);
                if (page < 1) page = 1;

                // Base WHERE & params
                List<SqlParameter> baseParams;
                string baseWhere = BuildBaseWhere(code, name, fromStr, toStr, out baseParams);

                // Total (clone params to avoid reuse issue)
                int total = Db.Scalar<int>("SELECT COUNT(*) FROM orders o" + baseWhere, CloneParams(baseParams));
                if (litTotal != null) litTotal.Text = total.ToString("N0");

                // Status counts (clone again)
                var counts = LoadStatusCounts(baseWhere, baseParams);
                if (litPills != null) litPills.Text = BuildStatusPills(counts, status, code, name, fromStr, toStr);

                // List WHERE (fresh params)
                List<SqlParameter> listParams;
                string where = BuildBaseWhere(code, name, fromStr, toStr, out listParams);
                if (!string.IsNullOrEmpty(status))
                {
                    AppendStatusFilter(status, listParams, ref where);
                }

                int filtered = Db.Scalar<int>("SELECT COUNT(*) FROM orders o" + where, listParams.ToArray());

                int pageCount = Math.Max(1, (int)Math.Ceiling(filtered / (double)PAGE_SIZE));
                if (page > pageCount) page = pageCount;
                int offset = (page - 1) * PAGE_SIZE;

                // Data params (fresh again)
                List<SqlParameter> dataParams;
                string where2 = BuildBaseWhere(code, name, fromStr, toStr, out dataParams);
                if (!string.IsNullOrEmpty(status))
                {
                    AppendStatusFilter(status, dataParams, ref where2);
                }
                dataParams.Add(Db.P("@offset", offset));
                dataParams.Add(Db.P("@limit", PAGE_SIZE));

                var sql = new StringBuilder(@"
SELECT 
  o.id,
  o.order_code,
  o.total_amount,
  o.payment,
  o.status,
  o.order_date,
  o.ship_name,
  o.ship_phone,
  m.full_name,
  m.email
FROM orders o
LEFT JOIN members m ON m.id = o.member_id");
                sql.Append(where2);
                sql.Append(" ORDER BY o.order_date DESC");
                sql.Append(@"
 OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;");

                var dt = Db.Query(sql.ToString(), dataParams.ToArray());

                var sb = new StringBuilder();
                if (dt.Rows.Count == 0)
                {
                    sb.Append("<tr><td colspan='7' class='px-8 py-12 text-center text-gray-400 text-xs italic'>No transactions recorded matching your search logic.</td></tr>");
                }
                else
                {
                    int i = offset;
                    for (int rix = 0; rix < dt.Rows.Count; rix++)
                    {
                        DataRow r = dt.Rows[rix];
                        i++;

                        int id = Convert.ToInt32(r["id"]);
                        string codeVal = Html(r["order_code"]);
                        string shipName = Html(r["ship_name"]);
                        string fullName = Html(r["full_name"]);
                        string email = Html(r["email"]);
                        string phone = Html(r["ship_phone"]);
                        string cust = !string.IsNullOrEmpty(shipName) ? shipName : (!string.IsNullOrEmpty(fullName) ? fullName : "-");

                        string payment = Html(r["payment"]);

                        string st = Convert.ToString(r["status"] ?? "");
                        string dtStr = "";
                        if (r["order_date"] != DBNull.Value)
                        {
                            DateTime od = Convert.ToDateTime(r["order_date"]);
                            dtStr = od.ToString("dd MMM, yyyy");
                        }

                        decimal totalAmt = 0m;
                        if (r["total_amount"] != DBNull.Value) totalAmt = Convert.ToDecimal(r["total_amount"]);

                        sb.Append("<tr class='hover:bg-off-white/30 transition-colors'>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-300'>").Append(codeVal).Append("</td>");
                        sb.Append("<td class='px-8 py-5'>").Append(RenderStatusBadge(st)).Append("</td>");
                        sb.Append("<td class='px-8 py-5'>");
                        sb.Append("<div class='text-sm font-serif text-text-dark'>").Append(cust).Append("</div>");
                        if (!string.IsNullOrEmpty(email))
                        {
                            sb.Append("<div class='text-[10px] text-gray-400 uppercase tracking-widest font-bold'>").Append(email).Append("</div>");
                        }
                        sb.Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-400'>").Append(string.IsNullOrEmpty(payment) ? "-" : payment).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-right font-bold text-text-dark text-sm'>RS ").Append(totalAmt.ToString("N2")).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-400'>").Append(dtStr).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-right'>");
                        sb.Append("<a class='text-primary hover:underline text-[10px] uppercase tracking-widest font-bold' href='OrderView.aspx?id=").Append(id).Append("'>View Details</a>");
                        sb.Append("</td>");
                        sb.Append("</tr>");
                    }
                }
                litRows.Text = sb.ToString();

                if (pagerLit != null)
                    pagerLit.Text = BuildPager(filtered, page, pageCount, code, name, status, fromStr, toStr);
            }
            catch (Exception ex)
            {
                Show(lbl, "System error: " + Server.HtmlEncode(ex.Message), false);
                litRows.Text = "<tr><td colspan='7' class='text-center text-red-500 py-12 text-xs uppercase tracking-widest font-bold'>Database communication failure.</td></tr>";
                if (pagerLit != null) pagerLit.Text = "";
            }
        }

        // Base WHERE: order_code, customer name, date range
        private static string BuildBaseWhere(string code, string name, string fromStr, string toStr, out List<SqlParameter> parms)
        {
            parms = new List<SqlParameter>();
            var sb = new StringBuilder(" WHERE 1=1 ");

            if (!string.IsNullOrWhiteSpace(code))
            {
                sb.Append(" AND o.order_code LIKE @code ");
                parms.Add(Db.P("@code", "%" + code + "%"));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                sb.Append(" AND ( COALESCE(o.ship_name,'') LIKE @name OR EXISTS (SELECT 1 FROM members mx WHERE mx.id=o.member_id AND mx.full_name LIKE @name) ) ");
                parms.Add(Db.P("@name", "%" + name + "%"));
            }

            DateTime fromDt;
            if (TryParseDate(fromStr, out fromDt))
            {
                fromDt = fromDt.Date;
                sb.Append(" AND o.order_date >= @from ");
                parms.Add(Db.P("@from", fromDt));
            }

            DateTime toDt;
            if (TryParseDate(toStr, out toDt))
            {
                DateTime toExclusive = toDt.Date.AddDays(1);
                sb.Append(" AND o.order_date < @to ");
                parms.Add(Db.P("@to", toExclusive));
            }

            return sb.ToString();
        }

        // Clone SqlParameters to avoid "already contained" errors
        private static SqlParameter[] CloneParams(IEnumerable<SqlParameter> src)
        {
            var list = new List<SqlParameter>();
            foreach (var p in src)
            {
                var np = new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value);
                list.Add(np);
            }
            return list.ToArray();
        }

        private static void AppendStatusFilter(string status, List<SqlParameter> parms, ref string where)
        {
            string s = (status ?? "").Trim().ToLowerInvariant();
            if (s == "accepted") where += " AND LOWER(COALESCE(o.status,'')) IN ('accepted','paid','processing') ";
            else if (s == "inprocess") where += " AND LOWER(COALESCE(o.status,'')) IN ('inprocess','delivering') ";
            else if (s == "delivered") where += " AND LOWER(COALESCE(o.status,'')) IN ('delivered','completed') ";
            else
            {
                where += " AND LOWER(COALESCE(o.status,'')) = @st ";
                parms.Add(Db.P("@st", s));
            }
        }

        // Status counts (by status) under base filters
        private static Dictionary<string, int> LoadStatusCounts(string baseWhere, List<SqlParameter> baseParams)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var dt = Db.Query(@"
SELECT LOWER(COALESCE(o.status,'')) AS s, COUNT(*) AS c
FROM orders o
" + baseWhere + @"
GROUP BY LOWER(COALESCE(o.status,''));", CloneParams(baseParams));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var r = dt.Rows[i];
                string s = Convert.ToString(r["s"] ?? "").Trim();
                int c = Convert.ToInt32(r["c"]);
                if (string.IsNullOrEmpty(s)) s = "unknown";
                if (s == "paid" || s == "processing") s = "accepted";
                else if (s == "delivering") s = "inprocess";
                else if (s == "completed") s = "delivered";

                int existing;
                if (map.TryGetValue(s, out existing)) map[s] = existing + c;
                else map[s] = c;
            }
            return map;
        }

        // Status pills (order: ALL, PENDING, ACCEPTED, DELIVERING, DELIVERED, CANCELED)
        private static string BuildStatusPills(Dictionary<string, int> counts, string current, string code, string name, string from, string to)
        {
            string[] keys = new[] { "", "pending", "accepted", "inprocess", "delivered", "canceled" };
            Func<string, string> label = k =>
            {
                if (string.IsNullOrEmpty(k)) return "All Events";
                if (string.Equals(k, "inprocess", StringComparison.OrdinalIgnoreCase)) return "IN PROCESS";
                return k.ToUpperInvariant();
            };

            Func<string, string> url = st =>
            {
                var qs = HttpUtility.ParseQueryString(string.Empty);
                if (!string.IsNullOrWhiteSpace(code)) qs["code"] = code;
                if (!string.IsNullOrWhiteSpace(name)) qs["name"] = name;
                if (!string.IsNullOrWhiteSpace(from)) qs["from"] = from;
                if (!string.IsNullOrWhiteSpace(to)) qs["to"] = to;
                if (!string.IsNullOrEmpty(st)) qs["status"] = st;
                qs["page"] = "1";
                return "Orders.aspx" + (qs.Count > 0 ? "?" + qs.ToString() : "");
            };

            var sb = new StringBuilder();
            for (int i = 0; i < keys.Length; i++)
            {
                string k = keys[i];
                bool isActive = (string.IsNullOrEmpty(current) && string.IsNullOrEmpty(k)) ||
                                (!string.IsNullOrEmpty(current) && string.Equals(current, k, StringComparison.OrdinalIgnoreCase));

                int count = 0;
                if (i == 0) { foreach (var v in counts.Values) count += v; }
                else { count = counts.ContainsKey(k) ? counts[k] : 0; }

                string colorClass = "bg-white text-gray-400 border-gray-100 hover:border-gray-300";
                if (isActive) colorClass = "bg-primary text-white border-primary shadow-lg shadow-primary/20 scale-105 z-10";

                sb.Append("<a href='").Append(url(k)).Append("' class='flex-shrink-0 flex items-center gap-4 px-6 py-4 border transition-all ").Append(colorClass).Append("'>");
                sb.Append("<span class='text-[10px] uppercase tracking-widest font-bold'>").Append(label(k)).Append("</span>");
                sb.Append("<span class='text-[10px] px-2 py-0.5 border ").Append(isActive ? "border-white/30 text-white" : "border-gray-50 text-gray-300").Append(" font-bold'>").Append(count.ToString("N0")).Append("</span>");
                sb.Append("</a>");
            }
            return sb.ToString();
        }

        private static string RenderStatusBadge(string status)
        {
            string s = (status ?? "").Trim().ToLowerInvariant();
            if (s.Length == 0) return "<span class='text-[8px] uppercase tracking-widest font-bold px-3 py-1 border border-gray-200 text-gray-300'>Unknown</span>";
            
            string cls = "border-gray-200 text-gray-300";
            if (s == "pending") cls = "border-orange-200 text-orange-400";
            else if (s == "accepted" || s == "paid" || s == "processing") cls = "border-green-200 text-green-500";
            else if (s == "inprocess" || s == "delivering") cls = "border-blue-200 text-blue-400";
            else if (s == "delivered" || s == "completed") cls = "border-primary text-primary";
            else if (s == "canceled") cls = "border-red-200 text-red-500";
            
            return "<span class='text-[8px] uppercase tracking-widest font-bold px-3 py-1 border " + cls + "'>" + HttpUtility.HtmlEncode(s.ToUpperInvariant()) + "</span>";
        }

        // ---- Pager ----
        private static string BuildPager(int totalFiltered, int page, int pageCount, string code, string name, string status, string from, string to)
        {
            var sb = new StringBuilder();
            sb.Append("<div class='flex items-center space-x-2'>");

            Func<int, string> url = p =>
            {
                var qs = HttpUtility.ParseQueryString(string.Empty);
                qs["page"] = p.ToString();
                if (!string.IsNullOrWhiteSpace(code)) qs["code"] = code;
                if (!string.IsNullOrWhiteSpace(name)) qs["name"] = name;
                if (!string.IsNullOrWhiteSpace(status)) qs["status"] = status;
                if (!string.IsNullOrWhiteSpace(from)) qs["from"] = from;
                if (!string.IsNullOrWhiteSpace(to)) qs["to"] = to;
                return "Orders.aspx" + (qs.Count > 0 ? "?" + qs.ToString() : "");
            };

            bool hasPrev = page > 1;
            sb.Append("<a href='").Append(hasPrev ? url(page - 1) : "#")
              .Append("' class='p-3 border ").Append(hasPrev ? "border-gray-200 text-text-dark hover:bg-primary hover:text-white" : "border-gray-100 text-gray-200 cursor-not-allowed")
              .Append(" transition-all'><i class='fa-solid fa-chevron-left text-[10px]'></i></a>");

            const int window = 7;
            int start = Math.Max(1, page - (window / 2));
            int end = Math.Min(pageCount, start + window - 1);
            if (end - start + 1 < window) start = Math.Max(1, end - window + 1);

            for (int p = start; p <= end; p++)
            {
                bool active = (p == page);
                sb.Append("<a href='").Append(url(p))
                  .Append("' class='w-10 h-10 flex items-center justify-center text-[10px] font-bold tracking-widest transition-all ")
                  .Append(active ? "bg-primary text-white" : "bg-white text-gray-400 hover:text-primary")
                  .Append("'>").Append(p).Append("</a>");
            }

            bool hasNext = page < pageCount;
            sb.Append("<a href='").Append(hasNext ? url(page + 1) : "#")
              .Append("' class='p-3 border ").Append(hasNext ? "border-gray-200 text-text-dark hover:bg-primary hover:text-white" : "border-gray-100 text-gray-200 cursor-not-allowed")
              .Append(" transition-all'><i class='fa-solid fa-chevron-right text-[10px]'></i></a>");

            sb.Append("</div>");
            return sb.ToString();
        }

        // ---- helpers ----
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
        private void SetText(string id, string v) { var t = Find<TextBox>(id); if (t != null) t.Text = v ?? ""; }
        private string GetText(string id) { var t = Find<TextBox>(id); return t != null ? t.Text.Trim() : ""; }
        private static string Html(object o) { return HttpUtility.HtmlEncode(Convert.ToString(o) ?? ""); }
        private static bool TryParseDate(string s, out DateTime d)
        {
            d = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(s)) return false;
            DateTime x;
            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out x)) { d = x; return true; }
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out x)) { d = x; return true; }
            return false;
        }
        private void Show(Label lbl, string msg, bool ok) { if (lbl == null) return; lbl.Text = HttpUtility.HtmlEncode(msg); lbl.Visible = true; }
    }
}
