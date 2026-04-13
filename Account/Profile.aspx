<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" CodeFile="Profile.aspx.cs"
    Inherits="serena.Site.Account.ProfilePage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Profile</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-12">
        <div class="max-w-6xl mx-auto">
            <!-- Header Section -->
            <div class="flex flex-col md:flex-row md:items-center justify-between gap-6 mb-12">
                <div>
                    <h1 class="font-serif text-4xl mb-2">My Profile</h1>
                    <p class="text-gray-400 text-sm uppercase tracking-widest">
                        Welcome back, <span class="text-text-dark font-bold">@<asp:Literal ID="litUsername" runat="server" /></span> • 
                        Member since <asp:Literal ID="litMemberSince" runat="server" />
                    </p>
                </div>
                <div class="flex gap-4">
                    <a href="~/Account/Orders/Index.aspx" runat="server" class="flex items-center gap-2 border border-gray-200 px-6 py-3 text-xs uppercase tracking-widest font-bold hover:bg-off-white transition-all">
                        <i class="fa-solid fa-box-archive"></i> My Orders
                    </a>
                    <a href="~/Account/Logout.aspx" runat="server" class="flex items-center gap-2 bg-text-dark text-white px-6 py-3 text-xs uppercase tracking-widest font-bold hover:bg-black transition-all">
                        <i class="fa-solid fa-right-from-bracket"></i> Logout
                    </a>
                </div>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-12">
                <!-- Left: Account & Security -->
                <div class="lg:col-span-2 space-y-12">
                    <!-- Account Settings -->
                    <section>
                        <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Account Settings</h3>
                        <asp:Label ID="lblAccountMsg" runat="server" CssClass="text-red-500 text-sm block mb-4" />
                        
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div>
                                <label class="block text-[10px] uppercase tracking-[0.2em] text-gray-400 font-bold mb-2">Username</label>
                                <asp:TextBox ID="txtUsername" runat="server" CssClass="w-full border border-gray-100 bg-off-white px-4 py-3 text-sm focus:outline-none" ReadOnly="true" />
                                <p class="text-[10px] text-gray-300 mt-1 uppercase tracking-wider italic">Username cannot be changed</p>
                            </div>
                            <div>
                                <label class="block text-[10px] uppercase tracking-[0.2em] text-gray-400 font-bold mb-2">Full Name</label>
                                <asp:TextBox ID="txtFullName" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors" />
                            </div>
                            <div>
                                <label class="block text-[10px] uppercase tracking-[0.2em] text-gray-400 font-bold mb-2">Email Address</label>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors" />
                            </div>
                            <div>
                                <label class="block text-[10px] uppercase tracking-[0.2em] text-gray-400 font-bold mb-2">Phone Number</label>
                                <asp:TextBox ID="txtPhone" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors" />
                            </div>
                        </div>
                        <div class="mt-8">
                            <asp:Button ID="btnSaveAccount" runat="server" Text="Update Profile"
                                CssClass="bg-primary text-white px-10 py-4 text-xs uppercase tracking-widest font-bold hover:bg-primary/90 transition-all cursor-pointer" 
                                OnClick="btnSaveAccount_Click" />
                        </div>
                    </section>

                    <!-- Security -->
                    <section>
                        <h3 class="font-serif text-2xl mb-8 border-b border-gray-100 pb-4">Security</h3>
                        <asp:Label ID="lblSecMsg" runat="server" CssClass="text-red-500 text-sm block mb-4" />
                        
                        <div class="space-y-6 max-w-md">
                            <div>
                                <label class="block text-[10px] uppercase tracking-[0.2em] text-gray-400 font-bold mb-2">Current Password</label>
                                <asp:TextBox ID="txtCur" runat="server" TextMode="Password" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors" />
                            </div>
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <label class="block text-[10px] uppercase tracking-[0.2em] text-gray-400 font-bold mb-2">New Password</label>
                                    <asp:TextBox ID="txtNew" runat="server" TextMode="Password" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors" />
                                </div>
                                <div>
                                    <label class="block text-[10px] uppercase tracking-[0.2em] text-gray-400 font-bold mb-2">Confirm New</label>
                                    <asp:TextBox ID="txtNew2" runat="server" TextMode="Password" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary transition-colors" />
                                </div>
                            </div>
                        </div>
                        <div class="mt-8">
                            <asp:Button ID="btnChangePassword" runat="server" Text="Change Password" 
                                CssClass="border border-text-dark text-text-dark px-10 py-4 text-xs uppercase tracking-widest font-bold hover:bg-off-white transition-all cursor-pointer"
                                OnClick="btnChangePassword_Click" />
                        </div>
                    </section>
                </div>

                <!-- Right: Addresses -->
                <div class="lg:col-span-1 border-l border-gray-100 lg:pl-12">
                    <div class="flex items-center justify-between mb-8 border-b border-gray-100 pb-4">
                        <h3 class="font-serif text-2xl">Addresses</h3>
                        <asp:LinkButton ID="btnNewAddress" runat="server" CssClass="text-[10px] uppercase tracking-widest font-bold text-primary hover:opacity-70" OnClick="btnNewAddress_Click">
                            <i class="fa-solid fa-plus mr-1"></i> Add New
                        </asp:LinkButton>
                    </div>

                    <!-- Address List -->
                    <div class="space-y-6 mb-12">
                        <asp:ListView ID="lvAddresses" runat="server" OnItemCommand="lvAddresses_ItemCommand">
                            <LayoutTemplate>
                                <div class="space-y-6">
                                    <asp:PlaceHolder ID="itemPlaceholder" runat="server" />
                                </div>
                            </LayoutTemplate>
                            <ItemTemplate>
                                <div class="group border border-gray-100 p-6 hover:border-primary transition-colors relative">
                                    <div class="flex justify-between items-start mb-4">
                                        <span class="text-[9px] uppercase tracking-widest font-bold px-2 py-1 <%# Convert.ToBoolean(Eval("is_default")) ? "bg-accent text-white" : "bg-gray-100 text-gray-400" %>">
                                            <%# Convert.ToBoolean(Eval("is_default")) ? "Default" : "Secondary" %>
                                        </span>
                                        <div class="flex gap-3 opacity-0 group-hover:opacity-100 transition-opacity">
                                            <asp:LinkButton ID="cmdEdit" runat="server" CssClass="text-gray-400 hover:text-primary transition-colors" CommandName="EditAddr" CommandArgument='<%# Eval("id") %>'>
                                                <i class="fa-regular fa-pen-to-square"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="cmdDefault" runat="server" CssClass="text-gray-400 hover:text-accent transition-colors" CommandName="MakeDefault" CommandArgument='<%# Eval("id") %>' OnClientClick="return confirm('Set this as default?');">
                                                <i class="fa-regular fa-star"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="cmdDelete" runat="server" CssClass="text-gray-400 hover:text-red-500 transition-colors" CommandName="DeleteAddr" CommandArgument='<%# Eval("id") %>' OnClientClick="return confirm('Delete address?');">
                                                <i class="fa-regular fa-trash-can"></i>
                                            </asp:LinkButton>
                                        </div>
                                    </div>
                                    <div class="text-sm leading-relaxed text-gray-500">
                                        <p class="font-bold text-text-dark mb-1 capitalize"><%# Server.HtmlEncode(Convert.ToString(Eval("city"))) %></p>
                                        <p><%# Server.HtmlEncode(Convert.ToString(Eval("address"))) %></p>
                                        <p><%# Server.HtmlEncode(Convert.ToString(Eval("township"))) %><%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("postal_code"))) ? "" : " " + Eval("postal_code") %></p>
                                        <p><%# Server.HtmlEncode(Convert.ToString(Eval("state"))) %>, <%# Server.HtmlEncode(Convert.ToString(Eval("country"))) %></p>
                                    </div>
                                </div>
                            </ItemTemplate>
                            <EmptyDataTemplate>
                                <div class="bg-off-white p-8 text-center text-gray-400 text-xs uppercase tracking-widest border border-dashed border-gray-200">
                                    No saved addresses
                                </div>
                            </EmptyDataTemplate>
                        </asp:ListView>
                    </div>

                    <!-- Address Editor (Hidden until New/Edit clicked) -->
                    <div id="addrEditor" runat="server" class="bg-off-white p-8 border border-gray-100">
                        <h4 class="text-xs uppercase tracking-[0.2em] font-bold mb-6 italic"><asp:Literal ID="litAddrAction" runat="server" Text="Add New Address" /></h4>
                        <asp:Label ID="lblAddrMsg" runat="server" CssClass="text-red-500 text-[10px] block mb-4" />
                        <asp:HiddenField ID="hidAddrId" runat="server" />
                        
                        <div class="space-y-4">
                            <div>
                                <label class="block text-[10px] uppercase tracking-widest text-gray-400 mb-2 font-bold">Street Address</label>
                                <asp:TextBox ID="txtAddr" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none bg-white" TextMode="MultiLine" Rows="2" />
                            </div>
                            <div class="grid grid-cols-2 gap-4">
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 mb-2 font-bold">Township</label>
                                    <asp:TextBox ID="txtTownship" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none bg-white" />
                                </div>
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 mb-2 font-bold">Postal Code</label>
                                    <asp:TextBox ID="txtPostal" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none bg-white" />
                                </div>
                            </div>
                            <div class="grid grid-cols-2 gap-4">
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 mb-2 font-bold">City</label>
                                    <asp:TextBox ID="txtCity" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none bg-white" />
                                </div>
                                <div>
                                    <label class="block text-[10px] uppercase tracking-widest text-gray-400 mb-2 font-bold">State</label>
                                    <asp:TextBox ID="txtState" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none bg-white" />
                                </div>
                            </div>
                            <div>
                                <label class="block text-[10px] uppercase tracking-widest text-gray-400 mb-2 font-bold">Country</label>
                                <asp:TextBox ID="txtCountry" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none bg-white" />
                            </div>
                            <div class="pt-2">
                                <label class="flex items-center gap-3 cursor-pointer group">
                                    <input id="chkDefault" runat="server" type="checkbox" class="accent-primary w-4 h-4" />
                                    <span class="text-[10px] uppercase tracking-widest text-gray-500 group-hover:text-text-dark transition-colors">Set as default</span>
                                </label>
                            </div>
                        </div>

                        <div class="mt-8 flex gap-3">
                            <asp:Button ID="btnSaveAddress" runat="server" Text="Save"
                                CssClass="bg-text-dark text-white px-8 py-3 text-[10px] uppercase tracking-widest font-bold hover:bg-black transition-all cursor-pointer" 
                                OnClick="btnSaveAddress_Click" />
                            <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel"
                                CssClass="border border-gray-200 text-gray-400 px-8 py-3 text-[10px] uppercase tracking-widest font-bold hover:bg-white hover:text-text-dark transition-all cursor-pointer" 
                                OnClick="btnNewAddress_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
