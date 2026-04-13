<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" CodeFile="Default.aspx.cs"
    Inherits="serena.Site.HomePage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">eGadgetHub | Premium Electronics Store</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Hero Section -->
    <section class="relative h-[80vh] overflow-hidden">
        <div class="absolute inset-0">
            <img src="https://images.unsplash.com/photo-1498049794561-7780e7231661?ixlib=rb-4.0.3&auto=format&fit=crop&w=1920&q=80" class="w-full h-full object-cover" alt="Latest Electronics" />
            <div class="absolute inset-0 bg-gradient-to-r from-black/60 to-transparent"></div>
        </div>
        <div class="relative container mx-auto px-4 lg:px-8 h-full flex items-center">
            <div class="max-w-2xl animate-fade-in">
                <span class="text-blue-400 text-xs uppercase tracking-[0.3em] font-bold mb-4 block">New Technology 2026</span>
                <h1 class="text-white text-5xl md:text-7xl font-bold mb-8 leading-tight">Elevate Your Digital Life</h1>
                <p class="text-white/90 text-lg mb-10 leading-relaxed max-w-lg">
                    Discover our curated selection of high-performance gadgets, from the latest smartphones to powerful computing solutions.
                </p>
                
                <!-- Hero Search Bar -->
                <div class="relative max-w-lg mb-10">
                    <div class="flex bg-white rounded-full overflow-hidden shadow-2xl p-1">
                        <input type="text" id="heroSearch" name="q" placeholder="Search for gadgets, accessories..." class="w-full px-6 py-4 text-sm focus:outline-none" />
                        <button type="button" onclick="window.location.href='Catalog.aspx?q=' + document.getElementById('heroSearch').value" class="bg-primary text-white px-8 py-4 rounded-full text-sm font-bold hover:bg-primary/90 transition-all">
                            <i class="fa-solid fa-magnifying-glass mr-2"></i> Search
                        </button>
                    </div>
                </div>

                <div class="flex flex-col sm:flex-row space-y-4 sm:space-y-0 sm:space-x-4">
                    <a runat="server" href="~/Catalog.aspx" class="bg-primary text-white px-10 py-5 text-sm uppercase tracking-widest font-bold hover:bg-white hover:text-primary transition-all duration-500 inline-block text-center rounded-sm">Explore All Gadgets</a>
                    <a runat="server" href="~/Catalog.aspx?cat=1" class="bg-transparent border border-white text-white px-10 py-5 text-sm uppercase tracking-widest font-bold hover:bg-white hover:text-text-dark transition-all duration-500 inline-block text-center text-nowrap rounded-sm">Latest Phones</a>
                </div>
            </div>
        </div>
    </section>

    <!-- Categories Section -->
    <section class="py-24 bg-white">
        <div class="container mx-auto px-4 lg:px-8">
            <div class="text-center mb-16">
                <h2 class="text-4xl font-serif mb-4">Shop by Category</h2>
                <div class="w-20 h-0.5 bg-primary mx-auto"></div>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
                <!-- Smartphones -->
                <a href="Catalog.aspx?cat=smartphones" class="group relative aspect-video overflow-hidden rounded-xl">
                    <img src="https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80" class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110" alt="Smartphones" />
                    <div class="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent transition-colors duration-500"></div>
                    <div class="absolute bottom-6 left-6">
                        <h3 class="text-white text-2xl font-bold">Smartphones</h3>
                        <span class="text-blue-400 text-xs uppercase tracking-widest mt-2 block opacity-0 group-hover:opacity-100 transition-opacity duration-500">Shop Phones</span>
                    </div>
                </a>
                <!-- Laptops -->
                <a href="Catalog.aspx?cat=laptops" class="group relative aspect-video overflow-hidden rounded-xl">
                    <img src="https://images.unsplash.com/photo-1496181133206-80ce9b88a853?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80" class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110" alt="Laptops" />
                    <div class="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent transition-colors duration-500"></div>
                    <div class="absolute bottom-6 left-6">
                        <h3 class="text-white text-2xl font-bold">Laptops</h3>
                        <span class="text-blue-400 text-xs uppercase tracking-widest mt-2 block opacity-0 group-hover:opacity-100 transition-opacity duration-500">Shop Computing</span>
                    </div>
                </a>
                <!-- Audio -->
                <a href="Catalog.aspx?cat=audio" class="group relative aspect-video overflow-hidden rounded-xl">
                    <img src="https://images.unsplash.com/photo-1505740420928-5e560c06d30e?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80" class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110" alt="Audio & Headphones" />
                    <div class="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent transition-colors duration-500"></div>
                    <div class="absolute bottom-6 left-6">
                        <h3 class="text-white text-2xl font-bold">Audio & Audio</h3>
                        <span class="text-blue-400 text-xs uppercase tracking-widest mt-2 block opacity-0 group-hover:opacity-100 transition-opacity duration-500">Shop Sound</span>
                    </div>
                </a>
            </div>
        </div>
    </section>

    <!-- New Arrivals -->
    <section class="py-24 bg-off-white">
        <div class="container mx-auto px-4 lg:px-8">
            <div class="flex flex-col md:flex-row md:items-end justify-between mb-16 space-y-4 md:space-y-0">
                <div>
                    <span class="text-primary text-xs uppercase tracking-widest font-bold mb-2 block">Our latest additions</span>
                    <h2 class="text-4xl font-serif">New Arrivals</h2>
                </div>
                <a runat="server" href="~/Catalog.aspx" class="text-sm uppercase tracking-widest font-bold border-b border-primary pb-1 hover:text-primary transition-colors">View All Products</a>
            </div>
            
            <!-- Products Grid (Rendered from C#) -->
            <asp:Literal ID="litNew" runat="server"></asp:Literal>
        </div>
    </section>

    <!-- About / Mission Section -->
    <section class="py-24 bg-white overflow-hidden">
        <div class="container mx-auto px-4 lg:px-8">
            <div class="flex flex-col lg:flex-row items-center gap-16">
                <div class="lg:w-1/2 relative">
                    <div class="absolute -top-8 -left-8 w-48 h-48 bg-secondary -z-0"></div>
                    <img src="https://images.unsplash.com/photo-1594026112284-02bb6f3352fe?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80" class="relative z-10 w-full shadow-2xl" alt="Our Craft" />
                </div>
                <div class="lg:w-1/2">
                    <span class="text-primary text-xs uppercase tracking-widest font-bold mb-4 block">Our Story</span>
                    <h2 class="text-4xl md:text-5xl font-bold mb-8 leading-tight">Innovation at Your <span class="text-primary underline">Fingerprints</span></h2>
                    <p class="text-gray-500 text-lg mb-8 leading-relaxed">
                        At eGadgetHub, we are passionate about technology. We source the most innovative electronics from around the globe to ensure you have access to the future, today.
                    </p>
                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-8 mb-10">
                        <div class="flex items-start space-x-4">
                            <div class="w-12 h-12 bg-blue-50 flex items-center justify-center flex-shrink-0 rounded-lg">
                                <i class="fa-solid fa-bolt-lightning text-primary"></i>
                            </div>
                            <div>
                                <h4 class="font-bold text-sm uppercase tracking-widest mb-2">Fast Service</h4>
                                <p class="text-xs text-gray-400">Same-day delivery on selected electronic items.</p>
                            </div>
                        </div>
                        <div class="flex items-start space-x-4">
                            <div class="w-12 h-12 bg-blue-50 flex items-center justify-center flex-shrink-0 rounded-lg">
                                <i class="fa-solid fa-shield-halved text-primary"></i>
                            </div>
                            <div>
                                <h4 class="font-bold text-sm uppercase tracking-widest mb-2">Authentic</h4>
                                <p class="text-xs text-gray-400">100% genuine products with manufacturer warranty.</p>
                            </div>
                        </div>
                    </div>
                    <a runat="server" href="~/About.aspx" class="bg-text-dark text-white px-8 py-4 text-xs uppercase tracking-widest font-bold hover:bg-primary transition-all duration-300 inline-block">Read More About Us</a>
                </div>
            </div>
        </div>
    </section>

    <!-- Top Picks -->
    <section class="py-24 bg-off-white">
        <div class="container mx-auto px-4 lg:px-8">
            <div class="text-center mb-16">
                <span class="text-primary text-xs uppercase tracking-widest font-bold mb-2 block">Customer favorites</span>
                <h2 class="text-4xl font-serif mb-4">Top Picks For You</h2>
                <div class="w-20 h-0.5 bg-primary mx-auto"></div>
            </div>
            
            <asp:Literal ID="litTop" runat="server"></asp:Literal>
        </div>
    </section>

    <!-- Newsletter Section -->
    <section class="py-24 bg-primary relative overflow-hidden">
        <!-- Abstract Decoration -->
        <div class="absolute top-0 right-0 w-96 h-96 border border-white/5 rounded-full -translate-y-1/2 translate-x-1/2"></div>
        <div class="absolute bottom-0 left-0 w-64 h-64 border border-white/5 rounded-full translate-y-1/2 -translate-x-1/2"></div>
        
        <div class="container mx-auto px-4 lg:px-8 relative z-10">
            <div class="max-w-3xl mx-auto text-center">
                <h2 class="text-4xl font-serif text-white mb-6">Stay Inspired</h2>
                <p class="text-white/80 text-lg mb-12">Sign up for our newsletter to receive exclusive offers, new product announcements, and interior design tips.</p>
                <div class="flex flex-col sm:flex-row gap-4">
                    <input type="email" placeholder="Your email address" class="flex-grow bg-white border-0 px-8 py-5 text-sm focus:ring-2 focus:ring-accent focus:outline-none" />
                    <button type="button" class="bg-text-dark text-white px-10 py-5 text-sm uppercase tracking-[0.2em] font-bold hover:bg-accent transition-all duration-300">Subscribe</button>
                </div>
                <p class="text-white/40 text-[10px] uppercase tracking-widest mt-6">By subscribing you agree to our privacy policy and terms.</p>
            </div>
        </div>
    </section>

</asp:Content>
