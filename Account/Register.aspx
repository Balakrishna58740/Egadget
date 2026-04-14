<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" CodeFile="Register.aspx.cs"
    Inherits="serena.Site.Account.RegisterPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Register</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-16">
        <div class="max-w-xl mx-auto">
            <div class="bg-white border border-gray-100 shadow-2xl p-8 md:p-12 eg-card rounded-2xl">
                <div class="text-center mb-10">
                    <h2 class="font-serif text-3xl mb-2">Create Account</h2>
                    <p class="text-gray-400 text-sm uppercase tracking-widest">Join the Saja community</p>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-red-500 text-sm block mb-6 text-center" />

                <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                    <div class="md:col-span-2">
                        <label for="<%= txtUsername.ClientID %>" class="block text-xs uppercase tracking-widest font-bold mb-2">Username</label>
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary bg-off-white transition-colors"
                                     AutoPostBack="true" OnTextChanged="txtUsername_TextChanged" />
                        <asp:Label ID="lblUserCheck" runat="server" CssClass="text-[10px] mt-1 block"></asp:Label>
                        <p class="text-[10px] text-gray-400 mt-1 uppercase tracking-wider">3–20 characters: letters, numbers, _ or -</p>
                    </div>

                    <div class="md:col-span-2">
                        <label for="<%= txtName.ClientID %>" class="block text-xs uppercase tracking-widest font-bold mb-2">Full Name</label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary bg-off-white transition-colors" />
                    </div>

                    <div class="md:col-span-2">
                        <label for="<%= txtEmail.ClientID %>" class="block text-xs uppercase tracking-widest font-bold mb-2">Email Address</label>
                        <asp:TextBox ID="txtEmail" runat="server" Type="Email" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary bg-off-white transition-colors" placeholder="you@example.com" />
                    </div>

                    <div>
                        <label for="<%= txtPassword.ClientID %>" class="block text-xs uppercase tracking-widest font-bold mb-2">Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary bg-off-white transition-colors" />
                        <p class="text-[10px] text-gray-400 mt-1 uppercase tracking-wider">At least 6 characters</p>
                    </div>

                    <div>
                        <label for="<%= txtConfirm.ClientID %>" class="block text-xs uppercase tracking-widest font-bold mb-2">Confirm Password</label>
                        <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary bg-off-white transition-colors" />
                    </div>
                </div>

                <div class="pt-4">
                    <asp:Button ID="btnRegister" runat="server" Text="Create Account" 
                        data-loading-text="Creating Account..."
                        CssClass="w-full bg-primary text-white py-4 text-sm uppercase tracking-widest font-bold hover:bg-primary/90 transition-all cursor-pointer" 
                        OnClick="btnRegister_Click" />
                </div>

                <div class="text-center pt-6 border-t border-gray-50 mt-8">
                    <p class="text-gray-400 text-sm">
                        Already have an account? 
                        <a href="~/Account/Login.aspx" runat="server" class="text-text-dark font-bold hover:text-primary transition-colors ml-1">Sign In</a>
                    </p>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
