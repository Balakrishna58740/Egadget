<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master" AutoEventWireup="true" CodeFile="Checkout.aspx.cs" Inherits="serena.Site.Account.CheckoutPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-12 checkout-page">
        <div class="max-w-6xl mx-auto">
            <h1 class="font-serif text-4xl mb-12 text-center md:text-left">Checkout</h1>

            <div ID="alertMsg" runat="server" visible="false" class="bg-red-50 text-red-500 p-4 mb-8 text-sm uppercase tracking-widest font-bold"></div>

            <asp:PlaceHolder ID="phEmptyCart" runat="server" Visible="false">
                <div class="bg-off-white p-16 text-center border border-dashed border-gray-200">
                    <i class="fa-solid fa-shopping-bag text-4xl text-gray-200 mb-4 block"></i>
                    <h2 class="font-serif text-2xl mb-4">Your cart is empty</h2>
                    <p class="text-gray-400 text-sm uppercase tracking-widest mb-8">Add some treasures before proceeding</p>
                    <a href="<%= ResolveUrl("~/Catalog.aspx") %>" class="inline-block bg-primary text-white px-10 py-4 text-xs uppercase tracking-widest font-bold hover:bg-primary/90 transition-all">
                        Discover Shop
                    </a>
                </div>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="phCheckout" runat="server" Visible="false">
                <div class="grid grid-cols-1 lg:grid-cols-12 gap-12 eg-card bg-white rounded-2xl p-6 md:p-8">
                    
                    <!-- Left: Shipping & Payment -->
                    <div class="lg:col-span-7 space-y-12">
                        <!-- Shipping -->
                        <section>
                            <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Shipping Information</h3>
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                                <div class="md:col-span-1">
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Recipient Name <span class="text-red-500">*</span></label>
                                    <input ID="txtShipName" runat="server" type="text" class="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors bg-off-white" maxlength="255" />
                                </div>
                                <div class="md:col-span-1">
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Phone Number</label>
                                    <input ID="txtShipPhone" runat="server" type="text" class="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors bg-off-white" maxlength="50" />
                                </div>
                                <div class="md:col-span-2">
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Street Address <span class="text-red-500">*</span></label>
                                    <textarea ID="txtAddress" runat="server" class="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors bg-off-white" rows="3"></textarea>
                                </div>
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Township</label>
                                    <input ID="txtTownship" runat="server" type="text" class="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors bg-off-white" maxlength="255" />
                                </div>
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Postal Code</label>
                                    <input ID="txtPostal" runat="server" type="text" class="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors bg-off-white" maxlength="20" />
                                </div>
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">City</label>
                                    <input ID="txtCity" runat="server" type="text" class="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors bg-off-white" maxlength="100" />
                                </div>
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">State / Province</label>
                                    <input ID="txtState" runat="server" type="text" class="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors bg-off-white" maxlength="100" />
                                </div>
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 font-bold mb-2">Country <span class="text-red-500">*</span></label>
                                    <input ID="txtCountry" runat="server" type="text" class="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors bg-off-white" maxlength="100" />
                                </div>
                            </div>
                        </section>

                        <!-- Payment -->
                        <section>
                            <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Payment Method</h3>
                            <div class="bg-off-white p-8 border border-gray-100">
                                <asp:RadioButtonList ID="rblPayment" runat="server" 
                                    RepeatDirection="Vertical" RepeatLayout="UnorderedList" 
                                    CssClass="space-y-4 checkout-payment-list" />
                                <p class="text-[10px] text-gray-400 mt-4 uppercase tracking-widest italic">All transactions are secure and encrypted.</p>
                            </div>
                        </section>
                    </div>

                    <!-- Right: Order Summary Sticky -->
                    <div class="lg:col-span-5">
                        <div class="lg:sticky lg:top-32 space-y-6">
                            <div class="bg-white border border-gray-100 shadow-xl p-8 eg-card rounded-2xl">
                                <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Summary</h3>
                                
                                <div class="max-h-[400px] overflow-y-auto mb-8 pr-2 custom-scrollbar">
                                    <asp:Literal ID="litCartTable" runat="server"></asp:Literal>
                                </div>

                                <div class="space-y-4 mb-8">
                                    <div class="flex justify-between items-center text-xs uppercase tracking-widest text-gray-400">
                                        <span>Items Count</span>
                                        <span class="text-text-dark font-bold"><asp:Literal ID="litItemsCount" runat="server" /></span>
                                    </div>
                                    <div class="flex justify-between items-center text-xs uppercase tracking-widest text-gray-400">
                                        <span>Shipping</span>
                                        <span class="text-green-600 font-bold">FREE</span>
                                    </div>
                                    <div class="pt-4 border-t border-gray-100 flex justify-between items-center">
                                        <span class="font-serif text-xl">Total</span>
                                        <span class="font-bold text-2xl text-primary">RS <asp:Literal ID="litSubtotal" runat="server" /></span>
                                    </div>
                                </div>

                                <asp:Button ID="btnPlaceOrder" runat="server" 
                                    Text="Place Order" 
                                    data-loading-text="Placing Order..."
                                    CssClass="w-full bg-primary text-white py-5 text-sm uppercase tracking-widest font-bold hover:bg-primary/90 transition-all cursor-pointer shadow-lg shadow-primary/20" 
                                    OnClick="btnPlaceOrder_Click" 
                                    UseSubmitBehavior="false" />
                                
                                <p class="text-[9px] text-gray-400 mt-4 text-center uppercase tracking-[0.2em]">
                                    By placing an order, you agree to our 
                                    <a href="#" class="underline hover:text-primary">Terms & Conditions</a>
                                </p>
                            </div>

                            <div class="bg-off-white/50 border border-gray-100 p-6 flex items-center gap-4">
                                <div class="w-12 h-12 bg-white rounded-full flex items-center justify-center text-primary border border-gray-100">
                                    <i class="fa-solid fa-shield-halved text-xl"></i>
                                </div>
                                <div>
                                    <p class="text-[10px] uppercase tracking-widest font-bold mb-1">Secure Checkout</p>
                                    <p class="text-[9px] text-gray-400 uppercase tracking-widest">Your data is protected by 256-bit SSL encryption</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </asp:PlaceHolder>
        </div>
    </div>

    <style>
        /* Specific override for RadioButtonList styling in Checkout */
        .checkout-payment-list { list-style: none; padding: 0; margin: 0; }
        .checkout-payment-list li { display: flex; align-items: center; gap: 1rem; border: 1px solid #eee; padding: 1rem; background: #fff; transition: all 0.3s; }
        .checkout-payment-list li:hover { border-color: #8B7355; }
        .checkout-payment-list input[type="radio"] { accent-color: #8B7355; width: 1.2rem; height: 1.2rem; }
        .checkout-payment-list label { font-size: 0.875rem; font-weight: 500; cursor: pointer; flex-grow: 1; margin: 0; padding: 0; }
        
        .custom-scrollbar::-webkit-scrollbar { width: 4px; }
        .custom-scrollbar::-webkit-scrollbar-track { background: #f1f1f1; }
        .custom-scrollbar::-webkit-scrollbar-thumb { background: #d1d1d1; }
        .custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #8B7355; }
    </style>
</asp:Content>
