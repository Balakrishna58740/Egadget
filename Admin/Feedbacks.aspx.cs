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
    public partial class FeedbacksPage : Page
    {
        private const int PAGE_SIZE = 10;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            EnsureFeedbackWorkflowSchema();

            if (!IsPostBack)
            {
                // Load counts for tabs (overall)
                int cntPending = 0;
                int cntComplete = 0;
                try
                {
                    cntPending = Db.Scalar<int>("SELECT COUNT(*) FROM feedbacks WHERE (LOWER(ISNULL(status,'')) IN ('open','inprogress') OR (ISNULL(status,'')='' AND ISNULL(is_resolved,0)=0))");
                    cntComplete = Db.Scalar<int>("SELECT COUNT(*) FROM feedbacks WHERE (LOWER(ISNULL(status,''))='resolved' OR ISNULL(is_resolved,0)=1)");
                }
                catch
                {
                    cntPending = Db.Scalar<int>("SELECT COUNT(*) FROM feedbacks WHERE ISNULL(is_resolved,0)=0");
                    cntComplete = Db.Scalar<int>("SELECT COUNT(*) FROM feedbacks WHERE ISNULL(is_resolved,0)=1");
                }
                SetLit("litCountPending", cntPending.ToString("N0"));
                SetLit("litCountComplete", cntComplete.ToString("N0"));

                // If a reply action is specified, load into form
                int replyId = SafeInt(Request.QueryString["reply"]);
                if (replyId > 0) LoadForReply(replyId);

                BindTable();
            }
        }

        // ---------- Events ----------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            var lbl = Find<Label>("lblMsg");
            var hid = Find<HiddenField>("hidId");
            var txt = Find<TextBox>("txtReply");

            int id = 0;
            if (hid != null && !string.IsNullOrEmpty(hid.Value)) int.TryParse(hid.Value, out id);
            string reply = txt != null ? (txt.Text ?? "").Trim() : "";

            if (id <= 0)
            {
                Show(lbl, "Nothing to save.", false);
                return;
            }
            if (string.IsNullOrWhiteSpace(reply))
            {
                Show(lbl, "Please enter a reply.", false);
                return;
            }

            try
            {
                int adminId = GetCurrentAdminId();
                object aid = adminId > 0 ? (object)adminId : DBNull.Value;

                Db.Execute(
                    @"UPDATE feedbacks 
                      SET reply=@r, status='resolved', is_resolved=1, admin_id=@aid, updated_at=GETDATE() 
                      WHERE id=@id",
                    Db.P("@r", reply), Db.P("@aid", aid), Db.P("@id", id));
            }
            catch
            {
                int adminId = GetCurrentAdminId();
                object aid = adminId > 0 ? (object)adminId : DBNull.Value;

                Db.Execute(
                    @"UPDATE feedbacks 
                      SET reply=@r, is_resolved=1, admin_id=@aid, updated_at=GETDATE() 
                      WHERE id=@id",
                    Db.P("@r", reply), Db.P("@aid", aid), Db.P("@id", id));
            }

            try
            {

                NotifyMemberFeedbackUpdate(id, "resolved");

                Show(lbl, "Reply saved and marked complete.", true);
                Response.Redirect("~/Admin/Feedbacks.aspx");
            }
            catch (Exception ex)
            {
                Show(lbl, "Save failed: " + Server.HtmlEncode(ex.Message), false);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Admin/Feedbacks.aspx");
        }

        // ---------- Data bind ----------
        private void BindTable()
        {
            var lit = Find<Literal>("litRows");
            var litTotal = Find<Literal>("litTotal");
            var pager = Find<Literal>("pager");
            var lbl = Find<Label>("lblMsg");
            if (lit == null) return;

            try
            {
                bool hasStatus = HasColumn("feedbacks", "status");
                bool hasTicket = HasColumn("feedbacks", "ticket_code");

                // Pagination
                int page = SafeInt(Request.QueryString["page"]);
                if (page < 1) page = 1;

                int total = Db.Scalar<int>("SELECT COUNT(*) FROM feedbacks");
                if (litTotal != null) litTotal.Text = "Total: <strong>" + total.ToString("N0") + "</strong>";

                int pageCount = Math.Max(1, (int)Math.Ceiling(total / (double)PAGE_SIZE));
                if (page > pageCount) page = pageCount;
                int offset = (page - 1) * PAGE_SIZE;

                var dataParams = new List<SqlParameter> { Db.P("@offset", offset), Db.P("@limit", PAGE_SIZE) };
                var dt = Db.Query(@"
SELECT id, " + (hasTicket ? "ticket_code" : "NULL AS ticket_code") + ", " + (hasStatus ? "status" : "NULL AS status") + @", member_id, admin_id, name, email, title, message, reply, is_resolved, created_at
FROM feedbacks
ORDER BY created_at DESC
OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;", dataParams.ToArray());

                var sb = new StringBuilder();
                if (dt.Rows.Count == 0)
                {
                    sb.Append("<tr><td colspan='7' class='text-center text-muted py-3'>No feedbacks.</td></tr>");
                }
                else
                {
                    int i = offset;
                    foreach (DataRow r in dt.Rows)
                    {
                        i++;
                        int id = Convert.ToInt32(r["id"]);
                        string name = Html(r["name"]);
                        string email = Html(r["email"]);
                        string ticket = Html(r["ticket_code"]);
                        string status = NormalizeStatus(Convert.ToString(r["status"]), Convert.ToBoolean(r["is_resolved"]), Convert.ToString(r["reply"]));
                        string title = Html(r["title"]);
                        DateTime created = Convert.ToDateTime(r["created_at"]);
                        string createdStr = created.ToString("yyyy-MM-dd HH:mm");

                        sb.Append("<tr>");
                        sb.Append("<td>").Append(i).Append("</td>");
                        sb.Append("<td>").Append(string.IsNullOrEmpty(name) ? "-" : name).Append("</td>");
                        sb.Append("<td>").Append(string.IsNullOrEmpty(email) ? "-" : email).Append("</td>");
                        sb.Append("<td>");
                        if (!string.IsNullOrWhiteSpace(ticket))
                            sb.Append("<div class='text-muted small'>").Append(ticket).Append("</div>");
                        sb.Append(title).Append("</td>");
                        sb.Append("<td><span class='badge ").Append(StatusBadge(status)).Append("'>").Append(DisplayStatus(status)).Append("</span></td>");
                        sb.Append("<td>").Append(createdStr).Append("</td>");
                        sb.Append("<td class='text-end'>");
                        bool isResolved = string.Equals(status, "resolved", StringComparison.OrdinalIgnoreCase);
                        sb.Append("<a class='btn btn-sm btn-outline-primary' href='Feedbacks.aspx?reply=").Append(id).Append("'>")
                          .Append(isResolved ? "View / Edit Reply" : "Reply")
                          .Append("</a>");
                        sb.Append("</td>");
                        sb.Append("</tr>");
                    }
                }
                lit.Text = sb.ToString();

                if (pager != null) pager.Text = BuildPager(page, pageCount);
            }
            catch (Exception ex)
            {
                Show(lbl, "Error: " + Server.HtmlEncode(ex.Message), false);
                if (lit != null) lit.Text = "<tr><td colspan='7' class='text-center text-danger py-3'>Failed to load feedbacks.</td></tr>";
                var p2 = Find<Literal>("pager"); if (p2 != null) p2.Text = "";
            }
        }

        private void LoadForReply(int id)
        {
            using (var con = Db.Open())
            using (var cmd = new SqlCommand(@"SELECT id, name, email, title, message, reply 
                                              FROM feedbacks WHERE id=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (r.Read())
                    {
                        SetHidden("hidId", Convert.ToString(r["id"]));
                        SetLit("litName", Html(r["name"]));
                        SetLit("litEmail", Html(r["email"]));
                        SetLit("litTitle", Html(r["title"]));
                        SetLit("litMessage", Html(r["message"]));
                        var txt = Find<TextBox>("txtReply");
                        if (txt != null) txt.Text = Convert.ToString(r["reply"]);

                        var lbl = Find<Label>("lblMsg");
                        if (lbl != null && !string.IsNullOrWhiteSpace(lbl.Text)) lbl.CssClass = (lbl.CssClass ?? "").Replace("d-none", "").Trim();

                        ClientScript.RegisterStartupScript(GetType(), "fbReplyModalOpen",
                            "(function(){var m=document.getElementById('fbReplyModal'); if(m){m.classList.remove('d-none'); m.setAttribute('aria-hidden','false');}})();", true);
                    }
                }
            }
        }

        private void NotifyMemberFeedbackUpdate(int feedbackId, string status)
        {
            try
            {
                var dt = Db.Query("SELECT TOP 1 member_id, ticket_code, title FROM feedbacks WHERE id=@id", Db.P("@id", feedbackId));
                if (dt == null || dt.Rows.Count == 0) return;

                int memberId = 0;
                int.TryParse(Convert.ToString(dt.Rows[0]["member_id"]), out memberId);
                if (memberId <= 0) return;

                string ticket = Convert.ToString(dt.Rows[0]["ticket_code"] ?? "-");
                string title = Convert.ToString(dt.Rows[0]["title"] ?? "Feedback");
                string displayStatus = DisplayStatus((status ?? "").Trim().ToLowerInvariant());

                Db.Execute(@"INSERT INTO dbo.notifications(recipient_member_id, is_admin, order_id, title, body, is_read, created_at)
VALUES (@mid, 0, NULL, @title, @body, 0, GETDATE())",
                    Db.P("@mid", memberId),
                    Db.P("@title", "Feedback status updated"),
                    Db.P("@body", "Ticket " + ticket + " (" + title + ") is now " + displayStatus + "."));
            }
            catch { }
        }

        private static bool HasColumn(string tableName, string columnName)
        {
            try
            {
                return Db.Scalar<int>(@"SELECT COUNT(*)
FROM sys.columns c
INNER JOIN sys.tables t ON t.object_id = c.object_id
WHERE t.name=@t AND c.name=@c", Db.P("@t", tableName), Db.P("@c", columnName)) > 0;
            }
            catch { return false; }
        }

        private static void EnsureFeedbackWorkflowSchema()
        {
            try
            {
                Db.Execute(@"
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

        private static string NormalizeStatus(string status, bool isResolved, string reply)
        {
            string s = (status ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(s)) s = isResolved || !string.IsNullOrWhiteSpace(reply) ? "resolved" : "open";
            if (s == "complete") s = "resolved";
            return s;
        }

        private static string DisplayStatus(string status)
        {
            if (status == "inprogress") return "In Progress";
            if (status == "resolved") return "Resolved";
            return "Open";
        }

        private static string StatusBadge(string status)
        {
            if (status == "resolved") return "text-bg-success";
            if (status == "inprogress") return "text-bg-warning";
            return "text-bg-secondary";
        }

        // ---------- Small helpers ----------
        private string GetTab()
        {
            string t = Convert.ToString(Request.QueryString["tab"]);
            if (string.Equals(t, "complete", StringComparison.OrdinalIgnoreCase)) return "complete";
            return "pending";
        }

        private int GetCurrentAdminId()
        {
            try
            {
                object o = Session != null ? Session["admin_id"] : null;
                if (o == null) return 0;
                int id;
                if (int.TryParse(Convert.ToString(o), out id)) return id;
                return 0;
            }
            catch { return 0; }
        }

        private static string BuildPager(int page, int pageCount)
        {
            var sb = new StringBuilder();
            sb.Append("<nav aria-label='Feedbacks pagination'><ul class='pagination pagination-sm mb-0'>");

            Func<int, string> url = p => "Feedbacks.aspx?page=" + p;

            Action<bool, string, string> add = (enabled, href, inner) =>
            {
                if (!enabled) sb.Append("<li class='page-item disabled'><span class='page-link'>").Append(inner).Append("</span></li>");
                else sb.Append("<li class='page-item'><a class='page-link' href='").Append(href).Append("'>").Append(inner).Append("</a></li>");
            };

            bool hasPrev = page > 1;
            add(hasPrev, hasPrev ? url(1) : "#", "<i class='fa fa-angle-double-left' aria-hidden='true'></i>");
            add(hasPrev, hasPrev ? url(page - 1) : "#", "<i class='fa fa-angle-left' aria-hidden='true'></i>");

            const int window = 7;
            int start = Math.Max(1, page - (window / 2));
            int end = Math.Min(pageCount, start + window - 1);
            if (end - start + 1 < window) start = Math.Max(1, end - window + 1);

            for (int p = start; p <= end; p++)
            {
                if (p == page)
                    sb.Append("<li class='page-item active' aria-current='page'><span class='page-link'>").Append(p).Append("</span></li>");
                else
                    sb.Append("<li class='page-item'><a class='page-link' href='").Append(url(p)).Append("'>").Append(p).Append("</a></li>");
            }

            bool hasNext = page < pageCount;
            add(hasNext, hasNext ? url(page + 1) : "#", "<i class='fa fa-angle-right' aria-hidden='true'></i>");
            add(hasNext, hasNext ? url(pageCount) : "#", "<i class='fa fa-angle-double-right' aria-hidden='true'></i>");

            sb.Append("</ul></nav>");
            return sb.ToString();
        }

        // ---- generic designer-free helpers ----
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
        private void SetLit(string id, string v) { var l = Find<Literal>(id); if (l != null) l.Text = v ?? ""; }

        private static string Html(object o)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(o) ?? "");
        }

        private static int SafeInt(string s)
        {
            int x; return int.TryParse(s, out x) ? x : 0;
        }

        private void Show(Label lbl, string msg, bool ok)
        {
            if (lbl == null) return;
            lbl.Text = Server.HtmlEncode(msg);
            lbl.CssClass = ok ? "fb-alert alert alert-success" : "fb-alert alert alert-danger";
        }
    }
}
