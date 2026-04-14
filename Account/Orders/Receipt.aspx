<%@ Page Language="C#"
    MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true"
    CodeFile="Receipt.aspx.cs"
    Inherits="serena.Site.Account.Orders.ReceiptPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">
  Receipt
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
  <div class="container mx-auto px-4 py-12">
    <div class="max-w-6xl mx-auto">

      <div class="flex flex-col md:flex-row md:items-end justify-between gap-6 mb-12">
        <div>
          <p class="text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Receipt</p>
          <h1 class="font-serif text-4xl mb-2">
            #<asp:Literal ID="litOrderCode" runat="server" />
          </h1>
          <p class="text-xs uppercase tracking-widest text-gray-400">A summary of your order and payment instructions.</p>
        </div>
        <div class="flex gap-4 flex-wrap">
          <asp:HyperLink ID="lnkPrint" runat="server" CssClass="bg-text-dark text-white px-8 py-3 text-[10px] uppercase tracking-widest font-bold hover:bg-black transition-all text-center">
            <i class="fa-solid fa-print mr-2"></i> Print
          </asp:HyperLink>
          <asp:HyperLink ID="lnkBackDetail" runat="server" CssClass="border border-gray-200 px-8 py-3 text-[10px] uppercase tracking-widest font-bold hover:bg-off-white transition-all text-center">
            Back to Order Detail
          </asp:HyperLink>
          <asp:HyperLink ID="lnkBackList" runat="server" CssClass="border border-gray-200 px-8 py-3 text-[10px] uppercase tracking-widest font-bold hover:bg-off-white transition-all text-center" NavigateUrl="~/Account/Orders/Index.aspx">
            Back to Orders
          </asp:HyperLink>
        </div>
      </div>

      <div id="alertMsg" runat="server" class="mb-8"></div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-12 eg-card bg-white rounded-2xl p-6 md:p-8">
        <div class="lg:col-span-1 space-y-12">
          <section>
            <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Summary</h3>
            <div class="bg-off-white p-8 border border-gray-100 space-y-6 eg-card rounded-xl">
              <div>
                <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Current Status</label>
                <span id="litOrderStatus" runat="server" class="text-[10px] uppercase tracking-widest font-bold px-4 py-2 rounded-full inline-block"></span>
              </div>
              <div>
                <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Payment Method</label>
                <p class="text-sm font-bold text-text-dark uppercase tracking-wider"><asp:Literal ID="litPayment" runat="server" /></p>
                <div class="mt-3 text-xs text-gray-500 leading-relaxed"><asp:Literal ID="litPaymentAdvice" runat="server" /></div>
              </div>
              <div>
                <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Order Date</label>
                <p class="text-sm text-text-dark"><asp:Literal ID="litOrderDate" runat="server" /></p>
              </div>
              <div class="grid grid-cols-2 gap-4 pt-4 border-t border-gray-100">
                <div>
                  <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Total Qty</label>
                  <p class="text-sm font-bold text-text-dark"><asp:Literal ID="litTotalQty" runat="server" /></p>
                </div>
                <div>
                  <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Amount</label>
                  <p class="text-sm font-bold text-primary">RS <asp:Literal ID="litTotalAmt" runat="server" /></p>
                </div>
              </div>
            </div>
          </section>
        </div>

        <div class="lg:col-span-2 space-y-12">
          <section>
            <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Shipping</h3>
            <div class="bg-white border border-gray-100 shadow-sm p-8 eg-card rounded-xl">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div>
                  <div class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Recipient</div>
                  <p class="text-sm font-bold text-text-dark"><asp:Literal ID="litShipName" runat="server" /></p>
                  <p class="text-xs text-gray-500 mt-1"><asp:Literal ID="litShipPhone" runat="server" /></p>
                </div>
                <div>
                  <div class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Address</div>
                  <div class="text-xs text-gray-500 leading-relaxed uppercase tracking-wider">
                    <asp:Literal ID="litShipAddr" runat="server" />
                  </div>
                </div>
              </div>
            </div>
          </section>

          <section>
            <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Items</h3>
            <div class="overflow-x-auto bg-white border border-gray-100 shadow-sm p-8 eg-card rounded-xl">
              <asp:Literal ID="litItemsTable" runat="server" />
            </div>
          </section>
        </div>
      </div>
    </div>
  </div>

  <style>
    table { width: 100%; border-collapse: collapse; }
    th { padding: 1.25rem 0; text-align: left; font-size: 10px; text-transform: uppercase; letter-spacing: 0.2em; color: #9ca3af; font-weight: 700; border-bottom: 1px solid #f3f4f6; }
    td { padding: 1.25rem 0; font-size: 0.875rem; border-bottom: 1px solid #f9fafb; vertical-align: middle; }
    .table-responsive { overflow-x: auto; }
  </style>
</asp:Content>
