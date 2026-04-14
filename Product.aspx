<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" CodeFile="Product.aspx.cs"
    Inherits="serena.Site.ProductPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal ID="litTitle" runat="server" /> | eGadgetHub
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    
    <section class="eg-surface py-12 mb-12">
        <div class="container mx-auto px-4 lg:px-8">
            <nav class="flex text-xs uppercase tracking-widest text-gray-400 mb-4">
                <a runat="server" href="~/Default.aspx" class="hover:text-primary transition-colors">Home</a>
                <span class="mx-2">/</span>
                <a runat="server" href="~/Catalog.aspx" class="hover:text-primary transition-colors">Shop</a>
                <span class="mx-2">/</span>
                <span class="text-text-dark truncate max-w-[200px]"><asp:Literal ID="litBreadcrumb" runat="server" /></span>
            </nav>
            <asp:HyperLink ID="lnkBack" runat="server" CssClass="inline-flex items-center text-[10px] uppercase tracking-widest font-bold text-primary group">
                <i class="fa-solid fa-arrow-left mr-2 transition-transform group-hover:-translate-x-1"></i> Back to collection
            </asp:HyperLink>
        </div>
    </section>

    <div class="container mx-auto px-4 lg:px-8 pb-24">
        <asp:Panel ID="pnlNotFound" runat="server" Visible="false" CssClass="py-24 text-center">
            <i class="fa-solid fa-circle-exclamation text-6xl text-gray-100 mb-6"></i>
            <h2 class="text-3xl font-serif mb-4">Product Not Found</h2>
            <p class="text-gray-500 mb-8">The piece you're looking for might have been moved or is no longer available.</p>
            <a class="bg-primary text-white px-8 py-4 text-xs uppercase tracking-widest font-bold" href="Catalog.aspx" runat="server">Explore Collection</a>
        </asp:Panel>

        <asp:Panel ID="pnlProduct" runat="server" Visible="false">
            <div class="flex flex-col lg:flex-row gap-16 eg-card bg-white rounded-2xl p-6 md:p-10">
                <!-- Product Gallery -->
                <div class="w-full lg:w-1/2">
                    <div class="sticky top-32">
                        <div class="bg-off-white overflow-hidden aspect-[4/5] rounded-xl">
                            <img id="imgProduct" runat="server" class="w-full h-full object-cover" alt="Product Image" />
                        </div>
                    </div>
                </div>

                <!-- Product Details -->
                <div class="w-full lg:w-1/2 lg:py-10">
                    <div class="mb-10">
                        <span class="text-primary text-xs uppercase tracking-[0.3em] font-bold mb-4 block">
                            <asp:Literal ID="litCategory" runat="server" />
                        </span>
                        <h1 class="text-4xl md:text-5xl font-serif mb-4 leading-tight">
                            <asp:Literal ID="litName" runat="server" />
                        </h1>
                        <div class="flex items-center gap-6 mb-8 pt-4 border-t border-gray-100">
                            <span class="text-3xl font-medium text-primary tracking-wide">
                                RS <asp:Literal ID="litPrice" runat="server" />
                            </span>
                            <span id="badgeStock" runat="server" class="text-[10px] uppercase tracking-widest px-3 py-1 bg-gray-100 text-gray-500 font-bold rounded-full"></span>
                        </div>
                    </div>

                    <div class="text-gray-500 leading-loose mb-12">
                        <asp:Literal ID="litDesc" runat="server" />
                    </div>

                    <!-- Actions -->
                    <div class="space-y-8 pt-8 border-t border-gray-100">
                        <div class="flex flex-col sm:flex-row items-center gap-4">
                            <!-- Qty Selector -->
                            <div class="flex items-center border border-gray-200 h-16 w-full sm:w-auto">
                                <asp:Button ID="btnMinus" runat="server" Text="-" CssClass="w-16 h-full flex items-center justify-center hover:bg-off-white transition-colors cursor-pointer text-xl" OnClick="btnMinus_Click" />
                                <asp:TextBox ID="txtQty" runat="server" Text="1" CssClass="w-20 h-full text-center font-bold focus:outline-none" />
                                <asp:Button ID="btnPlus" runat="server" Text="+" CssClass="w-16 h-full flex items-center justify-center hover:bg-off-white transition-colors cursor-pointer text-xl" OnClick="btnPlus_Click" />
                            </div>

                            <!-- Add to Cart -->
                            <asp:LinkButton ID="btnAdd" runat="server" CssClass="flex-grow h-16 bg-text-dark text-white text-xs uppercase tracking-[0.3em] font-bold flex items-center justify-center hover:bg-primary transition-all duration-500 disabled:opacity-50 disabled:cursor-not-allowed" OnClick="btnAdd_Click">
                                <i class="fa-solid fa-cart-shopping mr-3"></i> Add to cart
                            </asp:LinkButton>
                        </div>
                    </div>

                    <!-- Meta -->
                    <div class="mt-16 space-y-4 text-[10px] uppercase tracking-widest text-gray-400">
                        <div><span class="font-bold text-text-dark border-b border-gray-100 pb-2 mb-2 block">Our Promise</span></div>
                        <div class="flex items-center gap-4">
                            <i class="fa-solid fa-truck-fast text-primary"></i>
                            <span>Free delivery on orders over RS 500</span>
                        </div>
                        <div class="flex items-center gap-4">
                            <i class="fa-solid fa-shield-check text-primary"></i>
                            <span>2 Year limited warranty</span>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

    </div>
</asp:Content>
