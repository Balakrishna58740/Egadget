using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace serena.Admin
{
    public partial class AdminProfilePage : System.Web.UI.Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (IsPostBack) return;

            string username = Context.User.Identity.Name;
            using (var con = Db.Open())
            using (var cmd = new SqlCommand("SELECT id, full_name, username, role FROM admins WHERE username=@u", con))
            {
                cmd.Parameters.AddWithValue("@u", username);
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (r.Read())
                    {
                        ViewState["AdminId"] = Convert.ToInt32(r["id"]);
                        SetText("txtFull", r["full_name"] as string);

                        var litUser = Find<Literal>("litUser");
                        if (litUser != null) litUser.Text = Server.HtmlEncode(Convert.ToString(r["username"]));

                        var litRole = Find<Literal>("litRole");
                        if (litRole != null) litRole.Text = Server.HtmlEncode(Convert.ToString(r["role"]));
                    }
                }
            }
        }

        // Update FULL NAME only
        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            var lbl = Find<Label>("lblInfo");
            int adminId = (int)(ViewState["AdminId"] ?? 0);
            string full = GetText("txtFull");

            if (string.IsNullOrWhiteSpace(full))
            {
                Show(lbl, "Full name is required.", false);
                return;
            }

            try
            {
                using (var con = Db.Open())
                using (var upd = new SqlCommand(
                    "UPDATE admins SET full_name=@n, updated_at=GETDATE() WHERE id=@id", con))
                {
                    upd.Parameters.AddWithValue("@n", (object)full ?? DBNull.Value);
                    upd.Parameters.AddWithValue("@id", adminId);
                    upd.ExecuteNonQuery();
                }
                Show(lbl, "Profile updated successfully.", true);
            }
            catch
            {
                Show(lbl, "Unable to update profile right now.", false);
            }
        }

        // Change password
        protected void btnChangePwd_Click(object sender, EventArgs e)
        {
            var lbl = Find<Label>("lblPwd");
            int adminId = (int)(ViewState["AdminId"] ?? 0);
            string oldPwd = GetText("txtOld");
            string newPwd = GetText("txtNew");
            string confirm = GetText("txtConfirm");

            if (string.IsNullOrWhiteSpace(oldPwd) || string.IsNullOrWhiteSpace(newPwd))
            {
                Show(lbl, "Current and new password are required.", false);
                return;
            }
            if (!string.Equals(newPwd, confirm, StringComparison.Ordinal))
            {
                Show(lbl, "New password and confirm password do not match.", false);
                return;
            }

            try
            {
                string currentHash = null;
                using (var con = Db.Open())
                using (var cmd = new SqlCommand("SELECT [password] FROM admins WHERE id=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", adminId);
                    var o = cmd.ExecuteScalar();
                    if (o != null && o != DBNull.Value) currentHash = Convert.ToString(o);
                }

                if (currentHash == null || !string.Equals(currentHash, Sha256Hex(oldPwd), StringComparison.OrdinalIgnoreCase))
                {
                    Show(lbl, "Current password is incorrect.", false);
                    return;
                }

                using (var con = Db.Open())
                using (var upd = new SqlCommand(
                    "UPDATE admins SET [password]=@p, updated_at=GETDATE() WHERE id=@id", con))
                {
                    upd.Parameters.AddWithValue("@p", Sha256Hex(newPwd));
                    upd.Parameters.AddWithValue("@id", adminId);
                    upd.ExecuteNonQuery();
                }

                SetText("txtOld", ""); SetText("txtNew", ""); SetText("txtConfirm", "");
                Show(lbl, "Password changed successfully.", true);
            }
            catch
            {
                Show(lbl, "Unable to change password right now.", false);
            }
        }

        // helpers (designer-free)
        private T Find<T>(string id) where T : Control
        {
            var ph = Master.FindControl("MainContent");
            return ph != null ? FindRecursive<T>(ph, id) : null;
        }

        private static T FindRecursive<T>(Control root, string id) where T : Control
        {
            if (root == null) return null;
            var c = root.FindControl(id) as T;
            if (c != null) return c;
            foreach (Control child in root.Controls)
            {
                var f = FindRecursive<T>(child, id);
                if (f != null) return f;
            }
            return null;
        }

        private string GetText(string id)
        {
            var t = Find<TextBox>(id);
            return t != null ? t.Text.Trim() : string.Empty;
        }

        private void SetText(string id, string value)
        {
            var t = Find<TextBox>(id);
            if (t != null) t.Text = value ?? "";
        }

        private void Show(Label lbl, string msg, bool success)
        {
            if (lbl == null) return;
            lbl.Text = Server.HtmlEncode(msg);
            lbl.CssClass = success ? "st-alert st-alert-success" : "st-alert st-alert-danger";
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
    }
}
