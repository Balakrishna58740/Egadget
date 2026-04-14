using System;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace serena
{
    public partial class SetupPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Show connection hint
            var cs = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            var litConn = Find<Literal>("litConn");
            if (litConn != null) litConn.Text = cs != null ? Server.HtmlEncode(cs.ConnectionString) : "(no DefaultConnection)";

            // Gate by SetupKey (querystring k=)
            bool ok = IsAuthorized();
            var rowBtns = Find<Control>("rowBtns");
            if (rowBtns != null) rowBtns.Visible = ok;

            if (!ok)
            {
                Show(false, "Missing or invalid SetupKey. Add <code>?k=YOUR_KEY</code> to the URL and define it in <code>Web.config</code>.");
            }

            if (!IsPostBack && ok)
            {
                Show(true, "Ready. You can run migrations and/or seed data.");
            }
        }

        protected void btnMigrate_Click(object sender, EventArgs e)
        {
            if (!IsAuthorized()) { Show(false, "Unauthorized."); return; }
            string log;
            int n = Migrator.RunAll(out log);
            Find<Literal>("litLog").Text = Server.HtmlEncode(log);
            Show(true, n > 0 ? ("Applied " + n + " migration(s).") : "No migrations to apply.");
        }

        protected void btnSeed_Click(object sender, EventArgs e)
        {
            if (!IsAuthorized()) { Show(false, "Unauthorized."); return; }
            var sb = new StringBuilder();
            try
            {
                // Admin
                EnsureAdmin("eGadgetHub Administrator", "admin", "123456", "superadmin", sb);

                // Categories
                string[] cats = { "Smartphones", "Laptops", "Audio & Headphones", "Cameras", "Smartwatches", "Accessories" };
                foreach (var c in cats) EnsureCategory(c, sb);

                // Payment Methods (must match CHECK constraint)
                EnsurePayment("Cash On Delivery", sb);
                EnsurePayment("eSewa", sb);

                // Products (name, category, price, stock)
                SeedProduct("iPhone 15 Pro", "Smartphones", 999.00m, 15, sb);
                SeedProduct("MacBook Air M2", "Laptops", 1199.00m, 10, sb);
                SeedProduct("Sony WH-1000XM5", "Audio & Headphones", 349.00m, 25, sb);
                SeedProduct("Fujifilm X-T5", "Cameras", 1699.00m, 5, sb);
                SeedProduct("Apple Watch Series 9", "Smartwatches", 399.00m, 20, sb);
                SeedProduct("AirPods Pro (2nd Gen)", "Audio & Headphones", 249.00m, 30, sb);
                SeedProduct("Logitech MX Master 3S", "Accessories", 99.00m, 50, sb);
                SeedProduct("Dell XPS 13", "Laptops", 1099.00m, 12, sb);
                SeedProduct("Samsung Galaxy S24 Ultra", "Smartphones", 1299.00m, 18, sb);
                SeedProduct("DJI Mini 4 Pro", "Cameras", 759.00m, 8, sb);

                Find<Literal>("litLog").Text = Server.HtmlEncode(sb.ToString());
                Show(true, "Seed completed.");
            }
            catch (Exception ex)
            {
                AppendLog(sb, "ERROR: " + ex.Message);
                Find<Literal>("litLog").Text = Server.HtmlEncode(sb.ToString());
                Show(false, "Seed failed.");
            }
        }

        protected void btnBoth_Click(object sender, EventArgs e)
        {
            if (!IsAuthorized()) { Show(false, "Unauthorized."); return; }
            btnMigrate_Click(sender, e);
            btnSeed_Click(sender, e);
        }

        // ---------- auth ----------
        private bool IsAuthorized()
        {
            string required = ConfigurationManager.AppSettings["SetupKey"];
            string given = Request.QueryString["k"];
            return !string.IsNullOrEmpty(required) && string.Equals(required, given);
        }

        // ---------- seed helpers ----------
        private static void EnsureAdmin(string fullName, string user, string passPlain, string role, StringBuilder sb)
        {
            int exists = Db.Scalar<int>("SELECT COUNT(*) FROM dbo.admins WHERE username=@u", Db.P("@u", user));
            if (exists == 0)
            {
                string hash = Sha256Hex(passPlain);
                Db.Execute(@"INSERT INTO dbo.admins(full_name, username, [password], role, created_at, updated_at)
                             VALUES(@f,@u,@p,@r,GETDATE(),GETDATE())",
                             Db.P("@f", fullName), Db.P("@u", user), Db.P("@p", hash), Db.P("@r", role));
                AppendLog(sb, "admin: created '" + user + "'");
            }
            else AppendLog(sb, "admin: exists '" + user + "'");
        }

        private static int EnsureCategory(string name, StringBuilder sb)
        {
            object idObj = Db.Scalar<object>("SELECT id FROM dbo.categories WHERE name=@n", Db.P("@n", name));
            if (idObj != null && idObj != DBNull.Value) { AppendLog(sb, "category: exists '" + name + "'"); return Convert.ToInt32(idObj); }
            Db.Execute("INSERT INTO dbo.categories(name) VALUES(@n)", Db.P("@n", name));
            int id = Db.Scalar<int>("SELECT id FROM dbo.categories WHERE name=@n", Db.P("@n", name));
            AppendLog(sb, "category: created '" + name + "'");
            return id;
        }

        private static void EnsurePayment(string name, StringBuilder sb)
        {
            int exists = Db.Scalar<int>("SELECT COUNT(*) FROM dbo.payment_methods WHERE name=@n", Db.P("@n", name));
            if (exists == 0)
            {
                Db.Execute("INSERT INTO dbo.payment_methods(name, is_use, created_at, updated_at) VALUES(@n,1,GETDATE(),GETDATE())",
                    Db.P("@n", name));
                AppendLog(sb, "payment: created '" + name + "'");
            }
            else AppendLog(sb, "payment: exists '" + name + "'");
        }

        private static void SeedProduct(string name, string categoryName, decimal price, int stock, StringBuilder sb)
        {
            int exists = Db.Scalar<int>("SELECT COUNT(*) FROM dbo.products WHERE name=@n", Db.P("@n", name));
            if (exists > 0) { AppendLog(sb, "product: exists '" + name + "'"); return; }

            int catId = EnsureCategory(categoryName, sb);
            Db.Execute(@"INSERT INTO dbo.products(category_id, name, description, stock, price, image, is_show, created_at, updated_at)
                         VALUES(@c,@n,@d,@s,@p,NULL,1,GETDATE(),GETDATE())",
                         Db.P("@c", catId),
                         Db.P("@n", name),
                         Db.P("@d", "Sample description for " + name + "."),
                         Db.P("@s", stock),
                         Db.P("@p", price));
            AppendLog(sb, "product: created '" + name + "'");
        }

        private static string Sha256Hex(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? ""));
                var sb = new StringBuilder(bytes.Length * 2);
                for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("X2"));
                return sb.ToString();
            }
        }

        private void Show(bool ok, string msg)
        {
            var lbl = Find<Label>("lblMsg");
            if (lbl == null) return;
            lbl.Text = msg;
            lbl.CssClass = ok ? "alert alert-success" : "alert alert-danger";
            lbl.CssClass = lbl.CssClass.Replace(" hidden", "");
        }

        private static void AppendLog(StringBuilder sb, string line) { sb.AppendLine(line); }

        private T Find<T>(string id) where T : Control
        {
            return FindControl(id) as T;
        }
    }
}
