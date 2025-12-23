Imports System.Configuration
Imports System.Text

Public Class FacebookController
    Inherits System.Web.Mvc.Controller

    ' GET: Facebook/Test
    Function Test() As ActionResult
        Response.Charset = "utf-8"
        Response.ContentEncoding = Encoding.UTF8
        Return View()
    End Function

    ' GET: Facebook/WebhookInfo
    Function WebhookInfo() As ActionResult
        ViewData("WebhookUrl") = Request.Url.Scheme & "://" & Request.Url.Authority & "/api/Webhook"
        ViewData("VerifyToken") = ConfigurationManager.AppSettings("FacebookVerifyToken")
        Return View()
    End Function
End Class

