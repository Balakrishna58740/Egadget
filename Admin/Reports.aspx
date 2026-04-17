<%@ Page Language="C#" MasterPageFile="~/MasterPages/Admin.master"
    AutoEventWireup="true" CodeFile="Reports.aspx.cs"
    Inherits="serena.Admin.ReportsPage" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">Reports | eGadgetHub Admin</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="/Assets/css/admin-reports-redesign.css" />
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script src="/Assets/js/admin-reports.js"></script>
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">
    <div class="rp-wrap">
        <asp:Label ID="lblMsg" runat="server" CssClass="rp-alert d-none"></asp:Label>

        <div class="rp-card">
            <div class="rp-card-head">
                <h2>REPORTS STUDIO</h2>
                <p><strong>TRANSACTION SEARCH LOGIC</strong> - <asp:Literal ID="litPeriod" runat="server"></asp:Literal></p>
            </div>
            <div class="rp-card-body">
                <div class="rp-filter-grid">
                    <div class="rp-filter-type">
                        <label>TRANSACTION SEARCH LOGIC</label>
                        <asp:RadioButtonList ID="rblType" runat="server" RepeatDirection="Horizontal" CssClass="rp-rbl">
                            <asp:ListItem Value="range" Selected="True">Date Range</asp:ListItem>
                            <asp:ListItem Value="monthly">Monthly</asp:ListItem>
                            <asp:ListItem Value="yearly">Yearly</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>

                    <div id="grpRange" class="rp-range-grid">
                        <div>
                            <label>CHRONICLE FROM</label>
                            <asp:TextBox ID="txtFrom" runat="server" TextMode="Date" CssClass="rp-input"></asp:TextBox>
                        </div>
                        <div>
                            <label>CHRONICLE TO</label>
                            <asp:TextBox ID="txtTo" runat="server" TextMode="Date" CssClass="rp-input"></asp:TextBox>
                        </div>
                    </div>

                    <div id="grpMonth" class="rp-hidden">
                        <label>EVENT DATE (Month)</label>
                        <asp:TextBox ID="txtMonth" runat="server" ClientIDMode="Static" CssClass="rp-input"></asp:TextBox>
                    </div>

                    <div id="grpYear" class="rp-hidden">
                        <label>EVENT DATE (Year)</label>
                        <asp:TextBox ID="txtYear" runat="server" CssClass="rp-input" placeholder="2026"></asp:TextBox>
                    </div>
                </div>

                <div class="rp-actions">
                    <asp:Button ID="btnExecuteFilter" runat="server" Text="EXECUTE FILTER" CssClass="rp-btn rp-btn-primary" OnClick="btnExecuteFilter_Click"></asp:Button>
                    <asp:Button ID="btnReset" runat="server" Text="RESET STUDIO" CssClass="rp-btn rp-btn-ghost" CausesValidation="false" OnClick="btnReset_Click"></asp:Button>
                </div>
            </div>
        </div>

        <div class="rp-kpi-grid">
            <div class="rp-kpi-card"><div class="rp-kpi-label">ORDER MAGNITUDE</div><div class="rp-kpi-value"><asp:Literal ID="litTotalOrders" runat="server"></asp:Literal></div></div>
            <div class="rp-kpi-card"><div class="rp-kpi-label">ITEM MAGNITUDE</div><div class="rp-kpi-value"><asp:Literal ID="litTotalItems" runat="server"></asp:Literal></div></div>
            <div class="rp-kpi-card"><div class="rp-kpi-label">REVENUE MAGNITUDE</div><div class="rp-kpi-value">RS <asp:Literal ID="litRevenue" runat="server"></asp:Literal></div></div>
            <div class="rp-kpi-card"><div class="rp-kpi-label">AOV MAGNITUDE</div><div class="rp-kpi-value">RS <asp:Literal ID="litAov" runat="server"></asp:Literal></div></div>
        </div>

        <div class="rp-chart-grid">
            <div class="rp-card"><div class="rp-card-head compact"><h3>Revenue Trend</h3></div><div class="rp-card-body chart-body"><canvas id="revenueChart"></canvas></div></div>
            <div class="rp-card"><div class="rp-card-head compact"><h3>Payment Mix</h3></div><div class="rp-card-body chart-body"><canvas id="paymentChart"></canvas></div></div>
        </div>

        <div class="rp-card">
            <div class="rp-card-head compact"><h3>Transactions</h3></div>
            <div class="rp-card-body p0">
                <div class="rp-table-wrap">
                    <table class="rp-table">
                        <thead>
                            <tr><th>No.</th><th>Order Code</th><th>Customer</th><th>Payment</th><th class="right">MAGNITUDE</th><th>EVENT DATE</th></tr>
                        </thead>
                        <tbody><asp:Literal ID="litRows" runat="server"></asp:Literal></tbody>
                    </table>
                </div>
            </div>
        </div>

        <asp:HiddenField ID="hidLineLabels" runat="server" ClientIDMode="Static"></asp:HiddenField>
        <asp:HiddenField ID="hidLineData" runat="server" ClientIDMode="Static"></asp:HiddenField>
        <asp:HiddenField ID="hidPaymentLabels" runat="server" ClientIDMode="Static"></asp:HiddenField>
        <asp:HiddenField ID="hidPaymentData" runat="server" ClientIDMode="Static"></asp:HiddenField>
    </div>

</asp:Content>
