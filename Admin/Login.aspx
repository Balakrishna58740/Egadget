<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="serena.Admin.Login" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>Admin Login</title>
  <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
</head>
<body class="bg-light">
  <form id="form1" runat="server">
    <div class="container py-5">
      <div class="row justify-content-center">
        <div class="col-md-6 col-lg-4">
          <div class="card shadow-sm border-0">
            <div class="card-body p-4">
              <h1 class="h2 mb-3 text-center text-primary font-bold">eGadgetHub</h1>
              <h1 class="h5 mb-3 text-center">Login to your account</h1>

              <asp:Label ID="lblMsg" runat="server" CssClass="alert alert-danger d-none" EnableViewState="false"></asp:Label>

              <div class="mb-3">
                <label for="txtUser" class="form-label">Username</label>
                <asp:TextBox ID="txtUser" runat="server" CssClass="form-control" MaxLength="255" />
              </div>

              <div class="mb-3">
                <label for="txtPass" class="form-label">Password</label>
                <asp:TextBox ID="txtPass" runat="server" TextMode="Password" CssClass="form-control" MaxLength="255" />
              </div>

              <div class="d-grid">
                <asp:Button ID="btnLogin" runat="server" Text="Sign in" CssClass="btn btn-primary" OnClick="btnLogin_Click" />
              </div>

              <div class="text-center mt-4">
                <a href="~/Account/Login.aspx" runat="server" class="text-decoration-none small">Back to customer login</a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </form>

  <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
  <script>
    (function () {
      var el = document.getElementById("<%= lblMsg.ClientID %>");
      if (el && el.textContent && el.textContent.trim().length > 0) el.classList.remove("d-none");
    })();
  </script>
</body>
</html>
