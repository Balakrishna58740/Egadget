<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Members.aspx.cs"
    Inherits="serena.Admin.MembersPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Patron Dossier | Saja Admin</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Community Narrative Header -->
    <div class="mb-12 flex flex-col md:flex-row md:items-end justify-between gap-6">
        <div>
            <h2 class="text-3xl font-serif mb-2">Patron Community</h2>
            <p class="text-xs uppercase tracking-widest text-gray-400 font-bold">A chronicle of the individuals who appreciate the Saja philosophy.</p>
        </div>
        <div class="flex gap-4">
            <asp:Literal ID="litTopStats" runat="server" />
        </div>
    </div>

    <!-- Filter Mechanism -->
    <div class="bg-white border border-gray-100 p-8 shadow-sm mb-12">
        <div class="flex items-center gap-4 mb-8">
            <i class="fa-solid fa-magnifying-glass text-primary"></i>
            <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Community Search</h3>
        </div>
        <div class="grid grid-cols-1 md:grid-cols-4 lg:grid-cols-12 gap-6 items-end">
            <div class="lg:col-span-4">
                <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Legal Identity</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="w-full bg-off-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" MaxLength="255" placeholder="Search by name..." />
            </div>
            <div class="lg:col-span-3">
                <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Archive From</label>
                <asp:TextBox ID="dtFrom" runat="server" CssClass="w-full bg-off-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" TextMode="Date" />
            </div>
            <div class="lg:col-span-3">
                <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Archive To</label>
                <asp:TextBox ID="dtTo" runat="server" CssClass="w-full bg-off-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" TextMode="Date" />
            </div>
            <div class="lg:col-span-2 flex gap-2">
                <asp:Button ID="btnFilter" runat="server" CssClass="flex-1 bg-admin-bg text-white text-[10px] uppercase tracking-widest font-bold py-4 hover:bg-primary transition-all cursor-pointer" Text="Search" OnClick="btnFilter_Click" />
                <asp:Button ID="btnClear" runat="server" CssClass="flex-1 bg-white text-gray-400 text-[10px] uppercase tracking-widest font-bold py-4 border border-gray-100 hover:text-primary transition-all cursor-pointer" Text="Reset" OnClick="btnClear_Click" CausesValidation="false" />
            </div>
        </div>
    </div>

    <!-- Member Repository -->
    <div class="bg-white border border-gray-100 shadow-sm overflow-hidden">
        <div class="px-8 py-6 border-b border-gray-50 flex items-center justify-between">
            <div class="flex items-center gap-4">
                <i class="fa-solid fa-address-book text-primary"></i>
                <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Patron Registry</h3>
            </div>
        </div>

        <div class="overflow-x-auto">
            <table class="w-full text-left">
                <thead>
                    <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                        <th class="px-8 py-4 w-16">Ref</th>
                        <th class="px-8 py-4">Patron Identity</th>
                        <th class="px-8 py-4">Digital Signature</th>
                        <th class="px-8 py-4">Communication</th>
                        <th class="px-8 py-4">Registry Date</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-gray-50">
                    <asp:Literal ID="litRows" runat="server" />
                </tbody>
            </table>
        </div>

        <!-- Chronicle Pager -->
        <div class="px-8 py-6 border-t border-gray-50 bg-off-white/10">
            <asp:Literal ID="pager" runat="server" />
        </div>
    </div>
</asp:Content>
