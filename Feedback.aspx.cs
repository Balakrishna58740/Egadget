using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace serena.Site
{
    public partial class FeedbackPage : Page
    {
        // Resolve controls reliably inside master page's MainContent
        private T FindInMain<T>(string id) where T : Control
        {
            var cph = Master != null ? Master.FindControl("MainContent") as ContentPlaceHolder : null;
            return (cph != null ? cph.FindControl(id) : null) as T ?? FindControl(id) as T;
        }

        // HTML server controls
        private HtmlGenericControl Alert { get { return FindInMain<HtmlGenericControl>("alertMsg"); } }
        private HtmlInputText NameBox { get { return FindInMain<HtmlInputText>("txtName"); } }
        private HtmlInputGenericControl EmailBox { get { return FindInMain<HtmlInputGenericControl>("txtEmail"); } }
        private HtmlInputText TitleBox { get { return FindInMain<HtmlInputText>("txtTitle"); } }
        private HtmlTextArea MsgBox { get { return FindInMain<HtmlTextArea>("txtMsg"); } }
        private HtmlGenericControl PhHistory { get { return FindInMain<HtmlGenericControl>("phHistory"); } }
        private HtmlGenericControl PhGuest { get { return FindInMain<HtmlGenericControl>("phGuestNote"); } }
        private HtmlGenericControl LitList { get { return FindInMain<HtmlGenericControl>("litList"); } }
        private HtmlGenericControl Pager { get { return FindInMain<HtmlGenericControl>("pager"); } }

        // Filters (email removed)
        private HtmlInputText FilterQ { get { return FindInMain<HtmlInputText>("txtFilterQ"); } }
        private HtmlSelect FilterStatus { get { return FindInMain<HtmlSelect>("ddlStatus"); } }

        // Paging settings
        private const int PAGE_SIZE = 10;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ToggleHistory(alwaysShowHistory: true);

                // Autofill when logged in (unconditional on first load)
                PrefillIfMember();

                // Sync filters from querystring
                if (FilterQ != null) FilterQ.Value = (Request["q"] ?? "").Trim();
                if (FilterStatus != null) FilterStatus.Value = (Request["st"] ?? "").Trim().ToLowerInvariant();

                BindHistory();
            }
        }

        protected void btnSend_ServerClick(object sender, EventArgs e)
        {
            string name = NameBox != null ? (NameBox.Value ?? "").Trim() : "";
            string email = EmailBox != null ? (EmailBox.Value ?? "").Trim() : "";
            string title = TitleBox != null ? (TitleBox.Value ?? "").Trim() : "";
            string msg = MsgBox != null ? (MsgBox.Value ?? "").Trim() : "";

            if (name.Length == 0) { Show("Please enter your name.", false); return; }
            if (email.Length == 0) { Show("Please enter your email.", false); return; }
            if (title.Length == 0) { Show("Please enter a title.", false); return; }
            if (msg.Length == 0) { Show("Please enter a message.", false); return; }
            if (email.IndexOf("@") < 1 || email.LastIndexOf(".") < 3)
            {
                Show("Please enter a valid email.", false);
                return;
            }

            int? memberId = null;
            try
            {
                if (Session["MEMBER_ID"] != null)
                {
                    int midParsed;
                    if (int.TryParse(Convert.ToString(Session["MEMBER_ID"]), out midParsed))
                        memberId = midParsed;
                }

                try
                {
                    global::Db.Execute(
                        "INSERT INTO dbo.feedbacks (member_id, title, name, email, message) " +
                        "VALUES (@mid, @t, @n, @e, @m)",
                        global::Db.P("@mid", (object)memberId ?? DBNull.Value),
                        global::Db.P("@t", title),
                        global::Db.P("@n", name),
                        global::Db.P("@e", email),
                        global::Db.P("@m", msg)
                    );
                }
                catch
                {
                    global::Db.Execute(
                        "INSERT INTO dbo.feedbacks (member_id, name, email, message) " +
                        "VALUES (@mid, @n, @e, @m)",
                        global::Db.P("@mid", (object)memberId ?? DBNull.Value),
                        global::Db.P("@n", name),
                        global::Db.P("@e", email),
                        global::Db.P("@m", msg)
                    );
                }

                Show("Thanks! Your message has been sent.", true);
                if (MsgBox != null) MsgBox.Value = "";
                if (TitleBox != null) TitleBox.Value = "";

                BindHistory();
            }
            catch
            {
                Show("Sorry, we couldn't send your message right now. Please try again.", false);
            }
        }

        protected void btnApplyFilter_ServerClick(object sender, EventArgs e)
        {
            string q = FilterQ != null ? (FilterQ.Value ?? "").Trim() : "";
            string st = FilterStatus != null ? (FilterStatus.Value ?? "").Trim().ToLowerInvariant() : "";

            var qs = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(q)) qs["q"] = q;
            if (st == "pending" || st == "replied") qs["st"] = st;

            string url = Request.Path + (qs.Count > 0 ? "?" + qs.ToString() : "");
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnClearFilter_ServerClick(object sender, EventArgs e)
        {
            Response.Redirect(Request.Path, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void PrefillIfMember()
        {
            try
            {
                if (Session["MEMBER_ID"] == null) return;

                int memberId;
                if (!int.TryParse(Convert.ToString(Session["MEMBER_ID"]), out memberId)) return;

                DataTable dt = global::Db.Query(
                    "SELECT TOP 1 full_name, email FROM dbo.members WHERE id=@id",
                    global::Db.P("@id", memberId)
                );

                if (dt != null && dt.Rows.Count > 0)
                {
                    if (NameBox != null) NameBox.Value = Convert.ToString(dt.Rows[0]["full_name"]);
                    if (EmailBox != null) EmailBox.Value = Convert.ToString(dt.Rows[0]["email"]);
                }
            }
            catch { }
        }

        private void BindHistory()
        {
            if (LitList == null || Pager == null) return;

            string q = (Request["q"] ?? "").Trim();
            string st = (Request["st"] ?? "").Trim().ToLowerInvariant();

            int page = 1;
            int.TryParse(Request["page"], out page);
            if (page < 1) page = 1;

            try
            {
                // Build WHERE (keyword == title OR name ONLY; no message/reply)
                var where = new StringBuilder(" WHERE 1=1 ");
                var pars = new System.Collections.Generic.List<SqlParameter>();

                if (!string.IsNullOrEmpty(q))
                {
                    where.Append(" AND ((title LIKE @q) OR (name LIKE @q))");
                    pars.Add(global::Db.P("@q", "%" + q + "%"));
                }
                if (st == "pending")
                    where.Append(" AND ISNULL(is_resolved,0)=0");
                else if (st == "replied")
                    where.Append(" AND ISNULL(is_resolved,0)=1");

                int total = global::Db.Scalar<int>(
                    "SELECT COUNT(*) FROM dbo.feedbacks " + where.ToString(),
                    pars.ToArray()
                );
                if (total <= 0)
                {
                    LitList.InnerHtml = "<div class='text-gray-400 text-xs text-center py-20 border border-dashed border-gray-100'>No feedback matching your search.</div>";
                    Pager.InnerHtml = "";
                    return;
                }

                int totalPages = (total + PAGE_SIZE - 1) / PAGE_SIZE;
                if (page > totalPages) page = totalPages;
                int offset = (page - 1) * PAGE_SIZE;

                var listPars = new System.Collections.Generic.List<SqlParameter>(pars);
                listPars.Add(global::Db.P("@off", offset));
                listPars.Add(global::Db.P("@ps", PAGE_SIZE));

                DataTable dt = null;
                try
                {
                    // Modern schema (has title, is_resolved)
                    dt = global::Db.Query(
                        "SELECT id, title, name, email, message, reply, is_resolved, created_at, updated_at " +
                        "FROM dbo.feedbacks " + where.ToString() +
                        " ORDER BY created_at DESC, id DESC " +
                        " OFFSET @off ROWS FETCH NEXT @ps ROWS ONLY",
                        listPars.ToArray()
                    );
                }
                catch
                {
                    // Fallback schema: filter ONLY by name (title/is_resolved may not exist)
                    var where2 = new StringBuilder(" WHERE 1=1 ");
                    var pars2 = new System.Collections.Generic.List<SqlParameter>();

                    if (!string.IsNullOrEmpty(q))
                    {
                        where2.Append(" AND (name LIKE @q)");
                        pars2.Add(global::Db.P("@q", "%" + q + "%"));
                    }
                    if (st == "pending")
                    {
                        // approximate: pending if no reply content
                        where2.Append(" AND (reply IS NULL OR LTRIM(RTRIM(reply))='')");
                    }
                    else if (st == "replied")
                    {
                        where2.Append(" AND (reply IS NOT NULL AND LTRIM(RTRIM(reply))<>'')");
                    }

                    total = global::Db.Scalar<int>(
                        "SELECT COUNT(*) FROM dbo.feedbacks " + where2.ToString(),
                        pars2.ToArray()
                    );

                    int totalPages2 = (total + PAGE_SIZE - 1) / PAGE_SIZE;
                    if (page > totalPages2) page = Math.Max(1, totalPages2);
                    offset = (page - 1) * PAGE_SIZE;

                    pars2.Add(global::Db.P("@off", offset));
                    pars2.Add(global::Db.P("@ps", PAGE_SIZE));

                    dt = global::Db.Query(
                        "SELECT id, NULL AS title, name, email, message, reply, NULL AS is_resolved, created_at, updated_at " +
                        "FROM dbo.feedbacks " + where2.ToString() +
                        " ORDER BY created_at DESC, id DESC " +
                        " OFFSET @off ROWS FETCH NEXT @ps ROWS ONLY",
                        pars2.ToArray()
                    );
                }

                var sb = new StringBuilder();
                sb.Append("<div class='space-y-8'>");
                foreach (DataRow r in dt.Rows)
                {
                    string title = SafeStr(r["title"]);
                    string message = SafeStr(r["message"]);
                    string reply = SafeStr(r["reply"]);
                    bool resolved = SafeBool(r["is_resolved"]) || (!string.IsNullOrWhiteSpace(reply));
                    string created = SafeDate(r["created_at"]);
                    string updated = SafeDate(r["updated_at"]);
                    string name = SafeStr(r["name"]);
                    string email = SafeStr(r["email"]);

                    sb.Append("<div class='bg-white border border-gray-100 p-8 hover:shadow-md transition-shadow duration-500'>");

                    sb.Append("<div class='flex justify-between items-start mb-6'>");
                    sb.Append("<div>");
                    sb.Append("<span class='text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-1'>").Append(Html(created)).Append("</span>");
                    if (!string.IsNullOrWhiteSpace(name))
                        sb.Append("<span class='text-sm font-serif'>").Append(Html(name)).Append("</span>");
                    sb.Append("</div>");
                    sb.Append("<span class='text-[8px] uppercase tracking-widest font-bold px-3 py-1 border transition-colors ").Append(resolved ? "border-primary text-primary" : "border-gray-200 text-gray-400").Append("'>")
                      .Append(resolved ? "Replied" : "Pending").Append("</span>");
                    sb.Append("</div>");

                    if (!string.IsNullOrWhiteSpace(title))
                        sb.Append("<h3 class='text-xs uppercase tracking-[0.2em] font-bold mb-3'>").Append(Html(title)).Append("</h3>");

                    sb.Append("<div class='text-sm text-gray-500 leading-relaxed mb-6'>").Append(Nl2Br(Html(message))).Append("</div>");

                    if (!string.IsNullOrWhiteSpace(reply))
                    {
                        sb.Append("<div class='bg-off-white p-6 border-l-2 border-primary'>");
                        sb.Append("<div class='text-[9px] uppercase tracking-widest font-bold text-primary mb-2'>Studio Response</div>");
                        sb.Append("<div class='text-sm text-gray-600 leading-relaxed'>").Append(Nl2Br(Html(reply))).Append("</div>");
                        if (updated.Length > 0)
                            sb.Append("<div class='text-[8px] uppercase tracking-widest text-gray-400 mt-4'>Updated ").Append(Html(updated)).Append("</div>");
                        sb.Append("</div>");
                    }

                    sb.Append("</div>");
                }
                sb.Append("</div>");

                LitList.InnerHtml = sb.ToString();
                Pager.InnerHtml = BuildPagerHtml(page, totalPages);
            }
            catch
            {
                LitList.InnerHtml = "<div class='text-primary text-xs py-10'>Feedback service is currently undergoing maintenance.</div>";
                Pager.InnerHtml = "";
            }
        }


        private string BuildPagerHtml(int page, int totalPages)
        {
            if (totalPages <= 1) return "";

            var qs = HttpUtility.ParseQueryString(string.Empty);
            string q = (Request["q"] ?? "").Trim();
            string st = (Request["st"] ?? "").Trim().ToLowerInvariant();

            if (!string.IsNullOrEmpty(q)) qs["q"] = q;
            if (st == "pending" || st == "replied") qs["st"] = st;

            string path = Request.Path;

            var p = new StringBuilder();
            p.Append("<div class='flex items-center space-x-2'>");

            bool hasPrev = page > 1;
            if (hasPrev) qs["page"] = (page - 1).ToString();
            p.Append("<a href='").Append(hasPrev ? (path + "?" + qs.ToString()) : "#")
             .Append("' class='p-3 border ").Append(hasPrev ? "border-gray-200 text-text-dark hover:bg-primary hover:text-white" : "border-gray-100 text-gray-200 cursor-not-allowed")
             .Append(" transition-all'><i class='fa-solid fa-chevron-left text-[10px]'></i></a>");

            int start = Math.Max(1, page - 2);
            int end = Math.Min(totalPages, page + 2);
            for (int i = start; i <= end; i++)
            {
                qs["page"] = i.ToString();
                bool active = (i == page);
                p.Append("<a href='").Append(path).Append("?").Append(qs.ToString())
                 .Append("' class='w-10 h-10 flex items-center justify-center text-[10px] font-bold tracking-widest transition-all ")
                 .Append(active ? "bg-primary text-white" : "bg-white text-gray-400 hover:text-primary")
                 .Append("'>").Append(i).Append("</a>");
            }

            bool hasNext = page < totalPages;
            if (hasNext) qs["page"] = (page + 1).ToString();
            p.Append("<a href='").Append(hasNext ? (path + "?" + qs.ToString()) : "#")
             .Append("' class='p-3 border ").Append(hasNext ? "border-gray-200 text-text-dark hover:bg-primary hover:text-white" : "border-gray-100 text-gray-200 cursor-not-allowed")
             .Append(" transition-all'><i class='fa-solid fa-chevron-right text-[10px]'></i></a>");

            p.Append("</div>");
            return p.ToString();
        }

        private void ToggleHistory(bool alwaysShowHistory = false)
        {
            bool showHistory = alwaysShowHistory;

            if (PhHistory != null)
                PhHistory.Attributes["class"] = showHistory
                    ? (PhHistory.Attributes["class"] ?? "").Replace("hidden", "").Trim()
                    : AddClass(PhHistory, "hidden");

            if (PhGuest != null)
                PhGuest.Attributes["class"] = showHistory
                    ? AddClass(PhGuest, "hidden")
                    : (PhGuest.Attributes["class"] ?? "").Replace("hidden", "").Trim();
        }

        // Helpers
        private void Show(string text, bool ok)
        {
            var a = Alert;
            if (a == null) return;

            a.InnerText = text ?? "";
            string cls = "mb-12 p-6 text-[10px] uppercase tracking-widest font-bold border-l-4 ";
            cls += ok ? "bg-green-50 border-green-500 text-green-700" : "bg-red-50 border-red-500 text-red-700";
            
            a.Attributes["class"] = cls.Trim();
        }

        private static string AddClass(HtmlGenericControl c, string klass)
        {
            if (c == null) return "";
            string cls = c.Attributes["class"] ?? "";
            if (cls.IndexOf(klass, StringComparison.OrdinalIgnoreCase) < 0)
                cls = (cls + " " + klass).Trim();
            return cls;
        }

        private static string SafeStr(object o) { return o == null ? "" : Convert.ToString(o); }
        private static bool SafeBool(object o) { try { return Convert.ToBoolean(o); } catch { return false; } }
        private static string SafeDate(object o) { try { return Convert.ToDateTime(o).ToString("dd MMM yyyy"); } catch { return ""; } }
        private static string Html(string s) { return System.Web.HttpUtility.HtmlEncode(s ?? ""); }
        private static string Nl2Br(string s) { if (string.IsNullOrEmpty(s)) return ""; return s.Replace("\r\n", "<br/>").Replace("\n", "<br/>"); }
    }
}
