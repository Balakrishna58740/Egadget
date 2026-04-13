using System;
using System.Web.UI;

namespace serena.Site
{
    public partial class AboutPage : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack) BindStats();
        }

        private void BindStats()
        {
            int products = 0, categories = 0, methods = 0;

            try { products = Db.Scalar<int>("SELECT COUNT(*) FROM products WHERE is_show = 1"); } catch { }
            try { categories = Db.Scalar<int>("SELECT COUNT(*) FROM categories"); } catch { }
            try { methods = Db.Scalar<int>("SELECT COUNT(*) FROM payment_methods WHERE is_use = 1"); } catch { }

            // Write directly to the page controls (no designer needed in Web Site projects)
            if (litStatProducts != null) litStatProducts.Text = products.ToString();
            if (litStatCategories != null) litStatCategories.Text = categories.ToString();
            if (litStatPayments != null) litStatPayments.Text = methods.ToString();
        }
    }
}
