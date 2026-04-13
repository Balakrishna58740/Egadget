<%@ Page Language="C#"
    MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true"
    CodeFile="Index.aspx.cs"
    Inherits="serena.Site.Account.Orders.OrderIndexPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">
  My Orders
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-12">
        <div class="max-w-6xl mx-auto">
            <!-- Header section -->
            <div class="flex flex-col md:flex-row md:items-end justify-between gap-6 mb-12">
                <div>
                    <h1 class="font-serif text-4xl mb-2">My Orders</h1>
                    <p class="text-gray-400 text-sm uppercase tracking-widest">Track and manage your purchases</p>
                </div>
                
                <div class="w-full md:w-auto">
                    <asp:Panel runat="server" CssClass="flex gap-0" DefaultButton="btnSearch">
                        <asp:TextBox ID="txtCode" runat="server" 
                            CssClass="border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary bg-off-white w-full md:w-64 transition-colors" 
                            TextMode="Search" 
                            placeholder="Order Code..." />
                        <asp:Button ID="btnSearch" runat="server" 
                            CssClass="bg-text-dark text-white px-6 py-3 text-xs uppercase tracking-widest font-bold hover:bg-black transition-all cursor-pointer" 
                            Text="Search" 
                            OnClick="btnSearch_Click" />
                    </asp:Panel>
                </div>
            </div>

            <!-- Dashboard / Filters -->
            <div class="border-b border-gray-100 pb-8 mb-8">
                <div class="flex flex-wrap gap-4">
                    <asp:Repeater ID="rptStatus" runat="server">
                        <ItemTemplate>
                            <a class='<%# Eval("CssClass") %> px-6 py-2 text-[10px] uppercase tracking-widest font-bold transition-all border' 
                               href='<%# Eval("Href") %>'>
                                <%# Eval("LabelUpper") %> (<%# Eval("Count") %>)
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:HyperLink ID="lnkClear" runat="server" 
                        CssClass="px-6 py-2 text-[10px] uppercase tracking-widest font-bold text-gray-400 hover:text-text-dark transition-colors" 
                        Text="Reset Filters" />
                </div>
            </div>

            <!-- Orders Table -->
            <asp:PlaceHolder ID="phEmpty" runat="server" Visible="false">
                <div class="bg-off-white p-16 text-center border border-dashed border-gray-200">
                    <i class="fa-solid fa-box-open text-4xl text-gray-200 mb-4 block"></i>
                    <p class="text-gray-400 text-sm uppercase tracking-widest">No orders found</p>
                </div>
            </asp:PlaceHolder>

            <asp:ListView ID="lvOrders" runat="server">
                <LayoutTemplate>
                    <div class="overflow-x-auto">
                        <table class="w-full text-left border-collapse">
                            <thead>
                                <tr class="border-b border-gray-100">
                                    <th class="py-6 text-[10px] uppercase tracking-[0.2em] font-bold text-gray-400">Order Code</th>
                                    <th class="py-6 text-[10px] uppercase tracking-[0.2em] font-bold text-gray-400">Status</th>
                                    <th class="py-6 text-[10px] uppercase tracking-[0.2em] font-bold text-gray-400">Date</th>
                                    <th class="py-6 text-[10px] uppercase tracking-[0.2em] font-bold text-gray-400 text-right">Items</th>
                                    <th class="py-6 text-[10px] uppercase tracking-[0.2em] font-bold text-gray-400 text-right">Amount</th>
                                    <th class="py-6 text-[10px] uppercase tracking-[0.2em] font-bold text-gray-400 text-right">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr id="itemPlaceholder" runat="server"></tr>
                            </tbody>
                        </table>
                    </div>
                </LayoutTemplate>

                <ItemTemplate>
                    <tr class="border-b border-gray-50 group hover:bg-off-white/50 transition-colors">
                        <td class="py-6">
                            <span class="text-sm font-bold tracking-tight text-text-dark">#<%# Eval("order_code") %></span>
                        </td>
                        <td class="py-6">
                            <span class='text-[9px] uppercase tracking-widest font-bold px-3 py-1 rounded-full <%# StatusBadgeCss(Eval("status")) %>'>
                                <%# Upper(Eval("status")) %>
                            </span>
                        </td>
                        <td class="py-6">
                            <span class="text-xs text-gray-500 uppercase tracking-widest"><%# FmtDate(Eval("order_date")) %></span>
                        </td>
                        <td class="py-6 text-right text-sm text-gray-500">
                            <%# Eval("total_qty") %>
                        </td>
                        <td class="py-6 text-right">
                            <span class="text-sm font-bold text-text-dark">RS <%# string.Format("{0:N2}", Eval("total_amount")) %></span>
                        </td>
                        <td class="py-6 text-right">
                            <a class="inline-block text-[10px] uppercase tracking-widest font-bold text-primary hover:opacity-70 transition-opacity" 
                               href="<%# UrlForCode(Eval("order_code")) %>">View Details</a>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:ListView>

            <!-- Pagination -->
            <div class="mt-12 flex justify-center">
                <div class="flex items-center gap-2">
                    <asp:DataPager ID="pager" runat="server" PagedControlID="lvOrders" PageSize="10" QueryStringField="page">
                        <Fields>
                            <asp:NextPreviousPagerField 
                                ShowFirstPageButton="false" ShowLastPageButton="false" 
                                ShowPreviousPageButton="true" ShowNextPageButton="false" 
                                ButtonType="Link" PreviousPageText="<i class='fa-solid fa-chevron-left'></i>" 
                                ButtonCssClass="w-10 h-10 border border-gray-200 flex items-center justify-center hover:bg-off-white transition-all text-xs" 
                                RenderDisabledButtonsAsLabels="true" />
                            <asp:NumericPagerField 
                                ButtonCount="5" ButtonType="Link" 
                                NumericButtonCssClass="w-10 h-10 border border-gray-200 flex items-center justify-center hover:bg-off-white transition-all text-xs" 
                                CurrentPageLabelCssClass="w-10 h-10 bg-text-dark text-white flex items-center justify-center text-xs font-bold" />
                            <asp:NextPreviousPagerField 
                                ShowFirstPageButton="false" ShowLastPageButton="false" 
                                ShowPreviousPageButton="false" ShowNextPageButton="true" 
                                ButtonType="Link" NextPageText="<i class='fa-solid fa-chevron-right'></i>" 
                                ButtonCssClass="w-10 h-10 border border-gray-200 flex items-center justify-center hover:bg-off-white transition-all text-xs" 
                                RenderDisabledButtonsAsLabels="true" />
                        </Fields>
                    </asp:DataPager>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
