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
        private HtmlInputText FilterTicket { get { return FindInMain<HtmlInputText>("txtFilterTicket"); } }
        private HtmlSelect FilterStatus { get { return FindInMain<HtmlSelect>("ddlStatus"); } }

        // Paging settings
        private const int PAGE_SIZE = 10;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureFeedbackWorkflowSchema();

            if (!IsPostBack)
            {
                ToggleHistory(alwaysShowHistory: true);

                // Autofill when logged in (unconditional on first load)
                PrefillIfMember();
                PrefillFromProduct();

                // Sync filters from querystring
                if (FilterQ != null) FilterQ.Value = (Request["q"] ?? "").Trim();
                if (FilterTicket != null) FilterTicket.Value = (Request["tk"] ?? "").Trim();
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
            int? productId = SafeIntNullable(Request["pid"]);
            string ticketCode = BuildTicketCode();
            try
            {
                if (Session["MEMBER_ID"] != null)
                {
                    int midParsed;
                    if (int.TryParse(Convert.ToString(Session["MEMBER_ID"]), out midParsed))
                        memberId = midParsed;
                }

                bool hasTicketCol = HasFeedbackColumn("ticket_code");
                bool hasStatusCol = HasFeedbackColumn("status");
                bool hasProductCol = HasFeedbackColumn("product_id");
                bool hasIsResolvedCol = HasFeedbackColumn("is_resolved");
                bool hasCreatedCol = HasFeedbackColumn("created_at");
                bool hasUpdatedCol = HasFeedbackColumn("updated_at");

                var cols = new StringBuilder("member_id, title, name, email, message");
                var vals = new StringBuilder("@mid, @t, @n, @e, @m");
                var pars = new System.Collections.Generic.List<SqlParameter>
                {
                    global::Db.P("@mid", (object)memberId ?? DBNull.Value),
                    global::Db.P("@t", title),
                    global::Db.P("@n", name),
                    global::Db.P("@e", email),
                    global::Db.P("@m", msg)
                };

                if (hasProductCol)
                {
                    cols.Append(", product_id");
                    vals.Append(", @pid");
                    pars.Add(global::Db.P("@pid", (object)productId ?? DBNull.Value));
                }
                if (hasTicketCol)
                {
                    cols.Append(", ticket_code");
                    vals.Append(", @ticket");
                    pars.Add(global::Db.P("@ticket", ticketCode));
                }
                if (hasStatusCol)
                {
                    cols.Append(", status");
                    vals.Append(", 'open'");
                }
                if (hasIsResolvedCol)
                {
                    cols.Append(", is_resolved");
                    vals.Append(", 0");
                }
                if (hasCreatedCol)
                {
                    cols.Append(", created_at");
                    vals.Append(", GETDATE()");
                }
                if (hasUpdatedCol)
                {
                    cols.Append(", updated_at");
                    vals.Append(", GETDATE()");
                }

                global::Db.Execute("INSERT INTO dbo.feedbacks (" + cols + ") VALUES (" + vals + ")", pars.ToArray());

                NotifyAdminFeedback(ticketCode, title);

                Show("Thanks! Your message has been sent. Ticket: " + ticketCode, true);
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
            string tk = FilterTicket != null ? (FilterTicket.Value ?? "").Trim() : "";
            string st = FilterStatus != null ? (FilterStatus.Value ?? "").Trim().ToLowerInvariant() : "";

            var qs = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(q)) qs["q"] = q;
            if (!string.IsNullOrEmpty(tk)) qs["tk"] = tk;
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

        private void PrefillFromProduct()
        {
            try
            {
                int pid;
                if (!int.TryParse(Request["pid"], out pid) || pid <= 0) return;

                var dt = global::Db.Query("SELECT TOP 1 name FROM dbo.products WHERE id=@id", global::Db.P("@id", pid));
                if (dt == null || dt.Rows.Count == 0) return;

                string productName = Convert.ToString(dt.Rows[0]["name"] ?? "").Trim();
                if (string.IsNullOrWhiteSpace(productName)) return;

                if (TitleBox != null && string.IsNullOrWhiteSpace(TitleBox.Value))
                    TitleBox.Value = "Product feedback: " + productName;

                if (Alert != null)
                {
                    Alert.InnerText = "You are submitting feedback for product: " + productName;
                    Alert.Attributes["class"] = "mb-12 p-6 text-[10px] uppercase tracking-widest font-bold border-l-4 bg-blue-50 border-blue-500 text-blue-700";
                }
            }
            catch { }
        }

        private void BindHistory()
        {
            if (LitList == null || Pager == null) return;

            string q = (Request["q"] ?? "").Trim();
            string tk = (Request["tk"] ?? "").Trim();
            string st = (Request["st"] ?? "").Trim().ToLowerInvariant();
            bool hasTicketColumn = HasFeedbackColumn("ticket_code");
            bool hasStatusColumn = HasFeedbackColumn("status");

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
                if (!string.IsNullOrEmpty(tk) && hasTicketColumn)
                {
                    where.Append(" AND (ticket_code LIKE @tk)");
                    pars.Add(global::Db.P("@tk", "%" + tk + "%"));
                }
                if (st == "pending")
                    where.Append(hasStatusColumn
                        ? " AND (LOWER(ISNULL(status,'')) IN ('open','inprogress') OR (ISNULL(status,'')='' AND ISNULL(is_resolved,0)=0))"
                        : " AND ISNULL(is_resolved,0)=0");
                else if (st == "replied")
                    where.Append(hasStatusColumn
                        ? " AND (LOWER(ISNULL(status,''))='resolved' OR ISNULL(is_resolved,0)=1)"
                        : " AND ISNULL(is_resolved,0)=1");

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
                        "SELECT id, " + (hasTicketColumn ? "ticket_code" : "NULL AS ticket_code") + ", " + (hasStatusColumn ? "status" : "NULL AS status") + ", title, name, email, message, reply, is_resolved, created_at, updated_at " +
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
                    if (!string.IsNullOrEmpty(tk) && hasTicketColumn)
                    {
                        where2.Append(" AND (ticket_code LIKE @tk)");
                        pars2.Add(global::Db.P("@tk", "%" + tk + "%"));
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
                        "SELECT id, NULL AS ticket_code, CASE WHEN ISNULL(is_resolved,0)=1 THEN 'resolved' ELSE 'open' END AS status, NULL AS title, name, email, message, reply, NULL AS is_resolved, created_at, updated_at " +
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
                    string ticket = SafeStr(r["ticket_code"]);
                    string status = NormalizeFeedbackStatus(SafeStr(r["status"]), SafeBool(r["is_resolved"]), SafeStr(r["reply"]));
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
                    if (!string.IsNullOrWhiteSpace(ticket))
                        sb.Append("<span class='text-[10px] uppercase tracking-widest font-bold text-primary block mb-1'>Ticket ").Append(Html(ticket)).Append("</span>");
                    if (!string.IsNullOrWhiteSpace(name))
                        sb.Append("<span class='text-sm font-serif'>").Append(Html(name)).Append("</span>");
                    sb.Append("</div>");
                    sb.Append("<span class='text-[8px] uppercase tracking-widest font-bold px-3 py-1 border transition-colors ").Append(resolved ? "border-primary text-primary" : "border-gray-200 text-gray-400").Append("'>")
                      .Append(Html(DisplayFeedbackStatus(status))).Append("</span>");
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
            string tk = (Request["tk"] ?? "").Trim();
            string st = (Request["st"] ?? "").Trim().ToLowerInvariant();

            if (!string.IsNullOrEmpty(q)) qs["q"] = q;
            if (!string.IsNullOrEmpty(tk)) qs["tk"] = tk;
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

        private static string BuildTicketCode()
        {
            return "FB-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(10000, 99999);
        }

        private static string NormalizeFeedbackStatus(string status, bool isResolved, string reply)
        {
            string s = (status ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(s))
                s = isResolved || !string.IsNullOrWhiteSpace(reply) ? "resolved" : "open";
            if (s == "complete") s = "resolved";
            return s;
        }

        private static string DisplayFeedbackStatus(string status)
        {
            string s = (status ?? "open").Trim().ToLowerInvariant();
            if (s == "inprogress") return "In Progress";
            if (s == "resolved") return "Resolved";
            return "Open";
        }

        private static void EnsureFeedbackWorkflowSchema()
        {
            try
            {
                global::Db.Execute(@"
IF COL_LENGTH('dbo.feedbacks', 'ticket_code') IS NULL
    ALTER TABLE dbo.feedbacks ADD ticket_code VARCHAR(40) NULL;

IF COL_LENGTH('dbo.feedbacks', 'status') IS NULL
    ALTER TABLE dbo.feedbacks ADD [status] VARCHAR(20) NULL;

IF COL_LENGTH('dbo.feedbacks', 'product_id') IS NULL
    ALTER TABLE dbo.feedbacks ADD product_id INT NULL;

UPDATE dbo.feedbacks
SET [status] = CASE WHEN ISNULL(is_resolved,0)=1 THEN 'resolved' ELSE 'open' END
WHERE [status] IS NULL OR LTRIM(RTRIM([status]))='';
");
            }
            catch { }
        }

        private static void NotifyAdminFeedback(string ticketCode, string title)
        {
            try
            {
                global::Db.Execute(@"INSERT INTO dbo.notifications(recipient_member_id, is_admin, order_id, title, body, is_read, created_at)
VALUES (NULL, 1, NULL, @t, @b, 0, GETDATE())",
                    global::Db.P("@t", "New feedback submitted"),
                    global::Db.P("@b", "Ticket " + (ticketCode ?? "-") + " has been opened. Subject: " + (title ?? "-")));
            }
            catch { }
        }

        private static bool HasFeedbackColumn(string columnName)
        {
            try
            {
                return global::Db.Scalar<int>(@"SELECT COUNT(*)
FROM sys.columns c
INNER JOIN sys.tables t ON t.object_id = c.object_id
WHERE t.name='feedbacks' AND c.name=@c", global::Db.P("@c", columnName)) > 0;
            }
            catch { return false; }
        }

        private static int? SafeIntNullable(string s)
        {
            int x;
            return int.TryParse(s, out x) && x > 0 ? (int?)x : null;
        }
    }
}
