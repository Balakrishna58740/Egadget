<%@ Page Language="C#" AutoEventWireup="true" CodeFile="EsewaSuccess.aspx.cs" Inherits="serena.Site.Account.Orders.EsewaSuccessPage" MasterPageFile="~/MasterPages/Site.master" %>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-24 text-center">
        <div class="max-w-md mx-auto">
            <div class="w-20 h-20 bg-green-50 rounded-full flex items-center justify-center mx-auto mb-8 text-green-500">
                <i class="fa-solid fa-check text-4xl"></i>
            </div>
            <h1 class="font-serif text-4xl mb-4">Payment Successful</h1>
            <p class="text-gray-500 mb-8 uppercase tracking-widest text-sm">Your payment via eSewa has been processed.</p>
            
            <div class="bg-off-white p-8 mb-12 border border-gray-100">
                <p class="text-xs text-gray-400 uppercase tracking-widest mb-1">Order Reference</p>
                <p class="font-bold text-lg">#<asp:Literal ID="litOrderCode" runat="server" /></p>
            </div>

            <div class="flex flex-col gap-4">
                <asp:HyperLink ID="lnkOrder" runat="server" CssClass="bg-text-dark text-white py-4 text-xs uppercase tracking-widest font-bold hover:bg-black transition-all">
                    View Order Details
                </asp:HyperLink>
                <a href="<%= ResolveUrl("~/Catalog.aspx") %>" class="text-xs uppercase tracking-widest font-bold text-primary hover:opacity-70 transition-opacity">
                    Continue Shopping
                </a>
            </div>
        </div>
    </div>
</asp:Content>
