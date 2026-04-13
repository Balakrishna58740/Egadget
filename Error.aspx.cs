using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace serena
{
    public partial class Error : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Find controls at runtime (no designer needed)
            var litErrorId = FindControlRecursive<Literal>(this, "litErrorId");
            var pnlDebug = FindControlRecursive<Panel>(this, "pnlDebug");
            var litDetails = FindControlRecursive<Literal>(this, "litDetails");

            if (litErrorId != null)
            {
                var errorId = Context.Items["ErrorId"] as string 
                    ?? Request.QueryString["eid"] 
                    ?? "-";
                litErrorId.Text = Server.HtmlEncode(errorId);
            }

            if (Context.IsDebuggingEnabled && pnlDebug != null)
            {
                pnlDebug.Visible = true;
                var ex = Context.Items["LastError"] as Exception;
                if (litDetails != null)
                {
                    string details = ex != null
                        ? ex.GetType().FullName + ": " + ex.Message + "\r\n\r\n" + ex.StackTrace
                        : "No additional details available via context. Please check App_Data/errors.log for ID: " + Request.QueryString["eid"];
                    litDetails.Text = Server.HtmlEncode(details);
                }
            }
        }

        private static T FindControlRecursive<T>(Control root, string id) where T : Control
        {
            if (root == null) return null;
            T typed = root.FindControl(id) as T;
            if (typed != null) return typed;
            foreach (Control child in root.Controls)
            {
                T found = FindControlRecursive<T>(child, id);
                if (found != null) return found;
            }
            return null;
        }
    }
}
