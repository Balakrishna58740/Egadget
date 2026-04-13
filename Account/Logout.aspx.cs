using System;
using System.Web;
using System.Web.UI;

namespace serena.Site.Account
{
    public partial class LogoutPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            bool keepCart = string.Equals(Request.QueryString["keepCart"], "1", StringComparison.Ordinal);

            // Revoke member persistent token (best-effort)
            try
            {
                int memberId = 0;
                if (Session != null && Session["MEMBER_ID"] != null)
                {
                    int.TryParse(Session["MEMBER_ID"].ToString(), out memberId);
                }
                if (memberId > 0)
                {
                    Db.Execute(@"UPDATE dbo.members
                                 SET persistent_token = NULL, token_expires = NULL
                                 WHERE id = @id",
                        Db.P("@id", memberId));
                }
            }
            catch { /* ignore */ }

            // Expire the cookie
            var ck = new HttpCookie("MemberToken", "");
            ck.HttpOnly = true;
            ck.Expires = DateTime.UtcNow.AddDays(-1);
            Response.Cookies.Add(ck);

            // Session clearing
            if (keepCart)
            {
                Session.Remove("MEMBER_ID");
                Session.Remove("USER_USERNAME");
                Session.Remove("USER_NAME");
                Session.Remove("USER_EMAIL");
            }
            else
            {
                Session.Clear();
                Session.Abandon();
            }

            // Redirect back safely
            string returnUrl = Request.QueryString["returnUrl"];
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/", StringComparison.Ordinal))
            {
                Response.Redirect(returnUrl, true);
                return;
            }

            Response.Redirect(ResolveUrl("~/"), true);
        }
    }
}
