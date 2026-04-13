<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Profile.aspx.cs"
    Inherits="serena.Admin.AdminProfilePage " %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Identity Curation | Saja Admin</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Identity Header -->
    <div class="mb-12">
        <h2 class="text-3xl font-serif mb-2">Identity Curation</h2>
        <p class="text-xs uppercase tracking-widest text-gray-400 font-bold">Manage your administrative persona and security parameters.</p>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-12 gap-12">
        <!-- Profile Curation -->
        <div class="lg:col-span-6 space-y-12">
            <div class="bg-white border border-gray-100 p-8 shadow-sm">
                <div class="flex items-center gap-4 mb-8">
                    <i class="fa-solid fa-user-gear text-primary"></i>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Professional Dossier</h3>
                </div>

                <asp:Label ID="lblInfo" runat="server" CssClass="hidden mb-8 p-4 text-[10px] uppercase tracking-widest font-bold border-l-4" EnableViewState="false"></asp:Label>

                <div class="space-y-6">
                    <div class="grid grid-cols-2 gap-6">
                        <div>
                            <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Codename / Username</label>
                            <div class="px-4 py-3 bg-off-white text-sm font-serif text-text-dark border border-transparent italic">
                                <asp:Literal ID="litUser" runat="server" />
                            </div>
                        </div>
                        <div>
                            <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Authority Level</label>
                            <div class="px-4 py-3 bg-off-white text-sm font-serif text-text-dark border border-transparent uppercase tracking-wider font-bold text-[10px]">
                                <asp:Literal ID="litRole" runat="server" />
                            </div>
                        </div>
                    </div>

                    <div>
                        <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Full Identity / Legal Name</label>
                        <asp:TextBox ID="txtFull" runat="server" placeholder="Enter your full name..." 
                            CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" MaxLength="255" />
                    </div>

                    <div class="pt-4">
                        <asp:Button ID="btnSaveProfile" runat="server" CssClass="w-full bg-admin-bg text-white text-[10px] uppercase tracking-widest font-bold px-8 py-4 hover:bg-primary transition-all cursor-pointer shadow-lg shadow-primary/10" 
                            Text="Update Dossier" OnClick="btnSaveProfile_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Security Vault (Change Password) -->
        <div class="lg:col-span-6">
            <div class="bg-white border border-gray-100 p-8 shadow-sm">
                <div class="flex items-center gap-4 mb-8">
                    <i class="fa-solid fa-shield-halved text-primary"></i>
                    <h3 class="text-xs uppercase tracking-widest font-bold text-text-dark">Security Protocols</h3>
                </div>

                <asp:Label ID="lblPwd" runat="server" CssClass="hidden mb-8 p-4 text-[10px] uppercase tracking-widest font-bold border-l-4" EnableViewState="false"></asp:Label>

                <div class="space-y-6">
                    <div>
                        <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Existing Credential</label>
                        <asp:TextBox ID="txtOld" runat="server" TextMode="Password" placeholder="••••••••"
                            CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" MaxLength="255" />
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">New Credential</label>
                            <asp:TextBox ID="txtNew" runat="server" TextMode="Password" placeholder="••••••••"
                                CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" MaxLength="255" />
                        </div>
                        <div>
                            <label class="text-[10px] uppercase tracking-widest text-gray-400 font-bold block mb-2">Re-verify Credential</label>
                            <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" placeholder="••••••••"
                                CssClass="w-full bg-white border border-gray-100 px-4 py-3 text-sm font-serif focus:border-primary outline-none transition-all" MaxLength="255" />
                        </div>
                    </div>

                    <div class="pt-4">
                        <asp:Button ID="btnChangePwd" runat="server" CssClass="w-full bg-white text-gray-400 text-[10px] uppercase tracking-widest font-bold px-8 py-4 border border-gray-100 hover:text-primary transition-all cursor-pointer" 
                            Text="Rotate Security Credentials" OnClick="btnChangePwd_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script>
        (function () {
            function handleAlert(lblId) {
                var m = document.getElementById(lblId);
                if (m && m.textContent && m.textContent.trim().length > 0) {
                    m.classList.remove('hidden');
                    m.classList.add('block');
                    const isError = m.textContent.toLowerCase().includes('incorrect') || m.textContent.toLowerCase().includes('mismatch') || m.textContent.toLowerCase().includes('error') || m.textContent.toLowerCase().includes('failed');
                    m.classList.add(isError ? 'bg-red-50' : 'bg-green-50');
                    m.classList.add(isError ? 'border-red-500' : 'border-green-500');
                    m.classList.add(isError ? 'text-red-700' : 'text-green-700');
                }
            }
            handleAlert('<%= lblInfo.ClientID %>');
            handleAlert('<%= lblPwd.ClientID %>');
        })();
    </script>
</asp:Content>
