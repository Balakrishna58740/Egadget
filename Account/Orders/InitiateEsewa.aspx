<%@ Page Language="C#" AutoEventWireup="true" CodeFile="InitiateEsewa.aspx.cs" Inherits="serena.Site.Account.Orders.InitiateEsewaPage" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>eGadgetHub • Redirecting to eSewa...</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif; display: flex; align-items: center; justify-content: center; min-height: 100vh; background: #fff; margin: 0; color: #1a1a1a; }
        .loader-wrap { text-align: center; }
        .spinner { width: 40px; height: 40px; border: 3px solid #f3f3f3; border-top: 3px solid #8B7355; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 24px; }
        @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
        h2 { font-weight: 500; font-size: 1.25rem; letter-spacing: 0.05em; margin: 0; }
        p { color: #6b7280; font-size: 0.875rem; margin-top: 8px; }
    </style>
</head>
<body>
    <div class="loader-wrap">
        <div class="spinner"></div>
        <h2>Redirecting to eSewa</h2>
        <p>Please do not refresh the page or close the window.</p>
    </div>

    <form id="eSewaForm" method="post" action="https://rc-epay.esewa.com.np/api/epay/main/v2/form" style="display:none;">
        <input type="hidden" name="amount" id="amount" runat="server" />
        <input type="hidden" name="tax_amount" value="0" />
        <input type="hidden" name="total_amount" id="total_amount" runat="server" />
        <input type="hidden" name="transaction_uuid" id="transaction_uuid" runat="server" />
        <input type="hidden" name="product_code" value="EPAYTEST" />
        <input type="hidden" name="product_service_charge" value="0" />
        <input type="hidden" name="product_delivery_charge" value="0" />
        <input type="hidden" name="success_url" id="success_url" runat="server" />
        <input type="hidden" name="failure_url" id="failure_url" runat="server" />
        <input type="hidden" name="signed_field_names" value="total_amount,transaction_uuid,product_code" />
        <input type="hidden" name="signature" id="signature" runat="server" />
    </form>

    <script>
        window.onload = function() {
            document.getElementById('eSewaForm').submit();
        };
    </script>
</body>
</html>
