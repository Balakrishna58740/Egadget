<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Orders.aspx.cs"
    Inherits="serena.Admin.OrdersPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Archive Transactions | Saja Admin</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Page Header -->
    <div class="mb-12 flex justify-between items-end">
        <div>
            <h2 class="text-3xl font-serif mb-2">Commerce History</h2>
            <p class="text-xs uppercase tracking-widest text-gray-400 font-bold">Monitor and manage client transactions</p>
        </div>
        <div class="text-right">
             <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold bg-white px-4 py-2 border border-gray-100"><asp:Literal ID="litTotal" runat="server" /> recorded events</span>
        </div>
    </div>

    <!-- Status Tracking Studio -->
    <div class="mb-12">
        <div class="flex flex-nowrap overflow-x-auto pb-4 gap-4 no-scrollbar">
            <asp:Literal ID="litStatusPills" runat="server" />
        </div>
    </div>

    <div class="space-y-12">
        <!-- Filter Studio -->
        <div class="bg-white border border-gray-100 p-8 shadow-sm">
            <div class="flex items-center gap-4 mb-8">
                <i class="fa-solid fa-sliders text-primary"></i>
                <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Transaction Search Logic</h3>
            </div>
            
            <asp:Label ID="lblMsg" runat="server" CssClass="hidden mb-8 p-4 text-[10px] uppercase tracking-widest font-bold border-l-4" EnableViewState="false"></asp:Label>

            <div class="grid grid-cols-1 md:grid-cols-4 gap-8">
                <div>
                    <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Ref Code</label>
                    <asp:TextBox ID="txtCode" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-3 text-xs focus:bg-white focus:border-primary outline-none transition-all" placeholder="e.g. #7721" />
                </div>
                <div>
                    <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Client Signature</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-3 text-xs focus:bg-white focus:border-primary outline-none transition-all" placeholder="Enter name..." />
                </div>
                <div>
                    <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Chronicle From</label>
                    <asp:TextBox ID="txtFrom" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-3 text-xs focus:bg-white focus:border-primary outline-none transition-all" TextMode="Date" />
                </div>
                <div>
                    <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Chronicle To</label>
                    <asp:TextBox ID="txtTo" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-3 text-xs focus:bg-white focus:border-primary outline-none transition-all" TextMode="Date" />
                </div>
            </div>

            <div class="mt-8 flex gap-4">
                <asp:Button ID="btnFilter" runat="server" CssClass="bg-admin-bg text-white text-[10px] uppercase tracking-widest font-bold px-8 py-3 hover:bg-primary transition-all cursor-pointer" Text="Execute Filter" OnClick="btnFilter_Click" />
                <asp:Button ID="btnReset" runat="server" CssClass="text-gray-400 text-[10px] uppercase tracking-widest font-bold px-8 py-3 hover:text-primary transition-all cursor-pointer" Text="Reset Studio" OnClick="btnReset_Click" CausesValidation="false" />
            </div>
        </div>

        <!-- Transaction Grid -->
        <div class="bg-white border border-gray-100 shadow-sm overflow-hidden">
            <div class="overflow-x-auto">
                <table class="w-full text-left">
                    <thead>
                        <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                            <th class="px-8 py-4">Ref</th>
                            <th class="px-8 py-4">Status</th>
                            <th class="px-8 py-4">Client</th>
                            <th class="px-8 py-4">Finance</th>
                            <th class="px-8 py-4 text-right">Magnitude</th>
                            <th class="px-8 py-4">Event Date</th>
                            <th class="px-8 py-4 text-right">Settings</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-50">
                        <asp:Literal ID="litRows" runat="server" />
                    </tbody>
                </table>
            </div>
            <!-- Pagination -->
            <div class="px-8 py-6 border-t border-gray-50 bg-off-white/10">
                <asp:Literal ID="pager" runat="server" />
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
