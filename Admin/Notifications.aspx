<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master" AutoEventWireup="true" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Web.UI" %>
<%@ Import Namespace="System.Web.UI.WebControls" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Notifications</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
  <div class="max-w-6xl mx-auto p-6">
    <div class="admin-surface rounded-2xl p-6 border border-blue-100 mb-6 flex items-center justify-between">
      <div>
        <h2 class="text-2xl font-bold">Admin Notifications</h2>
        <p class="text-sm text-gray-500">Order alerts and system updates.</p>
      </div>
      <asp:LinkButton ID="btnMarkAllRead" runat="server" CssClass="px-4 py-2 rounded-lg bg-blue-600 text-white text-xs font-bold uppercase tracking-widest" OnClick="btnMarkAllRead_Click">Mark all read</asp:LinkButton>
    </div>

    <asp:Literal ID="litMsg" runat="server"></asp:Literal>

    <div class="admin-card rounded-2xl overflow-hidden border border-blue-100">
      <div class="overflow-x-auto">
        <table class="w-full text-left admin-table">
          <thead>
            <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-500">
              <th class="px-6 py-4">When</th>
              <th class="px-6 py-4">Title</th>
              <th class="px-6 py-4">Message</th>
              <th class="px-6 py-4 text-end">Action</th>
            </tr>
          </thead>
          <tbody>
            <asp:Literal ID="litRows" runat="server" />
          </tbody>
        </table>
      </div>
    </div>
  </div>
</asp:Content>

<script runat="server">
protected void Page_Load(object sender, EventArgs e)
{
    var litMsgCtl = GetLiteral("litMsg");
    string updated = Convert.ToString(Request.QueryString["updated"]);
    if (litMsgCtl != null)
    {
        if (string.Equals(updated, "all", StringComparison.OrdinalIgnoreCase))
            litMsgCtl.Text = "<div class='mb-4 text-xs text-green-700 bg-green-50 border border-green-200 rounded p-3'>All admin notifications marked as read.</div>";
        else if (string.Equals(updated, "one", StringComparison.OrdinalIgnoreCase))
            litMsgCtl.Text = "<div class='mb-4 text-xs text-green-700 bg-green-50 border border-green-200 rounded p-3'>Notification marked as read.</div>";
    }

    if (!IsPostBack)
    {
        int readId;
        if (int.TryParse(Request.QueryString["read"], out readId) && readId > 0)
        {
            try
            {
                Db.Execute(@"UPDATE dbo.notifications
SET is_read=1, read_at=GETDATE()
WHERE id=@id
  AND (ISNULL(is_admin,0)=1 OR recipient_member_id IS NULL)", Db.P("@id", readId));
                Response.Redirect("~/Admin/Notifications.aspx?updated=one", true);
                return;
            }
            catch { }
        }

        BindRows();
    }
}

protected void btnMarkAllRead_Click(object sender, EventArgs e)
{
    var litMsgCtl = GetLiteral("litMsg");
    try
    {
        Db.Execute(@"UPDATE dbo.notifications
SET is_read=1, read_at=GETDATE()
WHERE is_read=0
  AND (ISNULL(is_admin,0)=1 OR recipient_member_id IS NULL)");
        Response.Redirect("~/Admin/Notifications.aspx?updated=all", true);
        return;
    }
    catch
    {
        if (litMsgCtl != null) litMsgCtl.Text = "<div class='mb-4 text-xs text-red-700 bg-red-50 border border-red-200 rounded p-3'>Unable to mark notifications as read.</div>";
    }

    BindRows();
}

private void BindRows()
{
    var litRowsCtl = GetLiteral("litRows");
    if (litRowsCtl == null) return;

    DataTable dt = Db.Query(@"SELECT TOP 100
    n.id,
    n.title,
    n.body,
    n.is_read,
    n.created_at,
    n.order_id,
    o.order_code
FROM dbo.notifications n
LEFT JOIN dbo.orders o ON o.id = n.order_id
WHERE (ISNULL(n.is_admin,0)=1 OR n.recipient_member_id IS NULL)
ORDER BY n.created_at DESC, n.id DESC;") ?? new DataTable();

    StringBuilder sb = new StringBuilder();
    if (dt.Rows.Count == 0)
    {
        sb.Append("<tr><td colspan='4' class='px-6 py-10 text-center text-gray-400 text-sm'>No notifications yet.</td></tr>");
    }
    else
    {
        foreach (DataRow r in dt.Rows)
        {
            int id = Convert.ToInt32(r["id"]);
            bool read = Convert.ToBoolean(r["is_read"]);
            int orderId = 0;
            int.TryParse(Convert.ToString(r["order_id"]), out orderId);
            string orderCode = Convert.ToString(r["order_code"] ?? "");
            string when = "-";
            try { when = Convert.ToDateTime(r["created_at"]).ToString("yyyy-MM-dd HH:mm"); } catch { }

            sb.Append("<tr class='border-t border-blue-50 ");
            if (!read) sb.Append("bg-blue-50/40");
            sb.Append("'>");
            sb.Append("<td class='px-6 py-4 text-xs text-gray-500'>").Append(HttpUtility.HtmlEncode(when)).Append("</td>");
            sb.Append("<td class='px-6 py-4 text-sm font-bold text-slate-800'>").Append(HttpUtility.HtmlEncode(Convert.ToString(r["title"]))).Append("</td>");
            sb.Append("<td class='px-6 py-4 text-sm text-slate-600'>").Append(HttpUtility.HtmlEncode(Convert.ToString(r["body"]))).Append("</td>");
            sb.Append("<td class='px-6 py-4 text-end'>");
            if (!read)
                sb.Append("<a class='text-xs text-blue-700 font-bold hover:underline' href='Notifications.aspx?read=").Append(id).Append("'>Mark read</a>");
            else
                sb.Append("<span class='text-xs text-gray-400'>Read</span>");

            if (orderId > 0)
            {
                sb.Append("<span class='text-gray-300 mx-2'>|</span>");
                sb.Append("<a class='text-xs text-emerald-700 font-bold hover:underline' href='OrderView.aspx?id=").Append(orderId).Append("'>Track order");
                if (!string.IsNullOrWhiteSpace(orderCode))
                    sb.Append(" (").Append(HttpUtility.HtmlEncode(orderCode)).Append(")");
                sb.Append("</a>");
            }
            sb.Append("</td></tr>");
        }
    }

    litRowsCtl.Text = sb.ToString();
}

private Literal GetLiteral(string id)
{
    var direct = FindControl(id) as Literal;
    if (direct != null) return direct;

    var main = Master != null ? Master.FindControl("MainContent") : null;
    return FindLiteralRecursive(main, id);
}

private Literal FindLiteralRecursive(Control root, string id)
{
    if (root == null) return null;
    var hit = root.FindControl(id) as Literal;
    if (hit != null) return hit;
    foreach (Control child in root.Controls)
    {
        var nested = FindLiteralRecursive(child, id);
        if (nested != null) return nested;
    }
    return null;
}
</script>
