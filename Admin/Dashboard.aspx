<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master" AutoEventWireup="true" CodeFile="Dashboard.aspx.cs" Inherits="serena.Admin.Dashboard" %>

<asp:Content ID="c1" ContentPlaceHolderID="TitleContent" runat="server">Overview | eGadgetHub Admin</asp:Content>

<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="admin-content-stack db-page space-y-6">

        <%-- Redesigned: clean hero header and CTA layout. Kept: title/subtitle semantics and navigation actions. --%>
        <section class="db-hero rounded-2xl p-6 sm:p-8">
            <div class="db-hero-inner">
                <div>
                    <p class="db-kicker">Dashboard</p>
                    <h2 class="db-title">Admin Overview</h2>
                    <p class="db-subtitle">Monitor orders, revenue and stock from one clean panel.</p>
                </div>
                <div class="db-actions">
                    <a class="btn-view-orders" href="Orders.aspx">View Orders</a>
                    <a class="btn-manage-products" href="Products.aspx">Manage Products</a>
                </div>
            </div>
        </section>

        <%-- Redesigned: stat cards with icon chips, rounded corners, shadows, trend labels. Kept: all bound values and links. --%>
        <section class="db-stats-grid">
            <article class="db-stat-card accent-blue">
                <div class="db-stat-top">
                    <span class="db-stat-icon"><i class="fa-solid fa-shopping-bag"></i></span>
                    <a class="db-stat-link" href="Orders.aspx?filter=today">Details</a>
                </div>
                <div class="db-stat-label">Total Orders</div>
                <div class="db-stat-value"><asp:Literal ID="litOrdersToday" runat="server" /></div>
                <span class="db-stat-note note-blue">Orders today</span>
            </article>

            <article class="db-stat-card accent-green">
                <div class="db-stat-top">
                    <span class="db-stat-icon"><i class="fa-solid fa-dollar-sign"></i></span>
                    <span class="db-stat-note">Live</span>
                </div>
                <div class="db-stat-label">Revenue Today</div>
                <div class="db-stat-value"><asp:Literal ID="litRevenueToday" runat="server" /></div>
                <span class="db-stat-note note-green">Excluding canceled</span>
            </article>

            <article class="db-stat-card accent-amber">
                <div class="db-stat-top">
                    <span class="db-stat-icon"><i class="fa-solid fa-clock"></i></span>
                    <a class="db-stat-link" href="Orders.aspx?status=pending">Open</a>
                </div>
                <div class="db-stat-label">Pending Tasks</div>
                <div class="db-stat-value"><asp:Literal ID="litPending" runat="server" /></div>
                <span class="db-stat-note note-amber">Needs action</span>
            </article>

            <article class="db-stat-card accent-gray">
                <div class="db-stat-top">
                    <span class="db-stat-icon"><i class="fa-solid fa-truck"></i></span>
                    <a class="db-stat-link" href="Orders.aspx?status=delivering">Track</a>
                </div>
                <div class="db-stat-label">In Transit</div>
                <div class="db-stat-value"><asp:Literal ID="litDelivering" runat="server" /></div>
                <span class="db-stat-note note-gray">Delivery pipeline</span>
            </article>
        </section>

        <%-- Redesigned: 70/30 card layout for orders table and stock alerts. Kept: table rows, server literals, links and data flow. --%>
        <section class="db-main-grid">
            <div class="db-panel db-orders-panel">
                <div class="db-panel-head">
                    <h3>Recent Orders</h3>
                    <a href="Orders.aspx">View All</a>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left admin-table db-table db-orders-table">
                        <thead>
                            <tr>
                                <th class="px-7 py-4">Order ID</th>
                                <th class="px-7 py-4">Client</th>
                                <th class="px-7 py-4">Date</th>
                                <th class="px-7 py-4 text-right">Value</th>
                                <th class="px-7 py-4">Status</th>
                                <th class="px-7 py-4"></th>
                            </tr>
                        </thead>
                        <tbody id="latestOrdersBody" runat="server" class="divide-y divide-gray-50">
                            <asp:Literal ID="litLatestOrders" runat="server" />
                        </tbody>
                    </table>
                </div>
            </div>

            <div class="db-panel db-stock-panel">
                <div class="db-panel-head">
                    <h3>Stock Alerts</h3>
                    <a href="Products.aspx">Inventory</a>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left admin-table db-table db-stock-table">
                        <thead>
                            <tr>
                                <th class="px-7 py-4">Product</th>
                                <th class="px-7 py-4 text-right">Available</th>
                                <th class="px-7 py-4"></th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-50 text-sm">
                            <asp:Literal ID="litLowStock" runat="server" />
                        </tbody>
                    </table>
                </div>
            </div>
        </section>

    </div>
</asp:Content>
