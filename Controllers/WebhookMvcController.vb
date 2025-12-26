Imports System.Web.Mvc
Imports System.Configuration

Public Class WebhookMvcController
    Inherits Controller

    ' GET: WebhookMvc/Verify hoặc api/Webhook
    Function Verify(hub_mode As String, hub_verify_token As String, hub_challenge As String) As ActionResult
        Try
            ' Lấy parameters từ Request.QueryString (vì có thể có dấu chấm trong tên)
            If String.IsNullOrEmpty(hub_mode) Then hub_mode = Request.QueryString("hub.mode")
            If String.IsNullOrEmpty(hub_verify_token) Then hub_verify_token = Request.QueryString("hub.verify_token")
            If String.IsNullOrEmpty(hub_challenge) Then hub_challenge = Request.QueryString("hub.challenge")
            
            Dim verifyToken As String = ConfigurationManager.AppSettings("FacebookVerifyToken")
            
            ' Log để debug (có thể xóa sau)
            System.Diagnostics.Debug.WriteLine("Webhook Verify - hub_mode: " & hub_mode & ", token match: " & (hub_verify_token = verifyToken))
            
            If hub_mode = "subscribe" AndAlso hub_verify_token = verifyToken Then
                Response.ContentType = "text/plain"
                Return Content(hub_challenge, "text/plain")
            Else
                Response.ContentType = "text/plain"
                Return Content("Forbidden: hub_mode=" & hub_mode & ", token match=" & (hub_verify_token = verifyToken), "text/plain")
            End If
        Catch ex As Exception
            Response.ContentType = "text/plain"
            Return Content("Error: " & ex.Message, "text/plain")
        End Try
    End Function
End Class

