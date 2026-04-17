<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" CodeFile="Wishlist.aspx.cs"
    Inherits="serena.Site.WishlistPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Wishlist | eGadgetHub</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <section class="eg-surface py-12 mb-12">
        <div class="container mx-auto px-4 lg:px-8">
            <h1 class="text-4xl font-serif mb-2">Wishlist</h1>
            <nav class="flex text-xs uppercase tracking-widest text-gray-400">
                <a runat="server" href="~/Default.aspx" class="hover:text-primary transition-colors">Home</a>
                <span class="mx-2">/</span>
                <span class="text-text-dark">Wishlist</span>
            </nav>
        </div>
    </section>

    <div class="container mx-auto px-4 lg:px-8 pb-24 text-text-dark">
        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="py-24 text-center">
            <div class="max-w-md mx-auto">
                <div class="w-24 h-24 bg-off-white rounded-full flex items-center justify-center mx-auto mb-8">
                    <i class="fa-regular fa-heart text-3xl text-gray-200"></i>
                </div>
                <h2 class="text-3xl font-serif mb-4">Your wishlist is empty</h2>
                <p class="text-gray-500 mb-8 leading-relaxed">Save products you love and review them later.</p>
                <a class="inline-block bg-primary text-white px-10 py-5 text-sm uppercase tracking-widest font-bold hover:bg-primary/90 transition-all" href="~/Catalog.aspx?page=1" runat="server">
                    Explore Products
                </a>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlWish" runat="server" Visible="false">
            <div class="eg-card bg-white rounded-2xl p-6 md:p-8">
                <div class="border-b border-gray-100 pb-6 mb-10 flex items-center justify-between gap-4 flex-wrap">
                    <h2 class="text-2xl font-serif">Saved Items</h2>
                    <asp:LinkButton ID="btnClear" runat="server" CssClass="text-[10px] uppercase tracking-widest font-bold text-gray-400 hover:text-red-500 transition-colors" OnClick="btnClear_Click">
                        <i class="fa-solid fa-xmark mr-1"></i> Clear Wishlist
                    </asp:LinkButton>
                </div>

                <div class="space-y-8">
                    <asp:Repeater ID="rptWish" runat="server" OnItemCommand="rptWish_ItemCommand">
                        <ItemTemplate>
                            <div class="flex flex-col md:flex-row items-center gap-6 border-b border-gray-100 pb-8 last:border-0">
                                <div class="w-full md:w-1/2 flex items-center gap-4">
                                    <div class="w-20 h-24 bg-off-white flex-shrink-0">
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
                                        <div class="text-[10px] uppercase tracking-widest text-gray-400"><%# Html(Eval("category_name")) %></div>
                                        <div class="font-semibold text-primary mt-1">RS <%# Convert.ToDecimal(Eval("price")).ToString("N2") %></div>
                                    </div>
                                </div>

                                <div class="w-full md:w-1/2 flex items-center justify-end gap-3">
                                    <asp:LinkButton ID="btnMoveToCart" runat="server" CommandName="MoveToCart" CommandArgument='<%# Eval("id") %>'
                                        CssClass="bg-primary text-white px-5 py-3 text-[10px] uppercase tracking-widest font-bold hover:bg-primary/90 transition-all">
                                        <i class="fa-solid fa-cart-shopping mr-1"></i> Move to Cart
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnRemove" runat="server" CommandName="Remove" CommandArgument='<%# Eval("id") %>'
                                        CssClass="border border-gray-200 text-gray-500 px-5 py-3 text-[10px] uppercase tracking-widest font-bold hover:bg-off-white transition-all">
                                        Remove
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
