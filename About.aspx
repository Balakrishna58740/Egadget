<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" CodeFile="About.aspx.cs"
    Inherits="serena.Site.AboutPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Our Story | Saja</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Hero Section -->
    <section class="relative py-32 overflow-hidden bg-off-white">
        <div class="container mx-auto px-4 lg:px-8 relative z-10">
            <div class="max-w-3xl">
                <span class="text-primary text-xs uppercase tracking-[0.3em] font-bold mb-6 block">Established 2024</span>
                <h1 class="text-5xl md:text-7xl font-serif mb-8 leading-tight">Crafting spaces, <br/>defining moments.</h1>
                <p class="text-lg text-gray-500 leading-relaxed mb-10 max-w-2xl">
                    eGadgetHub was founded on a simple principle: your home should be a reflection of your journey. We curate timeless furniture pieces that blend modern aesthetics with enduring comfort.
                </p>
                <div class="flex flex-wrap gap-6">
                    <a runat="server" href="~/Catalog.aspx" class="bg-primary text-white px-10 py-5 text-xs uppercase tracking-widest font-bold hover:bg-primary/90 transition-all">Explore Collection</a>
                    <a runat="server" href="~/Contact.aspx" class="border border-gray-200 text-text-dark px-10 py-5 text-xs uppercase tracking-widest font-bold hover:bg-white transition-all">Get in Touch</a>
                </div>
            </div>
        </div>
        <!-- Decorative Circle -->
        <div class="absolute -right-20 -top-20 w-96 h-96 bg-secondary/30 rounded-full blur-3xl opacity-50"></div>
    </section>

    <!-- Stats Section -->
    <section class="py-24 border-b border-gray-100">
        <div class="container mx-auto px-4 lg:px-8">
            <div class="grid grid-cols-2 lg:grid-cols-4 gap-12 text-center">
                <div>
                    <div class="text-4xl font-serif text-primary mb-2"><asp:Literal ID="litStatProducts" runat="server" /></div>
                    <div class="text-[10px] uppercase tracking-widest text-gray-400 font-bold">Unique Pieces</div>
                </div>
                <div>
                    <div class="text-4xl font-serif text-primary mb-2"><asp:Literal ID="litStatCategories" runat="server" /></div>
                    <div class="text-[10px] uppercase tracking-widest text-gray-400 font-bold">Curated Collections</div>
                </div>
                <div>
                    <div class="text-4xl font-serif text-primary mb-2"><asp:Literal ID="litStatPayments" runat="server" /></div>
                    <div class="text-[10px] uppercase tracking-widest text-gray-400 font-bold">Secure Gateways</div>
                </div>
                <div>
                    <div class="text-4xl font-serif text-primary mb-2">99%+</div>
                    <div class="text-[10px] uppercase tracking-widest text-gray-400 font-bold">Client Satisfaction</div>
                </div>
            </div>
        </div>
    </section>

    <!-- Our Story -->
    <section class="py-32">
        <div class="container mx-auto px-4 lg:px-8">
            <div class="flex flex-col lg:flex-row items-center gap-20">
                <div class="w-full lg:w-1/2">
                    <div class="relative group">
                        <div class="absolute -inset-4 bg-secondary/20 translate-x-4 translate-y-4 transition-transform group-hover:translate-x-6 group-hover:translate-y-6"></div>
                        <img src="https://images.unsplash.com/photo-1556228453-efd6c1ff04f6?q=80&w=1470&auto=format&fit=crop" 
                             alt="Interior Design" 
                             class="relative w-full aspect-[4/5] object-cover" />
                    </div>
                </div>
                <div class="w-full lg:w-1/2">
                    <span class="text-primary text-xs uppercase tracking-[0.3em] font-bold mb-6 block">Our Philosophy</span>
                    <h2 class="text-4xl font-serif mb-8 leading-tight">The intersection of <br/>artistry and purpose.</h2>
                    <div class="space-y-6 text-gray-500 leading-loose">
                        <p>
                            We believe that every piece of furniture tells a story. From the selection of premium materials to the final finishing touches, we prioritize quality and craftsmanship above all else.
                        </p>
                        <p>
                            Our journey began with a passion for interior spaces that inspire productivity and peace. Today, eGadgetHub stands as a destination for those who seek more than just furniture — those who seek to build a sanctuary.
                        </p>
                    </div>
                    <div class="mt-12 pt-12 border-t border-gray-100 flex items-center gap-6">
                        <div class="w-16 h-16 rounded-full overflow-hidden">
                            <img src="https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?q=80&w=1374&auto=format&fit=crop" class="w-full h-full object-cover" alt="Founder" />
                        </div>
                        <div>
                            <div class="font-serif text-lg italic">Alexander Thorne</div>
                            <div class="text-[10px] uppercase tracking-widest text-gray-400 font-bold">Creative Director & Founder</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- Stand For -->
    <section class="py-32 bg-off-white">
        <div class="container mx-auto px-4 lg:px-8 text-center mb-20">
            <h2 class="text-4xl font-serif mb-6">Our Core Values</h2>
            <div class="w-20 h-px bg-primary mx-auto"></div>
        </div>
        <div class="container mx-auto px-4 lg:px-8">
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
                <!-- Value 1 -->
                <div class="bg-white p-12 text-center group hover:-translate-y-2 transition-transform duration-500 shadow-sm">
                    <div class="w-16 h-16 bg-off-white flex items-center justify-center mx-auto mb-8 group-hover:bg-primary group-hover:text-white transition-colors duration-500">
                        <i class="fa-solid fa-gem text-xl"></i>
                    </div>
                    <h3 class="text-xs uppercase tracking-[0.2em] font-bold mb-4">Uncompromising Quality</h3>
                    <p class="text-sm text-gray-500 leading-relaxed">We source only the finest materials, ensuring every piece withstands the test of time.</p>
                </div>
                <!-- Value 2 -->
                <div class="bg-white p-12 text-center group hover:-translate-y-2 transition-transform duration-500 shadow-sm">
                    <div class="w-16 h-16 bg-off-white flex items-center justify-center mx-auto mb-8 group-hover:bg-primary group-hover:text-white transition-colors duration-500">
                        <i class="fa-solid fa-palette text-xl"></i>
                    </div>
                    <h3 class="text-xs uppercase tracking-[0.2em] font-bold mb-4">Artisanal Design</h3>
                    <p class="text-sm text-gray-500 leading-relaxed">Every collection is curated with an eye for modern aesthetics and timeless appeal.</p>
                </div>
                <!-- Value 3 -->
                <div class="bg-white p-12 text-center group hover:-translate-y-2 transition-transform duration-500 shadow-sm">
                    <div class="w-16 h-16 bg-off-white flex items-center justify-center mx-auto mb-8 group-hover:bg-primary group-hover:text-white transition-colors duration-500">
                        <i class="fa-solid fa-leaf text-xl"></i>
                    </div>
                    <h3 class="text-xs uppercase tracking-[0.2em] font-bold mb-4">Sustainable Ethos</h3>
                    <p class="text-sm text-gray-500 leading-relaxed">Conscious choices in production and packaging to respect our environment.</p>
                </div>
                <!-- Value 4 -->
                <div class="bg-white p-12 text-center group hover:-translate-y-2 transition-transform duration-500 shadow-sm">
                    <div class="w-16 h-16 bg-off-white flex items-center justify-center mx-auto mb-8 group-hover:bg-primary group-hover:text-white transition-colors duration-500">
                        <i class="fa-solid fa-heart text-xl"></i>
                    </div>
                    <h3 class="text-xs uppercase tracking-[0.2em] font-bold mb-4">eGadgetHub Care</h3>
                    <p class="text-sm text-gray-500 leading-relaxed">Our relationship with you begins at purchase and continues through your eGadgetHub journey.</p>
                </div>
            </div>
        </div>
    </section>

    <!-- How it works -->
    <section class="py-32">
        <div class="container mx-auto px-4 lg:px-8 flex flex-col items-center">
            <h2 class="text-4xl font-serif mb-20">The eGadgetHub Experience</h2>
            <div class="grid grid-cols-1 md:grid-cols-4 w-full">
                <div class="relative pb-12 md:pb-0 px-8 text-center border-b md:border-b-0 md:border-r border-gray-100 last:border-0">
                    <div class="text-5xl font-serif text-gray-100 mb-6">01</div>
                    <h4 class="text-xs uppercase tracking-widest font-bold mb-4">Curated Curation</h4>
                    <p class="text-xs text-gray-400 leading-relaxed">Explore our hand-picked collections for every room.</p>
                </div>
                <div class="relative py-12 md:py-0 px-8 text-center border-b md:border-b-0 md:border-r border-gray-100 last:border-0">
                    <div class="text-5xl font-serif text-gray-100 mb-6">02</div>
                    <h4 class="text-xs uppercase tracking-widest font-bold mb-4">Seamless Selection</h4>
                    <p class="text-xs text-gray-400 leading-relaxed">Effortless cart management and guest checkout options.</p>
                </div>
                <div class="relative py-12 md:py-0 px-8 text-center border-b md:border-b-0 md:border-r border-gray-100 last:border-0">
                    <div class="text-5xl font-serif text-gray-100 mb-6">03</div>
                    <h4 class="text-xs uppercase tracking-widest font-bold mb-4">Secure Checkout</h4>
                    <p class="text-xs text-gray-400 leading-relaxed">Multiple payment methods including bank and cash options.</p>
                </div>
                <div class="relative pt-12 md:pt-0 px-8 text-center last:border-0">
                    <div class="text-5xl font-serif text-gray-100 mb-6">04</div>
                    <h4 class="text-xs uppercase tracking-widest font-bold mb-4">White Glove Delivery</h4>
                    <p class="text-xs text-gray-400 leading-relaxed">Your piece arrives safely at your doorstep with full tracking.</p>
                </div>
            </div>
        </div>
    </section>

</asp:Content>
