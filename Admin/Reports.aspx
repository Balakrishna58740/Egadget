<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Reports.aspx.cs"
    Inherits="serena.Admin.ReportsPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Archive Analytics | Saja Admin</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="grid grid-cols-1 lg:grid-cols-4 gap-12">
        <!-- Analytical Navigation (Sidebar) -->
        <div class="lg:col-span-1 space-y-4 no-print">
            <div class="bg-white border border-gray-100 shadow-sm overflow-hidden sticky top-24">
                <div class="px-6 py-4 border-b border-gray-50 flex items-center gap-3">
                    <i class="fa-solid fa-chart-pie text-primary"></i>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Analytics Menu</h3>
                </div>
                <div class="flex flex-col">
                    <a id="lnkTabProduct" runat="server" href="Reports.aspx?tab=product"
                        class="px-6 py-4 text-xs uppercase tracking-widest font-bold flex items-center gap-3 transition-all hover:bg-off-white group">
                        <i class="fa-solid fa-box text-gray-300 group-hover:text-primary"></i>
                        <span>Collection Magnitude</span>
                    </a>
                    <a id="lnkTabOrder" runat="server" href="Reports.aspx?tab=order"
                        class="px-6 py-4 text-xs uppercase tracking-widest font-bold flex items-center gap-3 transition-all hover:bg-off-white group border-t border-gray-50">
                        <i class="fa-solid fa-chart-line text-gray-300 group-hover:text-primary"></i>
                        <span>Fiscal Performance</span>
                    </a>
                </div>
            </div>
        </div>

        <!-- Intelligence Briefing Area -->
        <div class="lg:col-span-3 space-y-12">
            
            <!-- PRODUCT REPORT PANEL -->
            <asp:Panel ID="panProduct" runat="server" Visible="true">
                <div class="bg-white border border-gray-100 shadow-sm overflow-hidden">
                    <div class="px-8 py-6 border-b border-gray-50 flex items-center justify-between">
                        <div class="flex items-center gap-4">
                            <i class="fa-solid fa-box-open text-primary"></i>
                            <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Collection Magnitude Assessment</h3>
                        </div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold">
                            Chronicle Date: <asp:Literal ID="litProdGenerated" runat="server" />
                        </span>
                    </div>
                    <div class="p-8">
                        <div class="flex gap-4 mb-8 no-print">
                            <asp:Button ID="btnProdGet" runat="server" CssClass="bg-admin-bg text-white text-[10px] uppercase tracking-widest font-bold px-8 py-3 hover:bg-primary transition-all cursor-pointer" Text="Retrieve Data" OnClick="btnProdGet_Click" />
                            <asp:Button ID="btnProdPrint" runat="server" CssClass="bg-white text-gray-400 text-[10px] uppercase tracking-widest font-bold px-8 py-3 border border-gray-100 hover:text-primary transition-all cursor-pointer" 
                                Text="Export Dossier" OnClick="btnProdPrint_Click" />
                        </div>

                        <div class="mb-8"><asp:Literal ID="litProdSummary" runat="server" /></div>

                        <div class="overflow-x-auto">
                            <table class="w-full text-left">
                                <thead>
                                    <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                                        <th class="px-8 py-4 w-16">Ref</th>
                                        <th class="px-8 py-4">Designation</th>
                                        <th class="px-8 py-4 text-right">Volume Balance</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-gray-50">
                                    <asp:Literal ID="litProdRows" runat="server" />
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <!-- ORDER REVENUE REPORT PANEL -->
            <asp:Panel ID="panOrder" runat="server" Visible="false">
                <div class="bg-white border border-gray-100 shadow-sm overflow-hidden">
                    <div class="px-8 py-6 border-b border-gray-50 flex items-center justify-between">
                        <div class="flex items-center gap-4">
                            <i class="fa-solid fa-chart-column text-primary"></i>
                            <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Fiscal Performance Report</h3>
                        </div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold">
                            Assessment Period: <asp:Literal ID="litOrderPeriod" runat="server" />
                        </span>
                    </div>

                    <div class="p-8">
                        <!-- Investigation Controls -->
                        <div class="no-print space-y-8 mb-12 bg-off-white/30 p-8 border border-gray-50">
                            <div class="grid grid-cols-1 md:grid-cols-4 gap-8 items-end">
                                <div class="md:col-span-4">
                                    <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-4">Analytical Temporal Scope</label>
                                    <asp:RadioButtonList ID="rblType" runat="server" RepeatDirection="Horizontal" CssClass="premium-rbl">
                                        <asp:ListItem Text="Temporal Range" Value="range" Selected="True" />
                                        <asp:ListItem Text="Monthly Assessment" Value="monthly" />
                                        <asp:ListItem Text="Annual Overview" Value="yearly" />
                                    </asp:RadioButtonList>
                                </div>
                                <div class="md:col-span-4 grid grid-cols-1 md:grid-cols-2 gap-6">
                                    <div id="grpRange" runat="server" class="grid grid-cols-2 gap-4 col-span-2">
                                        <div>
                                            <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Genesis</label>
                                            <asp:TextBox ID="txtFrom" runat="server" TextMode="Date" CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" />
                                        </div>
                                        <div>
                                            <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Finality</label>
                                            <asp:TextBox ID="txtTo" runat="server" TextMode="Date" CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" />
                                        </div>
                                    </div>
                                    <div id="grpMonth" runat="server" class="hidden">
                                        <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Target Month</label>
                                        <asp:TextBox ID="txtMonth" runat="server" CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" />
                                    </div>
                                    <div id="grpYear" runat="server" class="hidden">
                                        <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Target Year</label>
                                        <asp:TextBox ID="txtYear" runat="server" CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" placeholder="e.g. 2024" />
                                    </div>
                                </div>
                            </div>

                            <div class="flex gap-4 pt-4">
                                <asp:Button ID="btnOrderGet" runat="server" CssClass="bg-admin-bg text-white text-[10px] uppercase tracking-widest font-bold px-8 py-4 hover:bg-primary transition-all cursor-pointer shadow-lg shadow-primary/10" Text="Generate Intelligence" OnClick="btnOrderGet_Click" />
                                <asp:Button ID="btnOrderPrint" runat="server" CssClass="bg-white text-gray-400 text-[10px] uppercase tracking-widest font-bold px-8 py-4 border border-gray-100 hover:text-primary transition-all cursor-pointer" Text="Export Performance Dossier" OnClick="btnOrderPrint_Click" />
                            </div>
                        </div>

                        <div class="mb-8 p-4 bg-primary/5 border-l-4 border-primary">
                            <asp:Literal ID="litOrderSummary" runat="server" />
                        </div>

                        <div class="overflow-x-auto">
                            <table class="w-full text-left">
                                <thead>
                                    <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                                        <th class="px-8 py-4 w-12">No.</th>
                                        <th class="px-8 py-4">Reference</th>
                                        <th class="px-8 py-4">Beneficiary</th>
                                        <th class="px-8 py-4">Financial Method</th>
                                        <th class="px-8 py-4 text-right">Magnitude</th>
                                        <th class="px-8 py-4">Registry Date</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-gray-50">
                                    <asp:Literal ID="litOrderRows" runat="server" />
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </div>

    <!-- Analytics Styles -->
    <style>
        .premium-rbl { width: 100%; }
        .premium-rbl input[type="radio"] { position: absolute; opacity: 0; }
        .premium-rbl label { 
            display: inline-block; 
            padding: 0.5rem 1.5rem; 
            border: 1px solid #f3f4f6; 
            background: #ffffff; 
            color: #9ca3af; 
            font-size: 10px; 
            font-weight: 700; 
            text-transform: uppercase; 
            letter-spacing: 0.1em; 
            cursor: pointer; 
            transition: all 0.2s;
            margin-right: 0.5rem;
        }
        .premium-rbl input[type="radio"]:checked + label { 
            background: #1a1a1a; 
            color: #ffffff; 
            border-color: #1a1a1a; 
            box-shadow: 0 4px 12px rgba(26,26,26,0.1);
        }
        .premium-rbl label:hover { border-color: #f1b305; color: #f1b305; }
        
        .active-tab { 
            background: #f9fafb; 
            color: #f1b305 !important; 
            border-left: 3px solid #f1b305; 
        }
        
        @media print { .no-print { display:none !important; } }
    </style>

    <script>
        (function () {
            function sync() {
                var sel = document.querySelector('input[name$="rblType"]:checked');
                var v = sel ? sel.value : 'range';
                var r = document.getElementById('<%= grpRange.ClientID %>');
                var m = document.getElementById('<%= grpMonth.ClientID %>');
                var y = document.getElementById('<%= grpYear.ClientID %>');
                if (r && m && y) {
                    r.classList.toggle('hidden', v !== 'range');
                    m.classList.toggle('hidden', v !== 'monthly');
                    y.classList.toggle('hidden', v !== 'yearly');
                }
            }
            document.addEventListener('change', function (e) {
                if (e.target && e.target.name && e.target.name.indexOf('rblType') !== -1) sync();
            });
            sync();

            // Set active tab style
            const urlParams = new URLSearchParams(window.location.search);
            const tab = urlParams.get('tab') || 'product';
            const tabEl = document.getElementById(tab === 'product' ? '<%= lnkTabProduct.ClientID %>' : '<%= lnkTabOrder.ClientID %>');
            if (tabEl) tabEl.classList.add('active-tab');

            // HTML5 month input
            var el = document.getElementById('<%= txtMonth.ClientID %>');
            if (el) el.setAttribute('type', 'month');
        })();

        function printTemplate(url) {
            try {
                var f = document.getElementById('printFrame');
                if (!f) {
                    f = document.createElement('iframe');
                    f.id = 'printFrame';
                    f.style.position = 'fixed';
                    f.style.right = '0';
                    f.style.bottom = '0';
                    f.style.width = '0';
                    f.style.height = '0';
                    f.style.border = '0';
                    f.setAttribute('aria-hidden', 'true');
                    document.body.appendChild(f);
                }
                var u = url + (url.indexOf('?') > -1 ? '&' : '?') + '_ts=' + new Date().getTime();
                f.src = u;
            } catch (e) { console && console.warn && console.warn(e); }
        }
    </script>
</asp:Content>
