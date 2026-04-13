<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="ReportOrder.aspx.cs"
    Inherits="serena.Admin.ReportOrderPrintPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Order Revenue Report</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
  <style>
    /* Print only the report area; hide the rest of the Admin master */
    @media print {
      body * { visibility: hidden !important; }
      .print-area, .print-area * { visibility: visible !important; }
      .print-area { position: absolute; left: 0; top: 0; width: 100%; }
      .card, .card-body, .card-header { border: none !important; box-shadow: none !important; }
    }
    .report-meta small { color:#6c757d; }
  </style>

  <div class="print-area">
    <div class="card shadow-sm">
      <div class="card-header bg-white d-flex align-items-center justify-content-between">
        <strong><i class="fa fa-line-chart me-2"></i>Order Revenue Report</strong>
        <div class="report-meta">
          <small>Period: <asp:Literal ID="litPeriod" runat="server" /> &nbsp;·&nbsp; Generated: <asp:Literal ID="litGenerated" runat="server" /></small>
        </div>
      </div>

      <div class="card-body">

        <!-- Summary -->
        <div class="mb-3">
          <asp:Literal ID="litSummary" runat="server" />
        </div>

        <!-- Table -->
        <div class="table-responsive">
          <table class="table table-sm align-middle">
            <thead class="table-light">
              <tr>
                <th style="width:8%">No.</th>
                <th style="width:20%">Order Code</th>
                <th>Customer</th>
                <th style="width:14%">Payment</th>
                <th class="text-end" style="width:14%">Amount (RS)</th>
                <th style="width:18%">Ordered Date</th>
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

  <script>
      // Auto print ONLY this template (loaded in hidden iframe)
      window.addEventListener('load', function () { setTimeout(function () { window.print(); }, 150); });
  </script>
</asp:Content>
