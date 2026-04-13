using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace serena.Site.Account
{
    public partial class ProfilePage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int uid = GetUserId();
            if (uid <= 0)
            {
                RedirectToLogin();
                return;
            }

            if (!IsPostBack)
            {
                LoadMember(uid);
                BindAddresses(uid);
                ClearAddressEditor(setDefaultTick: true); // ready to add
            }
        }

        private int GetUserId()
        {
            try
            {
                object value = Session["MEMBER_ID"];
                if (value == null) return 0;

                int id;
                return int.TryParse(Convert.ToString(value), out id) ? id : 0;
            }
            catch
            {
                return 0;
            }
        }

        private void RedirectToLogin()
        {
            Response.Redirect("~/Account/Login.aspx?returnUrl=" + HttpUtility.UrlEncode(Request.RawUrl), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void LoadMember(int id)
        {
            var dt = Db.Query(@"
SELECT id, username, full_name, email, phone, created_at
FROM members
WHERE id = @id;",
                new SqlParameter("@id", id));

            if (dt.Rows.Count == 0)
            {
                RedirectToLogin();
                return;
            }

            var r = dt.Rows[0];
            litUsername.Text = HttpUtility.HtmlEncode(Convert.ToString(r["username"]));
            litMemberSince.Text = Convert.ToDateTime(r["created_at"]).ToString("yyyy-MM-dd");

            txtUsername.Text = Convert.ToString(r["username"]);
            txtFullName.Text = Convert.ToString(r["full_name"]);
            txtEmail.Text = Convert.ToString(r["email"]);
            txtPhone.Text = r["phone"] == DBNull.Value ? "" : Convert.ToString(r["phone"]);
        }

        /* -------------------- Addresses (multiple) -------------------- */

        private void BindAddresses(int memberId)
        {
            var dt = Db.Query(@"
SELECT id, address, township, postal_code, city, state, country, is_default
FROM member_addresses
WHERE member_id=@mid
ORDER BY CASE WHEN is_default=1 THEN 0 ELSE 1 END, id DESC;",
                new SqlParameter("@mid", memberId));

            lvAddresses.DataSource = dt;
            lvAddresses.DataBind();
        }

        protected void lvAddresses_ItemCommand(object sender, System.Web.UI.WebControls.ListViewCommandEventArgs e)
        {
            int memberId = GetUserId();
            if (memberId <= 0) { RedirectToLogin(); return; }
            int id = Convert.ToInt32(e.CommandArgument);

            try
            {
                if (e.CommandName == "EditAddr")
                {
                    LoadAddressForEdit(memberId, id);
                }
                else if (e.CommandName == "MakeDefault")
                {
                    // set this as default; unset others
                    Db.Execute("UPDATE member_addresses SET is_default=1, updated_at=GETDATE() WHERE id=@id AND member_id=@mid;",
                        Db.P("@id", id), Db.P("@mid", memberId));
                    Db.Execute("UPDATE member_addresses SET is_default=0, updated_at=GETDATE() WHERE member_id=@mid AND id<>@id;",
                        Db.P("@mid", memberId), Db.P("@id", id));

                    BindAddresses(memberId);
                }
                else if (e.CommandName == "DeleteAddr")
                {
                    // was it default?
                    bool wasDefault = Db.Scalar<int>("SELECT COUNT(*) FROM member_addresses WHERE id=@id AND member_id=@mid AND is_default=1;",
                        Db.P("@id", id), Db.P("@mid", memberId)) > 0;

                    Db.Execute("DELETE FROM member_addresses WHERE id=@id AND member_id=@mid;",
                        Db.P("@id", id), Db.P("@mid", memberId));

                    if (wasDefault)
                    {
                        // promote another one to default if any exist
                        var dt = Db.Query("SELECT TOP 1 id FROM member_addresses WHERE member_id=@mid ORDER BY id DESC;",
                            Db.P("@mid", memberId));
                        if (dt.Rows.Count > 0)
                        {
                            int nid = Convert.ToInt32(dt.Rows[0]["id"]);
                            Db.Execute("UPDATE member_addresses SET is_default=1, updated_at=GETDATE() WHERE id=@id;", Db.P("@id", nid));
                            Db.Execute("UPDATE member_addresses SET is_default=0, updated_at=GETDATE() WHERE member_id=@mid AND id<>@id;",
                                Db.P("@mid", memberId), Db.P("@id", nid));
                        }
                    }

                    BindAddresses(memberId);
                    ClearAddressEditor();
                    lblAddrMsg.CssClass = "text-success d-block mb-2";
                    lblAddrMsg.Text = "Address deleted.";
                }
            }
            catch (Exception ex)
            {
                lblAddrMsg.CssClass = "text-danger d-block mb-2";
                lblAddrMsg.Text = "Address action failed. " + Server.HtmlEncode(ex.Message);
            }
        }

        private void LoadAddressForEdit(int memberId, int addrId)
        {
            var dt = Db.Query(@"
SELECT id, address, township, postal_code, city, state, country, is_default
FROM member_addresses
WHERE id=@id AND member_id=@mid;",
                new SqlParameter("@id", addrId),
                new SqlParameter("@mid", memberId));

            if (dt.Rows.Count == 0) { ClearAddressEditor(); return; }

            var r = dt.Rows[0];
            hidAddrId.Value = Convert.ToString(r["id"]);
            txtAddr.Text = r["address"] == DBNull.Value ? "" : Convert.ToString(r["address"]);
            txtTownship.Text = r["township"] == DBNull.Value ? "" : Convert.ToString(r["township"]);
            txtPostal.Text = r["postal_code"] == DBNull.Value ? "" : Convert.ToString(r["postal_code"]);
            txtCity.Text = r["city"] == DBNull.Value ? "" : Convert.ToString(r["city"]);
            txtState.Text = r["state"] == DBNull.Value ? "" : Convert.ToString(r["state"]);
            txtCountry.Text = r["country"] == DBNull.Value ? "" : Convert.ToString(r["country"]);
            chkDefault.Checked = Convert.ToBoolean(r["is_default"]);
            lblAddrMsg.Text = "";
        }

        protected void btnNewAddress_Click(object sender, EventArgs e)
        {
            ClearAddressEditor();
            lblAddrMsg.Text = "";
        }

        private void ClearAddressEditor(bool setDefaultTick = false)
        {
            hidAddrId.Value = "";
            txtAddr.Text = txtTownship.Text = txtPostal.Text = txtCity.Text = txtState.Text = txtCountry.Text = "";
            chkDefault.Checked = setDefaultTick;
        }

        protected void btnSaveAddress_Click(object sender, EventArgs e)
        {
            int memberId = GetUserId();
            if (memberId <= 0) { RedirectToLogin(); return; }

            string a = (txtAddr.Text ?? "").Trim();
            string tw = (txtTownship.Text ?? "").Trim();
            string pc = (txtPostal.Text ?? "").Trim();
            string ci = (txtCity.Text ?? "").Trim();
            string st = (txtState.Text ?? "").Trim();
            string co = (txtCountry.Text ?? "").Trim();
            bool makeDefault = chkDefault.Checked;

            try
            {
                int addrId;
                if (int.TryParse(hidAddrId.Value, out addrId) && addrId > 0)
                {
                    // Update existing
                    Db.Execute(@"
UPDATE member_addresses
SET address=@a, township=@tw, postal_code=@pc, city=@ci, state=@st, country=@co,
    is_default = CASE WHEN @def=1 THEN 1 ELSE is_default END,
    updated_at=GETDATE()
WHERE id=@aid AND member_id=@mid;",
                        Db.P("@a", string.IsNullOrWhiteSpace(a) ? (object)DBNull.Value : a),
                        Db.P("@tw", string.IsNullOrWhiteSpace(tw) ? (object)DBNull.Value : tw),
                        Db.P("@pc", string.IsNullOrWhiteSpace(pc) ? (object)DBNull.Value : pc),
                        Db.P("@ci", string.IsNullOrWhiteSpace(ci) ? (object)DBNull.Value : ci),
                        Db.P("@st", string.IsNullOrWhiteSpace(st) ? (object)DBNull.Value : st),
                        Db.P("@co", string.IsNullOrWhiteSpace(co) ? (object)DBNull.Value : co),
                        Db.P("@def", makeDefault ? 1 : 0),
                        Db.P("@aid", addrId),
                        Db.P("@mid", memberId)
                    );

                    if (makeDefault)
                    {
                        Db.Execute("UPDATE member_addresses SET is_default=0, updated_at=GETDATE() WHERE member_id=@mid AND id<>@aid;",
                            Db.P("@mid", memberId), Db.P("@aid", addrId));
                    }
                }
                else
                {
                    // Insert new
                    int existing = Db.Scalar<int>("SELECT COUNT(*) FROM member_addresses WHERE member_id=@m;", Db.P("@m", memberId));
                    int defFlag = (existing == 0) ? 1 : (makeDefault ? 1 : 0);

                    var dt = Db.Query(@"
INSERT INTO member_addresses (member_id, address, township, postal_code, city, state, country, is_default)
OUTPUT INSERTED.id
VALUES (@mid, @a, @tw, @pc, @ci, @st, @co, @def);",
                        Db.P("@mid", memberId),
                        Db.P("@a", string.IsNullOrWhiteSpace(a) ? (object)DBNull.Value : a),
                        Db.P("@tw", string.IsNullOrWhiteSpace(tw) ? (object)DBNull.Value : tw),
                        Db.P("@pc", string.IsNullOrWhiteSpace(pc) ? (object)DBNull.Value : pc),
                        Db.P("@ci", string.IsNullOrWhiteSpace(ci) ? (object)DBNull.Value : ci),
                        Db.P("@st", string.IsNullOrWhiteSpace(st) ? (object)DBNull.Value : st),
                        Db.P("@co", string.IsNullOrWhiteSpace(co) ? (object)DBNull.Value : co),
                        Db.P("@def", defFlag)
                    );

                    int newId = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0][0]) : 0;

                    if (defFlag == 1 && newId > 0)
                    {
                        Db.Execute("UPDATE member_addresses SET is_default=0, updated_at=GETDATE() WHERE member_id=@mid AND id<>@aid;",
                            Db.P("@mid", memberId), Db.P("@aid", newId));
                    }

                    hidAddrId.Value = newId.ToString();
                }

                BindAddresses(memberId);
                lblAddrMsg.CssClass = "text-success d-block mb-2";
                lblAddrMsg.Text = "Address saved.";
            }
            catch (Exception ex)
            {
                lblAddrMsg.CssClass = "text-danger d-block mb-2";
                lblAddrMsg.Text = "Error saving address. " + Server.HtmlEncode(ex.Message);
            }
        }

        /* -------------------- Security -------------------- */

        protected void btnSaveAccount_Click(object sender, EventArgs e)
        {
            int id = GetUserId();
            if (id <= 0) { RedirectToLogin(); return; }
            string fullName = (txtFullName.Text ?? "").Trim();
            string email = (txtEmail.Text ?? "").Trim();
            string phone = (txtPhone.Text ?? "").Trim();

            if (fullName.Length == 0) { ShowAccountError("Please enter your full name."); return; }
            if (!IsValidEmail(email)) { ShowAccountError("Please enter a valid email."); return; }

            try
            {
                var exists = Db.Query("SELECT 1 FROM members WHERE email=@e AND id<>@id;",
                    new SqlParameter("@e", email), new SqlParameter("@id", id));
                if (exists.Rows.Count > 0) { ShowAccountError("Email is already in use."); return; }

                Db.Query(@"
UPDATE members
SET full_name=@n, email=@e, phone=@p, updated_at=GETDATE()
WHERE id=@id;",
                    new SqlParameter("@n", fullName),
                    new SqlParameter("@e", email),
                    new SqlParameter("@p", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone),
                    new SqlParameter("@id", id));

                Session["USER_NAME"] = fullName;
                Session["USER_EMAIL"] = email;
                lblAccountMsg.CssClass = "text-success d-block mb-2";
                lblAccountMsg.Text = "Profile updated.";
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    ShowAccountError("Email is already in use."); return;
                }
                ShowAccountError("Error updating profile. " + HttpUtility.HtmlEncode(ex.Message));
            }
            catch (Exception ex)
            {
                ShowAccountError("Error updating profile. " + HttpUtility.HtmlEncode(ex.Message));
            }
        }

        private void ShowAccountError(string msg)
        {
            lblAccountMsg.CssClass = "text-danger d-block mb-2";
            lblAccountMsg.Text = HttpUtility.HtmlEncode(msg);
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        /* -------------------- Password helpers -------------------- */

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            int id = GetUserId();
            if (id <= 0) { RedirectToLogin(); return; }
            var cur = txtCur.Text ?? "";
            var n1 = txtNew.Text ?? "";
            var n2 = txtNew2.Text ?? "";

            if (n1.Length < 6) { SecMsg("Password must be at least 6 characters."); return; }
            if (n1 != n2) { SecMsg("New passwords do not match."); return; }

            var dt = Db.Query("SELECT password FROM members WHERE id=@id;", new SqlParameter("@id", id));
            if (dt.Rows.Count == 0) { SecMsg("Account not found."); return; }

            string stored = Convert.ToString(dt.Rows[0]["password"]) ?? "";
            if (!VerifyPasswordFlexible(cur, stored)) { SecMsg("Current password is incorrect."); return; }

            string token = CreatePasswordToken(n1);
            Db.Query("UPDATE members SET password=@p, updated_at=GETDATE() WHERE id=@id;",
                     new SqlParameter("@p", token), new SqlParameter("@id", id));

            lblSecMsg.CssClass = "text-success d-block mb-2";
            lblSecMsg.Text = "Password updated.";
            txtCur.Text = txtNew.Text = txtNew2.Text = "";
        }

        private void SecMsg(string m)
        {
            lblSecMsg.CssClass = "text-danger d-block mb-2";
            lblSecMsg.Text = HttpUtility.HtmlEncode(m);
        }

        private bool VerifyPasswordFlexible(string password, string stored)
        {
            if (!string.IsNullOrEmpty(stored) && stored.StartsWith("pbkdf2$", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var parts = stored.Split('$');
                    int iter = int.Parse(parts[1]);
                    byte[] salt = Convert.FromBase64String(parts[2]);
                    byte[] expected = Convert.FromBase64String(parts[3]);
                    byte[] test = Pbkdf2(password, salt, iter, expected.Length);
                    return SlowEquals(expected, test);
                }
                catch { return false; }
            }
            return string.Equals(password, stored, StringComparison.Ordinal);
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
            { try { pb.IterationCount = iterations; } catch { } return pb.GetBytes(length); }
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0; for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i]; return diff == 0;
        }
    }
}
