using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace serena.Admin
{
    public partial class ReportProductPrintPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                var lg = Find<Literal>("litGenerated");
                if (lg != null) lg.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                LoadData();
            }
        }

        private void LoadData()
        {
            var lbl = Find<Label>("lblMsg");
            var litSum = Find<Literal>("litSummary");
            var lit = Find<Literal>("litRows");

            try
            {
                var dt = Db.Query(@"SELECT id, name, stock FROM products ORDER BY name ASC;");

                var sb = new StringBuilder();
                int totalSkus = dt.Rows.Count;
                long totalStock = 0;

                if (totalSkus == 0)
                {
                    sb.Append("<tr><td colspan='3' class='text-center text-muted py-3'>No products found.</td></tr>");
                }
                else
                {
                    int i = 0;
                    foreach (DataRow r in dt.Rows)
                    {
                        i++;
                        string name = Html(r["name"]);
                        int stock = r["stock"] != DBNull.Value ? Convert.ToInt32(r["stock"]) : 0;
                        totalStock += stock;

                        sb.Append("<tr>");
                        sb.Append("<td>").Append(i).Append("</td>");
                        sb.Append("<td>").Append(string.IsNullOrEmpty(name) ? "-" : name).Append("</td>");
                        sb.Append("<td class='text-end'>").Append(stock).Append("</td>");
                        sb.Append("</tr>");
                    }
                }
                if (lit != null) lit.Text = sb.ToString();

                if (litSum != null)
                {
                    litSum.Text = "<div class='small text-muted'>Total SKUs: <strong>" +
                                  totalSkus.ToString("N0") + "</strong> &nbsp;|&nbsp; Total Stock: <strong>" +
                                  totalStock.ToString("N0") + "</strong></div>";
                }

                Show(lbl, "Loaded " + totalSkus + " products.", true);
            }
            catch (Exception ex)
            {
                if (lit != null) lit.Text = "<tr><td colspan='3' class='text-danger text-center py-3'>Failed to load products.</td></tr>";
                Show(lbl, "Error: " + Server.HtmlEncode(ex.Message), false);
            }
        }

        // helpers
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
        private static string Html(object o) { return HttpUtility.HtmlEncode(Convert.ToString(o) ?? ""); }
        private void Show(Label lbl, string msg, bool ok)
        {
            if (lbl == null) return;
            lbl.Text = Server.HtmlEncode(msg);
            lbl.CssClass = ok ? "alert alert-success" : "alert alert-danger";
        }
    }
}
