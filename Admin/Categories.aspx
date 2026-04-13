<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Categories.aspx.cs"
    Inherits="serena.Admin.CategoriesPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Archive Taxonomies | Saja Admin</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Header Section -->
    <div class="mb-12">
        <h2 class="text-3xl font-serif mb-2">Collection Taxonomies</h2>
        <p class="text-xs uppercase tracking-widest text-gray-400 font-bold">Curate and organize the aesthetic hierarchy of the collection.</p>
    </div>

    <asp:Label ID="lblMsg" runat="server" CssClass="hidden mb-12 p-4 text-[10px] uppercase tracking-widest font-bold border-l-4" EnableViewState="false"></asp:Label>

    <div class="grid grid-cols-1 lg:grid-cols-12 gap-12">
        <!-- Taxonomy Curator (Form) -->
        <div class="lg:col-span-4">
            <div class="bg-white border border-gray-100 p-8 shadow-sm sticky top-24">
                <div class="flex items-center gap-4 mb-8">
                    <i class="fa-solid fa-pen-nib text-primary"></i>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Taxonomy Curator</h3>
                </div>

                <asp:HiddenField ID="hidId" runat="server" />

                <div class="space-y-6">
                    <div>
                        <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Category Label</label>
                        <asp:TextBox ID="txtName" runat="server" placeholder="Enter collection name..." 
                            CssClass="w-full bg-off-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" MaxLength="255" />
                    </div>

                    <div class="pt-4 flex flex-col gap-3">
                        <asp:Button ID="btnSave" runat="server" CssClass="w-full bg-admin-bg text-white text-[10px] uppercase tracking-widest font-bold px-8 py-4 hover:bg-primary transition-all cursor-pointer shadow-lg shadow-primary/10" 
                            Text="Archive Taxonomy" OnClick="btnSave_Click" />
                        <asp:Button ID="btnCancel" runat="server" CssClass="w-full bg-white text-gray-400 text-[10px] uppercase tracking-widest font-bold px-8 py-4 border border-gray-100 hover:text-primary transition-all cursor-pointer" 
                            Text="Dismiss Changes" OnClick="btnCancel_Click" CausesValidation="false" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Taxonomy Repository (Table) -->
        <div class="lg:col-span-8">
            <div class="bg-white border border-gray-100 shadow-sm overflow-hidden">
                <div class="px-8 py-6 border-b border-gray-50 flex items-center justify-between">
                    <div class="flex items-center gap-4">
                        <i class="fa-solid fa-layer-group text-primary"></i>
                        <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Archive Repository</h3>
                    </div>
                </div>

                <div class="overflow-x-auto">
                    <table class="w-full text-left border-collapse">
                        <thead>
                            <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                                <th class="px-8 py-4 w-16">Ref</th>
                                <th class="px-8 py-4">Designation</th>
                                <th class="px-8 py-4 text-center">Volume</th>
                                <th class="px-8 py-4 text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-50">
                            <asp:Literal ID="litRows" runat="server" />
                        </tbody>
                    </table>
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
                const isError = m.textContent.toLowerCase().includes('sorry') || m.textContent.toLowerCase().includes('error') || m.textContent.toLowerCase().includes('failed');
                m.classList.add(isError ? 'bg-red-50' : 'bg-green-50');
                m.classList.add(isError ? 'border-red-500' : 'border-green-500');
                m.classList.add(isError ? 'text-red-700' : 'text-green-700');
            }
        })();
    </script>
</asp:Content>
