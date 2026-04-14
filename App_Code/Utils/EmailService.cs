using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web;

public static class EmailService
{
    private const int SmtpFailureAlertThreshold = 3;
    private static readonly TimeSpan SmtpFailureAlertCooldown = TimeSpan.FromMinutes(30);

    public static void SendCustomerOrderConfirmation(string customerEmail, string customerName, string orderCode, decimal totalAmount, string paymentMethod)
    {
        SendPaymentConfirmationEmail(customerEmail, customerName, orderCode, totalAmount, paymentMethod);
    }

    public static void SendStoreOrderAlert(string orderCode, decimal totalAmount, string paymentMethod)
    {
        SendPaymentReceivedAlert(orderCode, totalAmount, paymentMethod);
    }

    public static void SendPaymentConfirmationEmail(string customerEmail, string customerName, string orderCode, decimal totalAmount, string paymentMethod)
    {
        if (string.IsNullOrWhiteSpace(customerEmail)) return;

        string safeName = string.IsNullOrWhiteSpace(customerName) ? "Customer" : customerName.Trim();
        string subject = "Payment confirmed - " + (orderCode ?? "Order");
        string body = "Hi " + HttpUtility.HtmlEncode(safeName) + ",<br/><br/>"
                    + "We received your payment for order <b>" + HttpUtility.HtmlEncode(orderCode ?? "-") + "</b>.<br/>"
                    + "Amount: <b>RS " + totalAmount.ToString("N2") + "</b><br/>"
                    + "Payment method: <b>" + HttpUtility.HtmlEncode(paymentMethod ?? "-") + "</b><br/><br/>"
                    + "Thank you for shopping with eGadgetHub.";

        SafeSend(customerEmail, subject, body);
    }

    public static void SendPasswordResetEmail(string customerEmail, string customerName, string resetUrl)
    {
        if (string.IsNullOrWhiteSpace(customerEmail) || string.IsNullOrWhiteSpace(resetUrl)) return;

        string safeName = string.IsNullOrWhiteSpace(customerName) ? "Customer" : customerName.Trim();
        string subject = "Password reset request";
        string body = "Hi " + HttpUtility.HtmlEncode(safeName) + ",<br/><br/>"
                    + "We received a request to reset your password.<br/>"
                    + "Use the link below to set a new password (valid for 30 minutes):<br/><br/>"
                    + "<a href='" + HttpUtility.HtmlAttributeEncode(resetUrl) + "'>Reset your password</a><br/><br/>"
                    + "If you did not request this, you can ignore this email.";

        SafeSend(customerEmail, subject, body);
    }

    public static void SendPaymentReceivedAlert(string orderCode, decimal totalAmount, string paymentMethod)
    {
        string to = FirstNonEmpty(
            ConfigurationManager.AppSettings["StoreNotificationEmail"],
            ConfigurationManager.AppSettings["SupportEmail"],
            ConfigurationManager.AppSettings["SmtpUser"]);

        if (string.IsNullOrWhiteSpace(to)) return;

        string subject = "Payment received - " + (orderCode ?? "Order");
        string body = "A payment has been received.<br/><br/>"
                    + "Order: <b>" + HttpUtility.HtmlEncode(orderCode ?? "-") + "</b><br/>"
                    + "Amount: <b>RS " + totalAmount.ToString("N2") + "</b><br/>"
                    + "Payment method: <b>" + HttpUtility.HtmlEncode(paymentMethod ?? "-") + "</b>";

        SafeSend(to, subject, body);
    }

    public static void SendCustomerStatusUpdate(string customerEmail, string customerName, string orderCode, string status)
    {
        if (string.IsNullOrWhiteSpace(customerEmail)) return;

        string safeName = string.IsNullOrWhiteSpace(customerName) ? "Customer" : customerName.Trim();
        string displayStatus = ToDisplayStatus(status);
        string subject = "Order update - " + (orderCode ?? "Order");
        string body = "Hi " + HttpUtility.HtmlEncode(safeName) + ",<br/><br/>"
                    + "Your order <b>" + HttpUtility.HtmlEncode(orderCode ?? "-") + "</b> status is now <b>"
                    + HttpUtility.HtmlEncode(displayStatus) + "</b>.<br/><br/>"
                    + "Thank you for shopping with eGadgetHub.";

        SafeSend(customerEmail, subject, body);
    }

    private static void SafeSend(string to, string subject, string htmlBody)
    {
        try
        {
            string host = ConfigurationManager.AppSettings["SmtpHost"];
            int port = ToInt(ConfigurationManager.AppSettings["SmtpPort"], 587);
            bool enableSsl = ToBool(ConfigurationManager.AppSettings["SmtpEnableSsl"], true);
            string user = ConfigurationManager.AppSettings["SmtpUser"];
            string pass = ConfigurationManager.AppSettings["SmtpPassword"];

            string fromAddress = FirstNonEmpty(
                ConfigurationManager.AppSettings["EmailFromAddress"],
                ConfigurationManager.AppSettings["SmtpEmail"],
                user);

            string fromName = FirstNonEmpty(ConfigurationManager.AppSettings["SmtpFromName"], "eGadgetHub");

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromAddress))
            {
                WriteMailLog(to, subject, htmlBody, "SMTP not configured");
                RecordSmtpFailure("SMTP not configured");
                return;
            }

            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress(fromAddress, fromName);
                msg.To.Add(to);
                msg.Subject = subject ?? string.Empty;
                msg.Body = htmlBody ?? string.Empty;
                msg.IsBodyHtml = true;

                using (var smtp = new SmtpClient(host, port))
                {
                    smtp.EnableSsl = enableSsl;
                    if (!string.IsNullOrWhiteSpace(user))
                        smtp.Credentials = new NetworkCredential(user, pass ?? string.Empty);
                    smtp.Send(msg);
                }
            }

            ResetSmtpFailureState();
        }
        catch (Exception ex)
        {
            WriteMailLog(to, subject, htmlBody, ex.Message);
            RecordSmtpFailure(ex.Message);
        }
    }

    private static void WriteMailLog(string to, string subject, string body, string reason)
    {
        try
        {
            string path = GetAppDataPath("mail.log");

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.AppendAllText(path,
                "[" + DateTime.Now.ToString("s") + "] " + Environment.NewLine
                + "To: " + (to ?? "-") + Environment.NewLine
                + "Subject: " + (subject ?? "-") + Environment.NewLine
                + "Reason: " + (reason ?? "-") + Environment.NewLine
                + "Body: " + (body ?? "-") + Environment.NewLine
                + "---" + Environment.NewLine);
        }
        catch { }
    }

    private static void RecordSmtpFailure(string reason)
    {
        try
        {
            string statePath = GetAppDataPath("smtp_failure_state.txt");

            int failCount = 0;
            DateTime firstFailUtc = DateTime.UtcNow;
            DateTime lastAlertUtc = DateTime.MinValue;

            if (File.Exists(statePath))
            {
                string[] parts = (File.ReadAllText(statePath) ?? string.Empty).Split('|');
                if (parts.Length >= 3)
                {
                    int.TryParse(parts[0], out failCount);
                    DateTime.TryParse(parts[1], out firstFailUtc);
                    DateTime.TryParse(parts[2], out lastAlertUtc);
                }
            }

            failCount++;
            if (firstFailUtc == DateTime.MinValue) firstFailUtc = DateTime.UtcNow;

            bool thresholdReached = failCount >= SmtpFailureAlertThreshold;
            bool cooldownPassed = lastAlertUtc == DateTime.MinValue || DateTime.UtcNow.Subtract(lastAlertUtc) >= SmtpFailureAlertCooldown;

            if (thresholdReached && cooldownPassed)
            {
                string title = "SMTP failures detected";
                string body = string.Format("Email sending failed {0} times since {1:u}. Latest reason: {2}", failCount, firstFailUtc, reason ?? "-");

                try
                {
                    Db.Execute(@"INSERT INTO dbo.notifications(recipient_member_id, is_admin, order_id, title, body, is_read, created_at)
                                 VALUES (NULL, 1, NULL, @t, @b, 0, GETDATE())",
                        Db.P("@t", title),
                        Db.P("@b", body));
                }
                catch { }

                try
                {
                    File.AppendAllText(GetAppDataPath("alerts.log"),
                        "[" + DateTime.Now.ToString("s") + "] SMTP ALERT" + Environment.NewLine
                        + body + Environment.NewLine
                        + "---" + Environment.NewLine);
                }
                catch { }

                lastAlertUtc = DateTime.UtcNow;
                failCount = 0;
                firstFailUtc = DateTime.UtcNow;
            }

            File.WriteAllText(statePath, failCount + "|" + firstFailUtc.ToString("o") + "|" + (lastAlertUtc == DateTime.MinValue ? "" : lastAlertUtc.ToString("o")));
        }
        catch { }
    }

    private static void ResetSmtpFailureState()
    {
        try
        {
            File.WriteAllText(GetAppDataPath("smtp_failure_state.txt"), "0|" + DateTime.UtcNow.ToString("o") + "|");
        }
        catch { }
    }

    private static string GetAppDataPath(string fileName)
    {
        string path = HttpContext.Current != null
            ? HttpContext.Current.Server.MapPath("~/App_Data/" + fileName)
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", fileName);

        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return path;
    }

    private static string FirstNonEmpty(params string[] values)
    {
        if (values == null) return string.Empty;
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
        }
        return string.Empty;
    }

    private static int ToInt(string v, int fallback)
    {
        int n;
        return int.TryParse(v, out n) ? n : fallback;
    }

    private static bool ToBool(string v, bool fallback)
    {
        bool b;
        return bool.TryParse(v, out b) ? b : fallback;
    }

    private static string ToDisplayStatus(string status)
    {
        string s = (status ?? string.Empty).Trim().ToLowerInvariant();
        if (s == "paid") s = "accepted";
        else if (s == "delivering") s = "inprocess";
        else if (s == "completed") s = "delivered";

        if (s == "inprocess") return "In Process";
        if (s == "accepted") return "Accepted";
        if (s == "delivered") return "Delivered";
        if (s == "pending") return "Pending";
        if (s == "canceled") return "Canceled";
        return "Updated";
    }
}
