<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Profile.aspx.cs"
    Inherits="serena.Admin.AdminProfilePage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Settings | eGadgetHub Admin</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="/Assets/css/admin-profile-redesign.css" />
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="st-wrap">
        <div class="st-single-card">
            <div class="st-head">
                <h2>ADMIN SETTINGS</h2>
                <p>Professional account setup used in real-world admin systems.</p>
            </div>

            <div class="st-card-title">Account Overview</div>
            <div class="st-meta-grid">
                <div class="st-meta-item">
                    <span>Username</span>
                    <strong><asp:Literal ID="litUser" runat="server"></asp:Literal></strong>
                </div>
                <div class="st-meta-item">
                    <span>Role</span>
                    <strong><asp:Literal ID="litRole" runat="server"></asp:Literal></strong>
                </div>
            </div>

            <div class="st-divider"></div>

            <div class="st-card-title">Profile Details</div>
            <asp:Label ID="lblInfo" runat="server" CssClass="st-alert d-none" EnableViewState="false"></asp:Label>
            <div class="st-field">
                <label for="txtFull">Full Name</label>
                <asp:TextBox ID="txtFull" runat="server" MaxLength="255" CssClass="st-input" placeholder="Enter full name"></asp:TextBox>
            </div>
            <div class="st-actions">
                <asp:Button ID="btnSaveProfile" runat="server" Text="Save Profile" CssClass="st-btn st-btn-primary" OnClick="btnSaveProfile_Click"></asp:Button>
            </div>

            <div class="st-divider"></div>

            <div class="st-card-title">Security</div>
            <asp:Label ID="lblPwd" runat="server" CssClass="st-alert d-none" EnableViewState="false"></asp:Label>
            <div class="st-field">
                <label for="txtOld">Current Password</label>
                <asp:TextBox ID="txtOld" runat="server" TextMode="Password" CssClass="st-input" MaxLength="255" placeholder="••••••••"></asp:TextBox>
            </div>

            <div class="st-cols-2">
                <div class="st-field">
                    <label for="txtNew">New Password</label>
                    <asp:TextBox ID="txtNew" runat="server" TextMode="Password" CssClass="st-input" MaxLength="255" placeholder="••••••••"></asp:TextBox>
                </div>
                <div class="st-field">
                    <label for="txtConfirm">Confirm Password</label>
                    <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" CssClass="st-input" MaxLength="255" placeholder="••••••••"></asp:TextBox>
                </div>
            </div>

            <div class="st-actions">
                <asp:Button ID="btnChangePwd" runat="server" Text="Change Password" CssClass="st-btn st-btn-ghost" OnClick="btnChangePwd_Click"></asp:Button>
            </div>
        </div>
    </div>
</asp:Content>
