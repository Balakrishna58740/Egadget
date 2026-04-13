<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="PaymentMethods.aspx.cs"
    Inherits="serena.Admin.PaymentMethodsPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Payment Methods</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
  <div class="row">
    <div class="col-lg-6">
      <div class="card shadow-sm mb-4">
        <div class="card-header bg-white"><strong>Add / Edit Payment Method</strong></div>
        <div class="card-body">
          <asp:Label ID="lblMsg" runat="server" CssClass="alert d-none" EnableViewState="false"></asp:Label>

          <asp:HiddenField ID="hidId" runat="server" />

          <div class="mb-3">
            <label for="txtName" class="form-label">Name</label>
            <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="100" />
          </div>

          <div class="form-check mb-3">
            <asp:CheckBox ID="chkUse" runat="server" />
            <asp:Label runat="server" AssociatedControlID="chkUse" CssClass="form-check-label">
                Active (is_use)
            </asp:Label>
          </div>

          <div class="d-flex gap-2">
            <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSave_Click" />
            <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-outline-secondary" Text="Cancel" OnClick="btnCancel_Click" CausesValidation="false" />
          </div>
        </div>
      </div>
    </div>

    <div class="col-lg-12">
      <div class="card shadow-sm">
        <div class="card-header bg-white"><strong>All Payment Methods</strong></div>
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-sm align-middle mb-0">
              <thead class="table-light">
                <tr>
                  <th style="width:8%">No.</th>
                  <th style="width:52%">Name</th>
                  <th style="width:15%">Status</th>
                  <th class="text-end" style="width:25%">Actions</th>
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

  <script>
    (function () {
      var m = document.getElementById('<%= lblMsg.ClientID %>');
      if (m && m.textContent && m.textContent.trim().length > 0) m.classList.remove('d-none');
    })();
  </script>
</asp:Content>
