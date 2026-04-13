<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Setup.aspx.cs" Inherits="serena.SetupPage" %>
<!doctype html>
<html lang="en">
<head runat="server">
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>eGadgetHub • Setup</title>
  <link href="Assets/css/site.css" rel="stylesheet" />
  <style>
    body { padding: 24px; }
    .card { border:1px solid #e5e7eb; border-radius:12px; background:#fff; }
    .card .hd { padding:12px 16px; border-bottom:1px solid #e5e7eb; font-weight:700; }
    .card .bd { padding:16px; }
    .muted { color:#6b7280 }
    .actions{ display:flex; gap:.5rem; flex-wrap:wrap }
    pre{ background:#0b1020; color:#d1e7ff; padding:12px; border-radius:8px; overflow:auto; }
    .alert{ padding:10px 12px; border-radius:8px; margin-bottom:10px }
    .alert-success{ background:#ecfdf5; border:1px solid #10b98133; color:#065f46 }
    .alert-danger{ background:#fef2f2; border:1px solid #ef444433; color:#7f1d1d }
    .btn{ display:inline-block; padding:.5rem .85rem; border-radius:8px; border:1px solid #d1d5db; background:#fff }
    .btn-primary{ background:#2563eb; border-color:#2563eb; color:#fff }
    .btn-outline{ background:#fff; color:#374151 }
    .hidden{ display:none !important }
    .kv{ display:grid; grid-template-columns:160px 1fr; gap:.5rem .75rem; margin:.25rem 0 }
    code { background:#f8fafc; border:1px solid #e5e7eb; padding:.1rem .35rem; border-radius:6px }
  </style>
</head>
<body>
  <form id="form1" runat="server">
    <div class="card">
      <div class="hd">eGadgetHub • First-Run Setup</div>
      <div class="bd">
        <asp:Label ID="lblMsg" runat="server" CssClass="alert hidden"></asp:Label>

        <div class="kv">
          <div class="muted">Connection</div><div><asp:Literal ID="litConn" runat="server" /></div>
          <div class="muted">Protect with key</div><div>Append <code>?k=&lt;SetupKey&gt;</code> to the URL (see Web.config).</div>
        </div>

        <div class="actions" id="rowBtns" runat="server">
          <asp:Button ID="btnMigrate" runat="server" Text="Run Migrations" CssClass="btn btn-primary" OnClick="btnMigrate_Click" />
          <asp:Button ID="btnSeed" runat="server" Text="Seed Sample Data" CssClass="btn btn-outline" OnClick="btnSeed_Click" />
          <asp:Button ID="btnBoth" runat="server" Text="Run Both" CssClass="btn btn-outline" OnClick="btnBoth_Click" />
        </div>

        <p class="muted" style="margin:.75rem 0 0">Tip: remove or secure this page after you’ve initialized your DB.</p>

        <pre><asp:Literal ID="litLog" runat="server" /></pre>
      </div>
    </div>
  </form>
</body>
</html>
