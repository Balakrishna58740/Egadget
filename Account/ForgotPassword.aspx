<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Web" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Forgot Password</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-16 min-h-[60vh] flex items-center justify-center">
        <div class="w-full max-w-md">
            <div class="bg-white border border-gray-100 shadow-2xl p-8 md:p-12">
                <div class="text-center mb-8">
                    <h2 class="font-serif text-3xl mb-2">Forgot Password</h2>
                    <p class="text-gray-400 text-sm uppercase tracking-widest">Receive reset link by email</p>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-sm block mb-6 text-center" />

                <div class="space-y-6">
                    <div>
                        <label for="<%= txtEmail.ClientID %>" class="block text-xs uppercase tracking-widest font-bold mb-2">Email Address</label>
                        <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary bg-off-white transition-colors" placeholder="you@example.com" />
                    </div>

                    <div class="pt-2">
                        <asp:Button ID="btnSend" runat="server" Text="Send Reset Link"
                            data-loading-text="Sending Link..."
                            CssClass="w-full bg-primary text-white py-4 text-sm uppercase tracking-widest font-bold hover:bg-primary/90 transition-all cursor-pointer"
                            OnClick="btnSend_Click" />
                    </div>

                    <div class="text-center pt-4 border-t border-gray-50 mt-6">
                        <a runat="server" href="~/Account/Login.aspx" class="text-text-dark text-sm font-bold hover:text-primary transition-colors">Back to login</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<script runat="server">
    protected void btnSend_Click(object sender, EventArgs e)
    {
        string email = (txtEmail.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email) || email.IndexOf("@", StringComparison.Ordinal) < 1)
        {
            Show("Please enter a valid email address.", false);
            return;
        }

        try
        {
            EnsureResetColumns();

            DataTable dt = Db.Query("SELECT TOP 1 id, full_name, email FROM dbo.members WHERE email=@e", Db.P("@e", email));
            if (dt != null && dt.Rows.Count > 0)
            {
                string token = Guid.NewGuid().ToString("N");
                DateTime expiresUtc = DateTime.UtcNow.AddMinutes(30);

                int memberId = Convert.ToInt32(dt.Rows[0]["id"]);
                string fullName = Convert.ToString(dt.Rows[0]["full_name"]);
                string memberEmail = Convert.ToString(dt.Rows[0]["email"]);

                Db.Execute(@"UPDATE dbo.members
SET reset_token = @t,
    reset_expires = @x,
    updated_at = GETDATE()
WHERE id = @id",
                    Db.P("@t", token),
                    Db.P("@x", expiresUtc),
                    Db.P("@id", memberId));

                string relative = "~/Account/ResetPassword.aspx?token=" + HttpUtility.UrlEncode(token);
                string absolute = BuildAbsoluteUrl(relative);
                try { global::EmailService.SendPasswordResetEmail(memberEmail, fullName, absolute); } catch { }
            }

            Show("If this email exists, a password reset link has been sent.", true);
        }
        catch
        {
            Show("Unable to process request right now. Please try again.", false);
        }
    }

    private string BuildAbsoluteUrl(string appRelative)
    {
        string absPath = VirtualPathUtility.ToAbsolute(appRelative);
        if (Request == null || Request.Url == null) return absPath;
        return Request.Url.GetLeftPart(UriPartial.Authority) + absPath;
    }

    private void EnsureResetColumns()
    {
        Db.Execute(@"
IF COL_LENGTH('dbo.members', 'reset_token') IS NULL
    ALTER TABLE dbo.members ADD reset_token VARCHAR(64) NULL;
IF COL_LENGTH('dbo.members', 'reset_expires') IS NULL
    ALTER TABLE dbo.members ADD reset_expires DATETIME2(0) NULL;
");
    }

    private void Show(string message, bool success)
    {
        lblMessage.CssClass = (success ? "text-green-600" : "text-red-500") + " text-sm block mb-6 text-center";
        lblMessage.Text = Server.HtmlEncode(message ?? string.Empty);
    }
</script>
