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
            <div>
                <h2>Product Categories</h2>
                <p>Add, edit, and manage your product category hierarchy.</p>
            </div>
        </div>

        <asp:Label ID="lblListMsg" runat="server" CssClass="cat-alert" EnableViewState="false"></asp:Label>

        <div id="categoryEditorModal" class="prod-modal hidden">
            <div class="prod-modal-panel">
                <div class="cat-card cat-modal-card">
                    <div class="cat-panel-head cat-modal-head">
                        <div class="cat-modal-title">
                            <i class="fa-solid fa-layer-group"></i>
                            <div>
                                <h3>ADD / EDIT CATEGORY</h3>
                                <p>Create or update your product taxonomy</p>
                            </div>
                        </div>
                        <button type="button" class="prod-modal-close" onclick="closeCategoryModal()"><i class="fa-solid fa-xmark"></i></button>
                    </div>

                    <div class="cat-modal-body">
                        <asp:Label ID="lblMsg" runat="server" CssClass="cat-alert" EnableViewState="false"></asp:Label>

                        <asp:HiddenField ID="hidId" runat="server" />

                        <div class="cat-form">
                            <div>
                                <label>CATEGORY NAME</label>
                                <asp:TextBox ID="txtName" runat="server" placeholder="Enter category name..."
                                    CssClass="cat-input" MaxLength="255" />
                            </div>

                            <div class="cat-btn-wrap">
                                <asp:Button ID="btnCancel" runat="server" CssClass="cat-btn-cancel"
                                    Text="Cancel" OnClick="btnCancel_Click" CausesValidation="false" />
                                <asp:Button ID="btnSave" runat="server" CssClass="cat-btn-save"
                                    Text="Save Category" OnClick="btnSave_Click" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

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

            <div class="cat-bottom-actions">
                <button type="button" class="cat-btn-add" onclick="openCategoryModal()">
                    <i class="fa-solid fa-plus"></i> Add Category
                </button>
            </div>
        </div>
    </div>

    <script>
        function openCategoryModal() {
            var modal = document.getElementById('categoryEditorModal');
            if (modal) {
                modal.classList.remove('hidden');
                document.body.style.overflow = 'hidden';
            }
        }

        function closeCategoryModal() {
            var modal = document.getElementById('categoryEditorModal');
            if (modal) {
                modal.classList.add('hidden');
                document.body.style.overflow = 'auto';
            }
        }

        document.addEventListener('click', function (e) {
            var modal = document.getElementById('categoryEditorModal');
            if (!modal || modal.classList.contains('hidden')) return;
            if (e.target === modal) closeCategoryModal();
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') closeCategoryModal();
        });

        (function () {
            var hid = document.querySelector("[id$='hidId']");
            var msg = document.querySelector("[id$='lblMsg']");
            var hasEdit = hid && hid.value && hid.value.length > 0;
            var hasMsg = msg && msg.textContent && msg.textContent.trim().length > 0;
            if (hasEdit || hasMsg) openCategoryModal();
        })();
    </script>

</asp:Content>
