<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="OrderView.aspx.cs"
    Inherits="serena.Admin.OrderViewPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">
    Archive Dossier | <asp:Literal ID="litTitleCode" runat="server" />
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hidId" runat="server" />

    <!-- Dossier Header -->
    <div class="mb-12 flex justify-between items-end">
        <div>
            <div class="flex items-center gap-3 mb-2">
                <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold">Transaction Reference</span>
                <span id="badgeStatus" runat="server" class="text-[8px] uppercase tracking-widest font-bold px-3 py-1 border"></span>
            </div>
            <h2 class="text-3xl font-serif mb-2">Ref: <asp:Literal ID="litOrderCode" runat="server" /></h2>
            <p class="text-xs uppercase tracking-widest text-gray-400 font-bold">Recorded on <asp:Literal ID="litOrderDate" runat="server" /></p>
        </div>
        <div>
            <a href="Orders.aspx" class="text-gray-400 text-[10px] uppercase tracking-widest font-bold px-8 py-3 bg-white border border-gray-100 hover:text-primary transition-all">
                Return to History
            </a>
        </div>
    </div>

    <asp:Label ID="lblMsg" runat="server" CssClass="hidden mb-12 p-4 text-[10px] uppercase tracking-widest font-bold border-l-4" EnableViewState="false"></asp:Label>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-12">
        <!-- Main Narrative: Left Section -->
        <div class="lg:col-span-2 space-y-12">
            
            <!-- Management Actions -->
            <div class="bg-white border border-gray-100 p-8 shadow-sm">
                <div class="flex items-center gap-4 mb-8">
                    <i class="fa-solid fa-bolt-lightning text-primary"></i>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Workflow Controls</h3>
                </div>
                <div class="flex flex-wrap gap-4">
                    <asp:Button ID="btnAccept" runat="server" Visible="false" CssClass="bg-admin-bg text-white text-[10px] uppercase tracking-widest font-bold px-8 py-3 hover:bg-primary transition-all cursor-pointer" Text="Process Transaction" OnClick="btnAccept_Click" />
                    <asp:Button ID="btnDelivering" runat="server" Visible="false" CssClass="bg-accent text-white text-[10px] uppercase tracking-widest font-bold px-8 py-3 hover:opacity-90 transition-all cursor-pointer" Text="Initiate Transit" OnClick="btnDelivering_Click" />
                    <asp:Button ID="btnDelivered" runat="server" Visible="false" CssClass="bg-primary text-white text-[10px] uppercase tracking-widest font-bold px-8 py-3 hover:opacity-90 transition-all cursor-pointer" Text="Confirm Completion" OnClick="btnDelivered_Click" />
                    <asp:Button ID="btnCancel" runat="server" Visible="false" CssClass="text-red-500 border border-red-500 text-[10px] uppercase tracking-widest font-bold px-8 py-3 hover:bg-red-500 hover:text-white transition-all cursor-pointer" Text="Void Transaction" OnClick="btnCancel_Click" OnClientClick="return confirm('Purge this record from history?');" />
                </div>
            </div>

            <!-- Detailed Specifications -->
            <div class="bg-white border border-gray-100 p-8 shadow-sm">
                <div class="flex items-center gap-4 mb-8">
                    <i class="fa-solid fa-file-invoice text-primary"></i>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Order Specification</h3>
                </div>
                <div class="grid grid-cols-2 gap-y-8 gap-x-12">
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Finance Method</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litPayment" runat="server" /></span>
                    </div>
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Magnitude Balance</span>
                        <span class="text-lg font-bold text-primary">RS <asp:Literal ID="litTotalAmount" runat="server" /></span>
                    </div>
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Item Volume</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litTotalQty" runat="server" /> units</span>
                    </div>
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Current Status</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litStatusUpper" runat="server" /></span>
                    </div>
                </div>
            </div>

            <!-- Manifest -->
            <div class="bg-white border border-gray-100 shadow-sm overflow-hidden">
                <div class="px-8 py-6 border-b border-gray-50 flex items-center gap-4">
                    <i class="fa-solid fa-box-open text-primary"></i>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Collection Manifest</h3>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left">
                        <thead>
                            <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                                <th class="px-8 py-4">Item</th>
                                <th class="px-8 py-4 text-right">Unit Price</th>
                                <th class="px-8 py-4 text-center">Volume</th>
                                <th class="px-8 py-4 text-right">Magnitude</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-50">
                            <asp:Literal ID="litItemRows" runat="server" />
                        </tbody>
                    </table>
                </div>
            </div>

            <!-- Chronicle -->
            <div class="bg-white border border-gray-100 shadow-sm overflow-hidden">
                <div class="px-8 py-6 border-b border-gray-50 flex items-center gap-4">
                    <i class="fa-solid fa-timeline text-primary"></i>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Event Chronicle</h3>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left">
                        <thead>
                            <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                                <th class="px-8 py-4">Event Date</th>
                                <th class="px-8 py-4">Authorized By</th>
                                <th class="px-8 py-4">Transition</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-50">
                            <asp:Literal ID="litLogRows" runat="server" />
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

        <!-- Sidebar: Right Section -->
        <div class="space-y-12">
            <!-- Client Dossier -->
            <div class="bg-white border border-gray-100 p-8 shadow-sm">
                <div class="flex items-center gap-4 mb-8">
                    <i class="fa-solid fa-user-tie text-primary"></i>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Client Signature</h3>
                </div>
                <div class="space-y-6">
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Full Legal Name</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litCustName" runat="server" /></span>
                    </div>
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Digital Correspondence</span>
                        <span class="text-sm font-bold text-text-dark underline decoration-primary/30"><asp:Literal ID="litCustEmail" runat="server" /></span>
                    </div>
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Direct Line</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litCustPhone" runat="server" /></span>
                    </div>
                </div>
            </div>

            <!-- Logistical Destination -->
            <div class="bg-white border border-gray-100 p-8 shadow-sm">
                <div class="flex items-center gap-4 mb-8">
                    <i class="fa-solid fa-location-dot text-primary"></i>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Logistical Destination</h3>
                </div>
                <div class="space-y-6">
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Street Architecture</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litAddrLine" runat="server" /></span>
                    </div>
                    <div class="grid grid-cols-2 gap-6">
                        <div>
                            <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Locality</span>
                            <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litTownship" runat="server" /></span>
                        </div>
                        <div>
                            <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Postal Registry</span>
                            <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litPostal" runat="server" /></span>
                        </div>
                    </div>
                    <div class="grid grid-cols-2 gap-6">
                        <div>
                            <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Urban Center</span>
                            <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litCity" runat="server" /></span>
                        </div>
                        <div>
                            <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Territory</span>
                            <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litState" runat="server" /></span>
                        </div>
                    </div>
                    <div>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-1">Sovereign Nation</span>
                        <span class="text-sm font-serif text-text-dark"><asp:Literal ID="litCountry" runat="server" /></span>
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
        })();
    </script>
</asp:Content>
