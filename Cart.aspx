<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" CodeFile="Cart.aspx.cs"
    Inherits="serena.Site.CartPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Shopping Cart | eGadgetHub</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    
    <section class="bg-off-white py-12 mb-12">
        <div class="container mx-auto px-4 lg:px-8">
            <h1 class="text-4xl font-serif mb-2">Shopping Cart</h1>
            <nav class="flex text-xs uppercase tracking-widest text-gray-400">
                <a runat="server" href="~/Default.aspx" class="hover:text-primary transition-colors">Home</a>
                <span class="mx-2">/</span>
                <span class="text-text-dark">Cart</span>
            </nav>
        </div>
    </section>

    <div class="container mx-auto px-4 lg:px-8 pb-24 text-text-dark">
        <!-- Empty Cart -->
        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="py-24 text-center">
            <div class="max-w-md mx-auto">
                <div class="w-24 h-24 bg-off-white rounded-full flex items-center justify-center mx-auto mb-8">
                    <i class="fa-solid fa-cart-shopping text-3xl text-gray-200"></i>
                </div>
                <h2 class="text-3xl font-serif mb-4">Your cart is empty</h2>
                <p class="text-gray-500 mb-8 leading-relaxed">Browse our catalog to discover the latest gadgets, accessories, and everyday essentials.</p>
                <a class="inline-block bg-primary text-white px-10 py-5 text-sm uppercase tracking-widest font-bold hover:bg-primary/90 transition-all" href="~/Catalog.aspx?page=1" runat="server">
                    Browse Catalog
                </a>
            </div>
        </asp:Panel>

        <!-- Cart Content -->
        <asp:Panel ID="pnlCart" runat="server" Visible="false">
            <div class="flex flex-col xl:flex-row gap-16">
                <!-- Items List -->
                <div class="w-full xl:w-2/3">
                    <div class="border-b border-gray-100 pb-6 mb-10 hidden md:flex items-center text-[10px] uppercase tracking-[0.2em] font-bold text-gray-400">
                        <div class="w-1/2">Product</div>
                        <div class="w-1/6 text-center">Price</div>
                        <div class="w-1/6 text-center">Quantity</div>
                        <div class="w-1/6 text-right">Subtotal</div>
                    </div>

                    <div class="space-y-12">
                        <asp:Repeater ID="rptCart" runat="server" OnItemCommand="rptCart_ItemCommand">
                            <ItemTemplate>
                                <div class="flex flex-col md:flex-row items-center gap-8 border-b border-gray-100 pb-12 last:border-0">
                                    <!-- Image & Name -->
                                    <div class="w-full md:w-1/2 flex items-center gap-6">
                                        <div class="w-24 h-32 bg-off-white flex-shrink-0">
                                            <a href='<%# GetProductUrl(Eval("name")) %>'>
                                                <img src='<%# GetProductImageUrl(Eval("image"), Eval("name")) %>'
                                                     alt='<%# Html(Eval("name")) %>'
                                                     class="w-full h-full object-cover" />
                                            </a>
                                        </div>
                                        <div>
                                            <h3 class="font-serif text-xl mb-1">
                                                <a href='<%# GetProductUrl(Eval("name")) %>' class="hover:text-primary transition-colors uppercase tracking-tight">
                                                    <%# Html(Eval("name")) %>
                                                </a>
                                            </h3>
                                            <div class="text-[10px] uppercase tracking-widest text-gray-400 flex items-center gap-2">
                                                Status: <span class="text-green-600 font-bold">In Stock</span>
                                            </div>
                                            <asp:LinkButton ID="btnRemove" runat="server" CommandName="Remove"
                                                CommandArgument='<%# Eval("id") %>'
                                                CssClass="text-[10px] uppercase tracking-widest font-bold text-red-400 hover:text-red-600 transition-colors mt-4 inline-block">
                                                <i class="fa-regular fa-trash-can mr-1"></i> Remove
                                            </asp:LinkButton>
                                            <asp:HiddenField ID="hfProductId" runat="server" Value='<%# Eval("id") %>' />
                                            <asp:HiddenField ID="hfStock" runat="server" Value='<%# Eval("stock") %>' />
                                        </div>
                                    </div>

                                    <!-- Price -->
                                    <div class="w-full md:w-1/6 text-center">
                                        <span class="md:hidden text-xs uppercase tracking-widest font-bold text-gray-400 mr-2">Item price:</span>
                                        <span class="font-medium">RS <%# Convert.ToDecimal(Eval("price")).ToString("N2") %></span>
                                    </div>

                                    <!-- Quantity -->
                                    <div class="w-full md:w-1/6 flex justify-center">
                                        <div class="flex items-center border border-gray-200 h-12">
                                            <asp:LinkButton ID="btnDec" runat="server" CommandName="Dec"
                                                CommandArgument='<%# Eval("id") %>'
                                                CssClass="w-10 h-full flex items-center justify-center hover:bg-off-white transition-colors">-</asp:LinkButton>
                                            <asp:TextBox ID="txtQty" runat="server" Text='<%# Eval("qty") %>'
                                                CssClass="w-12 h-full text-center text-sm font-bold focus:outline-none" />
                                            <asp:LinkButton ID="btnInc" runat="server" CommandName="Inc"
                                                CommandArgument='<%# Eval("id") %>'
                                                CssClass="w-10 h-full flex items-center justify-center hover:bg-off-white transition-colors">+</asp:LinkButton>
                                        </div>
                                    </div>

                                    <!-- Line Total -->
                                    <div class="w-full md:w-1/6 text-right">
                                        <span class="md:hidden text-xs uppercase tracking-widest font-bold text-gray-400 mr-2">Subtotal:</span>
                                        <span class="font-serif text-xl text-primary">RS <%# Convert.ToDecimal(Eval("subtotal")).ToString("N2") %></span>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <div class="mt-12 flex flex-col sm:flex-row justify-between gap-6">
                        <a class="inline-flex items-center text-[10px] uppercase tracking-widest font-bold text-primary group" href="~/Catalog.aspx?page=1" runat="server">
                            <i class="fa-solid fa-arrow-left mr-2 transition-transform group-hover:-translate-x-1"></i> Continue Shopping
                        </a>
                        <asp:LinkButton ID="btnClear" runat="server" CssClass="text-[10px] uppercase tracking-widest font-bold text-gray-400 hover:text-red-500 transition-colors" OnClick="btnClear_Click">
                            <i class="fa-solid fa-xmark mr-1"></i> Clear Entire Cart
                        </asp:LinkButton>
                    </div>
                </div>

                <!-- Order Summary Sidebar -->
                <div class="w-full xl:w-1/3">
                    <div class="bg-off-white p-10 sticky top-32">
                        <h2 class="text-2xl font-serif mb-8 pb-6 border-b border-gray-200">Order Summary</h2>
                        
                        <div class="space-y-6 mb-8 text-sm">
                            <div class="flex justify-between items-center text-gray-500">
                                <span>Items subtotal (<asp:Literal ID="litItemCount" runat="server" /> items)</span>
                                <span>RS <asp:Literal ID="litSubtotal" runat="server" /></span>
                            </div>
                            <div class="flex justify-between items-center text-gray-500">
                                <span>Shipping &amp; handling</span>
                                <span class="text-green-600 font-bold uppercase text-[10px] tracking-widest">Included</span>
                            </div>
                            <div class="flex justify-between items-center text-gray-500 pt-6 border-t border-gray-200">
                                <span class="uppercase tracking-[0.2em] font-bold text-text-dark">Order total</span>
                                <span class="text-2xl font-serif text-primary">RS <asp:Literal ID="litSubtotal2" runat="server" /></span>
                            </div>
                        </div>

                        <asp:HyperLink ID="lnkCheckout" runat="server" NavigateUrl="~/Account/Checkout.aspx" CssClass="block w-full bg-text-dark text-white text-center py-5 text-sm uppercase tracking-[0.3em] font-bold hover:bg-primary transition-all duration-500 mb-6 shadow-xl" Text="Secure Checkout" />

                        <div class="space-y-4 pt-6">
                            <div class="flex items-start gap-4 text-[10px] uppercase tracking-widest text-gray-400">
                                <i class="fa-solid fa-lock text-primary"></i>
                                <span>Your payment details are encrypted and processed securely.</span>
                            </div>
                             <div class="flex items-start gap-4 text-[10px] uppercase tracking-widest text-gray-400">
                                <i class="fa-solid fa-truck text-primary"></i>
                                <span>Estimated delivery: 5-10 business days.</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
