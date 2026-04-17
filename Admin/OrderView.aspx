<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="OrderView.aspx.cs"
    Inherits="serena.Admin.OrderViewPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Order Details | eGadgetHub Admin</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="/Assets/css/admin-orderview-redesign.css" />
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hidId" runat="server" />

    <div class="ov-page">
    <div class="ov-shell">

    <!-- Dossier Header -->
    <div class="ov-header ov-card">
        <div class="ov-head-top">
            <div class="ov-breadcrumb" aria-label="Breadcrumb">
                <a href="Dashboard.aspx">Home</a>
                <span>›</span>
                <a href="Orders.aspx">Orders</a>
                <span>›</span>
                <strong>Order #<asp:Literal ID="litBreadcrumbCode" runat="server" /></strong>
            </div>
            <div class="ov-header-actions">
                <a href="Orders.aspx" class="ov-return-link text-gray-400 text-[10px] uppercase tracking-widest font-bold px-8 py-3 bg-white border border-gray-100 hover:text-primary transition-all">
                    Return to History
                </a>
                <button type="button" id="btnPrintInvoice" class="ov-return-link text-[10px] uppercase tracking-widest font-bold px-8 py-3">Print Invoice</button>
                <button type="button" id="btnDownloadPdf" class="ov-return-link text-[10px] uppercase tracking-widest font-bold px-8 py-3">Download PDF</button>
            </div>
        </div>

        <div class="ov-head-main">
            <h2 class="text-3xl font-serif ov-title">Order #<asp:Literal ID="litOrderCode" runat="server" /></h2>
            <span id="badgeStatus" runat="server" class="text-[8px] uppercase tracking-widest font-bold px-3 py-1 border ov-status-badge"></span>
        </div>

        <p class="ov-head-meta">Order Reference · Created on <asp:Literal ID="litOrderDate" runat="server" /> · <asp:Literal ID="litHeaderCustomer" runat="server" /></p>
    </div>

    <asp:Label ID="lblMsg" runat="server" CssClass="hidden mb-12 p-4 text-[10px] uppercase tracking-widest font-bold border-l-4 ov-alert" EnableViewState="false"></asp:Label>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-12 ov-layout">
        <!-- Main Narrative: Left Section -->
        <div class="lg:col-span-2 space-y-12">

            <div class="bg-white border border-gray-100 p-8 shadow-sm ov-card ov-summary-card ov-section-summary">
                <div class="flex items-center gap-4 mb-6">
                    <span class="ov-sec-icon ov-sec-icon-indigo"><i class="fa-solid fa-file-invoice"></i></span>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Order Summary</h3>
                </div>

                <div class="grid grid-cols-2 gap-y-6 gap-x-10 ov-summary-grid">
                    <div class="ov-kv">
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Payment Method</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litPayment" runat="server" /></span>
                    </div>
                    <div class="ov-kv">
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Order Total</span>
                        <span class="text-lg font-bold text-primary ov-order-total">₨ <asp:Literal ID="litTotalAmount" runat="server" /></span>
                    </div>
                    <div class="ov-kv">
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Quantity</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litQtyDisplay" runat="server" /></span>
                    </div>
                    <div class="ov-kv">
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Order Status</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litStatusUpper" runat="server" /></span>
                    </div>
                </div>

                <div class="ov-merged-block">
                    <div class="flex items-center gap-4 mb-4">
                        <span class="ov-sec-icon ov-sec-icon-blue"><i class="fa-solid fa-bolt-lightning"></i></span>
                        <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Order Actions</h3>
                    </div>
                    <div class="flex flex-wrap gap-3">
                        <asp:Button ID="btnAccept" runat="server" Visible="false" CssClass="ov-btn ov-btn-primary" Text="Process Order" OnClick="btnAccept_Click" />
                        <asp:Button ID="btnDelivering" runat="server" Visible="false" CssClass="ov-btn ov-btn-info" Text="Mark as In Transit" OnClick="btnDelivering_Click" />
                        <asp:Button ID="btnDelivered" runat="server" Visible="false" CssClass="ov-btn ov-btn-success" Text="Mark as Delivered" OnClick="btnDelivered_Click" />
                        <asp:Button ID="btnCancel" runat="server" Visible="false" CssClass="ov-btn ov-btn-danger" Text="Cancel Order" OnClick="btnCancel_Click" OnClientClick="return confirm('Cancel this order?');" />
                    </div>
                </div>

                <div class="ov-merged-block">
                    <div class="flex items-center gap-4 mb-4">
                        <span class="ov-sec-icon ov-sec-icon-purple"><i class="fa-solid fa-box-open"></i></span>
                        <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Order Items</h3>
                    </div>
                    <div class="overflow-x-auto">
                        <table class="w-full text-left ov-table">
                            <thead>
                                <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                                    <th class="px-8 py-4">Item</th>
                                    <th class="px-8 py-4 text-right">Unit Price</th>
                                    <th class="px-8 py-4 text-center">Volume</th>
                                    <th class="px-8 py-4 text-right">MAGNITUDE</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-gray-50">
                                <asp:Literal ID="litItemRows" runat="server" />
                            </tbody>
                        </table>
                    </div>
                </div>

                <div class="ov-merged-block ov-merged-block-last">
                    <div class="flex items-center gap-4 mb-4">
                        <span class="ov-sec-icon ov-sec-icon-slate"><i class="fa-solid fa-timeline"></i></span>
                        <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Order Timeline</h3>
                    </div>
                    <div class="overflow-x-auto">
                        <table class="w-full text-left ov-table">
                            <thead>
                                <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                                    <th class="px-8 py-4">EVENT DATE</th>
                                    <th class="px-8 py-4">Staff</th>
                                    <th class="px-8 py-4">Status</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-gray-50">
                                <asp:Literal ID="litLogRows" runat="server" />
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <!-- Sidebar: Right Section -->
        <div class="space-y-12 ov-sidebar">
            <div class="bg-white border border-gray-100 p-8 shadow-sm ov-card ov-contact-card">
                <div class="flex items-center gap-4 mb-6">
                    <span class="ov-sec-icon ov-sec-icon-green"><i class="fa-solid fa-user-tie"></i></span>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Customer Information</h3>
                </div>
                <div class="space-y-4">
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Customer Name</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litCustName" runat="server" /></span>
                    </div>
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Email Address</span>
                        <asp:HyperLink ID="lnkCustEmail" runat="server" CssClass="text-sm font-bold text-text-dark underline decoration-primary/30"></asp:HyperLink>
                    </div>
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Phone Number</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litCustPhone" runat="server" /></span>
                    </div>
                </div>

                <div class="ov-contact-split">
                    <div class="flex items-center gap-4 mb-6">
                    <span class="ov-sec-icon ov-sec-icon-orange"><i class="fa-solid fa-location-dot"></i></span>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Shipping Address</h3>
                    </div>
                    <div class="space-y-4">
                        <div>
                            <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Street Address</span>
                            <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litAddrLine" runat="server" /></span>
                        </div>
                        <div class="grid grid-cols-2 gap-5">
                            <div>
                                <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">City</span>
                                <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litTownship" runat="server" /></span>
                            </div>
                            <div>
                                <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Postal Code</span>
                                <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litPostal" runat="server" /></span>
                            </div>
                        </div>
                        <div class="grid grid-cols-2 gap-5">
                            <div>
                                <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">District</span>
                                <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litCity" runat="server" /></span>
                            </div>
                            <div>
                                <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">State/Province</span>
                                <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litState" runat="server" /></span>
                            </div>
                        </div>
                        <div>
                            <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Country</span>
                            <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litCountry" runat="server" /></span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    </div>
    </div>

    <script>
        (function () {
            var m = document.getElementById('<%= lblMsg.ClientID %>');
            if (m && m.textContent && m.textContent.trim().length > 0) {
                m.classList.remove('hidden');
                m.classList.add('block');
                const isError = m.textContent.toLowerCase().includes('sorry') || m.textContent.toLowerCase().includes('error');
                m.classList.add(isError ? 'bg-red-50' : 'bg-green-50');
                m.classList.add(isError ? 'border-red-500' : 'border-green-500');
                m.classList.add(isError ? 'text-red-700' : 'text-green-700');
            }

            var printBtn = document.getElementById('btnPrintInvoice');
            if (printBtn) printBtn.addEventListener('click', function () { window.print(); });

            var pdfBtn = document.getElementById('btnDownloadPdf');
            if (pdfBtn) pdfBtn.addEventListener('click', function () { window.print(); });

        })();
    </script>
</asp:Content>
