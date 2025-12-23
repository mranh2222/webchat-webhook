Imports System.Web.Http
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Configuration
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class WebhookController
    Inherits ApiController

    ' GET: api/Webhook - Xác thực webhook từ Facebook
    <HttpGet>
    Public Function VerifyWebhook(<FromUri> hub_mode As String,
                                   <FromUri> hub_verify_token As String,
                                   <FromUri> hub_challenge As String) As HttpResponseMessage

        ' Verify token - bạn cần thay đổi token này trong Web.config
        Dim verifyToken As String = ConfigurationManager.AppSettings("FacebookVerifyToken")
        
        If hub_mode = "subscribe" AndAlso hub_verify_token = verifyToken Then
            Dim response As New HttpResponseMessage(System.Net.HttpStatusCode.OK)
            response.Content = New StringContent(hub_challenge)
            Return response
        Else
            Return New HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
        End If
    End Function

    ' POST: api/Webhook - Nhận dữ liệu từ Facebook
    <HttpPost>
    Public Async Function ReceiveWebhook() As Task(Of HttpResponseMessage)
        Try
            Dim jsonContent As String = Await Request.Content.ReadAsStringAsync()
            Dim data As JObject = JsonConvert.DeserializeObject(Of JObject)(jsonContent)

            ' Facebook gửi dữ liệu dưới dạng object với field "entry"
            If data("object") IsNot Nothing Then
                Dim objectType As String = data("object").ToString()

                ' Xử lý entry array
                If data("entry") IsNot Nothing Then
                    For Each entry As JObject In data("entry")
                        ProcessEntry(entry, objectType)
                    Next
                End If
            End If

            Return New HttpResponseMessage(System.Net.HttpStatusCode.OK)
        Catch ex As Exception
            ' Log lỗi nếu cần
            Return New HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
        End Try
    End Function

    Private Sub ProcessEntry(entry As JObject, objectType As String)
        ' Xử lý comments trên bài viết
        If objectType = "page" OrElse objectType = "instagram" Then
            If entry("changes") IsNot Nothing Then
                For Each change As JObject In entry("changes")
                    Dim value As JObject = change("value")
                    
                    ' Xử lý comment
                    If value("item") IsNot Nothing AndAlso value("item").ToString() = "comment" Then
                        Dim postIdValue As String = ""
                        Dim commentIdValue As String = ""
                        Dim messageValue As String = ""
                        Dim fromNameValue As String = ""
                        Dim fromIdValue As String = ""
                        
                        If value("post_id") IsNot Nothing Then postIdValue = value("post_id").ToString()
                        If value("comment_id") IsNot Nothing Then commentIdValue = value("comment_id").ToString()
                        If value("message") IsNot Nothing Then messageValue = value("message").ToString()
                        If value("from") IsNot Nothing Then
                            If value("from")("name") IsNot Nothing Then fromNameValue = value("from")("name").ToString()
                            If value("from")("id") IsNot Nothing Then fromIdValue = value("from")("id").ToString()
                        End If
                        
                        Dim commentEntry As New WebhookEntry() With {
                            .Id = Guid.NewGuid().ToString(),
                            .Type = "Comment",
                            .PostId = postIdValue,
                            .CommentId = commentIdValue,
                            .Message = messageValue,
                            .FromName = fromNameValue,
                            .FromId = fromIdValue,
                            .Timestamp = DateTime.Now,
                            .RawData = value.ToString()
                        }
                        ' Lưu vào Firebase
                        Task.Run(Async Function()
                                     Await FirebaseService.AddEntryAsync(commentEntry)
                                 End Function)
                    End If
                Next
            End If
        End If

        ' Xử lý messages từ Messenger
        If entry("messaging") IsNot Nothing Then
            For Each messaging As JObject In entry("messaging")
                Dim message As JObject = messaging("message")
                If message IsNot Nothing Then
                    Dim senderIdValue As String = ""
                    Dim midValue As String = ""
                    Dim textValue As String = ""
                    
                    If messaging("sender") IsNot Nothing AndAlso messaging("sender")("id") IsNot Nothing Then
                        senderIdValue = messaging("sender")("id").ToString()
                    End If
                    If message("mid") IsNot Nothing Then midValue = message("mid").ToString()
                    If message("text") IsNot Nothing Then textValue = message("text").ToString()
                    
                    Dim messageEntry As New WebhookEntry() With {
                        .Id = Guid.NewGuid().ToString(),
                        .Type = "Message",
                        .PostId = senderIdValue,
                        .CommentId = midValue,
                        .Message = textValue,
                        .FromName = senderIdValue,
                        .FromId = senderIdValue,
                        .Timestamp = DateTime.Now,
                        .RawData = messaging.ToString()
                    }
                    ' Lưu vào Firebase
                    Task.Run(Async Function()
                                 Await FirebaseService.AddEntryAsync(messageEntry)
                             End Function)
                End If
            Next
        End If
    End Sub

    ' GET: api/Webhook/GetData - Lấy dữ liệu webhook để hiển thị
    <HttpGet>
    <Route("api/Webhook/GetData")>
    Public Async Function GetWebhookData() As Task(Of IHttpActionResult)
        Try
            Dim data As List(Of WebhookEntry) = Await FirebaseService.GetEntriesAsync(50)
            Return Ok(data)
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

    ' GET: api/Webhook/Clear - Xóa dữ liệu (để test)
    <HttpGet>
    <Route("api/Webhook/Clear")>
    Public Async Function ClearData() As Task(Of IHttpActionResult)
        Try
            Dim success As Boolean = Await FirebaseService.ClearAllEntriesAsync()
            If success Then
                Return Ok(New With {.message = "Data cleared"})
            Else
                Return InternalServerError(New Exception("Không thể xóa dữ liệu từ Firebase"))
            End If
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

    ' GET: api/Webhook/FirebaseInfo - Lấy thông tin Firebase
    <HttpGet>
    <Route("api/Webhook/FirebaseInfo")>
    Public Async Function GetFirebaseInfo() As Task(Of IHttpActionResult)
        Try
            Dim info As Object = FirebaseService.GetFirebaseInfo()
            Dim isConnected As Boolean = Await FirebaseService.TestConnectionAsync()
            Dim totalEntries As Integer = Await FirebaseService.GetEntriesCountAsync()
            Dim errorMessage As String = Nothing
            
            ' Nếu không kết nối được, thử lấy thông tin lỗi
            If Not isConnected AndAlso info.IsConfigured Then
                errorMessage = "Không thể kết nối đến Firebase. Kiểm tra URL và Secret trong Web.config"
            End If
            
            Return Ok(New With {
                .BaseUrl = info.BaseUrl,
                .IsConfigured = info.IsConfigured,
                .IsConnected = isConnected,
                .TotalEntries = totalEntries,
                .ErrorMessage = errorMessage
            })
        Catch ex As Exception
            Return Ok(New With {
                .BaseUrl = "",
                .IsConfigured = False,
                .IsConnected = False,
                .TotalEntries = 0,
                .ErrorMessage = ex.Message
            })
        End Try
    End Function
End Class

