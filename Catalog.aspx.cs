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
    public partial class CatalogPage : Page
    {
        protected int? SelectedCategoryId
        {
            get { return ViewState["SelectedCategoryId"] as int?; }
            set { ViewState["SelectedCategoryId"] = value; }
        }

        protected string CurrentSearch
        {
            get { return ViewState["CurrentSearch"] as string ?? string.Empty; }
            set { ViewState["CurrentSearch"] = value ?? string.Empty; }
        }

        protected string CurrentCategoryFilter
        {
            get { return ViewState["CurrentCategoryFilter"] as string ?? string.Empty; }
            set { ViewState["CurrentCategoryFilter"] = value ?? string.Empty; }
        }

        protected string CurrentSort
        {
            get { return ViewState["CurrentSort"] as string ?? "name_asc"; }
            set { ViewState["CurrentSort"] = string.IsNullOrWhiteSpace(value) ? "name_asc" : value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                try
                {
                    BindCategories();
                    LoadFiltersFromQuery();
                    BindProducts();
                }
                catch (Exception ex)
                {
                    lvProducts.DataSource = new DataTable();
                    lvProducts.DataBind();

                    var litPager = Find<Literal>("litPager");
                    if (litPager != null) litPager.Text = "<div class='text-red-500 text-xs'>" + Server.HtmlEncode(ex.Message) + "</div>";
                }
            }
        }

        private void LoadFiltersFromQuery()
        {
            int catId;
            if (int.TryParse(Request.QueryString["cat"], out catId))
            {
                SelectedCategoryId = catId;
                CurrentCategoryFilter = Db.Scalar<string>("SELECT TOP 1 name FROM categories WHERE id=@id", Db.P("@id", catId)) ?? string.Empty;
            }
            else
            {
                string catName = Request.QueryString["cat"];
                if (!string.IsNullOrEmpty(catName))
                {
                    object idObj = Db.Scalar<object>("SELECT id FROM categories WHERE name LIKE @catName", new SqlParameter("@catName", "%" + catName + "%"));
                    if (idObj != null && idObj != DBNull.Value)
                    {
                        SelectedCategoryId = Convert.ToInt32(idObj);
                        CurrentCategoryFilter = Convert.ToString(Db.Scalar<object>("SELECT name FROM categories WHERE id=@id", Db.P("@id", SelectedCategoryId.Value))) ?? catName;
                    }
                    else
                    {
                        CurrentCategoryFilter = catName;
                    }
                }
            }

            CurrentSearch = (Request.QueryString["q"] ?? string.Empty).Trim();
            CurrentSort = NormalizeSort(Request.QueryString["sort"]);

            txtSearch.Text = CurrentSearch;
            txtPriceMin.Text = Request.QueryString["min"] ?? string.Empty;
            txtPriceMax.Text = Request.QueryString["max"] ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(CurrentCategoryFilter))
            {
                var item = rblCategories.Items.FindByValue(CurrentCategoryFilter);
                if (item != null) item.Selected = true;
            }

            var ddlSort = Find<DropDownList>("ddlSort");
            if (ddlSort != null)
            {
                var sortItem = ddlSort.Items.FindByValue(CurrentSort);
                if (sortItem != null) ddlSort.SelectedValue = CurrentSort;
            }
        }

        protected string GetProductUrl(object nameObj)
        {
            var name = Convert.ToString(nameObj) ?? "";
            var slug = Slugify(name);
            return ResolveUrl("~/product/" + slug);
        }
        private string Slugify(string s)
        {
            // lower-case, keep letters/numbers, turn spaces to '-', collapse repeats
            var sb = new StringBuilder(s.Trim().ToLowerInvariant());
            for (int i = 0; i < sb.Length; i++)
            {
                char c = sb[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) continue;
                if (c == ' ' || c == '_' || c == '-') sb[i] = '-';
                else sb[i] = '-';
            }
            // collapse multiple '-'
            var t = new StringBuilder();
            bool dash = false;
            for (int i = 0; i < sb.Length; i++)
            {
                char c = sb[i];
                if (c == '-')
                {
                    if (!dash) { t.Append('-'); dash = true; }
                }
                else
                {
                    t.Append(c); dash = false;
                }
            }
            // trim leading/trailing '-'
            string res = t.ToString().Trim('-');
            return string.IsNullOrEmpty(res) ? "product" : res;
        }

        private void BindCategories()
        {
            var dt = Db.Query(@"
SELECT id, name
FROM categories
ORDER BY name ASC;");

            rblCategories.Items.Clear();
            rblCategories.Items.Add(new ListItem("All", ""));
            foreach (DataRow r in dt.Rows)
            {
                rblCategories.Items.Add(new ListItem(
                    HttpUtility.HtmlEncode(Convert.ToString(r["name"])),
                    Convert.ToString(r["id"])
                ));
            }
        }

        private void BindProducts()
        {
            int page = GetCurrentPage();
            int pageSize = 12;

            string where = " WHERE p.is_show = 1";
            var parameters = new List<SqlParameter>();

            if (SelectedCategoryId.HasValue)
            {
                where += " AND p.category_id = @cat";
                parameters.Add(new SqlParameter("@cat", SelectedCategoryId.Value));
            }

            if (!string.IsNullOrWhiteSpace(CurrentSearch))
            {
                where += " AND (p.name LIKE @q)";
                parameters.Add(new SqlParameter("@q", "%" + CurrentSearch + "%"));
            }

            decimal minPrice, maxPrice;
            if (decimal.TryParse(txtPriceMin.Text, out minPrice))
            {
                where += " AND p.price >= @min";
                parameters.Add(new SqlParameter("@min", minPrice));
            }
            if (decimal.TryParse(txtPriceMax.Text, out maxPrice))
            {
                where += " AND p.price <= @max";
                parameters.Add(new SqlParameter("@max", maxPrice));
            }

            int total = Db.Scalar<int>("SELECT COUNT(*) FROM products p" + where, parameters.ToArray());
            int pageCount = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            if (page > pageCount) page = pageCount;
            int offset = (page - 1) * pageSize;

            var dataParams = new List<SqlParameter>(parameters);
            dataParams.Add(Db.P("@offset", offset));
            dataParams.Add(Db.P("@limit", pageSize));

            var sql = new StringBuilder(@"
SELECT p.id, p.name, p.image, p.price, p.stock, c.name AS category_name
FROM products p
LEFT JOIN categories c ON c.id = p.category_id");
            sql.Append(where);
            sql.Append(" ORDER BY ").Append(BuildOrderBy(CurrentSort));
            sql.Append(@"
 OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;");

            var dt = Db.Query(sql.ToString(), dataParams.ToArray());

            lvProducts.DataSource = dt;
            lvProducts.DataBind();

            var litPager = Find<Literal>("litPager");
            if (litPager != null) litPager.Text = "<div class='w-full text-center text-[10px] text-gray-300 mb-4'>total=" + total + " pageCount=" + pageCount + "</div>" + BuildPager(page, pageCount);
        }

        // Build URL with current filters (cat,q,min,max,page)
        private string BuildUrl(int page)
        {
            var qs = HttpUtility.ParseQueryString(string.Empty);

            if (SelectedCategoryId.HasValue)
                qs["cat"] = string.IsNullOrWhiteSpace(CurrentCategoryFilter) ? SelectedCategoryId.Value.ToString() : CurrentCategoryFilter;
            else if (!string.IsNullOrWhiteSpace(CurrentCategoryFilter))
                qs["cat"] = CurrentCategoryFilter;

            if (!string.IsNullOrWhiteSpace(CurrentSearch))
                qs["q"] = CurrentSearch;

            decimal v;
            if (decimal.TryParse(txtPriceMin.Text, out v)) qs["min"] = v.ToString();
            if (decimal.TryParse(txtPriceMax.Text, out v)) qs["max"] = v.ToString();

            if (!string.IsNullOrWhiteSpace(CurrentSort))
                qs["sort"] = CurrentSort;

            qs["page"] = page.ToString();

            return "Catalog.aspx?" + qs.ToString();
        }

        protected void FilterControl_Changed(object sender, EventArgs e)
        {
            ApplyFiltersAndRedirect();
        }

        protected void btnApply_Click(object sender, EventArgs e)
        {
            ApplyFiltersAndRedirect();
        }

        private void ApplyFiltersAndRedirect()
        {
            CurrentCategoryFilter = rblCategories.SelectedValue;
            SelectedCategoryId = null;
            CurrentSearch = (txtSearch.Text ?? "").Trim();
            var ddlSort = Find<DropDownList>("ddlSort");
            CurrentSort = NormalizeSort(ddlSort != null ? ddlSort.SelectedValue : CurrentSort);

            Response.Redirect(BuildUrl(1), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            SelectedCategoryId = null;
            CurrentSearch = string.Empty;
            CurrentSort = "name_asc";
            Response.Redirect("Catalog.aspx?page=1&sort=name_asc", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void lvProducts_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);

            TextBox txtQty = (TextBox)e.Item.FindControl("txtQty");
            int qty = 1;
            int.TryParse(txtQty.Text, out qty);

            if (e.CommandName == "Increment")
            {
                txtQty.Text = (qty + 1).ToString();
            }
            else if (e.CommandName == "Decrement")
            {
                if (qty > 1) txtQty.Text = (qty - 1).ToString();
            }
            else if (e.CommandName == "AddToCart")
            {
                AddToCart(productId, qty);
                Response.Redirect(ResolveUrl("~/Cart.aspx"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void AddToCart(int productId, int qty)
        {
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

        protected string GetProductImageUrl(object dbValue, object nameObj = null)
        {
            return ProductImageResolver.Resolve(dbValue == DBNull.Value ? null : Convert.ToString(dbValue), Convert.ToString(nameObj));
        }

        private int GetCurrentPage()
        {
            int page;
            if (!int.TryParse(Request.QueryString["page"], out page) || page < 1)
                page = 1;
            return page;
        }

        private static string NormalizeSort(string sort)
        {
            switch ((sort ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "price_asc":
                case "price_desc":
                case "newest":
                case "name_asc":
                    return sort;
                default:
                    return "name_asc";
            }
        }

        private static string BuildOrderBy(string sort)
        {
            switch (NormalizeSort(sort))
            {
                case "price_asc":
                    return "p.price ASC, p.name ASC";
                case "price_desc":
                    return "p.price DESC, p.name ASC";
                case "newest":
                    return "p.created_at DESC, p.id DESC";
                default:
                    return "p.name ASC";
            }
        }

        private string BuildPager(int page, int pageCount)
        {
            var sb = new StringBuilder();
            sb.Append("<div class='flex items-center gap-2 flex-wrap justify-center'>");

            Func<int, string> url = p => BuildUrl(p);

            bool hasPrev = page > 1;
            sb.Append("<a href='").Append(hasPrev ? url(page - 1) : "#")
              .Append("' class='px-4 py-2 text-[10px] uppercase tracking-widest font-bold border ")
              .Append(hasPrev ? "border-gray-200 text-text-dark hover:bg-primary hover:text-white" : "border-gray-100 text-gray-200 cursor-not-allowed")
              .Append(" transition-all'>Prev</a>");

            const int window = 5;
            int start = Math.Max(1, page - (window / 2));
            int end = Math.Min(pageCount, start + window - 1);
            if (end - start + 1 < window) start = Math.Max(1, end - window + 1);

            for (int p = start; p <= end; p++)
            {
                bool active = p == page;
                sb.Append("<a href='").Append(url(p)).Append("' class='w-10 h-10 flex items-center justify-center text-[10px] font-bold tracking-widest transition-all ")
                  .Append(active ? "bg-primary text-white" : "bg-white border border-gray-200 text-gray-400 hover:text-primary")
                  .Append("'>").Append(p).Append("</a>");
            }

            bool hasNext = page < pageCount;
            sb.Append("<a href='").Append(hasNext ? url(page + 1) : "#")
              .Append("' class='px-4 py-2 text-[10px] uppercase tracking-widest font-bold border ")
              .Append(hasNext ? "border-gray-200 text-text-dark hover:bg-primary hover:text-white" : "border-gray-100 text-gray-200 cursor-not-allowed")
              .Append(" transition-all'>Next</a>");

            sb.Append("</div>");
            return sb.ToString();
        }

        protected string Html(object o)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(o) ?? "");
        }
    }
}
