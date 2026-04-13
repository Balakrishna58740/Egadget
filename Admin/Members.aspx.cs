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
    public partial class MembersPage : Page
    {
        private const int PAGE_SIZE = 10;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                // Handle filter seed
                var txt = Find<TextBox>("txtName");
                if (txt != null) txt.Text = Request.QueryString["q"];
                var from = Find<TextBox>("dtFrom");
                if (from != null) from.Text = Request.QueryString["from"];
                var to = Find<TextBox>("dtTo");
                if (to != null) to.Text = Request.QueryString["to"];

                BindTable();
            }
        }

        // ---------- Events ----------
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            string q = GetText("txtName");
            string from = GetText("dtFrom");
            string to = GetText("dtTo");

            var qs = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(q)) qs["q"] = q;
            if (!string.IsNullOrWhiteSpace(from)) qs["from"] = from;
            if (!string.IsNullOrWhiteSpace(to)) qs["to"] = to;
            qs["page"] = "1";

            Response.Redirect("~/Admin/Members.aspx" + (qs.Count > 0 ? "?" + qs.ToString() : ""));
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Admin/Members.aspx");
        }

        // ---------- Main bind ----------
        private void BindTable()
        {
            var lit = Find<Literal>("litRows");
            var litStats = Find<Literal>("litTopStats");
            var pager = Find<Literal>("pager");
            var lbl = Find<Label>("lblMsg");
            if (lit == null) return;

            try
            {
                string q = Request.QueryString["q"];
                string fromStr = Request.QueryString["from"];
                string toStr = Request.QueryString["to"];

                int page = 1;
                int.TryParse(Request.QueryString["page"], out page);
                if (page < 1) page = 1;

                // WHERE
                List<SqlParameter> countParams;
                string where = BuildWhere(q, fromStr, toStr, out countParams);

                // Count (filtered)
                int total = Db.Scalar<int>("SELECT COUNT(*) FROM members m" + where, countParams.ToArray());
                
                if (litStats != null)
                {
                    litStats.Text = string.Format(@"
                        <div class='bg-white border border-gray-100 px-6 py-4 shadow-sm flex items-center gap-4'>
                            <div class='w-10 h-10 bg-primary/10 rounded-full flex items-center justify-center text-primary'>
                                <i class='fa-solid fa-users'></i>
                            </div>
                            <div>
                                <span class='text-[10px] uppercase tracking-widest text-gray-400 font-bold block'>Patron Threshold</span>
                                <span class='text-lg font-bold text-text-dark'>{0:N0}</span>
                            </div>
                        </div>", total);
                }

                int pageCount = Math.Max(1, (int)Math.Ceiling(total / (double)PAGE_SIZE));
                if (page > pageCount) page = pageCount;
                int offset = (page - 1) * PAGE_SIZE;

                // Data params
                List<SqlParameter> dataParams;
                BuildWhere(q, fromStr, toStr, out dataParams);
                dataParams.Add(Db.P("@offset", offset));
                dataParams.Add(Db.P("@limit", PAGE_SIZE));

                var sql = new StringBuilder(@"
SELECT m.id, m.full_name, m.email, m.phone, m.created_at
FROM members m");
                sql.Append(where);
                sql.Append(" ORDER BY m.created_at DESC");
                sql.Append(@"
 OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;");

                var dt = Db.Query(sql.ToString(), dataParams.ToArray());

                var sb = new StringBuilder();
                if (dt.Rows.Count == 0)
                {
                    sb.Append("<tr><td colspan='5' class='px-8 py-12 text-center text-gray-400 text-xs italic'>The community repository currently houses no records matching your criteria.</td></tr>");
                }
                else
                {
                    int i = offset;
                    foreach (DataRow r in dt.Rows)
                    {
                        i++;
                        string fullName = Html(r["full_name"]);
                        string email = Html(r["email"]);
                        string phone = Html(r["phone"]);
                        DateTime created = Convert.ToDateTime(r["created_at"]);
                        string createdStr = created.ToString("dd MMM, yyyy · HH:mm");

                        sb.Append("<tr class='hover:bg-off-white/30 transition-colors'>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-400'>").Append(i.ToString("D3")).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-sm font-serif text-text-dark font-bold'>").Append(fullName).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-sm text-text-dark underline decoration-primary/20'>").Append(email).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-400'>").Append(string.IsNullOrEmpty(phone) ? "—" : phone).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-primary'>").Append(createdStr).Append("</td>");
                        sb.Append("</tr>");
                    }
                }
                lit.Text = sb.ToString();

                if (pager != null)
                    pager.Text = BuildPager(total, page, pageCount, q, fromStr, toStr);
            }
            catch (Exception ex)
            {
                if (lbl != null) { lbl.Text = "Architectural disruption: " + HttpUtility.HtmlEncode(ex.Message); lbl.Visible = true; }
                if (lit != null) lit.Text = "<tr><td colspan='5' class='px-8 py-12 text-center text-red-400 text-xs'>Failed to retrieve community dossier.</td></tr>";
            }
        }

        // ---------- WHERE helper ----------
        private static string BuildWhere(string q, string fromStr, string toStr, out List<SqlParameter> parms)
        {
            parms = new List<SqlParameter>();
            var sb = new StringBuilder(" WHERE 1=1 ");

            if (!string.IsNullOrWhiteSpace(q))
            {
                sb.Append(" AND m.full_name LIKE @q ");
                parms.Add(Db.P("@q", "%" + q + "%"));
            }

            DateTime fromDt;
            if (TryParseDate(fromStr, out fromDt))
            {
                fromDt = fromDt.Date; 
                sb.Append(" AND m.created_at >= @from ");
                parms.Add(Db.P("@from", fromDt));
            }

            DateTime toDt;
            if (TryParseDate(toStr, out toDt))
            {
                DateTime toExclusive = toDt.Date.AddDays(1);
                sb.Append(" AND m.created_at < @to ");
                parms.Add(Db.P("@to", toExclusive));
            }

            return sb.ToString();
        }

        // ---------- UI helpers ----------
        private static string BuildPager(int total, int page, int pageCount, string q, string from, string to)
        {
            if (pageCount <= 1) return "";
            var sb = new StringBuilder();
            sb.Append("<div class='flex items-center gap-2'>");

            Func<int, string> url = p => BuildUrl(p, q, from, to);
            
            Action<bool, int, string> item = (isActive, p, inner) =>
            {
                string baseCls = "w-10 h-10 flex items-center justify-center text-[10px] font-bold transition-all border ";
                if (isActive)
                    sb.Append("<span class='").Append(baseCls).Append("bg-primary border-primary text-white cursor-default shadow-lg shadow-primary/20'>").Append(inner).Append("</span>");
                else
                    sb.Append("<a href='").Append(url(p)).Append("' class='").Append(baseCls).Append("bg-white border-gray-100 text-gray-400 hover:border-primary hover:text-primary'>").Append(inner).Append("</a>");
            };

            const int window = 5;
            int start = Math.Max(1, page - (window / 2));
            int end = Math.Min(pageCount, start + window - 1);
            if (end - start + 1 < window) start = Math.Max(1, end - window + 1);

            if (page > 1) 
            {
                sb.Append("<a href='").Append(url(page - 1)).Append("' class='w-10 h-10 flex items-center justify-center bg-white border border-gray-100 text-gray-400 hover:text-primary transition-all'><i class='fa-solid fa-chevron-left text-[10px]'></i></a>");
            }

            for (int p = start; p <= end; p++)
            {
                item(p == page, p, p.ToString("D2"));
            }

            if (page < pageCount)
            {
                sb.Append("<a href='").Append(url(page + 1)).Append("' class='w-10 h-10 flex items-center justify-center bg-white border border-gray-100 text-gray-400 hover:text-primary transition-all'><i class='fa-solid fa-chevron-right text-[10px]'></i></a>");
            }

            sb.Append("</div>");
            return sb.ToString();
        }

        private static string BuildUrl(int p, string q, string from, string to)
        {
            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["page"] = p.ToString();
            if (!string.IsNullOrWhiteSpace(q)) qs["q"] = q;
            if (!string.IsNullOrWhiteSpace(from)) qs["from"] = from;
            if (!string.IsNullOrWhiteSpace(to)) qs["to"] = to;
            return "Members.aspx" + (qs.Count > 0 ? "?" + qs.ToString() : "");
        }

        // ---------- Generic helpers ----------
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

        private string GetText(string id)
        {
            var t = Find<TextBox>(id);
            return t != null ? t.Text.Trim() : "";
        }

        private static string Html(object o)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(o) ?? "");
        }

        private static bool TryParseDate(string s, out DateTime d)
        {
            d = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(s)) return false;
            DateTime x;
            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out x))
            {
                d = x;
                return true;
            }
            return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
        }
    }
}
