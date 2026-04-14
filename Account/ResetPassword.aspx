<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Security.Cryptography" %>
<%@ Import Namespace="System.Web" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Reset Password</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-16 min-h-[60vh] flex items-center justify-center">
        <div class="w-full max-w-md">
            <div class="bg-white border border-gray-100 shadow-2xl p-8 md:p-12">
                <div class="text-center mb-8">
                    <h2 class="font-serif text-3xl mb-2">Reset Password</h2>
                    <p class="text-gray-400 text-sm uppercase tracking-widest">Create a new password</p>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-sm block mb-6 text-center" />

                <asp:Panel ID="pnlForm" runat="server" CssClass="space-y-6">
                    <div>
                        <label for="<%= txtPassword.ClientID %>" class="block text-xs uppercase tracking-widest font-bold mb-2">New Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary bg-off-white transition-colors" />
                    </div>

                    <div>
                        <label for="<%= txtConfirm.ClientID %>" class="block text-xs uppercase tracking-widest font-bold mb-2">Confirm Password</label>
                        <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary bg-off-white transition-colors" />
                    </div>

                    <div class="pt-2">
                        <asp:Button ID="btnReset" runat="server" Text="Update Password"
                            data-loading-text="Updating Password..."
                            CssClass="w-full bg-primary text-white py-4 text-sm uppercase tracking-widest font-bold hover:bg-primary/90 transition-all cursor-pointer"
                            OnClick="btnReset_Click" />
                    </div>
                </asp:Panel>

                <div class="text-center pt-4 border-t border-gray-50 mt-6">
                    <a runat="server" href="~/Account/Login.aspx" class="text-text-dark text-sm font-bold hover:text-primary transition-colors">Back to login</a>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string token = (Request.QueryString["token"] ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                Show("Invalid reset link.", false);
                pnlForm.Visible = false;
                return;
            }

            try
            {
                EnsureResetColumns();
                if (!TokenIsUsable(token))
                {
                    Show("This reset link is invalid or expired.", false);
                    pnlForm.Visible = false;
                }
            }
            catch
            {
                Show("Unable to validate reset link.", false);
                pnlForm.Visible = false;
            }
        }
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        string token = (Request.QueryString["token"] ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            Show("Invalid reset link.", false);
            pnlForm.Visible = false;
            return;
        }

        string pass = txtPassword.Text ?? string.Empty;
        string confirm = txtConfirm.Text ?? string.Empty;

        if (pass.Length < 6)
        {
            Show("Password must be at least 6 characters.", false);
            return;
        }
        if (!string.Equals(pass, confirm, StringComparison.Ordinal))
        {
            Show("Passwords do not match.", false);
            return;
        }

        try
        {
            EnsureResetColumns();

            DataTable dt = Db.Query("SELECT TOP 1 id, reset_expires FROM dbo.members WHERE reset_token=@t", Db.P("@t", token));
            if (dt == null || dt.Rows.Count == 0)
            {
                Show("This reset link is invalid or expired.", false);
                pnlForm.Visible = false;
                return;
            }

            DateTime expires;
            if (!DateTime.TryParse(Convert.ToString(dt.Rows[0]["reset_expires"]), out expires) || expires <= DateTime.UtcNow)
            {
                Show("This reset link is invalid or expired.", false);
                pnlForm.Visible = false;
                return;
            }

            int memberId = Convert.ToInt32(dt.Rows[0]["id"]);
            string storedPassword = CreatePasswordToken(pass);

            Db.Execute(@"UPDATE dbo.members
SET [password] = @p,
    reset_token = NULL,
    reset_expires = NULL,
    persistent_token = NULL,
    token_expires = NULL,
    updated_at = GETDATE()
WHERE id = @id",
                Db.P("@p", storedPassword),
                Db.P("@id", memberId));

            Show("Password updated successfully. You can now sign in.", true);
            pnlForm.Visible = false;
        }
        catch
        {
            Show("Unable to reset password right now. Please try again.", false);
        }
    }

    private bool TokenIsUsable(string token)
    {
        DataTable dt = Db.Query("SELECT TOP 1 reset_expires FROM dbo.members WHERE reset_token=@t", Db.P("@t", token));
        if (dt == null || dt.Rows.Count == 0) return false;

        DateTime expires;
        if (!DateTime.TryParse(Convert.ToString(dt.Rows[0]["reset_expires"]), out expires)) return false;
        return expires > DateTime.UtcNow;
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

    private string CreatePasswordToken(string password)
    {
        var salt = new byte[16];
        using (var rng = new RNGCryptoServiceProvider()) { rng.GetBytes(salt); }
        const int iter = 10000;
        byte[] hash = Pbkdf2(password, salt, iter, 32);
        return "pbkdf2$" + iter + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
    }

    private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int length)
    {
        using (var pb = new Rfc2898DeriveBytes(password, salt))
        {
            try { pb.IterationCount = iterations; } catch { }
            return pb.GetBytes(length);
        }
    }
</script>
