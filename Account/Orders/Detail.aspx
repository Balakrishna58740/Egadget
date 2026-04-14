<%@ Page Language="C#"
    MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true"
    CodeFile="Detail.aspx.cs"
    Inherits="serena.Site.Account.Orders.OrderDetailPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">
  Order Detail
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-12">
        <div class="max-w-6xl mx-auto">
            
            <!-- Alert -->
            <div id="alertMsg" runat="server" class="mb-8"></div>

            <!-- Header -->
            <div class="flex flex-col md:flex-row md:items-end justify-between gap-6 mb-12">
                <div>
                    <p class="text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Order Details</p>
                    <h1 class="font-serif text-4xl mb-2">#<asp:Literal ID="litOrderCode" runat="server" /></h1>
                    <p class="text-xs uppercase tracking-widest text-gray-400">Placed on <asp:Literal ID="litOrderDate" runat="server" /></p>
                </div>
                <div class="flex gap-4">
                    <asp:HyperLink ID="lnkReceipt" runat="server" CssClass="bg-text-dark text-white px-8 py-3 text-[10px] uppercase tracking-widest font-bold hover:bg-black transition-all text-center">
                        <i class="fa-solid fa-file-invoice mr-2"></i> View Receipt
                    </asp:HyperLink>
                    <asp:HyperLink ID="lnkBack" runat="server" CssClass="border border-gray-200 px-8 py-3 text-[10px] uppercase tracking-widest font-bold hover:bg-off-white transition-all text-center" NavigateUrl="~/Account/Orders/Index.aspx">
                        Back to Orders
                    </asp:HyperLink>
                </div>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-12 eg-card bg-white rounded-2xl p-6 md:p-8">
                <!-- Left: Status & Shipping -->
                <div class="lg:col-span-1 space-y-12">
                    <section>
                        <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Status</h3>
                        <div class="bg-off-white p-8 border border-gray-100 eg-card rounded-xl">
                            <div class="space-y-6">
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Current Status</label>
                                    <span id="lblStatus" runat="server" class="text-[10px] uppercase tracking-widest font-bold px-4 py-2 rounded-full inline-block"></span>
                                </div>
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Payment Method</label>
                                    <p class="text-sm font-bold text-text-dark uppercase tracking-wider"><asp:Literal ID="litPayment" runat="server" /></p>
                                </div>
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Transaction Ref</label>
                                    <p class="text-xs font-bold text-text-dark uppercase tracking-wider"><asp:Literal ID="litTxnRef" runat="server" /></p>
                                </div>
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Payment State</label>
                                    <p class="text-xs font-bold text-text-dark uppercase tracking-wider"><asp:Literal ID="litTxnStatus" runat="server" /></p>
                                </div>
                            </div>
                        </div>
                    </section>

                    <section>
                        <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Shipping</h3>
                        <div class="space-y-6">
                            <div>
                                <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Recipient</label>
                                <p class="text-sm font-bold text-text-dark"><asp:Literal ID="litShipName" runat="server" /></p>
                                <p class="text-xs text-gray-500 mt-1"><asp:Literal ID="litShipPhone" runat="server" /></p>
                            </div>
                            <div>
                                <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Destination</label>
                                <div class="text-xs text-gray-500 leading-relaxed uppercase tracking-wider">
                                    <asp:Literal ID="litShipAddr" runat="server" />
                                </div>
                            </div>
                        </div>
                    </section>
                </div>

                <!-- Right: Items & Summary -->
                <div class="lg:col-span-2 space-y-12">
                    <section>
                        <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Order Items</h3>
                        <div class="overflow-x-auto">
                            <asp:Literal ID="litItemsTable" runat="server" />
                        </div>
                        
                        <div class="mt-8 border-t border-gray-100 pt-8 flex justify-end">
                            <div class="w-full md:w-64 space-y-4">
                                <div class="flex justify-between items-center text-[10px] uppercase tracking-widest text-gray-400 font-bold">
                                    <span>Total Quantity</span>
                                    <span class="text-text-dark font-black"><asp:Literal ID="litTotalQty" runat="server" /></span>
                                </div>
                                <div class="flex justify-between items-center">
                                    <span class="font-serif text-xl">Order Total</span>
                                    <span class="text-2xl font-bold text-primary">RS <asp:Literal ID="litTotalAmt" runat="server" /></span>
                                </div>
                            </div>
                        </div>
                    </section>

                    <!-- Activity History -->
                    <section>
                        <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Activity History</h3>
                        <div class="space-y-4">
                            <asp:Repeater ID="rptLogs" runat="server">
                                <ItemTemplate>
                                    <div class="eg-timeline-item flex justify-between items-center gap-4">
                                        <div class="text-[10px] uppercase tracking-widest font-bold">
                                            <%# Container.Page.GetType().GetMethod("Upper").Invoke(Container.Page, new object[]{ Eval("status") }) %>
                                        </div>
                                        <div class="text-[9px] text-gray-400 uppercase tracking-widest font-medium">
                                            <%# Container.Page.GetType().GetMethod("FmtDate").Invoke(Container.Page, new object[]{ Eval("created_at") }) %>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:PlaceHolder ID="phNoLogs" runat="server" Visible="false">
                                <p class="text-xs text-gray-400 italic">No activity recorded for this order.</p>
                            </asp:PlaceHolder>
                        </div>
                    </section>
                </div>
            </div>
        </div>
    </div>

    <style>
        /* Table overrides for litItemsTable if it generates standard markup */
        table { width: 100%; border-collapse: collapse; }
        th { padding: 1.5rem 0; text-align: left; font-size: 10px; text-transform: uppercase; letter-spacing: 0.2em; color: #9ca3af; font-weight: 700; border-bottom: 1px solid #f3f4f6; }
        td { padding: 1.5rem 0; font-size: 0.875rem; border-bottom: 1px solid #f9fafb; vertical-align: middle; }
    </style>
</asp:Content>
