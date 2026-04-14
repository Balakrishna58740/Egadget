using System;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace serena
{
    public partial class AdminMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Context == null || Context.User == null || !Context.User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/Admin/Login.aspx", true);
                return;
            }

            SetDisplayName();
            SetAdminNotificationCount();
            ApplyActiveNav();
        }

        private void SetDisplayName()
        {
            var lit = FindControl("litDisplayName") as Literal;
            if (lit == null) return;

            string display = Session["AdminFullName"] as string;
            if (string.IsNullOrEmpty(display))
            {
                string username = Context.User.Identity.Name ?? "";
                using (var con = Db.Open())
                using (var cmd = new SqlCommand("SELECT TOP 1 full_name FROM admins WHERE username=@u", con))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    var o = cmd.ExecuteScalar();
                    display = (o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(Convert.ToString(o)))
                              ? username        // fallback so it's never blank
                              : Convert.ToString(o);
                }
                Session["AdminFullName"] = display; // cache for this session
            }

            lit.Text = Server.HtmlEncode(display);
        }

        private void SetAdminNotificationCount()
        {
            var lit = FindControl("litAdminNotifCount") as Literal;
            var link = FindControl("lnkAdminNotifications") as HyperLink;
            if (lit == null) return;

            try
            {
                int count = Db.Scalar<int>("SELECT COUNT(*) FROM dbo.notifications WHERE is_admin=1 AND is_read=0");
                lit.Text = count.ToString();
                if (link != null)
                {
                    link.Visible = true;
                    if (count <= 0) link.Attributes["class"] = (link.Attributes["class"] ?? "") + " opacity-70";
                }
            }
            catch
            {
                lit.Text = "0";
            }
        }

        private void ApplyActiveNav()
        {
            string path = VirtualPathUtility.ToAppRelative(Request.AppRelativeCurrentExecutionFilePath).ToLowerInvariant();
            SetActive("navDashboard", path.EndsWith("/admin/dashboard.aspx"));
            SetActive("navCategories", path.EndsWith("/admin/categories.aspx"));
            SetActive("navProducts", path.EndsWith("/admin/products.aspx"));
            bool isOrders = path.EndsWith("/admin/orders.aspx") || path.EndsWith("/admin/orderview.aspx");
            SetActive("navOrders", isOrders);
            SetActive("navPaymentMethods", path.EndsWith("/admin/paymentmethods.aspx"));
            SetActive("navFeedbacks", path.EndsWith("/admin/feedbacks.aspx"));
            SetActive("navReports", path.EndsWith("/admin/reports.aspx"));
            bool isProfile = path.EndsWith("/admin/profile.aspx") || path.EndsWith("/admin/adminprofile.aspx");
            SetActive("navProfile", isProfile);
        }

        private void SetActive(string anchorId, bool active)
        {
            var a = FindControl(anchorId) as HtmlAnchor;
            if (a == null) return;
            string cls = a.Attributes["class"] ?? "";
            bool has = cls.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0;
            if (active && !has) { a.Attributes["class"] = (cls + " active").Trim(); a.Attributes["aria-current"] = "page"; }
            if (!active && has) { a.Attributes["class"] = (" " + cls + " ").Replace(" active ", " ").Trim(); a.Attributes.Remove("aria-current"); }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            var auth = new HttpCookie(FormsAuthentication.FormsCookieName, "") { Expires = DateTime.UtcNow.AddDays(-1) };
            Response.Cookies.Add(auth);
            var sess = new HttpCookie("ASP.NET_SessionId", "") { Expires = DateTime.UtcNow.AddDays(-1) };
            Response.Cookies.Add(sess);
            Response.Redirect("~/Admin/Login.aspx");
        }
    }
}
