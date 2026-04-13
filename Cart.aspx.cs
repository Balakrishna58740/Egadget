using System;
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
    public partial class CartPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
                BindCart();
        }

        private void BindCart()
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                pnlCart.Visible = false;
                pnlEmpty.Visible = true;
                return;
            }

            // Build IN (@id0,@id1,...) for the products in the cart
            var ids = new List<int>(cart.Keys);
            var prms = new List<SqlParameter>();
            var inParts = new List<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                string p = "@id" + i;
                inParts.Add(p);
                prms.Add(new SqlParameter(p, ids[i]));
            }

            var sql = @"
SELECT p.id, p.name, p.price, p.stock, p.image
FROM products p
WHERE p.is_show = 1 AND p.id IN (" + string.Join(",", inParts.ToArray()) + @")
ORDER BY p.name ASC;";

            var dt = Db.Query(sql, prms.ToArray());

            // Merge quantities and compute line subtotals
            dt.Columns.Add("qty", typeof(int));
            dt.Columns.Add("subtotal", typeof(decimal));

            int totalItems = 0;
            decimal subtotal = 0m;

            var toRemove = new List<int>();

            foreach (DataRow r in dt.Rows)
            {
                int id = Convert.ToInt32(r["id"]);
                int qty = cart.ContainsKey(id) ? cart[id] : 0;

                // clamp qty to stock if needed
                int stock = Convert.ToInt32(r["stock"]);
                if (stock <= 0)
                {
                    qty = 0;
                }
                else if (qty > stock)
                {
                    qty = stock;
                    cart[id] = qty; // normalize stored qty
                }

                if (qty <= 0)
                {
                    toRemove.Add(id);
                    continue;
                }

                r["qty"] = qty;

                decimal price = Convert.ToDecimal(r["price"]);
                decimal line = price * qty;
                r["subtotal"] = line;

                totalItems += qty;
                subtotal += line;
            }

            // Remove any invalid/zero qty items
            foreach (var rid in toRemove)
                cart.Remove(rid);
            SaveCart(cart);

            // If after cleanup cart is empty
            if (cart.Count == 0 || dt.Rows.Count == 0)
            {
                pnlCart.Visible = false;
                pnlEmpty.Visible = true;
                return;
            }

            rptCart.DataSource = dt;
            rptCart.DataBind();

            litItemCount.Text = totalItems.ToString();
            litSubtotal.Text = subtotal.ToString("N2");
            litSubtotal2.Text = subtotal.ToString("N2");

            pnlCart.Visible = true;
            pnlEmpty.Visible = false;
        }

        protected void rptCart_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);
            var cart = GetCart();

            // read current qty and stock from row controls
            var txtQty = (TextBox)e.Item.FindControl("txtQty");
            var hfStock = (HiddenField)e.Item.FindControl("hfStock");
            int qty = 1, stock = 0;

            int.TryParse(txtQty.Text, out qty);
            int.TryParse(hfStock.Value, out stock);

            if (!cart.ContainsKey(productId))
                cart[productId] = 0;

            if (e.CommandName == "Inc")
            {
                if (stock <= 0) cart[productId] = 0;
                else cart[productId] = Math.Min(stock, qty + 1);
            }
            else if (e.CommandName == "Dec")
            {
                cart[productId] = Math.Max(1, qty - 1);
            }
            else if (e.CommandName == "Remove")
            {
                cart.Remove(productId);
            }

            SaveCart(cart);
            BindCart();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            Session["CART_DICT"] = new Dictionary<int, int>();
            BindCart();
        }

        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Account/Checkout.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        // -------- Session cart helpers --------
        private Dictionary<int, int> GetCart()
        {
            const string key = "CART_DICT";
            object raw = Session[key];
            if (raw == null)
            {
                var dict = new Dictionary<int, int>();
                Session[key] = dict;
                return dict;
            }

            if (raw is Dictionary<int, int>)
                return (Dictionary<int, int>)raw;

            if (raw is System.Collections.Hashtable)
            {
                var dict = new Dictionary<int, int>();
                foreach (System.Collections.DictionaryEntry de in (System.Collections.Hashtable)raw)
                {
                    try { dict[Convert.ToInt32(de.Key)] = Convert.ToInt32(de.Value); } catch { }
                }
                Session[key] = dict;
                return dict;
            }

            // fallback: clear corrupted session
            var fresh = new Dictionary<int, int>();
            Session[key] = fresh;
            return fresh;
        }

        private void SaveCart(Dictionary<int, int> cart)
        {
            Session["CART_DICT"] = cart;
        }

        // -------- UI helpers --------
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

        private string Slugify(string s)
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
    }
}
