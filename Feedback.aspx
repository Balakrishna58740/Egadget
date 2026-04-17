<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" CodeFile="Feedback.aspx.cs"
    Inherits="serena.Site.FeedbackPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Feedback | eGadgetHub</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    
    <section class="eg-surface py-12 mb-12">
        <div class="container mx-auto px-4 lg:px-8">
            <h1 class="text-4xl font-serif mb-2">Thoughts & Feedback</h1>
            <nav class="flex text-xs uppercase tracking-widest text-gray-400">
                <a runat="server" href="~/Default.aspx" class="hover:text-primary transition-colors">Home</a>
                <span class="mx-2">/</span>
                <span class="text-text-dark">Feedback</span>
            </nav>
        </div>
    </section>

    <div class="container mx-auto px-4 lg:px-8 pb-32 feedback-page">
        <!-- page alert -->
        <div id="alertMsg" runat="server" class="hidden mb-12 p-6 text-sm uppercase tracking-widest font-bold"></div>

        <div class="flex flex-col lg:flex-row gap-16">
            <!-- Form Section -->
            <div class="w-full lg:w-1/2">
                <div class="bg-white border border-gray-100 p-10 shadow-sm eg-card rounded-xl">
                    <h2 class="text-2xl font-serif mb-8 border-b border-gray-100 pb-4">Share with us</h2>
                    
                    <div class="space-y-6">
                        <div>
                            <label for="txtName" class="text-[10px] uppercase tracking-widest font-bold text-gray-400 mb-2 block">Your Name</label>
                            <input id="txtName" runat="server" type="text" maxlength="255" class="w-full bg-off-white border border-transparent px-4 py-4 text-sm focus:border-primary focus:bg-white focus:outline-none transition-all" placeholder="Enter your full name" />
                        </div>

                        <div>
                            <label for="txtEmail" class="text-[10px] uppercase tracking-widest font-bold text-gray-400 mb-2 block">Email Address</label>
                            <input id="txtEmail" runat="server" type="email" maxlength="255" class="w-full bg-off-white border border-transparent px-4 py-4 text-sm focus:border-primary focus:bg-white focus:outline-none transition-all" placeholder="Enter your email" />
                        </div>

                        <div>
                            <label for="txtTitle" class="text-[10px] uppercase tracking-widest font-bold text-gray-400 mb-2 block">Subject</label>
                            <input id="txtTitle" runat="server" type="text" maxlength="255" class="w-full bg-off-white border border-transparent px-4 py-4 text-sm focus:border-primary focus:bg-white focus:outline-none transition-all" placeholder="What is this about?" />
                        </div>

                        <div>
                            <label for="txtMsg" class="text-[10px] uppercase tracking-widest font-bold text-gray-400 mb-2 block">Your Message</label>
                            <textarea id="txtMsg" runat="server" rows="6" class="w-full bg-off-white border border-transparent px-4 py-4 text-sm focus:border-primary focus:bg-white focus:outline-none transition-all resize-none" placeholder="Tell us more..."></textarea>
                        </div>

                        <button id="btnSend" runat="server" data-loading-text="Submitting..." class="w-full bg-text-dark text-white py-5 text-xs uppercase tracking-[0.2em] font-bold hover:bg-primary transition-all duration-500" onserverclick="btnSend_ServerClick">
                            Submit Feedback
                        </button>
                    </div>
                </div>
            </div>

            <!-- History Section -->
            <div class="w-full lg:w-1/2">
                <div id="phHistory" runat="server" class="h-full">
                    <div class="flex items-center justify-between mb-8 border-b border-gray-100 pb-4">
                        <h2 class="text-2xl font-serif">Community Feed</h2>
                        <span class="text-[10px] uppercase tracking-widest text-gray-400 font-bold">Latest First</span>
                    </div>

                    <!-- Filters -->
                    <div class="bg-off-white p-8 mb-10 space-y-6 eg-card rounded-xl">
                        <div class="grid grid-cols-1 sm:grid-cols-3 gap-6">
                            <div>
                                <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 mb-2 block" for="txtFilterQ">Keyword</label>
                                <input id="txtFilterQ" runat="server" type="text" class="w-full bg-white border border-gray-100 px-4 py-3 text-xs focus:border-primary focus:outline-none transition-all" placeholder="Search name/title..." />
                            </div>
                            <div>
                                <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 mb-2 block" for="txtFilterTicket">Track Ticket</label>
                                <input id="txtFilterTicket" runat="server" type="text" class="w-full bg-white border border-gray-100 px-4 py-3 text-xs focus:border-primary focus:outline-none transition-all" placeholder="e.g. FB-20260414-12345" />
                            </div>
                            <div>
                                <label class="text-[10px] uppercase tracking-widest font-bold text-gray-400 mb-2 block" for="ddlStatus">Status</label>
                                <select id="ddlStatus" runat="server" class="w-full bg-white border border-gray-100 px-4 py-3 text-xs focus:border-primary focus:outline-none transition-all appearance-none">
                                    <option value="">All Feedbacks</option>
                                    <option value="pending">Waiting for reply</option>
                                    <option value="replied">Answered pieces</option>
                                </select>
                            </div>
                        </div>
                        <div class="flex gap-4">
                            <button id="btnApplyFilter" runat="server" class="flex-grow bg-primary text-white py-4 text-[10px] uppercase tracking-widest font-bold hover:bg-primary/90 transition-all" onserverclick="btnApplyFilter_ServerClick">
                                Apply Filters
                            </button>
                            <button id="btnClearFilter" runat="server" class="px-8 border border-gray-200 text-text-dark text-[10px] uppercase tracking-widest font-bold hover:bg-white transition-all" onserverclick="btnClearFilter_ServerClick">
                                Clear
                            </button>
                        </div>
                    </div>

                    <!-- Feedback List Container -->
                    <div id="litList" runat="server" class="space-y-6"></div>

                    <!-- Pagination -->
                    <div id="pager" runat="server" class="mt-12 flex justify-center border-t border-gray-100 pt-8"></div>
                </div>

                <div id="phGuestNote" runat="server" class="hidden py-12 text-center bg-off-white border border-dashed border-gray-200">
                    <i class="fa-solid fa-user-lock text-3xl text-gray-200 mb-4"></i>
                    <p class="text-[10px] uppercase tracking-widest font-bold text-gray-400">Login to see your history</p>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
