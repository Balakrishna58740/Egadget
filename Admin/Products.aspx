<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Products.aspx.cs"
    Inherits="serena.Admin.ProductsPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Product Inventory | eGadgetHub Admin</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="/Assets/css/admin-products-redesign.css" />
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="prod-page">
        <div class="prod-header">
            <div>
                <h2>Product Inventory</h2>
                <p>Manage your electronics inventory</p>
            </div>
            <div class="prod-header-actions">
                <button type="button" class="prod-btn-add" onclick="openProductModal()">
                    <i class="fa-solid fa-plus"></i> Add Product
                </button>
                <span class="prod-total"><asp:Literal ID="litTotal" runat="server" /> catalog items</span>
            </div>
        </div>

        <div class="prod-layout">
            <div class="prod-list-col">
                <div class="prod-card prod-filter-card">
                    <div class="prod-filter-grid">
                        <div>
                            <label>Search Name</label>
                            <asp:TextBox ID="txtQ" runat="server" CssClass="prod-input" />
                        </div>
                        <div>
                            <label>Category</label>
                            <asp:DropDownList ID="ddlFilterCat" runat="server" CssClass="prod-input"></asp:DropDownList>
                        </div>
                        <div>
                            <label>Visibility</label>
                            <asp:DropDownList ID="ddlFilterShow" runat="server" CssClass="prod-input">
                                <asp:ListItem Text="All Items" Value="" />
                                <asp:ListItem Text="Live Only" Value="1" />
                                <asp:ListItem Text="Hidden Archive" Value="0" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label>Ordering</label>
                            <asp:DropDownList ID="ddlSort" runat="server" CssClass="prod-input">
                                <asp:ListItem Text="Alpha (A-Z)" Value="name_asc" />
                                <asp:ListItem Text="Alpha (Z-A)" Value="name_desc" />
                                <asp:ListItem Text="Value (Low to High)" Value="price_asc" />
                                <asp:ListItem Text="Value (High to Low)" Value="price_desc" />
                                <asp:ListItem Text="Stock (Low to High)" Value="stock_asc" />
                                <asp:ListItem Text="Stock (High to Low)" Value="stock_desc" />
                            </asp:DropDownList>
                        </div>
                    </div>

    <div id="productEditorModal" class="prod-modal hidden">
        <div class="prod-modal-panel">
            <div class="prod-card prod-editor-card">
                <div class="prod-card-head prod-modal-head">
                    <h3>Add / Edit Product</h3>
                    <button type="button" class="prod-modal-close" onclick="closeProductModal()"><i class="fa-solid fa-xmark"></i></button>
                </div>
                <div class="prod-card-body">
                    <asp:Label ID="lblMsg" runat="server" CssClass="prod-alert" EnableViewState="false"></asp:Label>

                    <asp:HiddenField ID="hidId" runat="server" />

                    <div class="prod-form-grid">
                        <div>
                            <label>Category</label>
                            <asp:DropDownList ID="ddlCat" runat="server" CssClass="prod-input"></asp:DropDownList>
                        </div>

                        <div>
                            <label>Product Name</label>
                            <asp:TextBox ID="txtName" runat="server" CssClass="prod-input" placeholder="e.g. Samsung Galaxy S24" />
                        </div>

                        <div>
                            <label>Description</label>
                            <asp:TextBox ID="txtDesc" runat="server" TextMode="MultiLine" Rows="3" CssClass="prod-input prod-textarea" placeholder="Describe features, specs and highlights..." />
                        </div>

                        <div class="prod-two-col">
                            <div>
                                <label>Availability</label>
                                <asp:TextBox ID="txtStock" runat="server" TextMode="Number" CssClass="prod-input" />
                            </div>
                            <div>
                                <label>Price (RS)</label>
                                <asp:TextBox ID="txtPrice" runat="server" TextMode="Number" CssClass="prod-input" />
                            </div>
                        </div>

                        <div class="prod-checkbox-row">
                            <asp:CheckBox ID="chkShow" runat="server" CssClass="prod-checkbox" />
                            <asp:Label runat="server" AssociatedControlID="chkShow" CssClass="prod-check-label">Active</asp:Label>
                        </div>

                        <div>
                            <label>Image</label>
                            <div class="prod-upload-box">
                                <asp:FileUpload ID="fuImg" runat="server" CssClass="prod-upload-input" />
                                <i class="fa-solid fa-cloud-upload-alt"></i>
                                <p>Choose Image</p>
                            </div>
                            <div class="prod-upload-hint"><asp:Literal ID="litImgHint" runat="server" /></div>
                        </div>

                        <div class="prod-btn-row">
                            <asp:Button ID="btnSave" runat="server" CssClass="prod-btn-save" Text="Save Product" OnClick="btnSave_Click" />
                            <asp:Button ID="btnCancel" runat="server" CssClass="prod-btn-cancel" Text="Cancel" OnClick="btnCancel_Click" CausesValidation="false" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
                    <div class="prod-filter-actions">
                        <asp:Button ID="btnFilter" runat="server" CssClass="prod-btn-search" Text="Search" OnClick="btnFilter_Click" />
                        <asp:Button ID="btnReset" runat="server" CssClass="prod-btn-reset" Text="Reset" OnClick="btnReset_Click" CausesValidation="false" />
                    </div>
                </div>

                <div class="prod-card prod-table-card">
                    <div class="prod-table-wrap">
                        <table class="prod-table">
                            <thead>
                                <tr>
                                    <th>Ref</th>
                                    <th>Product Name</th>
                                    <th>Collection</th>
                                    <th class="text-right">Value</th>
                                    <th class="text-right">Stock</th>
                                    <th>Live</th>
                                    <th class="text-right">Settings</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Literal ID="litRows" runat="server" />
                            </tbody>
                        </table>
                    </div>
                    <div class="prod-pager">
                        <asp:Literal ID="pager" runat="server" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div id="imgModal" class="hidden fixed inset-0 z-[100] flex items-center justify-center p-4 sm:p-6 bg-admin-bg/95 backdrop-blur-sm">
        <div class="relative max-w-4xl w-full">
            <button type="button" class="absolute -top-12 right-0 text-white hover:text-primary transition-colors text-2xl" onclick="closeImgModal()">
                <i class="fa-solid fa-times"></i>
            </button>
            <div class="bg-white p-2">
                <img id="imgPreview" src="" alt="Product image" class="w-full h-auto object-contain max-h-[80vh]" />
            </div>
        </div>
    </div>

    <script>
        function openImgModal(src) {
            var modal = document.getElementById('imgModal');
            var img = document.getElementById('imgPreview');
            img.src = src;
            modal.classList.remove('hidden');
            document.body.style.overflow = 'hidden';
        }

        function closeImgModal() {
            var modal = document.getElementById('imgModal');
            modal.classList.add('hidden');
            document.body.style.overflow = 'auto';
        }

        var lastTouchTime = 0;

        function onProductActionTap(e) {
            if (e.type === 'click' && (Date.now() - lastTouchTime) < 500) return;
            var btn = e.target.closest('.view-img');
            if (btn) {
                e.preventDefault();
                var src = btn.getAttribute('data-src');
                openImgModal(src);
                var wrap2 = btn.closest('.prod-settings-wrap');
                if (wrap2) wrap2.classList.remove('open');
                return;
            }
        }

        document.addEventListener('click', onProductActionTap);
        document.addEventListener('touchstart', function (e) {
            lastTouchTime = Date.now();
            onProductActionTap(e);
        }, { passive: false });

        function openProductModal() {
            var modal = document.getElementById('productEditorModal');
            if (modal) {
                modal.classList.remove('hidden');
                document.body.style.overflow = 'hidden';
            }
        }

        function closeProductModal() {
            var modal = document.getElementById('productEditorModal');
            if (modal) {
                modal.classList.add('hidden');
                document.body.style.overflow = 'auto';
            }
        }

        (function () {
            var hid = document.querySelector("[id$='hidId']");
            var msg = document.querySelector("[id$='lblMsg']");
            var hasEdit = hid && hid.value && hid.value.length > 0;
            var hasMsg = msg && msg.textContent && msg.textContent.trim().length > 0;
            if (hasEdit || hasMsg) openProductModal();
        })();
    </script>
</asp:Content>
