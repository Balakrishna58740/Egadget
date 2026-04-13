<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.StatusCode = 500;
        Response.TrySkipIisCustomErrors = true;
    }
</script>
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>Oops — Something went wrong</title>
  <style>
    :root{
      --bg:#0b0d12; --card:#141823; --muted:#8a95a7; --text:#e8eef7; --accent:#ffd166; --btn:#2b3342; --danger:#ff6b6b;
    }
    @media (prefers-color-scheme: light) {
      :root{ --bg:#f7f9fc; --card:#ffffff; --muted:#6b7280; --text:#111827; --accent:#b08900; --btn:#eef2f7; --danger:#b00020; }
    }
    *{ box-sizing:border-box; }
    body{ margin:0; font-family:system-ui,-apple-system,Segoe UI,Roboto,Ubuntu,Cantarell,'Helvetica Neue',Arial,'Noto Sans',sans-serif;
          background:linear-gradient(180deg, var(--bg) 0%, #121520 100%); color:var(--text); }
    .wrap{ min-height:100vh; display:flex; align-items:center; justify-content:center; padding:32px; }
    .card{ width:100%; max-width:780px; background:var(--card); border:1px solid rgba(255,255,255,0.06); border-radius:16px; padding:28px 24px; box-shadow:0 10px 30px rgba(0,0,0,0.35);}
    h1{ margin:0 0 6px; font-size:28px; letter-spacing:.2px; }
    p{ margin:8px 0 0; line-height:1.55; }
    .muted{ color:var(--muted); font-size:14px; }
    .row{ display:flex; flex-wrap:wrap; gap:12px; margin-top:20px;}
    .btn{ display:inline-flex; align-items:center; gap:8px; padding:10px 14px; border-radius:10px; border:1px solid rgba(255,255,255,0.12);
          background:var(--btn); color:var(--text); text-decoration:none; cursor:pointer; }
    .btn:hover{ filter:brightness(1.08); }
    .btn.primary{ background:var(--accent); color:#111; border-color:transparent; }
    .kbd{ font-family:ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', 'Courier New', monospace; font-size:13px;
          background:rgba(0,0,0,0.25); padding:2px 6px; border-radius:6px; }
    .idline{ margin-top:16px; font-size:14px; }
    details{ margin-top:14px; }
    summary{ cursor:pointer; user-select:none; color:var(--muted); }
    .hr{ height:1px; background:rgba(255,255,255,.08); border:0; margin:18px 0; }
  </style>
</head>
<body>
  <div class="wrap">
    <div class="card">
      <h1>Something went wrong</h1>
      <p>We’re sorry — an unexpected error occurred while processing your request.</p>

      <div class="idline muted">
        Error ID:
        <span class="kbd" id="errId"><%= Server.HtmlEncode(Request.QueryString["eid"] ?? "") %></span>
        <button class="btn" id="copyBtn" type="button" title="Copy error ID">Copy</button>
      </div>

      <details>
        <summary>Technical info</summary>
        <div class="muted" style="margin-top:8px">
          <div>Time (UTC): <span class="kbd"><%= DateTime.UtcNow.ToString("u") %></span></div>
          <div>Path: <span class="kbd"><%= Server.HtmlEncode(Request.RawUrl ?? "") %></span></div>
          <div>Status: <span class="kbd">500</span></div>
        </div>
      </details>

      <hr class="hr" />

      <div class="row">
        <a class="btn primary" href="<%= ResolveUrl("~/") %>">Back to Home</a>
        <button class="btn" type="button" onclick="window.history.back()">Go Back</button>
        <button class="btn" type="button" onclick="location.reload()">Try Again</button>
      </div>

      <p class="muted" style="margin-top:16px">
        If the problem persists, share the Error ID with the site administrator.
      </p>
    </div>
  </div>

  <script>
    (function(){
      var b=document.getElementById('copyBtn'), t=document.getElementById('errId');
      if(!b||!t) return;
      b.addEventListener('click', function(){
        var id=(t.textContent||t.innerText||'').trim();
        if(!id){ b.textContent='No ID'; return; }
        try{ navigator.clipboard.writeText(id).then(function(){ b.textContent='Copied'; setTimeout(function(){ b.textContent='Copy'; },1200); }); }
        catch(e){
          var ta=document.createElement('textarea'); ta.value=id; document.body.appendChild(ta); ta.select();
          document.execCommand('copy'); document.body.removeChild(ta);
          b.textContent='Copied'; setTimeout(function(){ b.textContent='Copy'; },1200);
        }
      });
    })();
  </script>
</body>
</html>
