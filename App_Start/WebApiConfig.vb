Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web.Http
Imports System.Net.Http.Formatting

Public Module WebApiConfig
    Public Sub Register(ByVal config As HttpConfiguration)
        ' Web API configuration and services

        ' Xóa XML formatter, chỉ dùng JSON
        config.Formatters.Remove(config.Formatters.XmlFormatter)
        ' Đặt JSON làm mặc định
        config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented

        ' Web API routes - MapHttpAttributeRoutes PHẢI được gọi trước
        config.MapHttpAttributeRoutes()
        
        ' Convention-based route (fallback)
        config.Routes.MapHttpRoute(
            name:="DefaultApi",
            routeTemplate:="api/{controller}/{id}",
            defaults:=New With {.id = RouteParameter.Optional}
        )
    End Sub
End Module
