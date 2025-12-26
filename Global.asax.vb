Imports System.Web.Http
Imports System.Web.Optimization
Imports System.Linq

Public Class MvcApplication
    Inherits System.Web.HttpApplication

    Protected Sub Application_Start()
        AreaRegistration.RegisterAllAreas()
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
        ' Đăng ký Web API TRƯỚC MVC để Web API routes được xử lý trước
        GlobalConfiguration.Configure(AddressOf WebApiConfig.Register)
        ' Sau đó mới đăng ký MVC routes
        RouteConfig.RegisterRoutes(RouteTable.Routes)
        BundleConfig.RegisterBundles(BundleTable.Bundles)
    End Sub

    Protected Sub Application_BeginRequest(sender As Object, e As EventArgs)
        ' Bỏ qua static files để tránh lỗi
        Dim path As String = Request.Path.ToLower()
        Dim staticExtensions As String() = {".css", ".js", ".jpg", ".jpeg", ".png", ".gif", ".ico", ".svg", ".woff", ".woff2", ".ttf", ".eot", ".map", ".json", ".xml"}
        Dim staticFolders As String() = {"/content/", "/scripts/", "/images/", "/fonts/"}
        Dim isStaticFile As Boolean = staticExtensions.Any(Function(ext) path.EndsWith(ext)) OrElse 
                                       staticFolders.Any(Function(folder) path.StartsWith(folder))
        
        ' Log để debug webhook requests - Log TẤT CẢ requests đến /api/Webhook (không phân biệt hoa thường)
        If path.Contains("/api/webhook") OrElse Request.RawUrl.ToLower().Contains("/api/webhook") Then
            System.Diagnostics.Debug.WriteLine("========================================")
            System.Diagnostics.Debug.WriteLine("=== Application_BeginRequest ===")
            System.Diagnostics.Debug.WriteLine("========================================")
            System.Diagnostics.Debug.WriteLine("Path: " & Request.Path)
            System.Diagnostics.Debug.WriteLine("Method: " & Request.HttpMethod)
            System.Diagnostics.Debug.WriteLine("QueryString: " & Request.QueryString.ToString())
            System.Diagnostics.Debug.WriteLine("RawUrl: " & Request.RawUrl)
            System.Diagnostics.Debug.WriteLine("Url: " & Request.Url.ToString())
            System.Diagnostics.Debug.WriteLine("IsSecureConnection: " & Request.IsSecureConnection)
            System.Diagnostics.Debug.WriteLine("ContentType: " & Request.ContentType)
            System.Diagnostics.Debug.WriteLine("ContentLength: " & Request.ContentLength.ToString())
            System.Diagnostics.Debug.WriteLine("UserAgent: " & Request.UserAgent)
            System.Diagnostics.Debug.WriteLine("========================================")
            
            ' Nếu là POST request, log thêm thông tin
            If Request.HttpMethod = "POST" Then
                System.Diagnostics.Debug.WriteLine(">>> POST REQUEST DETECTED TO /api/Webhook")
                System.Diagnostics.Debug.WriteLine(">>> This should be handled by WebhookController.ReceiveWebhook")
            End If
        End If
        
        If isStaticFile Then
            Return
        End If
        
        Try
            Response.Charset = "utf-8"
            Response.ContentEncoding = System.Text.Encoding.UTF8
            Response.HeaderEncoding = System.Text.Encoding.UTF8
        Catch
            ' Ignore errors for static files
        End Try
    End Sub
End Class
