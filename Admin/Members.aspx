<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Members.aspx.cs"
    Inherits="serena.Admin.MembersPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Customers | eGadgetHub Admin</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="/Assets/css/admin-members-redesign.css" />
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="mem-page">
    <div class="mb-12 flex flex-col md:flex-row md:items-end justify-between gap-6 mem-header">
        <div>
            <h2 class="text-3xl font-serif mb-2">Customers</h2>
            <p class="text-xs uppercase tracking-widest text-gray-400 font-bold">Manage your customer database and track registrations.</p>
        </div>
        <div class="mem-header-actions">
            <button type="button" class="mem-btn-export"><i class="fa-solid fa-file-export"></i> Export</button>
            <button type="button" class="mem-btn-add"><i class="fa-solid fa-user-plus"></i> Add Customer</button>
        </div>
    </div>

    <div class="bg-white border border-gray-100 p-8 shadow-sm mb-12 mem-card mem-unified-card">
        <div class="flex items-center gap-4 mb-8">
            <i class="fa-solid fa-magnifying-glass text-primary"></i>
            <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">TRANSACTION SEARCH LOGIC</h3>
        </div>

        <div class="mem-filter-grid">
            <div class="mem-filter-field mem-filter-name">
                <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2 mem-label">Name</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="w-full bg-off-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all mem-input" MaxLength="255" placeholder="Search by name..." />
            </div>
            <div class="mem-filter-field">
                <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2 mem-label">CHRONICLE FROM</label>
                <asp:TextBox ID="dtFrom" runat="server" CssClass="w-full bg-off-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all mem-input" TextMode="Date" />
            </div>
            <div class="mem-filter-field">
                <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2 mem-label">CHRONICLE TO</label>
                <asp:TextBox ID="dtTo" runat="server" CssClass="w-full bg-off-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all mem-input" TextMode="Date" />
            </div>
            <div class="mem-filter-actions">
                <asp:Button ID="btnFilter" runat="server" CssClass="bg-admin-bg text-white text-[10px] uppercase tracking-widest font-bold py-4 hover:bg-primary transition-all cursor-pointer mem-btn-search" Text="EXECUTE FILTER" OnClick="btnFilter_Click" />
                <asp:Button ID="btnClear" runat="server" CssClass="bg-white text-gray-400 text-[10px] uppercase tracking-widest font-bold py-4 border border-gray-100 hover:text-primary transition-all cursor-pointer mem-btn-reset" Text="RESET STUDIO" OnClick="btnClear_Click" CausesValidation="false" />
            </div>
        </div>
        <div class="mem-table-wrap-block">
            <asp:Label ID="lblMsg" runat="server" CssClass="hidden mb-4 p-4 text-[10px] uppercase tracking-widest font-bold border-l-4 mem-alert" EnableViewState="false"></asp:Label>

            <div class="overflow-x-auto">
                <table class="w-full text-left mem-table customers-table">
                    <thead>
                        <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                            <th class="px-8 py-4 w-16">#</th>
                            <th class="px-8 py-4">Name</th>
                            <th class="px-8 py-4">Email</th>
                            <th class="px-8 py-4">Phone</th>
                            <th class="px-8 py-4">Joined</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-50">
                        <asp:Literal ID="litRows" runat="server" />
                    </tbody>
                </table>
            </div>
        </div>

        <div class="px-8 py-6 border-t border-gray-50 bg-off-white/10 mem-pager mem-table-footer">
            <asp:Literal ID="pager" runat="server" />
        </div>
    </div>
    </div>
</asp:Content>
