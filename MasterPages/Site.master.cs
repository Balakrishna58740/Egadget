using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace serena
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) ApplyActiveNav();
            ShowMemberOrGuest();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            SetCartCount();
            SetWishlistCount();
            ApplyNoCache();
        }

        private void ApplyActiveNav()
        {
            string path = VirtualPathUtility.ToAppRelative(Request.AppRelativeCurrentExecutionFilePath).ToLowerInvariant();
            SetActive("navHome", path.EndsWith("/default.aspx"));
            SetActive("navShop", path.EndsWith("/catalog.aspx"));
            SetActive("navAbout", path.EndsWith("/about.aspx"));
            SetActive("navContact", path.EndsWith("/contact.aspx"));
            SetActive("navFeedback", path.EndsWith("/feedback.aspx"));
        }

        private void SetActive(string anchorId, bool active)
        {
            var a = FindControl(anchorId) as HtmlAnchor;
            if (a == null) return;

            string cls = a.Attributes["class"] ?? "";
            bool has = cls.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0;

            if (active && !has)
            {
                a.Attributes["class"] = (cls + " active").Trim();
                a.Attributes["aria-current"] = "page";
            }
            else if (!active && has)
            {
                a.Attributes["class"] = (" " + cls + " ").Replace(" active ", " ").Trim();
                a.Attributes.Remove("aria-current");
            }
        }

        private void ShowMemberOrGuest()
        {
            bool isMember = (Session["MEMBER_ID"] != null);

            var lnkLogin = FindControl("lnkLogin") as HtmlAnchor;
            var lnkRegister = FindControl("lnkRegister") as HtmlAnchor;
            var lnkProfile = FindControl("lnkProfile") as HtmlAnchor;
            var btnLogOut = FindControl("btnLogOut") as LinkButton;
            var lnkNotifications = FindControl("lnkNotifications") as HtmlAnchor;
            var lnkMobileNotifications = FindControl("lnkMobileNotifications") as HtmlAnchor;
            var lnkMobileProfile = FindControl("lnkMobileProfile") as HtmlAnchor;
            var lnkMobileLogin = FindControl("lnkMobileLogin") as HtmlAnchor;
            var lnkWishlist = FindControl("lnkWishlist") as HtmlAnchor;
            var lnkMobileWishlist = FindControl("lnkMobileWishlist") as HtmlAnchor;
            var notifBadge = FindControl("notifBadge") as HtmlGenericControl;
            var litNotifCount = FindControl("litNotifCount") as Literal;

            if (lnkLogin != null) lnkLogin.Visible = !isMember;
            if (lnkRegister != null) lnkRegister.Visible = !isMember;
            if (lnkProfile != null) lnkProfile.Visible = isMember;
            if (btnLogOut != null) btnLogOut.Visible = isMember;
            if (lnkMobileProfile != null) lnkMobileProfile.Visible = isMember;
            if (lnkMobileLogin != null) lnkMobileLogin.Visible = !isMember;

            if (lnkNotifications != null)
            {
                lnkNotifications.Visible = true;
                lnkNotifications.HRef = isMember
                    ? ResolveUrl("~/Account/Notifications.aspx")
                    : ResolveUrl("~/Account/Login.aspx?returnUrl=" + HttpUtility.UrlEncode("/Account/Notifications.aspx"));
            }

            if (lnkWishlist != null)
            {
                lnkWishlist.Visible = true;
                lnkWishlist.HRef = ResolveUrl("~/Wishlist.aspx");
            }

            if (lnkMobileWishlist != null)
            {
                lnkMobileWishlist.Visible = true;
                lnkMobileWishlist.HRef = ResolveUrl("~/Wishlist.aspx");
            }

            if (lnkMobileNotifications != null)
            {
                lnkMobileNotifications.Visible = true;
                lnkMobileNotifications.HRef = isMember
                    ? ResolveUrl("~/Account/Notifications.aspx")
                    : ResolveUrl("~/Account/Login.aspx?returnUrl=" + HttpUtility.UrlEncode("/Account/Notifications.aspx"));
            }

            if (isMember && litNotifCount != null)
            {
                int memberId;
                if (int.TryParse(Convert.ToString(Session["MEMBER_ID"]), out memberId) && memberId > 0)
                {
                    int count = 0;
                    try
                    {
                        count = Db.Scalar<int>("SELECT COUNT(*) FROM dbo.notifications WHERE recipient_member_id=@mid AND is_read=0", Db.P("@mid", memberId));
                    }
                    catch { }

                    litNotifCount.Text = count.ToString();
                    if (notifBadge != null) notifBadge.Visible = count > 0;
                }
                else
                {
                    litNotifCount.Text = "0";
                    if (notifBadge != null) notifBadge.Visible = false;
                }
            }
            else
            {
                if (litNotifCount != null) litNotifCount.Text = "0";
                if (notifBadge != null) notifBadge.Visible = false;
            }
        }

        protected void LogOut_Click(object sender, EventArgs e)
        {
            Session.Abandon();
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddMonths(-20);
            }
            if (Request.Cookies["MemberToken"] != null)
            {
                Response.Cookies["MemberToken"].Value = string.Empty;
                Response.Cookies["MemberToken"].Expires = DateTime.Now.AddMonths(-20);
            }
            Response.Redirect("~/Default.aspx");
        }

        private void SetCartCount()
        {
            int qty = 0;
            var dict = Session["CART_DICT"] as Dictionary<int, int>;
            if (dict != null)
            {
                foreach (var kv in dict) qty += kv.Value;
            }
            else
            {
                var ht = Session["CART_DICT"] as Hashtable;
                if (ht != null)
                {
                    foreach (DictionaryEntry de in ht)
                        qty += Convert.ToInt32(de.Value);
                }
            }

            Session["CartQty"] = qty;
            var badge = FindControl("cartBadge") as HtmlGenericControl;
            var lit = FindControl("litCartCount") as Literal;

            if (badge != null) badge.Visible = qty > 0;
            if (lit != null) lit.Text = qty.ToString();
        }

        private void SetWishlistCount()
        {
            int count = 0;
            var set = Session["WISHLIST_SET"] as HashSet<int>;
            if (set != null) count = set.Count;

            var badge = FindControl("wishBadge") as HtmlGenericControl;
            var lit = FindControl("litWishCount") as Literal;
            if (badge != null) badge.Visible = count > 0;
            if (lit != null) lit.Text = count.ToString();
        }

        private void ApplyNoCache()
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");
            Response.Headers["Pragma"] = "no-cache";
        }
    }
}
