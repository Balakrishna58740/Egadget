<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Feedbacks.aspx.cs"
    Inherits="serena.Admin.FeedbacksPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Feedbacks</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
  <div class="row">
    <!-- Reply panel -->
    <div class="col-lg-7">
      <div class="card shadow-sm mb-4">
        <div class="card-header bg-white"><strong>Reply</strong></div>
        <div class="card-body">
          <asp:Label ID="lblMsg" runat="server" CssClass="alert d-none" EnableViewState="false"></asp:Label>
          <asp:HiddenField ID="hidId" runat="server" />

          <div class="mb-2"><small class="text-muted">Name</small><div><asp:Literal ID="litName" runat="server" /></div></div>
          <div class="mb-2"><small class="text-muted">Email</small><div><asp:Literal ID="litEmail" runat="server" /></div></div>
          <div class="mb-2"><small class="text-muted">Title</small><div><asp:Literal ID="litTitle" runat="server" /></div></div>
          <div class="mb-3">
            <small class="text-muted">Message</small>
            <div class="border rounded p-2 bg-light" style="white-space:pre-wrap"><asp:Literal ID="litMessage" runat="server" /></div>
          </div>

          <div class="mb-3">
            <label for="txtReply" class="form-label">Your reply</label>
            <asp:TextBox ID="txtReply" runat="server" TextMode="MultiLine" Rows="5" CssClass="form-control" />
            <div class="form-text">Saving a reply will mark the feedback as <strong>Complete</strong>.</div>
          </div>

          <div class="d-flex gap-2">
            <asp:Button ID="btnSave" runat="server" CssClass="btn btn-success" Text="Save Reply" OnClick="btnSave_Click" />
            <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-outline-secondary" Text="Cancel" OnClick="btnCancel_Click" CausesValidation="false" />
          </div>
        </div>
      </div>
    </div>

    <!-- List -->
    <div class="col-lg-12">
      <div class="card shadow-sm">
        <div class="card-header bg-white d-flex align-items-center justify-content-between">
          <div>
            <strong>Feedbacks</strong>
            <span class="ms-2 text-muted"><asp:Literal ID="litTotal" runat="server" /></span>
          </div>
          <ul class="nav nav-tabs card-header-tabs">
            <li class="nav-item">
              <a class="nav-link" id="tabPending" href="Feedbacks.aspx?tab=pending">
                Pending (<asp:Literal ID="litCountPending" runat="server" />)
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link" id="tabComplete" href="Feedbacks.aspx?tab=complete">
                Complete (<asp:Literal ID="litCountComplete" runat="server" />)
              </a>
            </li>
          </ul>
        </div>

        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-sm align-middle mb-0">
              <thead class="table-light">
                <tr>
                  <th style="width:6%">No.</th>
                  <th style="width:18%">Name</th>
                  <th style="width:18%">Email</th>
                  <th style="width:38%">Title</th>
                  <th style="width:12%">Created</th>
                  <th class="text-end" style="width:8%">Actions</th>
                </tr>
              </thead>
              <tbody>
                <asp:Literal ID="litRows" runat="server" />
              </tbody>
            </table>
          </div>
          <div class="p-2 border-top">
            <asp:Literal ID="pager" runat="server" />
          </div>
        </div>
      </div>
    </div>
  </div>

  <script>
    (function () {
      var m = document.getElementById('<%= lblMsg.ClientID %>');
      if (m && m.textContent && m.textContent.trim().length > 0) m.classList.remove('d-none');

      // set active tab from server-provided css class
      var active = '<%= ActiveTabCss %>'; // "pending" or "complete"
      var p = document.getElementById('tabPending');
      var c = document.getElementById('tabComplete');
      if (active === 'pending' && p) { p.classList.add('active'); }
      if (active === 'complete' && c) { c.classList.add('active'); }
    })();
  </script>
</asp:Content>
