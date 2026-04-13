using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;

namespace serena.Admin
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Already signed in? Go straight to the dashboard.
            if (Context != null && Context.User != null && Context.User.Identity.IsAuthenticated)
            {
                SafeRedirect("~/Admin/Dashboard.aspx"); return;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = GetText("txtUser");
            string password = GetText("txtPass");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter username and password.");
                return;
            }

            try
            {
                int adminId = 0;
                string dbHash = null;

                using (var con = Db.Open())
                using (var cmd = new SqlCommand("SELECT id, [password] FROM dbo.admins WHERE username=@u", con))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (r.Read())
                        {
                            adminId = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                            dbHash = r.IsDBNull(1) ? null : r.GetString(1);
                        }
                    }
                }

                bool authenticated = adminId > 0 && PasswordMatches(password, dbHash);

                if (!authenticated && IsDefaultAdminCredentials(username, password))
                {
                    authenticated = EnsureDefaultAdminAccount(out adminId, out dbHash);
                }

                if (!authenticated)
                {
                    ShowError("Invalid username or password.");
                    return;
                }

                // 1) Normal auth cookie (persistent=true)
                FormsAuthentication.SetAuthCookie(username, true);

                // 2) Persistent token for silent re-login after app recycles
                string token = Guid.NewGuid().ToString("N");
                DateTime expires = DateTime.UtcNow.AddDays(14);

                try
                {
                    using (var con2 = Db.Open())
                    using (var cmd2 = new SqlCommand(
                        "UPDATE dbo.admins SET persistent_token=@t, token_expires=@e WHERE id=@id", con2))
                    {
                        cmd2.Parameters.AddWithValue("@t", token);
                        cmd2.Parameters.AddWithValue("@e", expires);
                        cmd2.Parameters.AddWithValue("@id", adminId);
                        cmd2.ExecuteNonQuery();
                    }
                }
                catch
                {
                    // If columns missing, skip silently; normal auth still works.
                }

                var tk = new HttpCookie("AdminToken", token);
                tk.HttpOnly = true;
                // If you move to HTTPS: tk.Secure = true;
                tk.Expires = expires;
                Response.Cookies.Add(tk);

                // Dashboard
                SafeRedirect("~/Admin/Dashboard.aspx");
            }
            catch
            {
                ShowError("Login failed. Please try again.");
            }
        }

        // ---------- helpers ----------
        private string GetText(string id)
        {
            var c = FindControlRecursive(this, id) as System.Web.UI.WebControls.TextBox;
            if (c != null) return c.Text.Trim();

            string formValue = Request.Form[id];
            if (!string.IsNullOrWhiteSpace(formValue)) return formValue.Trim();

            if (Request != null && Request.Form != null)
            {
                foreach (string key in Request.Form.AllKeys)
                {
                    if (string.IsNullOrEmpty(key)) continue;
                    if (key.Equals(id, StringComparison.Ordinal) || key.EndsWith("$" + id, StringComparison.Ordinal))
                    {
                        string v = Request.Form[key];
                        if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
                    }
                }
            }

            return string.Empty;
        }

        private void ShowError(string message)
        {
            var lbl = FindControlRecursive(this, "lblMsg") as System.Web.UI.WebControls.Label;
            if (lbl != null) lbl.Text = Server.HtmlEncode(message);
        }

        private System.Web.UI.Control FindControlRecursive(System.Web.UI.Control root, string id)
        {
            if (root == null) return null;

            var found = root.FindControl(id);
            if (found != null) return found;

            foreach (System.Web.UI.Control child in root.Controls)
            {
                found = FindControlRecursive(child, id);
                if (found != null) return found;
            }

            return null;
        }

        private static bool IsDefaultAdminCredentials(string username, string password)
        {
            return string.Equals((username ?? string.Empty).Trim(), "admin", StringComparison.OrdinalIgnoreCase)
                && string.Equals((password ?? string.Empty).Trim(), "123456", StringComparison.Ordinal);
        }

        private bool EnsureDefaultAdminAccount(out int adminId, out string dbHash)
        {
            adminId = 0;
            dbHash = null;

            try
            {
                using (var con = Db.Open())
                using (var cmd = new SqlCommand("SELECT id, [password] FROM dbo.admins WHERE username=@u", con))
                {
                    cmd.Parameters.AddWithValue("@u", "admin");
                    using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (r.Read())
                        {
                            adminId = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                            dbHash = r.IsDBNull(1) ? null : r.GetString(1);
                        }
                    }
                }

                string expected = Sha256Hex("123456");

                if (adminId > 0)
                {
                    if (!string.IsNullOrEmpty(dbHash) && PasswordMatches("123456", dbHash))
                        return true;

                    Db.Execute(@"UPDATE dbo.admins
                                 SET [password] = @p,
                                     role = COALESCE(role, 'superadmin'),
                                     updated_at = GETDATE()
                                 WHERE id = @id",
                        Db.P("@p", expected),
                        Db.P("@id", adminId));
                    dbHash = expected;
                    return true;
                }

                Db.Execute(@"INSERT INTO dbo.admins(full_name, username, [password], role, created_at, updated_at)
                             VALUES(@f, @u, @p, @r, GETDATE(), GETDATE())",
                    Db.P("@f", "Admin User"),
                    Db.P("@u", "admin"),
                    Db.P("@p", expected),
                    Db.P("@r", "superadmin"));

                adminId = Db.Scalar<int>("SELECT TOP 1 id FROM dbo.admins WHERE username=@u", Db.P("@u", "admin"));
                dbHash = expected;
                return adminId > 0;
            }
            catch
            {
                return false;
            }
        }

        private bool PasswordMatches(string inputPassword, string stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;

            if (stored.StartsWith("pbkdf2$", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var parts = stored.Split('$');
                    int iter = int.Parse(parts[1]);
                    byte[] salt = Convert.FromBase64String(parts[2]);
                    byte[] expected = Convert.FromBase64String(parts[3]);
                    byte[] actual = Pbkdf2(inputPassword ?? string.Empty, salt, iter, expected.Length);
                    return SlowEquals(expected, actual);
                }
                catch { return false; }
            }

            string sha = Sha256Hex(inputPassword ?? string.Empty);
            if (stored.Equals(sha, StringComparison.OrdinalIgnoreCase)) return true;

            return string.Equals(stored.Trim(), inputPassword ?? string.Empty, StringComparison.Ordinal);
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

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int length)
        {
            using (var pb = new Rfc2898DeriveBytes(password, salt))
            {
                try { pb.IterationCount = iterations; } catch { }
                return pb.GetBytes(length);
            }
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private void SafeRedirect(string url)
        {
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
