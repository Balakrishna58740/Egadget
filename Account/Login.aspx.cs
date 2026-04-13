using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace serena.Site.Account
{
    public partial class LoginPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Already logged in? Go to ReturnUrl or Profile
            if (!IsPostBack && Session["MEMBER_ID"] != null)
            {
                string returnUrl = Request.QueryString["returnUrl"];
                if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/", StringComparison.Ordinal))
                    Response.Redirect(returnUrl, true);
                else
                    Response.Redirect("~/Account/Profile.aspx", true);
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Read from controls no matter where they’re nested; fallback to Request.Form keys like ctl00$MainContent$txtUser
            string userOrEmail = ReadPosted("txtUser") ?? ReadPosted("txtUsername") ?? ReadPosted("txtEmail");
            string password = ReadPosted("txtPass") ?? ReadPosted("txtPassword");

            if (string.IsNullOrWhiteSpace(userOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                Show("Please enter your email/username and password.");
                return;
            }

            try
            {
                int memberId = 0;
                string stored = null;

                using (var con = Db.Open())
                using (var cmd = new SqlCommand(
                    "SELECT TOP 1 id, [password] FROM dbo.members WHERE (username=@u OR email=@u)", con))
                {
                    cmd.Parameters.AddWithValue("@u", userOrEmail);
                    using (var r = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                    {
                        if (r.Read())
                        {
                            memberId = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                            stored = r.IsDBNull(1) ? null : r.GetString(1);
                        }
                    }
                }

                if (memberId <= 0 || string.IsNullOrEmpty(stored) || !PasswordMatches(password, stored))
                {
                    Show("Invalid username/email or password.");
                    return;
                }

                // Success
                Session["MEMBER_ID"] = memberId;
                Session["USER_NAME"] = userOrEmail;
                IssuePersistentMemberToken(memberId);

                // Redirect
                string returnUrl = Request.QueryString["returnUrl"];
                if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/", StringComparison.Ordinal))
                {
                    Response.Redirect(returnUrl, false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    Response.Redirect("~/Account/Profile.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch
            {
                Show("Login failed. Please try again.");
            }
        }

        // ---------- robust posted value reader ----------
        private string ReadPosted(string id)
        {
            // 1) try to find a server TextBox anywhere on the page (nested controls, master, etc.)
            var ctl = FindControlRecursive(this, id) as TextBox;
            if (ctl != null) return (ctl.Text ?? "").Trim();

            // 2) fall back to Request.Form by exact key
            if (Request != null && Request.Form != null)
            {
                string v = Request.Form[id];
                if (!string.IsNullOrEmpty(v)) return v.Trim();

                // 3) try match by suffix (handles UniqueID like ctl00$MainContent$txtUser)
                string[] keys = Request.Form.AllKeys;
                if (keys != null)
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        string k = keys[i];
                        if (string.IsNullOrEmpty(k)) continue;
                        if (k.Equals(id, StringComparison.Ordinal) || k.EndsWith("$" + id, StringComparison.Ordinal))
                        {
                            string val = Request.Form[k];
                            if (!string.IsNullOrWhiteSpace(val)) return val.Trim();
                        }
                    }
                }
            }
            return null;
        }

        private Control FindControlRecursive(Control root, string id)
        {
            if (root == null) return null;
            Control match = root.FindControl(id);
            if (match != null) return match;
            foreach (Control c in root.Controls)
            {
                match = FindControlRecursive(c, id);
                if (match != null) return match;
            }
            return null;
        }

        private void IssuePersistentMemberToken(int memberId)
        {
            string token = Guid.NewGuid().ToString("N");
            DateTime expires = DateTime.UtcNow.AddDays(14);

            try
            {
                Db.Execute(@"UPDATE dbo.members
                             SET persistent_token = @t,
                                 token_expires = @e
                             WHERE id = @id",
                    Db.P("@t", token),
                    Db.P("@e", expires),
                    Db.P("@id", memberId));
            }
            catch
            {
                // Best-effort: cookie is still issued, and schema migrations can add the columns.
            }

            var ck = new HttpCookie("MemberToken", token)
            {
                HttpOnly = true,
                Expires = expires
            };
            Response.Cookies.Add(ck);
        }

        // ---------- password verification (pbkdf2 / sha256 / plain) ----------
        private bool PasswordMatches(string inputPassword, string stored)
        {
            if (stored.StartsWith("pbkdf2$", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var parts = stored.Split('$'); // pbkdf2$<iter>$<saltB64>$<hashB64>
                    int iter = int.Parse(parts[1]);
                    byte[] salt = Convert.FromBase64String(parts[2]);
                    byte[] expected = Convert.FromBase64String(parts[3]);
                    byte[] actual = Pbkdf2(inputPassword ?? "", salt, iter, expected.Length);
                    return SlowEquals(expected, actual);
                }
                catch { return false; }
            }

            if (IsLikelySha256Hex(stored))
            {
                string inputHex = Sha256Hex(inputPassword ?? "");
                return inputHex.Equals(stored, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(inputPassword ?? "", stored, StringComparison.Ordinal);
        }

        private static bool IsLikelySha256Hex(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != 64) return false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                bool hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!hex) return false;
            }
            return true;
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
            int diff = 0; for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i]; return diff == 0;
        }

        // ---------- ui helpers ----------
        private void Show(string message)
        {
            var lbl = FindControlRecursive(this, "lblMessage") as Label;
            if (lbl != null) lbl.Text = Server.HtmlEncode(message);
        }
    }
}
