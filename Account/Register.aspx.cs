using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace serena.Site.Account
{
    public partial class RegisterPage : Page
    {
        private static readonly Regex UsernameRegex = new Regex(@"^[a-zA-Z0-9_-]{3,20}$", RegexOptions.Compiled);

        protected void txtUsername_TextChanged(object sender, EventArgs e)
        {
            var u = (txtUsername.Text ?? "").Trim();
            if (!UsernameRegex.IsMatch(u))
            {
                lblUserCheck.Text = "Invalid username format (3–20 chars: letters, numbers, _ or -).";
                lblUserCheck.CssClass = "small text-danger";
                return;
            }

            var exists = Db.Query("SELECT 1 FROM members WHERE username=@u;", new SqlParameter("@u", u));
            if (exists.Rows.Count > 0)
            {
                lblUserCheck.Text = "Username is taken.";
                lblUserCheck.CssClass = "small text-danger";
            }
            else
            {
                lblUserCheck.Text = "Username is available.";
                lblUserCheck.CssClass = "small text-success";
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            var username = (txtUsername.Text ?? "").Trim();
            var fullName = (txtName.Text ?? "").Trim();
            var email = (txtEmail.Text ?? "").Trim();
            var password = txtPassword.Text ?? "";
            var confirm = txtConfirm.Text ?? "";

            if (!UsernameRegex.IsMatch(username)) { ShowError("Username must be 3–20 chars (letters, numbers, _ or -)."); return; }
            if (fullName.Length == 0) { ShowError("Please enter your full name."); return; }
            if (email.Length == 0) { ShowError("Email is required."); return; } // email NOT NULL in your schema
            if (password.Length < 6) { ShowError("Password must be at least 6 characters."); return; }
            if (!string.Equals(password, confirm, StringComparison.Ordinal)) { ShowError("Passwords do not match."); return; }

            // Uniqueness checks
            if (Db.Query("SELECT 1 FROM members WHERE username=@u;", new SqlParameter("@u", username)).Rows.Count > 0)
            { ShowError("Username already exists."); return; }
            if (Db.Query("SELECT 1 FROM members WHERE email=@e;", new SqlParameter("@e", email)).Rows.Count > 0)
            { ShowError("Email already in use."); return; }

            // Hash to single column: pbkdf2$10000$<salt>$<hash>
            string storedPassword = CreatePasswordToken(password);

            try
            {
                var dt = Db.Query(@"
INSERT INTO members (full_name, username, email, password, phone, created_at, updated_at)
VALUES (@n, @u, @e, @p, NULL, GETDATE(), GETDATE());
SELECT SCOPE_IDENTITY() AS id;",
                    new SqlParameter("@n", fullName),
                    new SqlParameter("@u", username),
                    new SqlParameter("@e", email),
                    new SqlParameter("@p", storedPassword));

                int memberId = 0;
                if (dt.Rows.Count > 0)
                    memberId = Convert.ToInt32(Convert.ToDecimal(dt.Rows[0]["id"]));

                if (memberId <= 0) { ShowError("Failed to create account. Please try again."); return; }

                // Sign-in
                Session["MEMBER_ID"] = memberId;
                Session["USER_USERNAME"] = username;
                Session["USER_NAME"] = fullName;
                Session["USER_EMAIL"] = email;
                IssuePersistentMemberToken(memberId);

                // Redirect
                string returnUrl = Request.QueryString["returnUrl"];
                if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/", StringComparison.Ordinal))
                {
                    Response.Redirect(returnUrl, false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
                Response.Redirect(ResolveUrl("~/"), false);
            }
            catch (SqlException ex)
            {
                // 2601/2627 = duplicate key
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    if (ex.Message.IndexOf("UQ_members_username", StringComparison.OrdinalIgnoreCase) >= 0
                        || ex.Message.IndexOf("username", StringComparison.OrdinalIgnoreCase) >= 0)
                        ShowError("Username already exists.");
                    else if (ex.Message.IndexOf("UQ_members_email", StringComparison.OrdinalIgnoreCase) >= 0
                        || ex.Message.IndexOf("email", StringComparison.OrdinalIgnoreCase) >= 0)
                        ShowError("Email already in use.");
                    else
                        ShowError("Duplicate value detected. Please change your details and try again.");
                    return;
                }
                // 515 = NOT NULL violation
                if (ex.Number == 515 && ex.Message.IndexOf("email", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ShowError("Email is required.");
                    return;
                }
                ShowError("Error creating account. " + HttpUtility.HtmlEncode(ex.Message));
            }
            catch (Exception ex)
            {
                ShowError("Error creating account. " + HttpUtility.HtmlEncode(ex.Message));
            }
        }

        private void ShowError(string msg) { lblMessage.Text = HttpUtility.HtmlEncode(msg); }

        private void IssuePersistentMemberToken(int memberId)
        {
            string token = Guid.NewGuid().ToString("N");
            DateTime expires = DateTime.UtcNow.AddDays(14);

            try
            {
                Db.Execute(@"UPDATE members
                             SET persistent_token = @t,
                                 token_expires = @e
                             WHERE id = @id",
                    new SqlParameter("@t", token),
                    new SqlParameter("@e", expires),
                    new SqlParameter("@id", memberId));
            }
            catch
            {
                // Best-effort: continue with the cookie even if the columns are not present yet.
            }

            var ck = new HttpCookie("MemberToken", token)
            {
                HttpOnly = true,
                Expires = expires
            };
            Response.Cookies.Add(ck);
        }

        // -------- Hashing to single column --------
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
            { try { pb.IterationCount = iterations; } catch { } return pb.GetBytes(length); }
        }
    }
}
