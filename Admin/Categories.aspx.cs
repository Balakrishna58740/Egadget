using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace serena.Admin
{
    public partial class CategoriesPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                // Handle delete via querystring (?del=ID) with a simple check
                int delId;
                if (TryGetQueryInt("del", out delId) && delId > 0)
                {
                    DeleteCategory(delId);
                }

                // If editing (?edit=ID), load the record into the form
                int editId;
                if (TryGetQueryInt("edit", out editId) && editId > 0)
                {
                    LoadForEdit(editId);
                }

                BindTable();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var lbl = Find<Label>("lblMsg");
            var hid = Find<HiddenField>("hidId");
            var txt = Find<TextBox>("txtName");

            string name = (txt != null ? txt.Text.Trim() : "");
            if (string.IsNullOrWhiteSpace(name))
            {
                Show(lbl, "Designation label is required for archival.", false);
                return;
            }

            int id = 0;
            if (hid != null && !string.IsNullOrEmpty(hid.Value)) int.TryParse(hid.Value, out id);

            try
            {
                // Unique name check (case-insensitive)
                int exists = Db.Scalar<int>(
                    id > 0
                        ? "SELECT COUNT(*) FROM categories WHERE LOWER(name)=LOWER(@n) AND id<>@id"
                        : "SELECT COUNT(*) FROM categories WHERE LOWER(name)=LOWER(@n)",
                    Db.P("@n", name), Db.P("@id", id));
                if (exists > 0)
                {
                    Show(lbl, "This taxonomy designation already exists in the repository.", false);
                    return;
                }

                if (id > 0)
                {
                    Db.Execute("UPDATE categories SET name=@n WHERE id=@id",
                               Db.P("@n", name), Db.P("@id", id));
                    Show(lbl, "Taxonomy record has been successfully updated.", true);
                }
                else
                {
                    Db.Execute("INSERT INTO categories (name) VALUES (@n)", Db.P("@n", name));
                    Show(lbl, "New taxonomy successfully archived in the repository.", true);
                }

                // Clear form & refresh list
                if (hid != null) hid.Value = "";
                if (txt != null) txt.Text = "";
                BindTable();
            }
            catch
            {
                Show(lbl, "System error encountered during archival process.", false);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Admin/Categories.aspx");
        }

        private void LoadForEdit(int id)
        {
            using (var con = Db.Open())
            using (var cmd = new SqlCommand("SELECT id, name FROM categories WHERE id=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (r.Read())
                    {
                        var hid = Find<HiddenField>("hidId");
                        var txt = Find<TextBox>("txtName");
                        if (hid != null) hid.Value = Convert.ToString(r["id"]);
                        if (txt != null) txt.Text = Convert.ToString(r["name"]);
                        
                        var btnSave = Find<Button>("btnSave");
                        if (btnSave != null) btnSave.Text = "Update Taxonomy";
                    }
                }
            }
        }

        private void DeleteCategory(int id)
        {
            var lbl = Find<Label>("lblMsg");

            try
            {
                // prevent deletion if products reference this category
                int used = Db.Scalar<int>("SELECT COUNT(*) FROM products WHERE category_id=@id", Db.P("@id", id));
                if (used > 0)
                {
                    Show(lbl, "Structural integrity protected: Taxonomy in use by active items.", false);
                    return;
                }

                int n = Db.Execute("DELETE FROM categories WHERE id=@id", Db.P("@id", id));
                if (n > 0) Show(lbl, "Taxonomy record successfully purged from repository.", true);
                else Show(lbl, "Specified record not found in repository.", false);
            }
            catch
            {
                Show(lbl, "Purge operation failed due to system constraint.", false);
            }
        }

        private void BindTable()
        {
            var lit = Find<Literal>("litRows");
            if (lit == null) return;

            var dt = Db.Query(@"
SELECT c.id, c.name, COUNT(p.id) AS product_count
FROM categories c
LEFT JOIN products p ON p.category_id = c.id
GROUP BY c.id, c.name
ORDER BY c.name ASC;");

            var sb = new StringBuilder();
            if (dt.Rows.Count == 0)
            {
                sb.Append("<tr><td colspan='4' class='px-8 py-12 text-center text-gray-400 text-xs italic'>The repository currently houses no taxonomies.</td></tr>");
            }
            else
            {
                int i = 0;
                foreach (DataRow r in dt.Rows)
                {
                    i++;
                    int id = Convert.ToInt32(r["id"]);
                    string name = Html(r["name"]);
                    int count = Convert.ToInt32(r["product_count"]);

                    sb.Append("<tr class='hover:bg-off-white/30 transition-colors'>");
                    sb.Append("<td class='px-8 py-5 text-[10px] uppercase tracking-widest font-bold text-gray-400'>").Append(i.ToString("D2")).Append("</td>");
                    sb.Append("<td class='px-8 py-5 text-sm font-serif text-text-dark'>").Append(name).Append("</td>");
                    sb.Append("<td class='px-8 py-5 text-center font-bold text-text-dark text-sm'>").Append(count).Append("</td>");
                    sb.Append("<td class='px-8 py-5 text-right'>");
                    sb.Append("<div class='flex justify-end gap-4'>");
                    sb.Append("<a href='Categories.aspx?edit=").Append(id).Append("' class='text-primary hover:text-text-dark transition-all'><i class='fa-solid fa-pen-to-square'></i></a>");
                    sb.Append("<a href='Categories.aspx?del=").Append(id).Append("' class='text-gray-300 hover:text-red-500 transition-all' ");
                    sb.Append("onclick=\"return confirm('Purge this taxonomy from the repository?');\"><i class='fa-solid fa-trash-can'></i></a>");
                    sb.Append("</div>");
                    sb.Append("</td>");
                    sb.Append("</tr>");
                }
            }
            lit.Text = sb.ToString();
        }


        // ------- helpers (designer-free) -------
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
            return HttpUtility.HtmlEncode(Convert.ToString(o) ?? "");
        }

        private static bool TryGetQueryInt(string key, out int value)
        {
            value = 0;
            string s = HttpContext.Current.Request.QueryString[key];
            return !string.IsNullOrEmpty(s) && int.TryParse(s, out value);
        }

        private void Show(Label lbl, string msg, bool ok)
        {
            if (lbl == null) return;
            lbl.Text = HttpUtility.HtmlEncode(msg);
            lbl.Visible = true;
        }
    }
}
