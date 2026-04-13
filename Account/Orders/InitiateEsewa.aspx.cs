using System;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace serena.Site.Account.Orders
{
    public partial class InitiateEsewaPage : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string orderCode = Request.QueryString["code"];
            if (string.IsNullOrEmpty(orderCode))
            {
                Response.Redirect("~/Account/Orders/Index.aspx");
                return;
            }

            // Fetch order total and confirm it exists
            DataTable dt = Db.Query("SELECT total_amount FROM dbo.orders WHERE order_code = @code", 
                                    Db.P("@code", orderCode));

            if (dt == null || dt.Rows.Count == 0)
            {
                Response.Redirect("~/Account/Orders/Index.aspx");
                return;
            }

            decimal totalAmount = Convert.ToDecimal(dt.Rows[0]["total_amount"]);
            // eSewa total_amount should not have commas, usually 1 decimal if needed or integer
            // But string.Format("{0}", totalAmount) is safest for test environment.
            string totalAmountStr = totalAmount.ToString("0.00", CultureInfo.InvariantCulture);
            string productCode = "EPAYTEST";

            // Set hidden fields
            amount.Value = totalAmountStr;
            total_amount.Value = totalAmountStr;
            transaction_uuid.Value = orderCode;
            
            // Generate full URLs for success and failure
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + ResolveUrl("~/Account/Orders/");
            success_url.Value = baseUrl + "EsewaSuccess.aspx";
            failure_url.Value = baseUrl + "EsewaFailure.aspx";

            // Generate Signature
            signature.Value = EsewaUtil.GenerateSignature(totalAmountStr, orderCode, productCode);
        }
    }
}
