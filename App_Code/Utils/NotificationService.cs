using System;

public static class NotificationService
{
    public static void NotifyAdminOrderPlaced(int orderId, string orderCode, decimal totalAmount, string payment)
    {
        SafeEnsureTable();
        SafeInsert(null, true, orderId,
            "New order placed",
            string.Format("Order {0} placed. Amount: RS {1:N2}, Payment: {2}", orderCode ?? "-", totalAmount, payment ?? "-"));
    }

    public static void NotifyMemberOrderPlaced(int memberId, int orderId, string orderCode, decimal totalAmount)
    {
        if (memberId <= 0) return;
        SafeEnsureTable();
        SafeInsert(memberId, false, orderId,
            "Order confirmed",
            string.Format("Your order {0} has been confirmed. Total: RS {1:N2}.", orderCode ?? "-", totalAmount));
    }

    public static void NotifyMemberOrderStatus(int memberId, int orderId, string orderCode, string status)
    {
        if (memberId <= 0) return;
        SafeEnsureTable();
        SafeInsert(memberId, false, orderId,
            "Order status updated",
            string.Format("Your order {0} is now {1}.", orderCode ?? "-", ToDisplayStatus(status)));
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

    private static void SafeInsert(int? memberId, bool isAdmin, int? orderId, string title, string body)
    {
        try
        {
            Db.Execute(@"INSERT INTO dbo.notifications(recipient_member_id, is_admin, order_id, title, body, is_read, created_at)
                         VALUES (@mid, @adm, @oid, @t, @b, 0, GETDATE())",
                Db.P("@mid", memberId.HasValue ? (object)memberId.Value : DBNull.Value),
                Db.P("@adm", isAdmin),
                Db.P("@oid", orderId.HasValue ? (object)orderId.Value : DBNull.Value),
                Db.P("@t", title ?? "Notification"),
                Db.P("@b", body ?? string.Empty));
        }
        catch { }
    }

    private static void SafeEnsureTable()
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

IF COL_LENGTH('dbo.notifications', 'recipient_member_id') IS NULL
  ALTER TABLE dbo.notifications ADD recipient_member_id INT NULL;
IF COL_LENGTH('dbo.notifications', 'is_admin') IS NULL
  ALTER TABLE dbo.notifications ADD is_admin BIT NOT NULL CONSTRAINT DF_notifications_is_admin DEFAULT(0);
IF COL_LENGTH('dbo.notifications', 'order_id') IS NULL
  ALTER TABLE dbo.notifications ADD order_id INT NULL;
IF COL_LENGTH('dbo.notifications', 'title') IS NULL
  ALTER TABLE dbo.notifications ADD title VARCHAR(200) NOT NULL CONSTRAINT DF_notifications_title DEFAULT('Notification');
IF COL_LENGTH('dbo.notifications', 'body') IS NULL
  ALTER TABLE dbo.notifications ADD body VARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.notifications', 'is_read') IS NULL
  ALTER TABLE dbo.notifications ADD is_read BIT NOT NULL CONSTRAINT DF_notifications_is_read DEFAULT(0);
IF COL_LENGTH('dbo.notifications', 'created_at') IS NULL
  ALTER TABLE dbo.notifications ADD created_at DATETIME2(0) NOT NULL CONSTRAINT DF_notifications_created_at DEFAULT(GETDATE());
IF COL_LENGTH('dbo.notifications', 'read_at') IS NULL
  ALTER TABLE dbo.notifications ADD read_at DATETIME2(0) NULL;
");
        }
        catch { }
    }
}
