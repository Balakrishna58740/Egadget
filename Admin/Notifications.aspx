<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master" AutoEventWireup="true" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.Web" %>
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
    if (!IsPostBack)
    {
        int readId;
        if (int.TryParse(Request.QueryString["read"], out readId) && readId > 0)
        {
            try
            {
                Db.Execute("UPDATE dbo.notifications SET is_read=1, read_at=GETDATE() WHERE id=@id AND is_admin=1", Db.P("@id", readId));
            }
            catch { }
        }

        BindRows();
    }
}

protected void btnMarkAllRead_Click(object sender, EventArgs e)
{
    var litMsg = FindControl("litMsg") as Literal;

    try
    {
        Db.Execute("UPDATE dbo.notifications SET is_read=1, read_at=GETDATE() WHERE is_admin=1 AND is_read=0");
        if (litMsg != null) litMsg.Text = "<div class='mb-4 text-xs text-green-700 bg-green-50 border border-green-200 rounded p-3'>All admin notifications marked as read.</div>";
    }
    catch
    {
        if (litMsg != null) litMsg.Text = "<div class='mb-4 text-xs text-red-700 bg-red-50 border border-red-200 rounded p-3'>Unable to mark notifications as read.</div>";
    }

    BindRows();
}

private void BindRows()
{
    var litRows = FindControl("litRows") as Literal;
    if (litRows == null) return;

    DataTable dt = Db.Query(@"SELECT TOP 100 id, title, body, is_read, created_at
FROM dbo.notifications
WHERE is_admin=1
ORDER BY created_at DESC, id DESC;") ?? new DataTable();

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
            sb.Append("</td></tr>");
        }
    }

    litRows.Text = sb.ToString();
}
</script>
