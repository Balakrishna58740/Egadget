using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace serena.Admin
{
    public partial class ProductsPage : Page
    {
        private const int PAGE_SIZE = 10;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                BindCategories();          // form category
                BindFilterCategories();    // filter category

                // use SafeInt directly (no TryGetQueryInt in this class)
                int toggleId = SafeInt(Request.QueryString["toggle"]);
                int delId = SafeInt(Request.QueryString["del"]);
                int editId = SafeInt(Request.QueryString["edit"]);

                if (toggleId > 0) ToggleShow(toggleId);
                if (delId > 0) DeleteProduct(delId);
                if (editId > 0) LoadForEdit(editId);

                // Seed filter controls from query string
                SetText("txtQ", Request.QueryString["q"]);
                SetDDL("ddlFilterCat", Request.QueryString["fcat"] ?? "0");
                SetDDL("ddlFilterShow", Request.QueryString["fshow"] ?? "");
                SetDDL("ddlSort", Request.QueryString["sort"] ?? "name_asc");

                BindTable();
            }

            var txtPrice = Find<TextBox>("txtPrice");
            if (txtPrice != null) txtPrice.Attributes["step"] = "0.01";
        }

        // ---------- Form actions ----------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            var lbl = Find<Label>("lblMsg");
            var hid = Find<HiddenField>("hidId");
            var ddl = Find<DropDownList>("ddlCat");
            var txtName = Find<TextBox>("txtName");
            var txtDesc = Find<TextBox>("txtDesc");
            var txtStock = Find<TextBox>("txtStock");
            var txtPrice = Find<TextBox>("txtPrice");
            var chkShow = Find<CheckBox>("chkShow");
            var fu = Find<FileUpload>("fuImg");

            int categoryId = SafeInt(ddl != null ? ddl.SelectedValue : "0");
            string name = txtName != null ? txtName.Text.Trim() : "";
            string desc = txtDesc != null ? txtDesc.Text.Trim() : ""; // optional
            int stock = SafeInt(txtStock != null ? txtStock.Text.Trim() : "0");
            decimal price = SafeDecimal(txtPrice != null ? txtPrice.Text.Trim() : "0");
            bool isShow = chkShow != null && chkShow.Checked;

            if (categoryId <= 0 || string.IsNullOrWhiteSpace(name))
            {
                Show(lbl, "Please select an archive category and enter a name.", false);
                return;
            }

            int id = 0;
            if (hid != null && !string.IsNullOrEmpty(hid.Value)) int.TryParse(hid.Value, out id);

            try
            {
                string newImagePath = null;
                if (fu != null && fu.HasFile)
                {
                    if (!IsImageExt(Path.GetExtension(fu.FileName)))
                    {
                        Show(lbl, "Archive must be .jpg, .jpeg, .png or .gif", false);
                        return;
                    }
                    string dir = Server.MapPath("~/Assets/images/products");
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    string ext = Path.GetExtension(fu.FileName).ToLowerInvariant();
                    string fileBase = Slugify(name);
                    string file = fileBase + ext;
                    int suffix = 1;
                    while (File.Exists(Path.Combine(dir, file)))
                    {
                        file = fileBase + "-" + suffix.ToString() + ext;
                        suffix++;
                    }
                    fu.SaveAs(Path.Combine(dir, file));
                    newImagePath = "Assets/images/products/" + file; // store relative path
                }

                if (id > 0)
                {
                    string oldImg = Db.Scalar<string>("SELECT image FROM products WHERE id=@id", Db.P("@id", id));

                    string sql = @"UPDATE products
                                   SET category_id=@c, name=@n, description=@d, stock=@s, price=@p,
                                       is_show=@show, updated_at=GETDATE()"
                                   + (newImagePath != null ? ", image=@img" : "")
                                   + " WHERE id=@id";

                    var prms = new List<SqlParameter> {
                        Db.P("@c", categoryId), Db.P("@n", name), Db.P("@d", desc),
                        Db.P("@s", stock), Db.P("@p", price), Db.P("@show", isShow), Db.P("@id", id)
                    };
                    if (newImagePath != null) prms.Add(Db.P("@img", newImagePath));

                    Db.Execute(sql, prms.ToArray());

                    if (newImagePath != null && !string.IsNullOrEmpty(oldImg))
                        TryDeletePhysical("~/" + oldImg);

                    Show(lbl, "Masterpiece archive updated.", true);
                }
                else
                {
                    Db.Execute(@"INSERT INTO products
                                 (category_id, name, description, stock, price, image, is_show, created_at, updated_at)
                                 VALUES (@c, @n, @d, @s, @p, @img, @show, GETDATE(), GETDATE())",
                               Db.P("@c", categoryId), Db.P("@n", name), Db.P("@d", desc),
                               Db.P("@s", stock), Db.P("@p", price),
                               Db.P("@img", (object)newImagePath ?? DBNull.Value),
                               Db.P("@show", isShow));

                    Show(lbl, "New masterpiece added to collection.", true);
                }

                // reset form
                if (hid != null) hid.Value = "";
                if (txtName != null) txtName.Text = "";
                if (txtDesc != null) txtDesc.Text = "";
                if (txtStock != null) txtStock.Text = "";
                if (txtPrice != null) txtPrice.Text = "";
                if (chkShow != null) chkShow.Checked = false;
                if (ddl != null && ddl.Items.Count > 0) ddl.SelectedIndex = 0;
                var hint = Find<Literal>("litImgHint"); if (hint != null) hint.Text = "";

                BindTable();
            }
            catch
            {
                Show(lbl, "System error during archival.", false);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Admin/Products.aspx");
        }

        // ---------- Filters ----------
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            string q = GetText("txtQ");
            string fcat = GetDDL("ddlFilterCat");
            string fshow = GetDDL("ddlFilterShow");
            string sort = GetDDL("ddlSort");

            string url = "~/Admin/Products.aspx?q=" + Server.UrlEncode(q ?? "");
            url += "&fcat=" + Server.UrlEncode(fcat ?? "0");
            url += "&fshow=" + Server.UrlEncode(fshow ?? "");
            url += "&sort=" + Server.UrlEncode(sort ?? "name_asc");
            url += "&page=1"; // reset to first page after filtering
            Response.Redirect(url);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Admin/Products.aspx");
        }

        // ---------- Data helpers ----------
        private void BindCategories()
        {
            var ddl = Find<DropDownList>("ddlCat");
            if (ddl == null) return;
            var dt = Db.Query("SELECT id, name FROM categories ORDER BY name ASC;");
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("-- Select Collection --", "0"));
            foreach (DataRow r in dt.Rows)
                ddl.Items.Add(new ListItem(Convert.ToString(r["name"]), Convert.ToString(r["id"])));
        }

        private void BindFilterCategories()
        {
            var ddl = Find<DropDownList>("ddlFilterCat");
            if (ddl == null) return;
            var dt = Db.Query("SELECT id, name FROM categories ORDER BY name ASC;");
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("All Collections", "0"));
            foreach (DataRow r in dt.Rows)
                ddl.Items.Add(new ListItem(Convert.ToString(r["name"]), Convert.ToString(r["id"])));
        }

        private void LoadForEdit(int id)
        {
            using (var con = Db.Open())
            using (var cmd = new SqlCommand("SELECT id, category_id, name, description, stock, price, image, is_show FROM products WHERE id=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (r.Read())
                    {
                        SetHidden("hidId", Convert.ToString(r["id"]));
                        SetDDL("ddlCat", Convert.ToString(r["category_id"]));
                        SetText("txtName", Convert.ToString(r["name"]));
                        SetText("txtDesc", Convert.ToString(r["description"])); // can be empty
                        SetText("txtStock", Convert.ToString(r["stock"]));
                        SetText("txtPrice", Convert.ToDecimal(r["price"]).ToString("0.00", CultureInfo.InvariantCulture));
                        SetCheck("chkShow", Convert.ToBoolean(r["is_show"]));

                        var hint = Find<Literal>("litImgHint");
                        string img = r["image"] as string;
                        if (hint != null && !string.IsNullOrEmpty(img))
                            hint.Text = "Current Archive: " + HttpUtility.HtmlEncode(img);
                    }
                }
            }
        }

        private void DeleteProduct(int id)
        {
            var lbl = Find<Label>("lblMsg");
            try
            {
                int used = Db.Scalar<int>("SELECT COUNT(*) FROM order_items WHERE product_id=@id", Db.P("@id", id));
                if (used > 0) { Show(lbl, "Cannot remove: history contains active transactions.", false); return; }

                string img = Db.Scalar<string>("SELECT image FROM products WHERE id=@id", Db.P("@id", id));
                int n = Db.Execute("DELETE FROM products WHERE id=@id", Db.P("@id", id));
                if (n > 0)
                {
                    if (!string.IsNullOrEmpty(img)) TryDeletePhysical("~/" + img);
                    Show(lbl, "Masterpiece removed from collection.", true);
                }
                else Show(lbl, "Masterpiece not found.", false);
            }
            catch { Show(lbl, "Removal logic error.", false); }
        }

        private void ToggleShow(int id)
        {
            try { Db.Execute("UPDATE products SET is_show = CASE WHEN is_show=1 THEN 0 ELSE 1 END, updated_at=GETDATE() WHERE id=@id", Db.P("@id", id)); }
            catch { }
        }

        private void BindTable()
        {
            var lit = Find<Literal>("litRows");
            var litTotal = Find<Literal>("litTotal");
            var pagerLit = Find<Literal>("pager");
            var lbl = Find<Label>("lblMsg");
            if (lit == null) return;

            try
            {
                string q = Request.QueryString["q"];
                int fcat = SafeInt(Request.QueryString["fcat"]);
                string fshow = Request.QueryString["fshow"];
                string sort = Request.QueryString["sort"] ?? "name_asc";

                // Pagination
                int page = 1;
                int.TryParse(Request.QueryString["page"], out page);
                if (page < 1) page = 1;

                // WHERE and ORDER BY (declare out vars first for C#5)
                List<SqlParameter> countParams;
                string where = BuildWhere(q, fcat, fshow, out countParams);
                string orderBy = BuildOrderBy(sort);

                // Filtered total
                int totalFiltered = Db.Scalar<int>("SELECT COUNT(*) FROM products p" + where, countParams.ToArray());
                if (litTotal != null)
                    litTotal.Text = totalFiltered.ToString("N0");

                int pageCount = Math.Max(1, (int)Math.Ceiling(totalFiltered / (double)PAGE_SIZE));
                if (page > pageCount) page = pageCount;
                int offset = (page - 1) * PAGE_SIZE;

                // Data params (fresh list)
                List<SqlParameter> dataParams;
                BuildWhere(q, fcat, fshow, out dataParams);
                dataParams.Add(Db.P("@offset", offset));
                dataParams.Add(Db.P("@limit", PAGE_SIZE));

                var sql = new StringBuilder(@"
SELECT p.id, p.name, p.stock, p.price, p.is_show, p.image, c.name AS category_name
FROM products p
LEFT JOIN categories c ON c.id = p.category_id");
                sql.Append(where);
                sql.Append(orderBy);
                sql.Append(@"
 OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;");

                var dt = Db.Query(sql.ToString(), dataParams.ToArray());

                var sb = new StringBuilder();
                if (dt.Rows.Count == 0)
                {
                    sb.Append("<tr><td colspan='7' class='px-8 py-12 text-center text-gray-400 text-xs italic italic'>No pieces matching your search logic.</td></tr>");
                }
                else
                {
                    int i = offset;
                    foreach (DataRow r in dt.Rows)
                    {
                        i++;
                        int id = Convert.ToInt32(r["id"]);
                        string name = Html(r["name"]);
                        string cat = Html(r["category_name"]);
                        int stock = Convert.ToInt32(r["stock"]);
                        decimal price = Convert.ToDecimal(r["price"]);
                        bool show = Convert.ToBoolean(r["is_show"]);
                        string img = r["image"] as string;
                        string imgUrl = null;
                        if (!string.IsNullOrEmpty(img))
                            imgUrl = System.Web.VirtualPathUtility.ToAbsolute("~/" + img.TrimStart('~', '/'));

                        string stockColor = stock <= 0 ? "text-red-500" : (stock <= 5 ? "text-orange-400" : "text-gray-400");

                        sb.Append("<tr class='hover:bg-off-white/30 transition-colors'>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-300'>").Append(i).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-sm font-serif text-text-dark'>").Append(name).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-400'>").Append(string.IsNullOrEmpty(cat) ? "-" : cat).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-right font-bold text-text-dark text-sm'>RS ").Append(price.ToString("N2")).Append("</td>");
                        sb.Append("<td class='px-8 py-5 text-right font-bold ").Append(stockColor).Append("'>").Append(stock).Append("</td>");
                        sb.Append("<td class='px-8 py-5'>");
                        sb.Append("<span class='text-[8px] uppercase tracking-widest font-bold px-3 py-1 border ").Append(show ? "border-primary text-primary" : "border-gray-200 text-gray-300").Append("'>")
                          .Append(show ? "Live" : "Archived").Append("</span></td>");
                        sb.Append("<td class='px-8 py-5 text-right flex justify-end gap-3 items-center'>");
                        
                        sb.Append("<a class='text-primary hover:underline text-[10px] uppercase tracking-widest font-bold' href='Products.aspx?edit=").Append(id).Append("'>Edit</a>");
                        
                        sb.Append("<a class='text-gray-400 hover:text-text-dark text-[10px] uppercase tracking-widest font-bold' href='Products.aspx?toggle=").Append(id).Append("'>");
                        sb.Append(show ? "Hide" : "Show").Append("</a>");
                        
                        if (!string.IsNullOrEmpty(imgUrl))
                        {
                            sb.Append("<a href='#' class='text-accent hover:underline text-[10px] uppercase tracking-widest font-bold view-img' data-src='")
                              .Append(Html(imgUrl)).Append("'>View</a>");
                        }
                        
                        sb.Append("<a class='text-gray-200 hover:text-red-500 text-[10px] uppercase tracking-widest font-bold' href='Products.aspx?del=").Append(id).Append("' ");
                        sb.Append("onclick=\"return confirm('Purge this masterpiece from history?');\"><i class='fa-solid fa-trash-can'></i></a>");
                        
                        sb.Append("</td>");
                        sb.Append("</tr>");
                    }
                }
                lit.Text = sb.ToString();

                if (pagerLit != null)
                {
                    pagerLit.Text = BuildPager(totalFiltered, page, pageCount, q, fcat, fshow, sort);
                }
            }
            catch (Exception ex)
            {
                Show(lbl, "System error: " + Server.HtmlEncode(ex.Message), false);
                if (lit != null) lit.Text = "<tr><td colspan='7' class='text-center text-red-500 py-10 text-xs uppercase tracking-widest font-bold'>Database communication failure.</td></tr>";
                if (pagerLit != null) pagerLit.Text = "";
            }
        }

        // ---------- WHERE/ORDER helpers (each call creates NEW parameters) ----------
        private static string BuildWhere(string q, int fcat, string fshow, out List<SqlParameter> parms)
        {
            var sb = new StringBuilder(" WHERE 1=1 ");
            parms = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(q))
            {
                // NAME-ONLY filter
                sb.Append(" AND p.name LIKE @q ");
                parms.Add(Db.P("@q", "%" + q + "%"));
            }
            if (fcat > 0)
            {
                sb.Append(" AND p.category_id=@fc ");
                parms.Add(Db.P("@fc", fcat));
            }
            if (fshow == "1" || fshow == "0")
            {
                sb.Append(" AND p.is_show=@fs ");
                parms.Add(Db.P("@fs", fshow == "1"));
            }

            return sb.ToString();
        }

        private static string BuildOrderBy(string sort)
        {
            switch ((sort ?? "").ToLowerInvariant())
            {
                case "name_desc": return " ORDER BY p.name DESC";
                case "price_asc": return " ORDER BY p.price ASC";
                case "price_desc": return " ORDER BY p.price DESC";
                case "stock_asc": return " ORDER BY p.stock ASC";
                case "stock_desc": return " ORDER BY p.stock DESC";
                default: return " ORDER BY p.name ASC";
            }
        }

        // ---------- find & ui helpers ----------
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

        private static string Html(object o)
        {
            if (o == null || o == DBNull.Value) return "";
            return HttpUtility.HtmlEncode(Convert.ToString(o) ?? "");
        }

        private static int SafeInt(string s)
        {
            int x; return int.TryParse(s, out x) ? x : 0;
        }

        private static decimal SafeDecimal(string s)
        {
            decimal d; return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d) ? d : 0m;
        }

        private void SetText(string id, string v) { var t = Find<TextBox>(id); if (t != null) t.Text = v ?? ""; }
        private void SetHidden(string id, string v) { var h = Find<HiddenField>(id); if (h != null) h.Value = v ?? ""; }
        private void SetDDL(string id, string v) { var d = Find<DropDownList>(id); if (d != null && v != null && d.Items.FindByValue(v) != null) d.SelectedValue = v; }
        private string GetText(string id) { var t = Find<TextBox>(id); return t != null ? t.Text.Trim() : ""; }
        private string GetDDL(string id) { var d = Find<DropDownList>(id); return d != null ? d.SelectedValue : ""; }

        private void SetCheck(string id, bool v) { var c = Find<CheckBox>(id); if (c != null) c.Checked = v; }

        private void Show(Label lbl, string msg, bool ok)
        {
            if (lbl != null)
            {
                lbl.Text = HttpUtility.HtmlEncode(msg);
                lbl.Visible = true;
                // JavaScript in ASPX handles the CSS classes based on visibility and content
            }
        }

        private static bool IsImageExt(string ext)
        {
            if (string.IsNullOrEmpty(ext)) return false;
            ext = ext.ToLowerInvariant();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif";
        }

        private void TryDeletePhysical(string virtualPath)
        {
            try { string p = Server.MapPath(virtualPath); if (File.Exists(p)) File.Delete(p); }
            catch { /* ignore */ }
        }

        private static string Slugify(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "product";

            var sb = new StringBuilder(value.Length);
            bool dash = false;
            foreach (char c in value.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                    dash = false;
                }
                else if (!dash)
                {
                    sb.Append('-');
                    dash = true;
                }
            }

            string result = sb.ToString().Trim('-');
            return string.IsNullOrEmpty(result) ? "product" : result;
        }

        // ----- pager helpers -----
        private static string BuildPager(int totalFiltered, int page, int pageCount, string q, int fcat, string fshow, string sort)
        {
            var sb = new StringBuilder();
            sb.Append("<div class='flex items-center space-x-2'>");

            Func<int, string> url = p => BuildProductsUrl(p, q, fcat, fshow, sort);
            
            bool hasPrev = page > 1;
            sb.Append("<a href='").Append(hasPrev ? url(page - 1) : "#")
              .Append("' class='p-3 border ").Append(hasPrev ? "border-gray-200 text-text-dark hover:bg-primary hover:text-white" : "border-gray-100 text-gray-200 cursor-not-allowed")
              .Append(" transition-all'><i class='fa-solid fa-chevron-left text-[10px]'></i></a>");

            const int window = 7;
            int start = Math.Max(1, page - (window / 2));
            int end = Math.Min(pageCount, start + window - 1);
            if (end - start + 1 < window) start = Math.Max(1, end - window + 1);

            for (int p = start; p <= end; p++)
            {
                bool active = (p == page);
                sb.Append("<a href='").Append(url(p))
                  .Append("' class='w-10 h-10 flex items-center justify-center text-[10px] font-bold tracking-widest transition-all ")
                  .Append(active ? "bg-primary text-white" : "bg-white text-gray-400 hover:text-primary")
                  .Append("'>").Append(p).Append("</a>");
            }

            bool hasNext = page < pageCount;
            sb.Append("<a href='").Append(hasNext ? url(page + 1) : "#")
              .Append("' class='p-3 border ").Append(hasNext ? "border-gray-200 text-text-dark hover:bg-primary hover:text-white" : "border-gray-100 text-gray-200 cursor-not-allowed")
              .Append(" transition-all'><i class='fa-solid fa-chevron-right text-[10px]'></i></a>");

            sb.Append("</div>");
            return sb.ToString();
        }

        private static string BuildProductsUrl(int p, string q, int fcat, string fshow, string sort)
        {
            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["page"] = p.ToString();
            if (!string.IsNullOrWhiteSpace(q)) qs["q"] = q;
            if (fcat > 0) qs["fcat"] = fcat.ToString();
            if (!string.IsNullOrEmpty(fshow)) qs["fshow"] = fshow;
            if (!string.IsNullOrEmpty(sort)) qs["sort"] = sort;
            return "Products.aspx?" + qs.ToString();
        }
    }
}
