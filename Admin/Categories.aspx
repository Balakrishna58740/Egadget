<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Categories.aspx.cs"
    Inherits="serena.Admin.CategoriesPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Product Categories | eGadgetHub Admin</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="/Assets/css/admin-categories-redesign.css" />
</asp:Content>


<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="cat-page">
        <div class="cat-header">
            <h2>Product Categories</h2>
            <p>Add, edit, and manage your product category hierarchy.</p>
        </div>

        <asp:Label ID="lblMsg" runat="server" CssClass="cat-alert" EnableViewState="false"></asp:Label>

        <div class="cat-grid">
            <div class="cat-left">
                <div class="cat-card cat-sticky">
                    <div class="cat-panel-head">
                        <i class="fa-solid fa-layer-group"></i>
                        <h3>ADD CATEGORY</h3>
                    </div>

                    <asp:HiddenField ID="hidId" runat="server" />

                    <div class="cat-form">
                        <div>
                            <label>CATEGORY NAME</label>
                            <asp:TextBox ID="txtName" runat="server" placeholder="Enter category name..."
                                CssClass="cat-input" MaxLength="255" />
                        </div>

                        <div class="cat-btn-wrap">
                            <asp:Button ID="btnSave" runat="server" CssClass="cat-btn-save"
                                Text="SAVE CATEGORY" OnClick="btnSave_Click" />
                            <asp:Button ID="btnCancel" runat="server" CssClass="cat-btn-cancel"
                                Text="CANCEL" OnClick="btnCancel_Click" CausesValidation="false" />
                        </div>
                    </div>
                </div>
            </div>

            <div class="cat-right">
                <div class="cat-card cat-table-card">
                    <div class="cat-panel-head cat-table-head">
                        <div class="cat-panel-title">
                            <i class="fa-solid fa-layer-group"></i>
                            <h3>CATEGORY LIST</h3>
                        </div>
                    </div>

                    <div class="cat-table-wrap">
                        <table class="cat-table">
                            <thead>
                                <tr>
                                    <th class="w-ref">REF</th>
                                    <th>CATEGORY NAME</th>
                                    <th class="text-center">PRODUCTS</th>
                                    <th class="text-right">ACTIONS</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Literal ID="litRows" runat="server" />
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
