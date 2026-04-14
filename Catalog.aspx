<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" CodeFile="Catalog.aspx.cs"
    Inherits="serena.Site.CatalogPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Shop Gadgets | eGadgetHub</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    
    <!-- Hero Header for Catalog -->
    <section class="eg-surface py-12 mb-12">
        <div class="container mx-auto px-4 lg:px-8">
            <h1 class="text-4xl md:text-5xl font-serif font-semibold tracking-tight mb-2"><span class="eg-heading-band">Shop Gadgets</span></h1>
            <nav class="flex text-[10px] sm:text-xs uppercase tracking-[0.24em] text-gray-400 font-medium">
                <a runat="server" href="~/Default.aspx" class="hover:text-primary transition-colors">Home</a>
                <span class="mx-2">/</span>
                <span class="text-text-dark">Shop</span>
            </nav>
        </div>
    </section>

    <div class="container mx-auto px-4 lg:px-8 pb-24">
        <div class="flex flex-col lg:flex-row gap-16 lg:gap-20">

            <!-- Filters Sidebar -->
            <aside class="w-full lg:w-80 shrink-0 lg:sticky lg:top-28 h-fit bg-white border border-gray-100 p-8 shadow-sm space-y-10 eg-card">
                <!-- Search -->
                <div>
                    <h4 class="text-[10px] uppercase tracking-[0.28em] font-semibold mb-6 text-text-dark">Search</h4>
                    <div class="relative">
                        <asp:TextBox ID="txtSearch" runat="server"
                            CssClass="w-full bg-white border border-gray-100 px-4 py-3 pr-12 text-sm font-sans focus:border-primary focus:outline-none transition-colors"
                            onkeydown="if (event.key === 'Enter') { return applyCatalogFilters(); }"
                            placeholder="Type to search..."></asp:TextBox>
                        <asp:LinkButton ID="btnSearch" runat="server" OnClick="btnApply_Click"
                            OnClientClick="return applyCatalogFilters();"
                            CausesValidation="false"
                            CssClass="absolute right-4 top-1/2 -translate-y-1/2 text-gray-300 hover:text-primary transition-colors"
                            ToolTip="Apply search">
                            <i class="fa-solid fa-magnifying-glass"></i>
                        </asp:LinkButton>
                    </div>
                </div>

                <!-- Categories -->
                <div>
                    <h4 class="text-[10px] uppercase tracking-[0.28em] font-semibold mb-6 text-text-dark">Categories</h4>
                    <asp:RadioButtonList ID="rblCategories" runat="server" CssClass="space-y-4 text-sm text-gray-500 font-sans" RepeatLayout="UnorderedList"></asp:RadioButtonList>
                </div>

                <!-- Price Range -->
                <div>
                    <h4 class="text-[10px] uppercase tracking-[0.28em] font-semibold mb-6 text-text-dark">Price Range</h4>
                    <div class="grid grid-cols-[1fr_auto_1fr] items-center gap-3">
                        <asp:TextBox ID="txtPriceMin" runat="server" CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm font-sans focus:border-primary focus:outline-none text-center" placeholder="Min"></asp:TextBox>
                        <span class="text-gray-300" aria-hidden="true">&ndash;</span>
                        <asp:TextBox ID="txtPriceMax" runat="server" CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm font-sans focus:border-primary focus:outline-none text-center" placeholder="Max"></asp:TextBox>
                    </div>
                </div>

                <!-- Filter Actions -->
                <div class="flex flex-col gap-4">
                    <asp:Button ID="btnApply" runat="server" Text="Apply Filters" CssClass="bg-primary text-white text-xs uppercase tracking-widest font-bold py-4 hover:bg-primary/90 transition-all cursor-pointer" OnClick="btnApply_Click" OnClientClick="return applyCatalogFilters();" CausesValidation="false" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear All" CssClass="border border-gray-100 text-text-dark text-xs uppercase tracking-widest font-bold py-4 hover:bg-off-white transition-all cursor-pointer" OnClick="btnClear_Click" />
                </div>
            </aside>

            <!-- Products section -->
            <section class="flex-grow">
                <!-- Toolbar -->
                <div class="flex items-center justify-between mb-10 border-b border-gray-100 pb-6 gap-4 flex-wrap">
                    <p class="text-[10px] sm:text-sm text-gray-400 uppercase tracking-[0.24em] font-medium">Showing all results</p>
                    <div class="flex items-center gap-4">
                        <span class="text-[10px] uppercase tracking-[0.24em] text-gray-400 font-medium">Sort:</span>
                        <asp:DropDownList ID="ddlSort" runat="server" CssClass="text-[10px] sm:text-xs uppercase tracking-[0.22em] font-semibold font-sans focus:outline-none bg-transparent">
                            <asp:ListItem Text="Default Sorting" Value="name_asc" />
                            <asp:ListItem Text="Price: Low to High" Value="price_asc" />
                            <asp:ListItem Text="Price: High to Low" Value="price_desc" />
                            <asp:ListItem Text="Newest Arrivals" Value="newest" />
                        </asp:DropDownList>
                    </div>
                </div>

                <asp:ListView ID="lvProducts" runat="server"
                    OnItemCommand="lvProducts_ItemCommand">
                    <LayoutTemplate>
                        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-12">
                            <asp:PlaceHolder ID="itemPlaceholder" runat="server"></asp:PlaceHolder>
                        </div>
                    </LayoutTemplate>

                    <ItemTemplate>
                        <div class="group eg-card rounded-xl p-3 bg-white">
                            <!-- Image Container -->
                            <div class="relative aspect-[3/4] overflow-hidden bg-off-white mb-6 rounded-lg">
                                <a href='<%# GetProductUrl(Eval("name")) %>'>
                                    <img class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
                                         src='<%# GetProductImageUrl(Eval("image"), Eval("name")) %>'
                                         loading="lazy" decoding="async"
                                         alt='<%# Html(Eval("name")) %>' />
                                </a>
                                
                                <!-- Hover Actions -->
                                <div class="absolute inset-x-0 bottom-0 p-4 translate-y-full group-hover:translate-y-0 transition-transform duration-500 bg-white/90 backdrop-blur-sm border-t border-gray-100">
                                    <div class="flex items-center gap-2">
                                        <div class="flex items-center border border-gray-200 h-10">
                                            <asp:Button ID="btnMinus" runat="server" Text="-" CommandName="Decrement"
                                                CommandArgument='<%# Eval("id") %>' CssClass="w-8 h-full flex items-center justify-center hover:bg-off-white transition-colors cursor-pointer" />
                                            <asp:TextBox ID="txtQty" runat="server" Text="1" CssClass="w-10 h-full text-center text-xs focus:outline-none" />
                                            <asp:Button ID="btnPlus" runat="server" Text="+" CommandName="Increment"
                                                CommandArgument='<%# Eval("id") %>' CssClass="w-8 h-full flex items-center justify-center hover:bg-off-white transition-colors cursor-pointer" />
                                        </div>
                                        <asp:LinkButton ID="btnAdd" runat="server"
                                              CommandName="AddToCart" CommandArgument='<%# Eval("id") %>'
                                              CssClass="flex-grow bg-primary text-white text-[10px] uppercase tracking-widest font-bold h-10 flex items-center justify-center hover:bg-primary/90 transition-colors"
                                              Enabled='<%# Convert.ToInt32(Eval("stock")) > 0 %>'>
                                            <i class="fa-solid fa-cart-shopping mr-2"></i> Add To Cart
                                        </asp:LinkButton>
                                    </div>
                                </div>

                                <asp:Literal ID="litSoldOut" runat="server" Visible='<%# Convert.ToInt32(Eval("stock")) <= 0 %>'>
                                    <div class="absolute top-4 left-4 bg-text-dark text-white text-[10px] uppercase tracking-widest px-3 py-1">Sold Out</div>
                                </asp:Literal>
                            </div>

                            <!-- Info -->
                            <div class="text-center">
                                <div class="text-[10px] uppercase tracking-[0.2em] text-gray-400 mb-2">
                                     <%# Html(Eval("category_name")) %>
                                </div>
                                <h3 class="font-serif text-xl font-medium mb-2 tracking-tight">
                                    <a href='<%# GetProductUrl(Eval("name")) %>' class="hover:text-primary transition-colors">
                                        <%# Html(Eval("name")) %>
                                    </a>
                                </h3>
                                <div class="font-sans font-semibold text-primary tracking-wide">RS <%# Convert.ToDecimal(Eval("price")).ToString("N2") %></div>
                            </div>
                        </div>
                    </ItemTemplate>

                    <EmptyDataTemplate>
                        <div class="py-20 text-center text-gray-400">
                            <i class="fa-solid fa-magnifying-glass text-4xl mb-4 opacity-20"></i>
                            <p class="uppercase tracking-widest text-sm">No products matched your filters.</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:ListView>

                <!-- Custom Styled Pagination -->
                <div class="mt-20 flex justify-center border-t border-gray-100 pt-10">
                    <asp:Literal ID="litPager" runat="server"></asp:Literal>
                </div>
            </section>
        </div>
    </div>

    <script type="text/javascript">
        function applyCatalogFilters() {
            var search = document.getElementById('<%= txtSearch.ClientID %>');
            var minPrice = document.getElementById('<%= txtPriceMin.ClientID %>');
            var maxPrice = document.getElementById('<%= txtPriceMax.ClientID %>');
            var sort = document.getElementById('<%= ddlSort.ClientID %>');
            var category = document.querySelector('input[name*="rblCategories"]:checked');

            var qs = new URLSearchParams(window.location.search);
            qs.set('page', '1');
            qs.set('q', search ? (search.value || '').trim() : '');

            if (category && category.value) qs.set('cat', category.value);
            else qs.delete('cat');

            if (minPrice && minPrice.value.trim()) qs.set('min', minPrice.value.trim());
            else qs.delete('min');

            if (maxPrice && maxPrice.value.trim()) qs.set('max', maxPrice.value.trim());
            else qs.delete('max');

            if (sort && sort.value) qs.set('sort', sort.value);

            window.location.href = 'Catalog.aspx?' + qs.toString();
            return false;
        }

        function wireCatalogFilters() {
            var sort = document.getElementById('<%= ddlSort.ClientID %>');
            var radios = document.querySelectorAll('input[type="radio"][name*="rblCategories"]');

            if (sort) {
                sort.addEventListener('change', applyCatalogFilters);
            }

            for (var i = 0; i < radios.length; i++) {
                radios[i].addEventListener('change', applyCatalogFilters);
            }
        }

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', wireCatalogFilters);
        } else {
            wireCatalogFilters();
        }
    </script>
</asp:Content>
