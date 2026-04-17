<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Feedbacks.aspx.cs"
    Inherits="serena.Admin.FeedbacksPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Feedbacks</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="/Assets/css/admin-feedbacks-redesign.css" />
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="fb-page row">
        <asp:Label ID="lblMsg" runat="server" ClientIDMode="Static" CssClass="alert d-none fb-alert mb-3" EnableViewState="false"></asp:Label>
        <asp:HiddenField ID="hidId" runat="server" ClientIDMode="Static"></asp:HiddenField>

        <div class="col-lg-12">
            <div class="card shadow-sm fb-card">
                <div class="card-header bg-white d-flex align-items-center justify-content-between fb-card-head">
                    <div>
                        <strong>Feedbacks</strong>
                        <span class="ms-2 text-muted"><asp:Literal ID="litTotal" runat="server"></asp:Literal></span>
                    </div>
                    <div class="fb-counts">
                        <span class="fb-count-pill">Pending: <asp:Literal ID="litCountPending" runat="server"></asp:Literal></span>
                        <span class="fb-count-pill">Complete: <asp:Literal ID="litCountComplete" runat="server"></asp:Literal></span>
                    </div>
                </div>

                <div class="card-body p-0 fb-card-body">
                    <div class="table-responsive">
                        <table class="table table-sm align-middle mb-0">
                            <thead class="table-light">
                                <tr>
                                    <th style="width:6%">No.</th>
                                    <th style="width:18%">Name</th>
                                    <th style="width:18%">Email</th>
                                    <th style="width:30%">Title</th>
                                    <th style="width:10%">Status</th>
                                    <th style="width:12%">Created</th>
                                    <th class="text-end" style="width:16%">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Literal ID="litRows" runat="server"></asp:Literal>
                            </tbody>
                        </table>
                    </div>
                    <div class="p-2 border-top">
                        <asp:Literal ID="pager" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div id="fbReplyModal" class="fb-modal d-none" aria-hidden="true">
        <div class="fb-modal-dialog">
            <div class="card shadow-sm fb-card">
                <div class="card-header bg-white fb-card-head d-flex justify-content-between align-items-center">
                    <strong>Reply</strong>
                    <a href="Feedbacks.aspx" class="btn btn-sm btn-outline-secondary">Close</a>
                </div>
                <div class="card-body fb-card-body">
                    <div class="mb-2"><small class="text-muted">Name</small><div><asp:Literal ID="litName" runat="server"></asp:Literal></div></div>
                    <div class="mb-2"><small class="text-muted">Email</small><div><asp:Literal ID="litEmail" runat="server"></asp:Literal></div></div>
                    <div class="mb-2"><small class="text-muted">Title</small><div><asp:Literal ID="litTitle" runat="server"></asp:Literal></div></div>
                    <div class="mb-3">
                        <small class="text-muted">Message</small>
                        <div class="border rounded p-2 bg-light" style="white-space:pre-wrap"><asp:Literal ID="litMessage" runat="server"></asp:Literal></div>
                    </div>

                    <div class="mb-3">
                        <label for="txtReply" class="form-label">Your reply</label>
                        <asp:TextBox ID="txtReply" runat="server" TextMode="MultiLine" Rows="5" CssClass="form-control"></asp:TextBox>
                        <div class="form-text">Saving a reply will mark the feedback as <strong>Complete</strong>.</div>
                    </div>

                    <div class="d-flex gap-2">
                        <asp:Button ID="btnSave" runat="server" CssClass="btn btn-success" Text="Save Reply" OnClick="btnSave_Click"></asp:Button>
                        <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-outline-secondary" Text="Cancel" OnClick="btnCancel_Click" CausesValidation="false"></asp:Button>
                    </div>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
