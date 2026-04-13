<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" CodeFile="Contact.aspx.cs"
    Inherits="serena.Site.ContactPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Contact Us | eGadgetHub</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Hero Header -->
    <section class="bg-off-white py-20 mb-12">
        <div class="container mx-auto px-4 lg:px-8 text-center">
            <h1 class="text-4xl md:text-5xl font-serif mb-4">Connect with us</h1>
            <p class="text-gray-400 max-w-2xl mx-auto leading-relaxed">Whether you have a question about our collection, need styling advice, or just want to share your thoughts, we're here to help.</p>
        </div>
    </section>

    <div class="container mx-auto px-4 lg:px-8 pb-32">
        <!-- Contact Cards -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 mb-24">
            <div class="bg-white border border-gray-100 p-10 text-center hover:shadow-xl transition-shadow duration-500">
                <div class="w-12 h-12 bg-off-white rounded-full flex items-center justify-center mx-auto mb-6 text-primary">
                    <i class="fa-solid fa-location-dot"></i>
                </div>
                <h3 class="text-[10px] uppercase tracking-widest font-bold mb-4">Our Studio</h3>
                <p class="text-sm text-gray-500">128 Jalan Ampang,<br/>Kathmandu Nepal, 44600</p>
            </div>
            <div class="bg-white border border-gray-100 p-10 text-center hover:shadow-xl transition-shadow duration-500">
                <div class="w-12 h-12 bg-off-white rounded-full flex items-center justify-center mx-auto mb-6 text-primary">
                    <i class="fa-solid fa-envelope"></i>
                </div>
                <h3 class="text-[10px] uppercase tracking-widest font-bold mb-4">Email Us</h3>
                <p class="text-sm text-gray-500">egadget@support.com<br/>egadget@support.com</p>
            </div>
            <div class="bg-white border border-gray-100 p-10 text-center hover:shadow-xl transition-shadow duration-500">
                <div class="w-12 h-12 bg-off-white rounded-full flex items-center justify-center mx-auto mb-6 text-primary">
                    <i class="fa-solid fa-phone"></i>
                </div>
                <h3 class="text-[10px] uppercase tracking-widest font-bold mb-4">Call Us</h3>
                <p class="text-sm text-gray-500">9865513209<br/>Mon-Fri, 9am - 6pm</p>
            </div>
            <div class="bg-white border border-gray-100 p-10 text-center hover:shadow-xl transition-shadow duration-500">
                <div class="w-12 h-12 bg-off-white rounded-full flex items-center justify-center mx-auto mb-6 text-primary">
                    <i class="fa-solid fa-share-nodes"></i>
                </div>
                <h3 class="text-[10px] uppercase tracking-widest font-bold mb-4">Socials</h3>
                <div class="flex justify-center gap-4 text-gray-400">
                    <a href="#" class="hover:text-primary transition-colors"><i class="fa-brands fa-instagram"></i></a>
                    <a href="#" class="hover:text-primary transition-colors"><i class="fa-brands fa-pinterest"></i></a>
                    <a href="#" class="hover:text-primary transition-colors"><i class="fa-brands fa-facebook"></i></a>
                </div>
            </div>
        </div>

        <div class="flex flex-col lg:flex-row gap-20">
            <!-- Map -->
            <div class="w-full lg:w-1/2">
                <h2 class="text-2xl font-serif mb-8 border-b border-gray-100 pb-4">Visit Our Showroom</h2>
                <div class="aspect-video bg-off-white grayscale hover:grayscale-0 transition-all duration-700 overflow-hidden border border-gray-100">
                    <iframe
                        src="https://www.google.com/maps?q=Kuala+Lumpur&output=embed"
                        class="w-full h-full"
                        style="border:0;"
                        allowfullscreen="" 
                        loading="lazy"></iframe>
                </div>
                <p class="mt-6 text-sm text-gray-400 italic">Parking is available for visitors at the main entrance.</p>
            </div>

            <!-- FAQ -->
            <div class="w-full lg:w-1/2">
                <h2 class="text-2xl font-serif mb-8 border-b border-gray-100 pb-4">Frequently Asked</h2>
                <div class="space-y-4">
                    <!-- FAQ 1 -->
                    <details class="group border-b border-gray-50 pb-4" open>
                        <summary class="flex justify-between items-center cursor-pointer list-none py-2">
                            <span class="text-xs uppercase tracking-widest font-bold group-open:text-primary transition-colors">How long does delivery take?</span>
                            <span class="transition-transform group-open:rotate-180"><i class="fa-solid fa-chevron-down text-[10px]"></i></span>
                        </summary>
                        <div class="text-sm text-gray-400 leading-relaxed mt-4">
                            Standard delivery within West Malaysia takes 3-5 business days. For East Malaysia and international orders, please allow 7-14 business days.
                        </div>
                    </details>
                    <!-- FAQ 2 -->
                    <details class="group border-b border-gray-50 pb-4">
                        <summary class="flex justify-between items-center cursor-pointer list-none py-2">
                            <span class="text-xs uppercase tracking-widest font-bold group-open:text-primary transition-colors">What is your return policy?</span>
                            <span class="transition-transform group-open:rotate-180"><i class="fa-solid fa-chevron-down text-[10px]"></i></span>
                        </summary>
                        <div class="text-sm text-gray-400 leading-relaxed mt-4">
                            We offer a 14-day return period for unused pieces in their original packaging. Please contact our concierge team to initiate a return.
                        </div>
                    </details>
                    <!-- FAQ 3 -->
                    <details class="group border-b border-gray-50 pb-4">
                        <summary class="flex justify-between items-center cursor-pointer list-none py-2">
                            <span class="text-xs uppercase tracking-widest font-bold group-open:text-primary transition-colors">Do you offer furniture assembly?</span>
                            <span class="transition-transform group-open:rotate-180"><i class="fa-solid fa-chevron-down text-[10px]"></i></span>
                        </summary>
                        <div class="text-sm text-gray-400 leading-relaxed mt-4">
                            Yes, we offer complimentary white-glove assembly for larger pieces within the Klang Valley area. This can be scheduled at checkout.
                        </div>
                    </details>
                </div>
            </div>
        </div>
    </div>
</asp:Content>