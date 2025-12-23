Imports System.Web.Http
Imports System.Web.Optimization

Public Class MvcApplication
    Inherits System.Web.HttpApplication

    Protected Sub Application_Start()
        AreaRegistration.RegisterAllAreas()
        GlobalConfiguration.Configure(AddressOf WebApiConfig.Register)
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
        RouteConfig.RegisterRoutes(RouteTable.Routes)
        BundleConfig.RegisterBundles(BundleTable.Bundles)
    End Sub

    Protected Sub Application_BeginRequest(sender As Object, e As EventArgs)
        Response.Charset = "utf-8"
        Response.ContentEncoding = System.Text.Encoding.UTF8
        Response.HeaderEncoding = System.Text.Encoding.UTF8
    End Sub
End Class
