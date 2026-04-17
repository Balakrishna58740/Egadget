<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="PaymentMethods.aspx.cs"
    Inherits="serena.Admin.PaymentMethodsPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Payment Methods</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
  <link rel="stylesheet" href="/Assets/css/admin-payments-redesign.css" />
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
  <div class="pay-page">
    <div class="pay-shell">
      <div class="pay-header">
        <div class="pay-header-row">
          <div>
          <h2>Payments</h2>
          <p>Control payment methods and monitor transaction activity.</p>
          </div>
          <button type="button" id="btnOpenAdd" class="pay-btn pay-btn-primary">Add Payment Method</button>
        </div>
      </div>

      <div class="pay-grid">
        <div class="pay-card pay-methods-card">
          <div class="pay-card-head">
            <h3>All Payment Methods</h3>
          </div>
          <div class="pay-card-body pay-card-body-tight">
            <asp:Label ID="lblMsg" runat="server" CssClass="hidden pay-alert" EnableViewState="false"></asp:Label>
          </div>
          <div class="pay-table-wrap">
            <table class="pay-table">
              <thead>
                <tr>
                  <th style="width:10%">No.</th>
                  <th style="width:45%">Name</th>
                  <th style="width:15%">Status</th>
                  <th class="text-right" style="width:30%">Actions</th>
                </tr>
              </thead>
              <tbody>
                <asp:Literal ID="litRows" runat="server" />
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <div class="pay-card pay-transactions-card">
        <div class="pay-card-head pay-transactions-head">
          <h3>Payment Transactions</h3>
          <span class="pay-total"><asp:Literal ID="litTxnTotal" runat="server" /> entries</span>
        </div>
        <div class="pay-table-wrap">
          <table class="pay-table pay-table-transactions">
            <thead>
              <tr>
                <th>Ref</th>
                <th>Client</th>
                <th>Method</th>
                <th>Transaction Ref</th>
                <th>Status</th>
                <th class="text-right">Magnitude</th>
                <th>Event Date</th>
                <th class="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              <asp:Literal ID="litTxnRows" runat="server" />
            </tbody>
          </table>
        </div>
      </div>

      <div id="payMethodModal" class="pay-modal hidden" aria-hidden="true">
        <div class="pay-modal-dialog" role="dialog" aria-modal="true" aria-labelledby="payMethodModalTitle">
          <div class="pay-card pay-modal-card">
            <div class="pay-card-head pay-modal-head">
              <h3 id="payMethodModalTitle">Add / Edit Payment Method</h3>
              <button type="button" class="pay-modal-close" id="btnModalX" aria-label="Close">&times;</button>
            </div>
            <div class="pay-card-body">
              <asp:HiddenField ID="hidId" runat="server" />

              <div class="pay-field">
                <label for="txtName" class="pay-label">Method Name</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="pay-input" MaxLength="100" />
                <p class="pay-help">Supported: Cash On Delivery, eSewa</p>
              </div>

              <div class="pay-check-wrap">
                <asp:CheckBox ID="chkUse" runat="server" />
                <asp:Label runat="server" AssociatedControlID="chkUse" CssClass="pay-check-label">
                    Active (is_use)
                </asp:Label>
              </div>

              <div class="pay-actions">
                <asp:Button ID="btnSave" runat="server" CssClass="pay-btn pay-btn-primary" Text="Save" OnClick="btnSave_Click" />
                <button type="button" id="btnCloseModal" class="pay-btn pay-btn-muted">Cancel</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <script>
    (function () {
      var m = document.getElementById('<%= lblMsg.ClientID %>');
      var hid = document.getElementById('<%= hidId.ClientID %>');
      var txt = document.getElementById('<%= txtName.ClientID %>');
      var chk = document.getElementById('<%= chkUse.ClientID %>');
      var modal = document.getElementById('payMethodModal');
      var openBtn = document.getElementById('btnOpenAdd');
      var closeBtn = document.getElementById('btnCloseModal');
      var closeX = document.getElementById('btnModalX');

      function openModal() {
        if (!modal) return;
        modal.classList.remove('hidden');
        modal.classList.add('block');
        modal.setAttribute('aria-hidden', 'false');
      }

      function closeModal() {
        if (!modal) return;
        modal.classList.add('hidden');
        modal.classList.remove('block');
        modal.setAttribute('aria-hidden', 'true');
      }

      if (openBtn) {
        openBtn.addEventListener('click', function () {
          if (hid) hid.value = '';
          if (txt) txt.value = '';
          if (chk) chk.checked = true;
          openModal();
          if (txt) txt.focus();
        });
      }

      if (closeBtn) closeBtn.addEventListener('click', closeModal);
      if (closeX) closeX.addEventListener('click', closeModal);

      if (modal) {
        modal.addEventListener('click', function (e) {
          if (e.target === modal) closeModal();
        });
      }

      if (m && m.textContent && m.textContent.trim().length > 0) {
        m.classList.remove('hidden');
        m.classList.add('block');
      }

      if ((hid && hid.value) || (m && m.classList.contains('alert-danger'))) {
        openModal();
      }
    })();
  </script>
</asp:Content>
