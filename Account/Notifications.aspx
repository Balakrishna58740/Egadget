<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master" AutoEventWireup="true" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">My Notifications</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
  <div class="container mx-auto px-4 py-12 max-w-5xl">
    <div class="bg-white border border-gray-100 p-8 mb-6 eg-card rounded-2xl">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="font-serif text-3xl">My Notifications</h1>
          <p class="text-gray-400 text-xs uppercase tracking-widest">Order updates and confirmations</p>
        </div>
        <asp:LinkButton ID="btnMarkAllRead" runat="server" OnClick="btnMarkAllRead_Click" CssClass="px-4 py-2 text-xs uppercase tracking-widest font-bold border border-gray-200 hover:bg-off-white">Mark all read</asp:LinkButton>
      </div>
    </div>

    <asp:Literal ID="litMsg" runat="server" />

    <div class="bg-white border border-gray-100 overflow-hidden eg-card rounded-2xl">
      <table class="w-full text-left">
        <thead>
          <tr class="text-[10px] uppercase tracking-widest text-gray-400 border-b border-gray-100 bg-off-white">
            <th class="px-6 py-4">When</th>
            <th class="px-6 py-4">Title</th>
            <th class="px-6 py-4">Message</th>
            <th class="px-6 py-4 text-right">Action</th>
          </tr>
        </thead>
        <tbody>
          <asp:Literal ID="litRows" runat="server" />
        </tbody>
      </table>
    </div>
  </div>
</asp:Content>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        int memberId = GetMemberId();
        if (memberId <= 0)
        {
            string ru = HttpUtility.UrlEncode(Request.RawUrl);
            Response.Redirect("~/Account/Login.aspx?returnUrl=" + ru, true);
            return;
        }

        if (!IsPostBack)
        {
            int readId;
            if (int.TryParse(Request.QueryString["read"], out readId) && readId > 0)
            {
                try
                {
                    Db.Execute("UPDATE dbo.notifications SET is_read=1, read_at=GETDATE() WHERE id=@id AND recipient_member_id=@mid",
                        Db.P("@id", readId), Db.P("@mid", memberId));
                }
                catch { }
            }
            BindRows(memberId);
        }
    }

    protected void btnMarkAllRead_Click(object sender, EventArgs e)
    {
        int memberId = GetMemberId();
        if (memberId <= 0) return;

        try
        {
            Db.Execute("UPDATE dbo.notifications SET is_read=1, read_at=GETDATE() WHERE recipient_member_id=@mid AND is_read=0", Db.P("@mid", memberId));
            litMsg.Text = "<div class='mb-4 p-3 border border-green-200 bg-green-50 text-green-700 text-xs uppercase tracking-widest'>All notifications marked as read.</div>";
        }
        catch
        {
            litMsg.Text = "<div class='mb-4 p-3 border border-red-200 bg-red-50 text-red-700 text-xs uppercase tracking-widest'>Unable to mark notifications.</div>";
        }

        BindRows(memberId);
    }

    private void BindRows(int memberId)
    {
        var dt = Db.Query(@"SELECT TOP 100 n.id, n.title, n.body, n.is_read, n.created_at, o.order_code
                            FROM dbo.notifications n
                            LEFT JOIN dbo.orders o ON o.id = n.order_id
                            WHERE n.recipient_member_id=@mid
                            ORDER BY n.created_at DESC, n.id DESC", Db.P("@mid", memberId));

        var sb = new System.Text.StringBuilder();
        if (dt == null || dt.Rows.Count == 0)
        {
            sb.Append("<tr><td colspan='4' class='px-6 py-10 text-center text-gray-400 text-sm'>No notifications yet.</td></tr>");
        }
        else
        {
            foreach (System.Data.DataRow r in dt.Rows)
            {
                int id = Convert.ToInt32(r["id"]);
                bool isRead = Convert.ToBoolean(r["is_read"]);
                string orderCode = Convert.ToString(r["order_code"]);
                string when = "-";
                try { when = Convert.ToDateTime(r["created_at"]).ToString("yyyy-MM-dd HH:mm"); } catch { }

                sb.Append("<tr class='border-b border-gray-50 ");
                if (!isRead) sb.Append("bg-blue-50/40");
                sb.Append("'>");
                sb.Append("<td class='px-6 py-4 text-xs text-gray-400'>").Append(HttpUtility.HtmlEncode(when)).Append("</td>");
                sb.Append("<td class='px-6 py-4 text-sm font-bold text-text-dark'>").Append(HttpUtility.HtmlEncode(Convert.ToString(r["title"]))).Append("</td>");
                sb.Append("<td class='px-6 py-4 text-sm text-gray-500'>").Append(HttpUtility.HtmlEncode(Convert.ToString(r["body"]))).Append("</td>");
                sb.Append("<td class='px-6 py-4 text-right'>");
                if (!isRead)
                    sb.Append("<a class='text-xs text-primary font-bold hover:underline mr-3' href='Notifications.aspx?read=").Append(id).Append("'>Mark read</a>");
                else
                    sb.Append("<span class='text-xs text-gray-300 mr-3'>Read</span>");

                if (!string.IsNullOrWhiteSpace(orderCode))
                    sb.Append("<a class='text-xs text-blue-700 font-bold hover:underline' href='Orders/Detail.aspx?code=").Append(HttpUtility.UrlEncode(orderCode)).Append("'>View order</a>");
                sb.Append("</td></tr>");
            }
        }

        litRows.Text = sb.ToString();
    }

    private int GetMemberId()
    {
        try { return Convert.ToInt32(Session["MEMBER_ID"] ?? 0); }
        catch { return 0; }
    }
</script>
