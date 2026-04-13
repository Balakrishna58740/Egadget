using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using serena.BLL;
using serena.Entities;
using serena;

namespace serena.Site
{
    public partial class HomePage : Page
    {
        private ProductBLL _productBll = new ProductBLL();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // --- handle add-to-cart via querystring: /Default.aspx?add=15&qty=1 ---
            int addId;
            if (int.TryParse(Request.QueryString["add"], out addId) && addId > 0)
            {
                int qty = 1;
                int tmp;
                if (int.TryParse(Request.QueryString["qty"], out tmp) && tmp > 0) qty = tmp;

                AddToCart(addId, qty);

                // redirect to cart so the added item is visible immediately
                Response.Redirect(Abs("~/Cart.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                BindNewArrivals();
                BindTopPicks();
            }
        }

        // ---------- Sections ----------
        private void BindNewArrivals()
        {
            var products = _productBll.GetNewArrivals(8);
            var litNew = Find<Literal>("litNew");
            if (litNew != null) litNew.Text = BuildProductGrid(products);
        }

        private void BindTopPicks()
        {
            var products = _productBll.GetTopPicks(8);
            var litTop = Find<Literal>("litTop");
            if (litTop != null) litTop.Text = BuildProductGrid(products);
        }

        // ---------- Helpers ----------
        private string BuildProductGrid(List<Product> products)
        {
            var sb = new StringBuilder();

            if (products == null || products.Count == 0)
                return "<div class='bg-off-white p-8 text-center text-gray-400'>No products found.</div>";

            sb.Append("<div class='grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8'>");
            foreach (var p in products)
            {
                int id = p.Id;
                string name = Html(p.Name);
                string cat = Html(p.CategoryName);
                decimal price = p.Price;
                int stock = p.Stock;
                string imgUrl = GetProductImageUrl(p.Image, p.Name);

                sb.Append("<div class='group'>");
                // Image Container
                sb.Append("  <div class='relative aspect-[3/4] overflow-hidden bg-off-white mb-6'>");
                sb.Append("    <img class='w-full h-full object-cover transition-transform duration-700 group-hover:scale-110' src='")
                  .Append(Html(imgUrl)).Append("' alt='").Append(name)
                  .Append("' />");

                // Hover Actions
                sb.Append("    <div class='absolute inset-0 bg-black/5 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center gap-2'>");
                sb.Append("      <a class='bg-white text-text-dark w-12 h-12 flex items-center justify-center rounded-full hover:bg-primary hover:text-white transition-all transform translate-y-4 group-hover:translate-y-0 duration-500' href='").Append(Abs("~/Product.aspx?id=" + id)).Append("' title='Quick View'><i class='fa-regular fa-eye'></i></a>");
                sb.Append("      <a class='bg-white text-text-dark w-12 h-12 flex items-center justify-center rounded-full hover:bg-primary hover:text-white transition-all transform translate-y-4 group-hover:translate-y-0 duration-500 delay-75' href='").Append(Abs("~/Default.aspx?add=" + id + "&qty=1")).Append("'")
                  .Append(stock > 0 ? "" : " onclick='return false;' class='opacity-50 cursor-not-allowed'")
                  .Append(" title='Add to Cart'><i class='fa-solid fa-cart-shopping'></i></a>");
                sb.Append("    </div>");

                if (stock <= 0)
                    sb.Append("    <div class='absolute top-4 left-4 bg-text-dark text-white text-[10px] uppercase tracking-widest px-3 py-1'>Sold Out</div>");
                sb.Append("  </div>");

                // Info
                sb.Append("  <div class='text-center'>");
                sb.Append("    <div class='text-[10px] uppercase tracking-[0.2em] text-gray-400 mb-2'>").Append(string.IsNullOrEmpty(cat) ? "&nbsp;" : cat).Append("</div>");
                sb.Append("    <h3 class='font-serif text-lg mb-2'><a href='").Append(Abs("~/Product.aspx?id=" + id)).Append("' class='hover:text-primary transition-colors'>").Append(name).Append("</a></h3>");
                sb.Append("    <div class='font-medium text-primary tracking-wide'>RS ").Append(price.ToString("N2")).Append("</div>");
                sb.Append("  </div>");

                sb.Append("</div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        // --- Cart helpers (session-based) ---
        private void AddToCart(int productId, int qty)
        {
            if (qty < 1) qty = 1;

            // Optional: Business rule check via BLL
            if (!_productBll.IsInStock(productId, qty))
            {
                // In a real app, you might show a message. Here we just return or add whatever is available.
            }

            var cart = GetCart();
            if (cart.ContainsKey(productId))
                cart[productId] += qty;
            else
                cart[productId] = qty;
            SaveCart(cart);
        }

        private Dictionary<int, int> GetCart()
        {
            const string key = "CART_DICT";
            if (Session[key] == null)
                Session[key] = new Dictionary<int, int>();
            return (Dictionary<int, int>)Session[key];
        }

        private void SaveCart(Dictionary<int, int> cart)
        {
            Session["CART_DICT"] = cart;
        }

        private string GetProductImageUrl(string dbValue, string seed = null)
        {
            return ProductImageResolver.Resolve(dbValue, seed);
        }

        private static string Html(object o)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(o) ?? "");
        }

        private string Abs(string virtualPath)
        {
            return VirtualPathUtility.ToAbsolute(virtualPath);
        }

        private T Find<T>(string id) where T : Control
        {
            var root = Master != null ? Master.FindControl("MainContent") : null;
            return root != null ? FindRecursive<T>(root, id) : null;
        }

        private static T FindRecursive<T>(Control root, string id) where T : Control
        {
            if (root == null) return null;
            var found = root.FindControl(id) as T;
            if (found != null) return found;
            foreach (Control child in root.Controls)
            {
                var nested = FindRecursive<T>(child, id);
                if (nested != null) return nested;
            }
            return null;
        }
    }
}

