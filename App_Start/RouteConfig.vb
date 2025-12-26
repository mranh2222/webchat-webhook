Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Mvc
Imports System.Web.Routing

Public Module RouteConfig
    Public Sub RegisterRoutes(ByVal routes As RouteCollection)
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")
        
        ' Route đặc biệt cho Webhook GET - chỉ match chính xác api/Webhook (không có path sau)
        ' Route này sẽ KHÔNG match api/Webhook/GetData vì không có catchall
        routes.MapRoute(
            name:="WebhookVerify",
            url:="api/Webhook",
            defaults:=New With {.controller = "WebhookMvc", .action = "Verify"},
            constraints:=New With {.httpMethod = New System.Web.Routing.HttpMethodConstraint("GET")}
        )
        
        ' Ignore tất cả api routes khác để Web API xử lý (trừ api/Webhook GET đã match ở trên)
        ' Lưu ý: IgnoreRoute chỉ áp dụng cho MVC, không ảnh hưởng Web API
        routes.IgnoreRoute("api/{*pathInfo}")

        routes.MapRoute(
            name:="Default",
            url:="{controller}/{action}/{id}",
            defaults:=New With {.controller = "Home", .action = "Index", .id = UrlParameter.Optional}
        )
    End Sub
End Module