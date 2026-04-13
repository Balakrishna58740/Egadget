<%@ Page Language="C#" AutoEventWireup="true" CodeFile="EsewaFailure.aspx.cs" Inherits="serena.Site.Account.Orders.EsewaFailurePage" MasterPageFile="~/MasterPages/Site.master" %>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-24 text-center">
        <div class="max-w-md mx-auto">
            <div class="w-20 h-20 bg-red-50 rounded-full flex items-center justify-center mx-auto mb-8 text-red-500">
                <i class="fa-solid fa-xmark text-4xl"></i>
            </div>
            <h1 class="font-serif text-4xl mb-4">Payment Failed</h1>
            <p class="text-gray-500 mb-12 uppercase tracking-widest text-sm">We couldn't process your payment via eSewa. Your order is still pending.</p>
            
            <div class="flex flex-col gap-4">
                <a href="<%= ResolveUrl("~/Account/Orders/Index.aspx") %>" class="bg-text-dark text-white py-4 text-xs uppercase tracking-widest font-bold hover:bg-black transition-all">
                    Retry from Order History
                </a>
                <a href="<%= ResolveUrl("~/Cart.aspx") %>" class="text-xs uppercase tracking-widest font-bold text-primary hover:opacity-70 transition-opacity">
                    Return to Cart
                </a>
            </div>
        </div>
    </div>
</asp:Content>
