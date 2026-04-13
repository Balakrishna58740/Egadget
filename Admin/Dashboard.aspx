<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Dashboard.aspx.cs"
    Inherits="serena.Admin.Dashboard" %>

<asp:Content ID="c1" ContentPlaceHolderID="TitleContent" runat="server">Overview | eGadgetHub Admin</asp:Content>

<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
    
    <!-- Page Header -->
    <div class="admin-surface rounded-3xl p-6 sm:p-8 border border-blue-100 bg-gradient-to-r from-blue-100 via-white to-cyan-100">
        <div class="flex flex-col lg:flex-row lg:items-end lg:justify-between gap-6">
            <div>
                <p class="text-[10px] uppercase tracking-[0.3em] text-blue-500 font-bold mb-3">Admin overview</p>
                <h2 class="text-4xl font-bold text-text-dark mb-3 tracking-tight">Workspace Overview</h2>
                <p class="text-sm text-gray-500 max-w-2xl">Track daily orders, monitor revenue, and respond to low-stock items from a cleaner command center.</p>
            </div>
            <div class="flex gap-3 flex-wrap">
                <a class="inline-flex items-center gap-2 rounded-full bg-blue-600 text-white px-5 py-3 text-[10px] uppercase tracking-widest font-bold hover:bg-blue-700 transition-all shadow-lg shadow-blue-200" href="<%: ResolveUrl("~/Admin/Orders.aspx") %>">
                    <i class="fa-solid fa-receipt"></i> View Orders
                </a>
                <a class="inline-flex items-center gap-2 rounded-full border border-blue-200 bg-white px-5 py-3 text-[10px] uppercase tracking-widest font-bold text-blue-700 hover:bg-blue-50 transition-all" href="<%: ResolveUrl("~/Admin/Products.aspx") %>">
                    <i class="fa-solid fa-box"></i> Manage Products
                </a>
            </div>
        </div>
    </div>

    <!-- KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-5 mb-6">
        <!-- Card 1 -->
        <div class="admin-card p-8 transition-all duration-300 border-t-4 border-blue-500 bg-gradient-to-br from-white via-blue-50 to-sky-50">
            <div class="flex justify-between items-start mb-6">
                <div class="admin-stat-icon">
                    <i class="fa-solid fa-shopping-bag text-sm"></i>
                </div>
                <a class="text-[10px] uppercase tracking-widest font-bold text-primary hover:underline" href="<%: ResolveUrl("~/Admin/Orders.aspx?filter=today") %>">Details</a>
            </div>
            <div class="text-[10px] uppercase tracking-widest font-bold text-gray-400 mb-2">Orders Today</div>
            <div class="text-5xl font-bold text-text-dark tracking-tighter"><asp:Literal ID="litOrdersToday" runat="server" /></div>
        </div>

        <!-- Card 2 -->
        <div class="admin-card p-8 transition-all duration-300 border-t-4 border-blue-400 bg-gradient-to-br from-white via-blue-50 to-cyan-50">
            <div class="flex justify-between items-start mb-6">
                <div class="admin-stat-icon">
                    <i class="fa-solid fa-dollar-sign text-sm"></i>
                </div>
                <span class="text-[10px] uppercase tracking-widest font-bold text-gray-400">Excl. Cancelled</span>
            </div>
            <div class="text-[10px] uppercase tracking-widest font-bold text-gray-400 mb-2">Revenue Today</div>
            <div class="text-5xl font-bold text-text-dark tracking-tighter"><asp:Literal ID="litRevenueToday" runat="server" /></div>
        </div>

        <!-- Card 3 -->
        <div class="admin-card p-8 transition-all duration-300 border-t-4 border-sky-500 bg-gradient-to-br from-white via-sky-50 to-blue-50">
            <div class="flex justify-between items-start mb-6">
                <div class="admin-stat-icon">
                    <i class="fa-solid fa-clock text-sm"></i>
                </div>
                <a class="text-[10px] uppercase tracking-widest font-bold text-orange-400 hover:underline" href="<%: ResolveUrl("~/Admin/Orders.aspx?status=pending") %>">Process</a>
            </div>
            <div class="text-[10px] uppercase tracking-widest font-bold text-gray-400 mb-2">Pending Tasks</div>
            <div class="text-5xl font-bold text-text-dark tracking-tighter"><asp:Literal ID="litPending" runat="server" /></div>
        </div>

        <!-- Card 4 -->
        <div class="admin-card p-8 transition-all duration-300 border-t-4 border-indigo-500 bg-gradient-to-br from-white via-indigo-50 to-cyan-50">
            <div class="flex justify-between items-start mb-6">
                <div class="admin-stat-icon">
                    <i class="fa-solid fa-truck text-sm"></i>
                </div>
                <a class="text-[10px] uppercase tracking-widest font-bold text-indigo-400 hover:underline" href="<%: ResolveUrl("~/Admin/Orders.aspx?status=delivering") %>">Track</a>
            </div>
            <div class="text-[10px] uppercase tracking-widest font-bold text-gray-400 mb-2">In Transit</div>
            <div class="text-5xl font-bold text-text-dark tracking-tighter"><asp:Literal ID="litDelivering" runat="server" /></div>
        </div>
    </div>

    <div class="flex flex-col xl:flex-row gap-8">
        <!-- Latest Orders -->
        <div class="w-full xl:w-2/3">
            <div class="admin-card overflow-hidden border border-blue-100">
                <div class="px-8 py-6 border-b border-gray-100 flex items-center justify-between bg-off-white/50">
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Recent Transactions</h3>
                    <a class="text-[10px] uppercase tracking-widest font-bold text-primary hover:underline" href="<%: ResolveUrl("~/Admin/Orders.aspx") %>">All Orders</a>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left admin-table">
                        <thead>
                            <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-gray-50/50">
                                <th class="px-8 py-4">Ref Code</th>
                                <th class="px-8 py-4">Client</th>
                                <th class="px-8 py-4">Date</th>
                                <th class="px-8 py-4 text-right">Value</th>
                                <th class="px-8 py-4">Status</th>
                                <th class="px-8 py-4"></th>
                            </tr>
                        </thead>
                        <tbody id="latestOrdersBody" runat="server" class="divide-y divide-gray-50">
                            <asp:Literal ID="litLatestOrders" runat="server" />
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

        <!-- Low Stock -->
        <div class="w-full xl:w-1/3">
            <div class="admin-card overflow-hidden border border-blue-100">
                <div class="px-8 py-6 border-b border-gray-100 flex items-center justify-between bg-off-white/50">
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Stock Alerts</h3>
                    <a class="text-[10px] uppercase tracking-widest font-bold text-primary hover:underline" href="<%: ResolveUrl("~/Admin/Products.aspx") %>">Inventory</a>
                </div>
                <div class="p-0">
                    <table class="w-full text-left admin-table">
                        <thead>
                            <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-gray-50/50">
                                <th class="px-8 py-4">Product Name</th>
                                <th class="px-8 py-4 text-right">Available</th>
                                <th class="px-8 py-4"></th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-50 text-sm">
                            <asp:Literal ID="litLowStock" runat="server" />
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
