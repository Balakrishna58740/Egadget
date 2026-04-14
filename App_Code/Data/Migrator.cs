using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace serena
{
    public class Migrator
    {
        public class Migration
        {
            public int Version;
            public string Name;
            public string Sql;
        }

        public static Migration[] All()
        {
            // v1: create all core tables (guarded with IF NOT EXISTS) + indexes/constraints
            var sb = new StringBuilder();

            // migrations bookkeeping
            sb.Append(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'schema_migrations' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.schema_migrations (
    version INT NOT NULL PRIMARY KEY,
    name    VARCHAR(200) NULL,
    applied_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;
");

            // v6: payment transaction tracking table
            var v6 = new StringBuilder();
            v6.Append(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='payment_transactions' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.payment_transactions(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    order_id INT NOT NULL,
    order_code VARCHAR(50) NOT NULL,
    payment_method VARCHAR(100) NOT NULL,
    transaction_ref VARCHAR(120) NULL,
    provider_status VARCHAR(50) NULL,
    amount DECIMAL(10,2) NULL,
    raw_response VARCHAR(MAX) NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_payment_transactions_orders')
  ALTER TABLE dbo.payment_transactions WITH CHECK
    ADD CONSTRAINT FK_payment_transactions_orders FOREIGN KEY(order_id) REFERENCES dbo.orders(id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_payment_transactions_order_id' AND object_id=OBJECT_ID('dbo.payment_transactions'))
  CREATE NONCLUSTERED INDEX IX_payment_transactions_order_id ON dbo.payment_transactions(order_id, id DESC);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_payment_transactions_ref' AND object_id=OBJECT_ID('dbo.payment_transactions'))
  CREATE NONCLUSTERED INDEX IX_payment_transactions_ref ON dbo.payment_transactions(transaction_ref);
");

            // admins
            sb.Append(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='admins' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.admins(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    full_name VARCHAR(255) NULL,
    username VARCHAR(255) NOT NULL,
    [password] VARCHAR(255) NOT NULL,
    role VARCHAR(50) NULL,
    persistent_token VARCHAR(64) NULL,
    token_expires DATETIME2(0) NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UQ_admins_username' AND object_id=OBJECT_ID('dbo.admins'))
  CREATE UNIQUE NONCLUSTERED INDEX UQ_admins_username ON dbo.admins(username);
");

            // members
            sb.Append(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='members' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.members(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    full_name VARCHAR(255) NULL,
    username VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    [password] VARCHAR(255) NOT NULL,
    phone VARCHAR(50) NULL,
    persistent_token VARCHAR(64) NULL,
    token_expires DATETIME2(0) NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UQ_members_username' AND object_id=OBJECT_ID('dbo.members'))
  CREATE UNIQUE NONCLUSTERED INDEX UQ_members_username ON dbo.members(username);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UQ_members_email' AND object_id=OBJECT_ID('dbo.members'))
  CREATE UNIQUE NONCLUSTERED INDEX UQ_members_email ON dbo.members(email);
IF COL_LENGTH('dbo.members', 'persistent_token') IS NULL
  ALTER TABLE dbo.members ADD persistent_token VARCHAR(64) NULL;
IF COL_LENGTH('dbo.members', 'token_expires') IS NULL
  ALTER TABLE dbo.members ADD token_expires DATETIME2(0) NULL;
");

            // member_addresses
            sb.Append(@"
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
END;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_member_addresses_members')
  ALTER TABLE dbo.member_addresses WITH CHECK
    ADD CONSTRAINT FK_member_addresses_members FOREIGN KEY(member_id) REFERENCES dbo.members(id);
");

            // categories
            sb.Append(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='categories' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.categories(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] VARCHAR(255) NOT NULL
  );
END;
");

            // products
            sb.Append(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='products' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.products(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    category_id INT NOT NULL,
    [name] VARCHAR(255) NOT NULL,
    [description] VARCHAR(MAX) NOT NULL,
    stock INT NOT NULL DEFAULT (0),
    price DECIMAL(10,2) NOT NULL,
    [image] VARCHAR(255) NULL,
    is_show BIT NOT NULL DEFAULT (1),
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_products_categories')
  ALTER TABLE dbo.products WITH CHECK
    ADD CONSTRAINT FK_products_categories FOREIGN KEY(category_id) REFERENCES dbo.categories(id);
");

            // payment_methods
            sb.Append(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='payment_methods' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.payment_methods(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [name] VARCHAR(100) NOT NULL,
    is_use BIT NOT NULL DEFAULT(1),
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;
IF EXISTS (SELECT 1 FROM sys.tables WHERE name='payment_methods' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name='CK_payment_methods_name' AND parent_object_id = OBJECT_ID('dbo.payment_methods'))
    ALTER TABLE dbo.payment_methods DROP CONSTRAINT CK_payment_methods_name;

  ALTER TABLE dbo.payment_methods WITH CHECK ADD CONSTRAINT CK_payment_methods_name
    CHECK ([name] IN ('Cash On Delivery','eSewa'));
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UQ_payment_methods_name' AND object_id=OBJECT_ID('dbo.payment_methods'))
  CREATE UNIQUE NONCLUSTERED INDEX UQ_payment_methods_name ON dbo.payment_methods([name]);
");

            // orders
            sb.Append(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='orders' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.orders(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    order_code VARCHAR(50) NOT NULL,
    member_id INT NOT NULL,
    ship_name VARCHAR(255) NULL,
    ship_phone VARCHAR(50) NULL,
    [status] VARCHAR(50) NULL,
    total_qty INT NOT NULL,
    total_amount DECIMAL(10,2) NOT NULL,
    payment VARCHAR(255) NOT NULL,
    order_date DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UQ_orders_order_code' AND object_id=OBJECT_ID('dbo.orders'))
  CREATE UNIQUE NONCLUSTERED INDEX UQ_orders_order_code ON dbo.orders(order_code);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_orders_members')
  ALTER TABLE dbo.orders WITH CHECK
    ADD CONSTRAINT FK_orders_members FOREIGN KEY(member_id) REFERENCES dbo.members(id);
-- Helpful composite index for status/date filtering
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_orders_status_date' AND object_id=OBJECT_ID('dbo.orders'))
  CREATE NONCLUSTERED INDEX IX_orders_status_date ON dbo.orders([status], order_date DESC);
");

            // order_items
            sb.Append(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='order_items' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.order_items(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_order_items_orders')
  ALTER TABLE dbo.order_items WITH CHECK
    ADD CONSTRAINT FK_order_items_orders FOREIGN KEY(order_id) REFERENCES dbo.orders(id);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_order_items_products')
  ALTER TABLE dbo.order_items WITH CHECK
    ADD CONSTRAINT FK_order_items_products FOREIGN KEY(product_id) REFERENCES dbo.products(id);
");

            // order_addresses
            sb.Append(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='order_addresses' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.order_addresses(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    order_id INT NOT NULL,
    [address] VARCHAR(MAX) NULL,
    township VARCHAR(255) NULL,
    postal_code VARCHAR(20) NULL,
    city VARCHAR(100) NULL,
    [state] VARCHAR(100) NULL,
    country VARCHAR(100) NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_order_addresses_orders')
  ALTER TABLE dbo.order_addresses WITH CHECK
    ADD CONSTRAINT FK_order_addresses_orders FOREIGN KEY(order_id) REFERENCES dbo.orders(id);
");

            // order_logs
            sb.Append(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='order_logs' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.order_logs(
    id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    order_id INT NOT NULL,
    [status] VARCHAR(50) NOT NULL,
    admin_id INT NOT NULL,
    created_at DATETIME2(0) NOT NULL DEFAULT (GETDATE()),
    updated_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_order_logs_orders')
  ALTER TABLE dbo.order_logs WITH CHECK
    ADD CONSTRAINT FK_order_logs_orders FOREIGN KEY(order_id) REFERENCES dbo.orders(id);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_order_logs_admins')
  ALTER TABLE dbo.order_logs WITH CHECK
    ADD CONSTRAINT FK_order_logs_admins FOREIGN KEY(admin_id) REFERENCES dbo.admins(id);
");

            // products_images cache helper is not required; images live under Assets/images/products

            // feedbacks
            sb.Append(@"
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
  ALTER TABLE dbo.feedbacks WITH CHECK
    ADD CONSTRAINT FK_feedbacks_admins FOREIGN KEY(admin_id) REFERENCES dbo.admins(id);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_feedbacks_members')
  ALTER TABLE dbo.feedbacks WITH CHECK
    ADD CONSTRAINT FK_feedbacks_members FOREIGN KEY(member_id) REFERENCES dbo.members(id);
");

            // notifications
            sb.Append(@"
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
  ALTER TABLE dbo.notifications WITH CHECK
    ADD CONSTRAINT FK_notifications_members FOREIGN KEY(recipient_member_id) REFERENCES dbo.members(id);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_notifications_orders')
  ALTER TABLE dbo.notifications WITH CHECK
    ADD CONSTRAINT FK_notifications_orders FOREIGN KEY(order_id) REFERENCES dbo.orders(id);
");

            // v2: allow 'eSewa' in payment methods and seed it
            var v2 = new StringBuilder();
            v2.Append(@"
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name='CK_payment_methods_name')
BEGIN
  ALTER TABLE dbo.payment_methods DROP CONSTRAINT CK_payment_methods_name;
END;

ALTER TABLE dbo.payment_methods WITH CHECK ADD CONSTRAINT CK_payment_methods_name
    CHECK ([name] IN ('Cash On Delivery','eSewa'));

IF NOT EXISTS (SELECT 1 FROM dbo.payment_methods WHERE name = 'eSewa')
BEGIN
  INSERT INTO dbo.payment_methods (name, is_use) VALUES ('eSewa', 1);
END;
");

            // v3: seed default admin admin/123456
            var v3 = new StringBuilder();
            v3.Append(@"
IF NOT EXISTS (SELECT 1 FROM dbo.admins WHERE username = 'admin')
BEGIN
  INSERT INTO dbo.admins (full_name, username, [password], role)
  VALUES ('Admin User', 'admin', '8D969EEF6ECAD3C29A3A629280E686CF0C3F5D5A86AFF3CA12020C923ADC6C92', 'superadmin');
END;
");

            // v4: notifications table for order alerts
            var v4 = new StringBuilder();
            v4.Append(@"
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

            // v5: restrict payment methods to cash on delivery + eSewa
            var v5 = new StringBuilder();
            v5.Append(@"
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name='CK_payment_methods_name')
BEGIN
  ALTER TABLE dbo.payment_methods DROP CONSTRAINT CK_payment_methods_name;
END;

ALTER TABLE dbo.payment_methods WITH CHECK ADD CONSTRAINT CK_payment_methods_name
    CHECK ([name] IN ('Cash On Delivery','eSewa'));
");

            return new[]
            {
                new Migration { Version = 1, Name = "create_core_tables", Sql = sb.ToString() },
                new Migration { Version = 2, Name = "add_esewa_payment", Sql = v2.ToString() },
                new Migration { Version = 3, Name = "seed_default_admin", Sql = v3.ToString() },
                new Migration { Version = 4, Name = "add_notifications", Sql = v4.ToString() },
                new Migration { Version = 5, Name = "restrict_payment_methods", Sql = v5.ToString() },
                new Migration { Version = 6, Name = "add_payment_transactions", Sql = v6.ToString() }
            };
        }

        public static int RunAll(out string log)
        {
            var sb = new StringBuilder();
            int applied = 0;

            using (var con = Db.Open())
            {
                // Ensure migrations table exists first
                using (var cmd0 = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='schema_migrations' AND schema_id=SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.schema_migrations (
    version INT NOT NULL PRIMARY KEY,
    name    VARCHAR(200) NULL,
    applied_at DATETIME2(0) NOT NULL DEFAULT (GETDATE())
  );
END;", con))
                { cmd0.ExecuteNonQuery(); }

                foreach (var m in All())
                {
                    int count = Convert.ToInt32(Db.Scalar<int>("SELECT COUNT(*) FROM dbo.schema_migrations WHERE version=@v",
                        Db.P("@v", m.Version)));

                    if (count > 0)
                    {
                        sb.AppendLine("✔  " + m.Version + " - " + m.Name + " (already applied)");
                        continue;
                    }

                    using (var tx = con.BeginTransaction())
                    using (var cmd = new SqlCommand(m.Sql, con, tx))
                    {
                        try
                        {
                            cmd.ExecuteNonQuery();
                            using (var mark = new SqlCommand("INSERT INTO dbo.schema_migrations(version, name) VALUES(@v, @n)", con, tx))
                            {
                                mark.Parameters.AddWithValue("@v", m.Version);
                                mark.Parameters.AddWithValue("@n", m.Name ?? (object)DBNull.Value);
                                mark.ExecuteNonQuery();
                            }
                            tx.Commit();
                            applied++;
                            sb.AppendLine("↑  " + m.Version + " - " + m.Name + " (applied)");
                        }
                        catch (Exception ex)
                        {
                            tx.Rollback();
                            sb.AppendLine("✖  " + m.Version + " - " + m.Name + " FAILED: " + ex.Message);
                            break;
                        }
                    }
                }
            }

            log = sb.ToString();
            return applied;
        }
    }
}
