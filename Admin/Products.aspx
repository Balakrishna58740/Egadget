<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Products.aspx.cs"
    Inherits="serena.Admin.ProductsPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Inventory | Saja Admin</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Page Header -->
    <div class="mb-12 flex justify-between items-end">
        <div>
            <h2 class="text-3xl font-serif mb-2">Product Inventory</h2>
            <p class="text-xs uppercase tracking-widest text-gray-400 font-bold">Manage your furniture collection</p>
        </div>
        <div class="text-right">
             <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold bg-white px-4 py-2 border border-gray-100"><asp:Literal ID="litTotal" runat="server" /> catalog items</span>
        </div>
    </div>

    <div class="flex flex-col lg:flex-row gap-12">
        <!-- Editor Column -->
        <div class="w-full lg:w-1/3">
            <div class="bg-white border border-gray-100 shadow-sm sticky top-28">
                <div class="px-8 py-6 border-b border-gray-100 bg-off-white/50">
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Management Studio</h3>
                </div>
                <div class="p-8">
                    <asp:Label ID="lblMsg" runat="server" CssClass="hidden mb-8 p-4 text-[10px] uppercase tracking-widest font-bold border-l-4" EnableViewState="false"></asp:Label>

                    <asp:HiddenField ID="hidId" runat="server" />

                    <div class="space-y-6">
                        <div>
                            <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Category</label>
                            <asp:DropDownList ID="ddlCat" runat="server" CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm focus:border-primary outline-none transition-colors"></asp:DropDownList>
                        </div>

                        <div>
                            <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Collection Name</label>
                            <asp:TextBox ID="txtName" runat="server" CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm focus:border-primary outline-none transition-colors" placeholder="e.g. Minimalist Oak Chair" />
                        </div>

                        <div>
                            <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Product Essence</label>
                            <asp:TextBox ID="txtDesc" runat="server" TextMode="MultiLine" Rows="3" CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm focus:border-primary outline-none transition-colors resize-none" placeholder="Describe the materials, craftsmanship..." />
                        </div>

                        <div class="grid grid-cols-2 gap-4">
                            <div>
                                <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Availability</label>
                                <asp:TextBox ID="txtStock" runat="server" TextMode="Number" CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm focus:border-primary outline-none transition-colors" />
                            </div>
                            <div>
                                <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Value (RS)</label>
                                <asp:TextBox ID="txtPrice" runat="server" TextMode="Number" CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm focus:border-primary outline-none transition-colors" />
                            </div>
                        </div>

                        <div class="flex items-center gap-3 py-2">
                            <asp:CheckBox ID="chkShow" runat="server" CssClass="w-4 h-4 accent-primary" />
                            <asp:Label runat="server" AssociatedControlID="chkShow" CssClass="text-[10px] uppercase tracking-widest font-bold text-text-dark cursor-pointer">Live on Studio Store</asp:Label>
                        </div>

                        <div>
                            <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Image Curating</label>
                            <div class="border border-dashed border-gray-200 p-4 text-center group hover:border-primary transition-colors cursor-pointer relative">
                                <asp:FileUpload ID="fuImg" runat="server" CssClass="absolute inset-0 opacity-0 cursor-pointer" />
                                <i class="fa-solid fa-cloud-upload-alt text-gray-300 group-hover:text-primary mb-2"></i>
                                <p class="text-[10px] uppercase tracking-widest font-bold text-gray-400">Choose Archive</p>
                            </div>
                            <div class="text-[8px] uppercase tracking-widest text-gray-300 mt-2"><asp:Literal ID="litImgHint" runat="server" /></div>
                        </div>

                        <div class="flex gap-4 pt-4">
                            <asp:Button ID="btnSave" runat="server" CssClass="flex-1 bg-admin-bg text-white text-[10px] uppercase tracking-widest font-bold py-4 hover:bg-primary transition-all cursor-pointer" Text="Save Archive" OnClick="btnSave_Click" />
                            <asp:Button ID="btnCancel" runat="server" CssClass="px-8 bg-white border border-gray-100 text-gray-400 text-[10px] uppercase tracking-widest font-bold py-4 hover:text-red-500 hover:border-red-100 transition-all cursor-pointer" Text="Discard" OnClick="btnCancel_Click" CausesValidation="false" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Catalog Column -->
        <div class="w-full lg:w-2/3 space-y-12">
            <!-- Filter Section -->
            <div class="bg-white border border-gray-100 p-8 shadow-sm">
                <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
                    <div>
                        <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Search Name</label>
                        <asp:TextBox ID="txtQ" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-2 text-xs focus:bg-white outline-none transition-all" />
                    </div>
                    <div>
                        <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Category</label>
                        <asp:DropDownList ID="ddlFilterCat" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-2 text-xs focus:bg-white outline-none transition-all"></asp:DropDownList>
                    </div>
                    <div>
                        <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Visibility</label>
                        <asp:DropDownList ID="ddlFilterShow" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-2 text-xs focus:bg-white outline-none transition-all">
                            <asp:ListItem Text="All Items" Value="" />
                            <asp:ListItem Text="Live Only" Value="1" />
                            <asp:ListItem Text="Hidden Archive" Value="0" />
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 block mb-2">Ordering</label>
                        <asp:DropDownList ID="ddlSort" runat="server" CssClass="w-full bg-off-white/50 border border-gray-50 px-4 py-2 text-xs focus:bg-white outline-none transition-all">
                            <asp:ListItem Text="Alpha (A-Z)" Value="name_asc" />
                            <asp:ListItem Text="Alpha (Z-A)" Value="name_desc" />
                            <asp:ListItem Text="Value (Low→High)" Value="price_asc" />
                            <asp:ListItem Text="Value (High→Low)" Value="price_desc" />
                            <asp:ListItem Text="Stock (Low→High)" Value="stock_asc" />
                            <asp:ListItem Text="Stock (High→Low)" Value="stock_desc" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="mt-8 flex gap-4">
                    <asp:Button ID="btnFilter" runat="server" CssClass="bg-primary text-white text-[10px] uppercase tracking-widest font-bold px-8 py-3 hover:bg-admin-bg transition-all cursor-pointer" Text="Refresh Results" OnClick="btnFilter_Click" />
                    <asp:Button ID="btnReset" runat="server" CssClass="text-gray-400 text-[10px] uppercase tracking-widest font-bold px-8 py-3 hover:text-primary transition-all cursor-pointer" Text="Reset Logic" OnClick="btnReset_Click" CausesValidation="false" />
                </div>
            </div>

            <!-- Data Table -->
            <div class="bg-white border border-gray-100 shadow-sm overflow-hidden">
                <div class="overflow-x-auto">
                    <table class="w-full text-left">
                        <thead>
                            <tr class="text-[10px] uppercase tracking-widest font-bold text-gray-400 border-b border-gray-100 bg-off-white/30">
                                <th class="px-8 py-4">Ref</th>
                                <th class="px-8 py-4">Masterpiece</th>
                                <th class="px-8 py-4">Collection</th>
                                <th class="px-8 py-4 text-right">Value</th>
                                <th class="px-8 py-4 text-right">Stock</th>
                                <th class="px-8 py-4">Live</th>
                                <th class="px-8 py-4 text-right">Settings</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-50">
                            <asp:Literal ID="litRows" runat="server" />
                        </tbody>
                    </table>
                </div>
                <!-- Pager -->
                <div class="px-8 py-6 border-t border-gray-50 bg-off-white/10">
                    <asp:Literal ID="pager" runat="server" />
                </div>
            </div>
        </div>
    </div>

    <!-- Image preview modal -->
    <div id="imgModal" class="hidden fixed inset-0 z-[100] flex items-center justify-center p-4 sm:p-6 bg-admin-bg/95 backdrop-blur-sm">
        <div class="relative max-w-4xl w-full">
            <button type="button" class="absolute -top-12 right-0 text-white hover:text-primary transition-colors text-2xl" onclick="closeImgModal()">
                <i class="fa-solid fa-times"></i>
            </button>
            <div class="bg-white p-2">
                <img id="imgPreview" src="" alt="Product image" class="w-full h-auto object-contain max-h-[80vh]" />
            </div>
        </div>
    </div>

    <script>
        function openImgModal(src) {
            const modal = document.getElementById('imgModal');
            const img = document.getElementById('imgPreview');
            img.src = src;
            modal.classList.remove('hidden');
            document.body.style.overflow = 'hidden';
        }

        function closeImgModal() {
            const modal = document.getElementById('imgModal');
            modal.classList.add('hidden');
            document.body.style.overflow = 'auto';
        }

        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.view-img');
            if (!btn) return;
            e.preventDefault();
            var src = btn.getAttribute('data-src');
            openImgModal(src);
        });

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
