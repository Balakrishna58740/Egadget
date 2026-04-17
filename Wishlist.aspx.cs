using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using serena;

namespace serena.Site
{
    public partial class WishlistPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack) BindWishlist();
        }

        private void BindWishlist()
        {
            var wish = GetWishlist();
            var pnlWish = Find<Panel>("pnlWish");
            var pnlEmpty = Find<Panel>("pnlEmpty");
            var rptWish = Find<Repeater>("rptWish");

            if (wish.Count == 0)
            {
                if (pnlWish != null) pnlWish.Visible = false;
                if (pnlEmpty != null) pnlEmpty.Visible = true;
                return;
            }

            var ids = new List<int>(wish);
            var prms = new List<SqlParameter>();
            var inParts = new List<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                string p = "@id" + i;
                inParts.Add(p);
                prms.Add(new SqlParameter(p, ids[i]));
            }

            var sql = @"
SELECT p.id, p.name, p.price, p.stock, p.image, c.name AS category_name
FROM products p
LEFT JOIN categories c ON c.id = p.category_id
WHERE p.is_show = 1 AND p.id IN (" + string.Join(",", inParts.ToArray()) + @")
ORDER BY p.name ASC;";

            var dt = Db.Query(sql, prms.ToArray());

            // remove non-existing items from wishlist session
            var valid = new HashSet<int>();
            foreach (DataRow r in dt.Rows) valid.Add(Convert.ToInt32(r["id"]));
            wish.IntersectWith(valid);
            SaveWishlist(wish);

            if (dt.Rows.Count == 0)
            {
                if (pnlWish != null) pnlWish.Visible = false;
                if (pnlEmpty != null) pnlEmpty.Visible = true;
                return;
            }

            if (rptWish != null)
            {
                rptWish.DataSource = dt;
                rptWish.DataBind();
            }

            if (pnlWish != null) pnlWish.Visible = true;
            if (pnlEmpty != null) pnlEmpty.Visible = false;
        }

        protected void rptWish_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            int productId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out productId) || productId <= 0)
                return;

            var wish = GetWishlist();

            if (e.CommandName == "Remove")
            {
                wish.Remove(productId);
                SaveWishlist(wish);
                BindWishlist();
                return;
            }

            if (e.CommandName == "MoveToCart")
            {
                if (wish.Contains(productId))
                {
                    var cart = GetCart();
                    if (cart.ContainsKey(productId)) cart[productId] += 1;
                    else cart[productId] = 1;

                    wish.Remove(productId);
                    SaveCart(cart);
                    SaveWishlist(wish);
                }

                Response.Redirect(ResolveUrl("~/Cart.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            SaveWishlist(new HashSet<int>());
            BindWishlist();
        }

        private Dictionary<int, int> GetCart()
        {
            const string key = "CART_DICT";
            var dict = Session[key] as Dictionary<int, int>;
            if (dict != null) return dict;

            var fromHash = Session[key] as Hashtable;
            if (fromHash != null)
            {
                dict = new Dictionary<int, int>();
                foreach (DictionaryEntry de in fromHash)
                {
                    try { dict[Convert.ToInt32(de.Key)] = Convert.ToInt32(de.Value); } catch { }
                }
                Session[key] = dict;
                return dict;
            }

            dict = new Dictionary<int, int>();
            Session[key] = dict;
            return dict;
        }

        private void SaveCart(Dictionary<int, int> cart)
        {
            Session["CART_DICT"] = cart;
        }

        private HashSet<int> GetWishlist()
        {
            const string key = "WISHLIST_SET";
            var set = Session[key] as HashSet<int>;
            if (set != null) return set;

            var fromList = Session[key] as List<int>;
            if (fromList != null)
            {
                set = new HashSet<int>(fromList);
                Session[key] = set;
                return set;
            }

            set = new HashSet<int>();
            Session[key] = set;
            return set;
        }

        private void SaveWishlist(HashSet<int> set)
        {
            Session["WISHLIST_SET"] = set;
        }

        protected string GetProductImageUrl(object dbValue, object nameObj = null)
        {
            return ProductImageResolver.Resolve(dbValue == DBNull.Value ? null : Convert.ToString(dbValue), Convert.ToString(nameObj));
        }

        protected string GetProductUrl(object nameObj)
        {
            string name = Convert.ToString(nameObj) ?? "";
            return ResolveUrl("~/product/" + Slugify(name));
        }

        protected string Html(object o)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(o) ?? "");
        }

        private static string Slugify(string s)
        {
            var input = (s ?? "").Trim().ToLowerInvariant();
            var sb = new StringBuilder(input.Length);
            bool lastDash = false;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    lastDash = false;
                }
                else
                {
                    if (!lastDash)
                    {
                        sb.Append('-');
                        lastDash = true;
                    }
                }
            }
            string res = sb.ToString().Trim('-');
            return string.IsNullOrEmpty(res) ? "product" : res;
        }

        private T Find<T>(string id) where T : Control
        {
            var root = Master != null ? Master.FindControl("MainContent") : null;
            return root != null ? FindRecursive<T>(root, id) : null;
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
    }
}
