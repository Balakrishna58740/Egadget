<%@ Application Language="C#" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="System.Web.SessionState" %>

<script runat="server">
    void RegisterRoutes(RouteCollection routes)
    {
        // Pretty route: /product/{slug} → Product.aspx
        routes.MapPageRoute("ProductBySlug", "product/{slug}", "~/Product.aspx");
    }

    void Application_Start(object sender, EventArgs e)
    {
        // Ensure App_Data exists (for logs)
        string appData = Server.MapPath("~/App_Data");
        if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);

        RegisterRoutes(RouteTable.Routes);
        EnsureMemberAddressesTable();
        EnsureFeedbacksTable();
        EnsurePaymentMethodsTable();
        EnsureNotificationsTable();
    }

    void EnsureMemberAddressesTable()
    {
        try
        {
            Db.Execute(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='member_addresses' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.member_addresses(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    member_id INT NOT NULL,
    [address] VARCHAR(MAX) NULL,
    township VARCHAR(255) NULL,
    postal_code VARCHAR(20) NULL,
    city VARCHAR(100) NULL,
    [state] VARCHAR(100) NULL,
    country VARCHAR(100) NULL,
    is_default BIT NOT NULL DEFAULT(0),
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;");
        }
        catch { /* best-effort schema bootstrap */ }
    }

    void EnsureNotificationsTable()
    {
        try
        {
            Db.Execute(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='notifications' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.notifications(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    recipient_member_id INT NULL,
    is_admin BIT NOT NULL DEFAULT(0),
    order_id INT NULL,
    title VARCHAR(200) NOT NULL,
    body VARCHAR(MAX) NULL,
    is_read BIT NOT NULL DEFAULT(0),
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    read_at DATETIME2(0) NULL
  );
END;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_notifications_members')
  ALTER TABLE dbo.notifications WITH CHECK ADD CONSTRAINT FK_notifications_members FOREIGN KEY(recipient_member_id) REFERENCES dbo.members(id);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_notifications_orders')
  ALTER TABLE dbo.notifications WITH CHECK ADD CONSTRAINT FK_notifications_orders FOREIGN KEY(order_id) REFERENCES dbo.orders(id);
");
        }
        catch { /* best-effort schema bootstrap */ }
    }

    void EnsurePaymentMethodsTable()
    {
        try
        {
            Db.Execute(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='payment_methods' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.payment_methods(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] VARCHAR(100) NOT NULL UNIQUE,
    is_use BIT NOT NULL DEFAULT(1),
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.payment_methods WHERE [name] = 'Cash On Delivery')
  INSERT INTO dbo.payment_methods ([name], is_use) VALUES ('Cash On Delivery', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.payment_methods WHERE [name] = 'Card')
  INSERT INTO dbo.payment_methods ([name], is_use) VALUES ('Card', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.payment_methods WHERE [name] = 'Bank')
  INSERT INTO dbo.payment_methods ([name], is_use) VALUES ('Bank', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.payment_methods WHERE [name] = 'eSewa')
  INSERT INTO dbo.payment_methods ([name], is_use) VALUES ('eSewa', 1);
");
        }
        catch { /* best-effort schema bootstrap */ }
    }

    void EnsureFeedbacksTable()
    {
        try
        {
            Db.Execute(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='feedbacks' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.feedbacks(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    member_id INT NULL,
    admin_id INT NULL,
    [name] VARCHAR(255) NULL,
    email VARCHAR(255) NULL,
    title VARCHAR(255) NOT NULL,
    [message] VARCHAR(MAX) NOT NULL,
    reply VARCHAR(MAX) NULL,
    is_resolved BIT NOT NULL DEFAULT(0),
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_feedbacks_admins')
  ALTER TABLE dbo.feedbacks WITH CHECK ADD CONSTRAINT FK_feedbacks_admins FOREIGN KEY(admin_id) REFERENCES dbo.admins(id);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_feedbacks_members')
  ALTER TABLE dbo.feedbacks WITH CHECK ADD CONSTRAINT FK_feedbacks_members FOREIGN KEY(member_id) REFERENCES dbo.members(id);
");
        }
        catch { /* best-effort schema bootstrap */ }
    }

    // Lightweight "remember me" rehydrate (Option A). Safe: only runs when Session is available.
    void Application_AcquireRequestState(object sender, EventArgs e)
    {
        if (Context == null) return;

        var h = Context.Handler;
        bool needsSession = (h is IRequiresSessionState) || (h is IReadOnlySessionState);
        if (!needsSession) return;

        if (Session != null && Session["MEMBER_ID"] == null)
        {
            HttpCookie mt = (Request != null) ? Request.Cookies["MemberToken"] : null;
            if (mt == null || string.IsNullOrEmpty(mt.Value)) return;

            try
            {
                DataTable dt = Db.Query(@"
                    SELECT TOP 1 id
                    FROM dbo.members
                    WHERE persistent_token = @t AND token_expires > GETUTCDATE()",
                    Db.P("@t", mt.Value));

                if (dt != null && dt.Rows.Count > 0)
                {
                    int memberId;
                    if (int.TryParse(dt.Rows[0]["id"].ToString(), out memberId) && memberId > 0)
                        Session["MEMBER_ID"] = memberId;
                }
            }
            catch { /* best-effort only */ }
        }
    }

    void Application_End(object sender, EventArgs e) { }

    // Hardened error handler: avoids recursion and always produces a response
    void Application_Error(object sender, EventArgs e)
    {
        Exception ex = Server.GetLastError();
        Exception baseEx = ex != null ? ex.GetBaseException() : null;
        HttpException httpEx = ex as HttpException;
        int statusCode = (httpEx != null) ? httpEx.GetHttpCode() : 500;

        // Current path (safely)
        string path = "";
        if (Request != null && Request.AppRelativeCurrentExecutionFilePath != null)
            path = Request.AppRelativeCurrentExecutionFilePath;

        // 1) Skip static files to avoid loops (Error.aspx might include CSS/JS/images)
        string ext = System.IO.Path.GetExtension(path);
        if (!string.IsNullOrEmpty(ext))
        {
            ext = ext.ToLowerInvariant();
            if (ext == ".css" || ext == ".js" || ext == ".png" || ext == ".jpg" || ext == ".jpeg" ||
                ext == ".gif" || ext == ".svg" || ext == ".ico" || ext == ".webp" ||
                ext == ".woff" || ext == ".woff2" || ext == ".ttf" || ext == ".eot" || ext == ".axd")
            {
                return; // let static handler/IIS deal with these
            }
        }

        // 2) Don't try to render the error page while already on it
        if (string.Equals(path, "~/Error.aspx", StringComparison.OrdinalIgnoreCase))
            return;

        // 3) Log (best-effort)
        string errorId = Guid.NewGuid().ToString("N");
        try
        {
            string appData = Server.MapPath("~/App_Data");
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);

            string logFile = Path.Combine(appData, "errors.log");
            string url = (Request != null && Request.Url != null) ? Request.Url.ToString() : "-";

            using (var sw = new StreamWriter(logFile, true))
            {
                sw.WriteLine("=== {0:u} | {1} | {2} ===", DateTime.UtcNow, errorId, statusCode);
                sw.WriteLine("URL: {0}", url);
                sw.WriteLine("MESSAGE: {0}", ex != null ? ex.Message : "-");
                sw.WriteLine("BASE MESSAGE: {0}", baseEx != null ? baseEx.Message : "-");
                sw.WriteLine("STACK: {0}", ex != null ? ex.StackTrace : "-");
                sw.WriteLine("BASE STACK: {0}", baseEx != null ? baseEx.StackTrace : "-");
                sw.WriteLine();
            }
        }
        catch { /* ignore logging failures */ }

        // 4) Redirect to the UI error page; fallback to minimal HTML if redirect fails
        try
        {
            Server.ClearError();
            Response.Redirect("~/Error.aspx?eid=" + errorId, false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch
        {
            try
            {
                Server.ClearError();
                Response.Clear();
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;

                string home = "/";
                try { home = VirtualPathUtility.ToAbsolute("~/"); } catch { home = "/"; }

                Response.Write("<!doctype html><html><head><meta charset='utf-8'><title>Error</title></head><body>");
                Response.Write("<h1>Something went wrong</h1>");
                Response.Write("<p>Error ID: " + Server.HtmlEncode(errorId) + "</p>");
                Response.Write("<p><a href='" + Server.HtmlEncode(home) + "'>Back to home</a></p>");
                Response.Write("</body></html>");
                try { Response.End(); } catch { }
            }
            catch { /* swallow */ }
        }
    }

    void Session_Start(object sender, EventArgs e) { }
    void Session_End(object sender, EventArgs e) { }
</script>
