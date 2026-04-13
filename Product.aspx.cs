using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using serena.BLL;
using serena.Entities;
using serena;

namespace serena.Site
{
    public partial class ProductPage : Page
    {
        private ProductBLL _productBll = new ProductBLL();

        private int ProductId
        {
            get { return ViewState["ProductId"] == null ? 0 : (int)ViewState["ProductId"]; }
            set { ViewState["ProductId"] = value; }
        }

        private int CurrentStock
        {
            get { return ViewState["CurrentStock"] == null ? 0 : (int)ViewState["CurrentStock"]; }
            set { ViewState["CurrentStock"] = value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                // Prefer route slug
                string slug = Convert.ToString(Page.RouteData.Values["slug"]);
                if (!string.IsNullOrEmpty(slug))
                {
                    LoadProductBySlug(slug);
                    return;
                }

                // (optional) fallback: id querystring
                int id;
                if (int.TryParse(Request.QueryString["id"], out id))
                {
                    LoadProduct(id);
                    return;
                }

                ShowNotFound();
            }
        }

        private void LoadProductBySlug(string slug)
        {
            var p = _productBll.GetProductBySlug(slug);
            if (p == null)
            {
                ShowNotFound();
                return;
            }
            BindProduct(p);
        }

        private void LoadProduct(int id)
        {
            var p = _productBll.GetProduct(id);
            if (p == null)
            {
                ShowNotFound();
                return;
            }
            BindProduct(p);
        }

        private void BindProduct(Product p)
        {
            ProductId = p.Id;
            CurrentStock = p.Stock;

            // Core fields
            string name = Html(p.Name);
            string category = Html(p.CategoryName);
            decimal price = p.Price;
            string desc = Html(p.Description);
            string imgUrl = GetProductImageUrl(p.Image, p.Name);

            // Bind to UI
            litTitle.Text = name;
            litBreadcrumb.Text = name;
            litName.Text = name;
            litCategory.Text = string.IsNullOrEmpty(category) ? "-" : category;
            litPrice.Text = price.ToString("N2");
            litDesc.Text = desc.Replace("\n", "<br/>");

            imgProduct.Src = imgUrl;
            imgProduct.Alt = name;

            // Stock
            var badge = (System.Web.UI.HtmlControls.HtmlGenericControl)FindControl("badgeStock");
            if (badge != null)
            {
                badge.InnerText = p.Stock > 0 ? "In stock: " + p.Stock : "Out of stock";
                badge.Attributes["class"] = p.Stock > 0 ? "badge text-bg-success" : "badge text-bg-secondary";
            }

            // Category link back
            lnkBack.NavigateUrl = ResolveUrl("~/Catalog.aspx?page=1");

            // Enable add to cart only if in stock
            btnAdd.Enabled = p.Stock > 0;

            pnlProduct.Visible = true;
            pnlNotFound.Visible = false;
        }

        private void ShowNotFound()
        {
            pnlProduct.Visible = false;
            pnlNotFound.Visible = true;
        }

        // ---------- Cart ----------
        protected void btnMinus_Click(object sender, EventArgs e)
        {
            int q = GetQty();
            if (q > 1) q--;
            txtQty.Text = q.ToString();
        }

        protected void btnPlus_Click(object sender, EventArgs e)
        {
            int q = GetQty();
            if (CurrentStock > 0 && q < CurrentStock) q++;
            else q++;
            txtQty.Text = q.ToString();
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (ProductId <= 0) return;

            int qty = GetQty();
            if (qty < 1) qty = 1;
            if (CurrentStock > 0 && qty > CurrentStock) qty = CurrentStock;

            var cart = GetCart();
            if (cart.ContainsKey(ProductId)) cart[ProductId] += qty;
            else cart[ProductId] = qty;
            SaveCart(cart);

            Response.Redirect(ResolveUrl("~/Cart.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private int GetQty()
        {
            int q;
            if (!int.TryParse(txtQty.Text, out q)) q = 1;
            return q;
        }

        private System.Collections.Generic.Dictionary<int, int> GetCart()
        {
            const string key = "CART_DICT";
            if (Session[key] == null)
                Session[key] = new System.Collections.Generic.Dictionary<int, int>();
            return (System.Collections.Generic.Dictionary<int, int>)Session[key];
        }

        private void SaveCart(System.Collections.Generic.Dictionary<int, int> cart)
        {
            Session["CART_DICT"] = cart;
        }

        // ---------- Helpers ----------
        private string GetProductImageUrl(string dbValue, string name = null)
        {
            return ProductImageResolver.Resolve(dbValue, name);
        }

        private static string Html(object o)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(o) ?? "");
        }
    }
}
