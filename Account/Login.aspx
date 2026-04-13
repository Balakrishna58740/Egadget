<%@ Page Language="C#" MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true" CodeFile="Login.aspx.cs"
    Inherits="serena.Site.Account.LoginPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Login</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-16 min-h-[60vh] flex items-center justify-center">
        <div class="w-full max-w-md">
            <div class="bg-white border border-gray-100 shadow-2xl p-8 md:p-12">
                <div class="text-center mb-10">
                    <h2 class="font-serif text-3xl mb-2">Welcome Back</h2>
                    <p class="text-gray-400 text-sm uppercase tracking-widest">Sign in to your account</p>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-red-500 text-sm block mb-4 text-center" />

                <div class="space-y-6">
                    <div>
                        <label for="<%= txtUser.ClientID %>" class="block text-xs uppercase tracking-widest font-bold mb-2">Username</label>
                        <asp:TextBox ID="txtUser" runat="server" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary bg-off-white transition-colors" />
                    </div>
                    
                    <div>
                        <div class="flex justify-between items-center mb-2">
                            <label for="<%= txtPassword.ClientID %>" class="block text-xs uppercase tracking-widest font-bold">Password</label>
                            <a href="#" class="text-[10px] uppercase tracking-widest text-gray-400 hover:text-primary">Forgot?</a>
                        </div>
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="w-full border border-gray-200 px-4 py-3 text-sm focus:outline-none focus:border-primary bg-off-white transition-colors" />
                    </div>

                    <div class="pt-4">
                        <asp:Button ID="btnLogin" runat="server" Text="Sign In" 
                            CssClass="w-full bg-primary text-white py-4 text-sm uppercase tracking-widest font-bold hover:bg-primary/90 transition-all cursor-pointer" 
                            OnClick="btnLogin_Click" />
                    </div>

                    <div class="text-center pt-6 border-t border-gray-50 mt-8">
                        <p class="text-gray-400 text-sm">
                            New to eGadgetHub? 
                            <a href="~/Account/Register.aspx" runat="server" class="text-text-dark font-bold hover:text-primary transition-colors ml-1">Create an account</a>
                        </p>
                        <p class="text-gray-400 text-xs uppercase tracking-widest mt-4">
                            Admin login? <a href="~/Admin/Login.aspx" runat="server" class="text-primary font-bold hover:underline">Go to admin sign in</a>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
