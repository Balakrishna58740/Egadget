<%@ WebHandler Language="C#" Class="serena.Api.OrdersReorderHandler" %>

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace serena.Api
{
    public class OrdersReorderHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            if (context.User == null || context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 401;
                context.Response.Write("{\"ok\":false,\"message\":\"Unauthorized\"}");
                return;
            }

            try
            {
                string body;
                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    body = reader.ReadToEnd();
                }

                var serializer = new JavaScriptSerializer();
                var payload = serializer.Deserialize<ReorderPayload>(body ?? "{}") ?? new ReorderPayload();

                var ids = payload.orderIds ?? new List<string>();
                context.Session["AdminOrdersCustomOrder"] = ids;

                if (payload.orderId > 0 && !string.IsNullOrWhiteSpace(payload.statusChange))
                {
                    Db.Execute("UPDATE orders SET status=@s WHERE id=@id", Db.P("@s", payload.statusChange.Trim()), Db.P("@id", payload.orderId));
                }

                WriteAuditLog(context, payload);

                context.Response.StatusCode = 200;
                context.Response.Write("{\"ok\":true}");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("{\"ok\":false,\"message\":" + Quote(ex.Message) + "}");
            }
        }

        private static string Quote(string s)
        {
            return "\"" + (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private static void WriteAuditLog(HttpContext context, ReorderPayload payload)
        {
            try
            {
                string dir = context.Server.MapPath("~/App_Data");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, "order-reorder-audit.log");

                string user = context.User != null && context.User.Identity != null ? context.User.Identity.Name : "unknown";
                string ids = payload.orderIds != null ? string.Join(",", payload.orderIds.ToArray()) : "";
                string line = DateTime.UtcNow.ToString("o") + " | user=" + user + " | orderId=" + payload.orderId + " | newPosition=" + payload.newPosition + " | statusChange=" + (payload.statusChange ?? "") + " | ids=" + ids;
                File.AppendAllText(path, line + Environment.NewLine);
            }
            catch
            {
            }
        }

        private class ReorderPayload
        {
            public int orderId { get; set; }
            public int newPosition { get; set; }
            public string statusChange { get; set; }
            public List<string> orderIds { get; set; }
        }
    }
}
