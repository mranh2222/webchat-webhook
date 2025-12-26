Imports System.Web.Http
Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Configuration
Imports System.Linq
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class WebhookController
    Inherits ApiController

    ' GET: api/Webhook - Xử lý verification từ Facebook
    <HttpGet>
    <Route("api/Webhook")>
    Public Function VerifyWebhook() As HttpResponseMessage
        Try
            System.Diagnostics.Debug.WriteLine("=== WebhookController.VerifyWebhook CALLED ===")
            System.Diagnostics.Debug.WriteLine("Request URI: " & Request.RequestUri.ToString())
            System.Diagnostics.Debug.WriteLine("Query String: " & Request.RequestUri.Query)
            
            Dim hubMode As String = Request.GetQueryNameValuePairs().FirstOrDefault(Function(x) x.Key = "hub.mode").Value
            Dim hubVerifyToken As String = Request.GetQueryNameValuePairs().FirstOrDefault(Function(x) x.Key = "hub.verify_token").Value
            Dim hubChallenge As String = Request.GetQueryNameValuePairs().FirstOrDefault(Function(x) x.Key = "hub.challenge").Value
            
            Dim verifyToken As String = ConfigurationManager.AppSettings("FacebookVerifyToken")
            
            ' Log để debug
            System.Diagnostics.Debug.WriteLine("hub_mode: " & If(hubMode, "null"))
            System.Diagnostics.Debug.WriteLine("hub_verify_token: " & If(hubVerifyToken, "null"))
            System.Diagnostics.Debug.WriteLine("hub_challenge: " & If(hubChallenge, "null"))
            System.Diagnostics.Debug.WriteLine("verifyToken from config: " & verifyToken)
            System.Diagnostics.Debug.WriteLine("token match: " & (hubVerifyToken = verifyToken))
            
            If hubMode = "subscribe" AndAlso hubVerifyToken = verifyToken Then
                Dim response As New HttpResponseMessage(System.Net.HttpStatusCode.OK)
                response.Content = New StringContent(hubChallenge, System.Text.Encoding.UTF8, "text/plain")
                response.Headers.Add("X-Content-Type-Options", "nosniff")
                Return response
            Else
                Dim response As New HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                response.Content = New StringContent("Forbidden: hub_mode=" & If(hubMode, "null") & ", token match=" & (hubVerifyToken = verifyToken), System.Text.Encoding.UTF8, "text/plain")
                Return response
            End If
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Webhook Verify Error: " & ex.Message & " - " & ex.StackTrace)
            Dim response As New HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            response.Content = New StringContent("Error: " & ex.Message, System.Text.Encoding.UTF8, "text/plain")
            Return response
        End Try
    End Function

    ' POST: api/Webhook - Nhận dữ liệu từ Facebook
    <HttpPost>
    <Route("api/Webhook")>
    Public Async Function ReceiveWebhook() As Task(Of HttpResponseMessage)
        Try
            System.Diagnostics.Debug.WriteLine("========================================")
            System.Diagnostics.Debug.WriteLine("=== WebhookController.ReceiveWebhook CALLED ===")
            System.Diagnostics.Debug.WriteLine("========================================")
            System.Diagnostics.Debug.WriteLine("Method: POST")
            System.Diagnostics.Debug.WriteLine("Request URI: " & Request.RequestUri.ToString())
            System.Diagnostics.Debug.WriteLine("Headers: " & String.Join(", ", Request.Headers.Select(Function(h) h.Key & "=" & String.Join(";", h.Value))))
            
            Dim jsonContent As String = Await Request.Content.ReadAsStringAsync()
            System.Diagnostics.Debug.WriteLine("Received JSON Content Length: " & jsonContent.Length)
            System.Diagnostics.Debug.WriteLine("JSON Content: " & jsonContent)
            System.Diagnostics.Debug.WriteLine("========================================")
            
            Dim data As JObject = JsonConvert.DeserializeObject(Of JObject)(jsonContent)

            ' Facebook có 2 format:
            ' 1. Test format: {"field": "messages", "value": {...}}
            ' 2. Production format: {"object": "page", "entry": [...]}
            
            ' Xử lý TEST format (từ nút "Thử nghiệm")
            System.Diagnostics.Debug.WriteLine(">>> Checking for TEST format...")
            System.Diagnostics.Debug.WriteLine(">>> data('field'): " & If(data("field") IsNot Nothing, data("field").ToString(), "NULL"))
            System.Diagnostics.Debug.WriteLine(">>> data('value'): " & If(data("value") IsNot Nothing, "EXISTS", "NULL"))

            If data("field") IsNot Nothing AndAlso data("value") IsNot Nothing Then
                Dim fieldName As String = data("field").ToString()
                System.Diagnostics.Debug.WriteLine("=== TEST FORMAT DETECTED ===")
                System.Diagnostics.Debug.WriteLine("Field: " & fieldName)
                System.Diagnostics.Debug.WriteLine("Full test data: " & data.ToString())

                Dim value As JObject = data("value")

                System.Diagnostics.Debug.WriteLine(">>> Comparing fieldName: '" & fieldName & "' == 'messages'? " & (fieldName = "messages").ToString())
                System.Diagnostics.Debug.WriteLine(">>> Comparing fieldName: '" & fieldName & "' == 'feed'? " & (fieldName = "feed").ToString())

                If fieldName = "messages" Then
                    System.Diagnostics.Debug.WriteLine(">>> MATCH: Field is 'messages' - Calling ProcessTestMessage")
                    System.Diagnostics.Debug.WriteLine("Processing test messages event")
                    ProcessTestMessage(value)
                ElseIf fieldName = "feed" Then
                    System.Diagnostics.Debug.WriteLine(">>> MATCH: Field is 'feed' - Calling ProcessTestFeed")
                    System.Diagnostics.Debug.WriteLine("Processing test feed event")
                    ProcessTestFeed(value)
                Else
                    System.Diagnostics.Debug.WriteLine(">>> NO MATCH: Field is not 'messages' or 'feed': " & fieldName)
                    System.Diagnostics.Debug.WriteLine("Test format but field is not 'messages' or 'feed': " & fieldName)
                    System.Diagnostics.Debug.WriteLine("Value structure: " & value.ToString())
                End If
                ' Xử lý PRODUCTION format (từ Facebook thực tế)
            ElseIf data("object") IsNot Nothing Then
                System.Diagnostics.Debug.WriteLine(">>> Checking for PRODUCTION format...")
                System.Diagnostics.Debug.WriteLine(">>> data('object'): " & If(data("object") IsNot Nothing, data("object").ToString(), "NULL"))
                Dim objectType As String = data("object").ToString()
                System.Diagnostics.Debug.WriteLine("=== PRODUCTION FORMAT DETECTED ===")
                System.Diagnostics.Debug.WriteLine("Object Type: " & objectType)

                ' Xử lý entry array
                If data("entry") IsNot Nothing Then
                    System.Diagnostics.Debug.WriteLine("Found " & data("entry").Count() & " entry/entries")
                    For Each entry As JObject In data("entry")
                        ProcessEntry(entry, objectType)
                    Next
                Else
                    System.Diagnostics.Debug.WriteLine("No 'entry' field found in data")
                End If
            Else
                System.Diagnostics.Debug.WriteLine("=== UNKNOWN FORMAT ===")
                System.Diagnostics.Debug.WriteLine("No 'field'/'value' or 'object' field found in data")
                System.Diagnostics.Debug.WriteLine("Available keys: " & String.Join(", ", data.Properties().Select(Function(p) p.Name)))
                System.Diagnostics.Debug.WriteLine("Full data object: " & data.ToString())
            End If

            System.Diagnostics.Debug.WriteLine("=== WebhookController.ReceiveWebhook SUCCESS ===")
            Return New HttpResponseMessage(System.Net.HttpStatusCode.OK)
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("=== WebhookController.ReceiveWebhook ERROR ===")
            System.Diagnostics.Debug.WriteLine("Error: " & ex.Message)
            System.Diagnostics.Debug.WriteLine("StackTrace: " & ex.StackTrace)
            Return New HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
        End Try
    End Function

    ' Xử lý TEST format (từ nút "Thử nghiệm" trong Facebook)
    Private Sub ProcessTestMessage(value As JObject)
        Try
            System.Diagnostics.Debug.WriteLine("========================================")
            System.Diagnostics.Debug.WriteLine("=== ProcessTestMessage START ===")
            System.Diagnostics.Debug.WriteLine("========================================")
            System.Diagnostics.Debug.WriteLine("Value object: " & value.ToString())
            System.Diagnostics.Debug.WriteLine("Value object keys: " & String.Join(", ", value.Properties().Select(Function(p) p.Name)))
            
            Dim senderIdValue As String = ""
            Dim recipientIdValue As String = ""
            Dim midValue As String = ""
            Dim textValue As String = ""
            Dim timestampValue As Long = 0
            
            ' Lấy sender ID
            If value("sender") IsNot Nothing AndAlso value("sender")("id") IsNot Nothing Then
                senderIdValue = value("sender")("id").ToString()
                System.Diagnostics.Debug.WriteLine("Sender ID: " & senderIdValue)
            End If
            
            ' Lấy recipient ID
            If value("recipient") IsNot Nothing AndAlso value("recipient")("id") IsNot Nothing Then
                recipientIdValue = value("recipient")("id").ToString()
                System.Diagnostics.Debug.WriteLine("Recipient ID: " & recipientIdValue)
            End If
            
            ' Lấy timestamp
            If value("timestamp") IsNot Nothing Then
                Dim timestampStr As String = value("timestamp").ToString()
                If Long.TryParse(timestampStr, timestampValue) Then
                    System.Diagnostics.Debug.WriteLine("Timestamp: " & timestampValue)
                End If
            End If
            
            ' Lấy message object
            Dim message As JObject = value("message")
            If message IsNot Nothing Then
                System.Diagnostics.Debug.WriteLine("Message object: " & message.ToString())
                
                ' Lấy message ID
                If message("mid") IsNot Nothing Then
                    midValue = message("mid").ToString()
                    System.Diagnostics.Debug.WriteLine("Message ID (mid): " & midValue)
                End If
                
                ' Lấy text
                If message("text") IsNot Nothing Then
                    textValue = message("text").ToString()
                    System.Diagnostics.Debug.WriteLine("Message text: " & textValue)
                Else
                    System.Diagnostics.Debug.WriteLine("WARNING: Message has no 'text' field")
                    If message("attachments") IsNot Nothing Then
                        System.Diagnostics.Debug.WriteLine("Message has attachments")
                        textValue = "[Attachment/Media]"
                    ElseIf message("commands") IsNot Nothing Then
                        System.Diagnostics.Debug.WriteLine("Message has commands")
                        textValue = "[Commands]"
                    Else
                        textValue = "[Unknown message type]"
                    End If
                End If
                
                ' Chỉ lưu nếu có sender ID và message ID
                If Not String.IsNullOrEmpty(senderIdValue) AndAlso Not String.IsNullOrEmpty(midValue) Then
                    Dim messageEntry As New WebhookEntry() With {
                        .Id = Guid.NewGuid().ToString(),
                        .Type = "Message",
                        .PostId = senderIdValue,
                        .CommentId = midValue,
                        .Message = textValue,
                        .FromName = senderIdValue,
                        .FromId = senderIdValue,
                        .Timestamp = If(timestampValue > 0, DateTimeOffset.FromUnixTimeSeconds(timestampValue).DateTime, DateTime.Now),
                        .RawData = value.ToString()
                    }
                    System.Diagnostics.Debug.WriteLine("=== Saving TEST MESSAGE to Firebase ===")
                    ' Lưu vào Firebase
                    Task.Run(Async Function()
                                 Dim success As Boolean = Await FirebaseService.AddEntryAsync(messageEntry)
                                 System.Diagnostics.Debug.WriteLine("Firebase save result: " & success.ToString())
                             End Function)
                Else
                    System.Diagnostics.Debug.WriteLine("SKIP: Missing sender ID or message ID")
                    System.Diagnostics.Debug.WriteLine("  senderIdValue: " & If(String.IsNullOrEmpty(senderIdValue), "EMPTY", senderIdValue))
                    System.Diagnostics.Debug.WriteLine("  midValue: " & If(String.IsNullOrEmpty(midValue), "EMPTY", midValue))
                End If
            Else
                System.Diagnostics.Debug.WriteLine(">>> ERROR: No 'message' field in value object")
                System.Diagnostics.Debug.WriteLine(">>> Available fields in value: " & String.Join(", ", value.Properties().Select(Function(p) p.Name)))
            End If
            
            System.Diagnostics.Debug.WriteLine("========================================")
            System.Diagnostics.Debug.WriteLine("=== ProcessTestMessage END ===")
            System.Diagnostics.Debug.WriteLine("========================================")
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("========================================")
            System.Diagnostics.Debug.WriteLine(">>> ProcessTestMessage ERROR ===")
            System.Diagnostics.Debug.WriteLine(">>> Error: " & ex.Message)
            System.Diagnostics.Debug.WriteLine(">>> StackTrace: " & ex.StackTrace)
            System.Diagnostics.Debug.WriteLine("========================================")
        End Try
    End Sub

    ' Xử lý TEST format cho feed (comments, posts, etc.)
    Private Sub ProcessTestFeed(value As JObject)
        Try
            System.Diagnostics.Debug.WriteLine("========================================")
            System.Diagnostics.Debug.WriteLine("=== ProcessTestFeed START ===")
            System.Diagnostics.Debug.WriteLine("========================================")
            System.Diagnostics.Debug.WriteLine("Value object: " & value.ToString())
            System.Diagnostics.Debug.WriteLine("Value object keys: " & String.Join(", ", value.Properties().Select(Function(p) p.Name)))
            
            ' Test format của feed có thể có nhiều cấu trúc khác nhau
            ' Có thể là: item (comment, status/post, etc.), post_id, comment_id, message, from, verb, etc.
            
            Dim itemType As String = ""
            Dim verbType As String = ""
            Dim postIdValue As String = ""
            Dim commentIdValue As String = ""
            Dim messageValue As String = ""
            Dim fromNameValue As String = ""
            Dim fromIdValue As String = ""
            Dim publishedValue As Integer = 0
            Dim createdTimeValue As Long = 0
            
            ' Lấy item type (comment, status/post, etc.)
            If value("item") IsNot Nothing Then
                itemType = value("item").ToString()
                System.Diagnostics.Debug.WriteLine(">>> Item type: " & itemType)
            Else
                System.Diagnostics.Debug.WriteLine(">>> WARNING: No 'item' field found in value")
            End If
            
            ' Lấy verb (add, edit, delete, etc.)
            If value("verb") IsNot Nothing Then
                verbType = value("verb").ToString()
                System.Diagnostics.Debug.WriteLine(">>> Verb: " & verbType)
            End If
            
            ' Lấy post_id
            If value("post_id") IsNot Nothing Then
                postIdValue = value("post_id").ToString()
                System.Diagnostics.Debug.WriteLine(">>> Post ID: " & postIdValue)
            End If
            
            ' Lấy comment_id
            If value("comment_id") IsNot Nothing Then
                commentIdValue = value("comment_id").ToString()
                System.Diagnostics.Debug.WriteLine(">>> Comment ID: " & commentIdValue)
            End If
            
            ' Lấy message
            If value("message") IsNot Nothing Then
                messageValue = value("message").ToString()
                System.Diagnostics.Debug.WriteLine(">>> Message: " & messageValue)
            End If
            
            ' Lấy published
            If value("published") IsNot Nothing Then
                publishedValue = Convert.ToInt32(value("published"))
                System.Diagnostics.Debug.WriteLine(">>> Published: " & publishedValue.ToString())
            End If
            
            ' Lấy created_time
            If value("created_time") IsNot Nothing Then
                createdTimeValue = Convert.ToInt64(value("created_time"))
                System.Diagnostics.Debug.WriteLine(">>> Created time: " & createdTimeValue.ToString())
            End If
            
            ' Lấy from
            If value("from") IsNot Nothing Then
                Dim fromObj As JObject = value("from")
                If fromObj("name") IsNot Nothing Then
                    fromNameValue = fromObj("name").ToString()
                    System.Diagnostics.Debug.WriteLine(">>> From name: " & fromNameValue)
                End If
                If fromObj("id") IsNot Nothing Then
                    fromIdValue = fromObj("id").ToString()
                    System.Diagnostics.Debug.WriteLine(">>> From ID: " & fromIdValue)
                End If
            End If
            
            ' Xử lý COMMENT
            If itemType = "comment" Then
                System.Diagnostics.Debug.WriteLine("=== Processing TEST COMMENT ===")
                
                ' Lấy post_id
                If value("post_id") IsNot Nothing Then
                    postIdValue = value("post_id").ToString()
                    System.Diagnostics.Debug.WriteLine("Post ID: " & postIdValue)
                End If
                
                ' Lấy comment_id
                If value("comment_id") IsNot Nothing Then
                    commentIdValue = value("comment_id").ToString()
                    System.Diagnostics.Debug.WriteLine("Comment ID: " & commentIdValue)
                End If
                
                ' Lấy message
                If value("message") IsNot Nothing Then
                    messageValue = value("message").ToString()
                    System.Diagnostics.Debug.WriteLine("Message: " & messageValue)
                End If
                
                System.Diagnostics.Debug.WriteLine(">>> Processing TEST COMMENT")
                
                ' Lưu vào Firebase
                If Not String.IsNullOrEmpty(commentIdValue) Then
                    Dim commentEntry As New WebhookEntry() With {
                        .Id = Guid.NewGuid().ToString(),
                        .Type = "Comment",
                        .PostId = postIdValue,
                        .CommentId = commentIdValue,
                        .Message = messageValue,
                        .FromName = fromNameValue,
                        .FromId = fromIdValue,
                        .Timestamp = If(createdTimeValue > 0, DateTimeOffset.FromUnixTimeSeconds(createdTimeValue).DateTime, DateTime.Now),
                        .RawData = value.ToString()
                    }
                    System.Diagnostics.Debug.WriteLine(">>> Saving TEST COMMENT to Firebase")
                    Task.Run(Async Function()
                                 Dim success As Boolean = Await FirebaseService.AddEntryAsync(commentEntry)
                                 System.Diagnostics.Debug.WriteLine(">>> Firebase save result for COMMENT: " & success.ToString())
                             End Function)
                Else
                    System.Diagnostics.Debug.WriteLine(">>> SKIP: Missing comment ID for comment event")
                End If
            ' Xử lý POST/STATUS
            ElseIf itemType = "status" OrElse itemType = "post" OrElse (String.IsNullOrEmpty(itemType) AndAlso Not String.IsNullOrEmpty(postIdValue)) Then
                System.Diagnostics.Debug.WriteLine(">>> Processing TEST POST/STATUS")
                System.Diagnostics.Debug.WriteLine(">>> Item type: " & If(String.IsNullOrEmpty(itemType), "N/A (detected by post_id)", itemType))
                System.Diagnostics.Debug.WriteLine(">>> Verb: " & If(String.IsNullOrEmpty(verbType), "N/A", verbType))
                
                ' Lưu post vào Firebase (nếu có post_id)
                If Not String.IsNullOrEmpty(postIdValue) Then
                    Dim postEntry As New WebhookEntry() With {
                        .Id = Guid.NewGuid().ToString(),
                        .Type = "Post",
                        .PostId = postIdValue,
                        .CommentId = "", ' Post không có comment_id
                        .Message = messageValue,
                        .FromName = fromNameValue,
                        .FromId = fromIdValue,
                        .Timestamp = If(createdTimeValue > 0, DateTimeOffset.FromUnixTimeSeconds(createdTimeValue).DateTime, DateTime.Now),
                        .RawData = value.ToString()
                    }
                    System.Diagnostics.Debug.WriteLine(">>> Saving TEST POST to Firebase")
                    Task.Run(Async Function()
                                 Dim success As Boolean = Await FirebaseService.AddEntryAsync(postEntry)
                                 System.Diagnostics.Debug.WriteLine(">>> Firebase save result for POST: " & success.ToString())
                             End Function)
                Else
                    System.Diagnostics.Debug.WriteLine(">>> SKIP: Missing post ID for post/status event")
                End If
            Else
                System.Diagnostics.Debug.WriteLine(">>> SKIP: Item type is not 'comment' or 'status/post': " & If(String.IsNullOrEmpty(itemType), "N/A", itemType))
                System.Diagnostics.Debug.WriteLine(">>> Full value object: " & value.ToString())
                System.Diagnostics.Debug.WriteLine(">>> Available fields: " & String.Join(", ", value.Properties().Select(Function(p) p.Name & "=" & p.Value.ToString())))
            End If
            
            System.Diagnostics.Debug.WriteLine("========================================")
            System.Diagnostics.Debug.WriteLine("=== ProcessTestFeed END ===")
            System.Diagnostics.Debug.WriteLine("========================================")
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("ProcessTestFeed ERROR: " & ex.Message)
            System.Diagnostics.Debug.WriteLine("StackTrace: " & ex.StackTrace)
        End Try
    End Sub

    Private Sub ProcessEntry(entry As JObject, objectType As String)
        System.Diagnostics.Debug.WriteLine("=== ProcessEntry ===")
        System.Diagnostics.Debug.WriteLine("Object Type: " & objectType)
        System.Diagnostics.Debug.WriteLine("Entry: " & entry.ToString())
        
        ' Xử lý comments trên bài viết
        If objectType = "page" OrElse objectType = "instagram" Then
            If entry("changes") IsNot Nothing Then
                For Each change As JObject In entry("changes")
                    Dim value As JObject = change("value")
                    
                    ' Xử lý comment
                    If value("item") IsNot Nothing AndAlso value("item").ToString() = "comment" Then
                        System.Diagnostics.Debug.WriteLine("=== Processing COMMENT ===")
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
            System.Diagnostics.Debug.WriteLine("Found messaging array with " & entry("messaging").Count() & " items")
            For Each messaging As JObject In entry("messaging")
                System.Diagnostics.Debug.WriteLine("=== Processing messaging item ===")
                System.Diagnostics.Debug.WriteLine("Full messaging object: " & messaging.ToString())
                
                ' Log tất cả các fields có trong messaging object
                System.Diagnostics.Debug.WriteLine("Messaging object keys: " & String.Join(", ", messaging.Properties().Select(Function(p) p.Name)))
                
                ' Bỏ qua read receipt và delivery
                If messaging("read") IsNot Nothing Then
                    System.Diagnostics.Debug.WriteLine("SKIP: This is a read receipt event")
                    Continue For
                End If
                If messaging("delivery") IsNot Nothing Then
                    System.Diagnostics.Debug.WriteLine("SKIP: This is a delivery event")
                    Continue For
                End If
                
                ' Xử lý message
                Dim message As JObject = messaging("message")
                If message IsNot Nothing Then
                    System.Diagnostics.Debug.WriteLine("=== Found MESSAGE field ===")
                    System.Diagnostics.Debug.WriteLine("Message object: " & message.ToString())
                    System.Diagnostics.Debug.WriteLine("Message object keys: " & String.Join(", ", message.Properties().Select(Function(p) p.Name)))
                    
                    ' Kiểm tra xem có phải echo (message từ bot) không
                    Dim isEcho As Boolean = False
                    If message("is_echo") IsNot Nothing Then
                        isEcho = Convert.ToBoolean(message("is_echo"))
                        System.Diagnostics.Debug.WriteLine("is_echo: " & isEcho.ToString())
                    End If
                    
                    ' Bỏ qua echo messages (messages từ bot gửi đi)
                    If isEcho Then
                        System.Diagnostics.Debug.WriteLine("SKIP: This is an echo message (from bot)")
                        Continue For
                    End If
                    
                    System.Diagnostics.Debug.WriteLine("=== Processing MESSAGE (not echo) ===")
                    Dim senderIdValue As String = ""
                    Dim recipientIdValue As String = ""
                    Dim midValue As String = ""
                    Dim textValue As String = ""
                    Dim timestampValue As Long = 0
                    
                    ' Lấy sender ID
                    If messaging("sender") IsNot Nothing AndAlso messaging("sender")("id") IsNot Nothing Then
                        senderIdValue = messaging("sender")("id").ToString()
                        System.Diagnostics.Debug.WriteLine("Sender ID: " & senderIdValue)
                    End If
                    
                    ' Lấy recipient ID
                    If messaging("recipient") IsNot Nothing AndAlso messaging("recipient")("id") IsNot Nothing Then
                        recipientIdValue = messaging("recipient")("id").ToString()
                        System.Diagnostics.Debug.WriteLine("Recipient ID: " & recipientIdValue)
                    End If
                    
                    ' Lấy timestamp
                    If messaging("timestamp") IsNot Nothing Then
                        timestampValue = Convert.ToInt64(messaging("timestamp"))
                        System.Diagnostics.Debug.WriteLine("Timestamp: " & timestampValue)
                    End If
                    
                    ' Lấy message ID
                    If message("mid") IsNot Nothing Then
                        midValue = message("mid").ToString()
                        System.Diagnostics.Debug.WriteLine("Message ID (mid): " & midValue)
                    End If
                    
                    ' Lấy text
                    If message("text") IsNot Nothing Then
                        textValue = message("text").ToString()
                        System.Diagnostics.Debug.WriteLine("Message text: " & textValue)
                    Else
                        System.Diagnostics.Debug.WriteLine("WARNING: Message has no 'text' field")
                        ' Có thể là attachment, sticker, etc.
                        If message("attachments") IsNot Nothing Then
                            System.Diagnostics.Debug.WriteLine("Message has attachments")
                            textValue = "[Attachment/Media]"
                        ElseIf message("sticker_id") IsNot Nothing Then
                            System.Diagnostics.Debug.WriteLine("Message is a sticker")
                            textValue = "[Sticker]"
                        Else
                            textValue = "[Unknown message type]"
                        End If
                    End If
                    
                    ' Chỉ lưu nếu có sender ID và message ID
                    If Not String.IsNullOrEmpty(senderIdValue) AndAlso Not String.IsNullOrEmpty(midValue) Then
                        Dim messageEntry As New WebhookEntry() With {
                            .Id = Guid.NewGuid().ToString(),
                            .Type = "Message",
                            .PostId = senderIdValue,
                            .CommentId = midValue,
                            .Message = textValue,
                            .FromName = senderIdValue,
                            .FromId = senderIdValue,
                            .Timestamp = If(timestampValue > 0, DateTimeOffset.FromUnixTimeMilliseconds(timestampValue).DateTime, DateTime.Now),
                            .RawData = messaging.ToString()
                        }
                        System.Diagnostics.Debug.WriteLine("=== Saving MESSAGE to Firebase ===")
                        ' Lưu vào Firebase
                        Task.Run(Async Function()
                                     Dim success As Boolean = Await FirebaseService.AddEntryAsync(messageEntry)
                                     System.Diagnostics.Debug.WriteLine("Firebase save result: " & success.ToString())
                                 End Function)
                    Else
                        System.Diagnostics.Debug.WriteLine("SKIP: Missing sender ID or message ID")
                        System.Diagnostics.Debug.WriteLine("  senderIdValue: " & If(String.IsNullOrEmpty(senderIdValue), "EMPTY", senderIdValue))
                        System.Diagnostics.Debug.WriteLine("  midValue: " & If(String.IsNullOrEmpty(midValue), "EMPTY", midValue))
                    End If
                Else
                    System.Diagnostics.Debug.WriteLine("No 'message' field in messaging object")
                    System.Diagnostics.Debug.WriteLine("Available fields: " & String.Join(", ", messaging.Properties().Select(Function(p) p.Name)))
                End If
                
                ' Xử lý optin (notification_messages)
                Dim optin As JObject = messaging("optin")
                If optin IsNot Nothing Then
                    System.Diagnostics.Debug.WriteLine("=== Processing OPTIN ===")
                    Dim senderIdValue As String = ""
                    Dim optinType As String = ""
                    Dim payload As String = ""
                    
                    If messaging("sender") IsNot Nothing AndAlso messaging("sender")("id") IsNot Nothing Then
                        senderIdValue = messaging("sender")("id").ToString()
                    End If
                    If optin("type") IsNot Nothing Then optinType = optin("type").ToString()
                    If optin("payload") IsNot Nothing Then payload = optin("payload").ToString()
                    
                    Dim optinEntry As New WebhookEntry() With {
                        .Id = Guid.NewGuid().ToString(),
                        .Type = "Optin",
                        .PostId = senderIdValue,
                        .CommentId = optinType,
                        .Message = "Optin: " & optinType & " - " & payload,
                        .FromName = senderIdValue,
                        .FromId = senderIdValue,
                        .Timestamp = DateTime.Now,
                        .RawData = messaging.ToString()
                    }
                    ' Lưu vào Firebase
                    Task.Run(Async Function()
                                 Await FirebaseService.AddEntryAsync(optinEntry)
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
            Dim totalEntries As Integer = 0
            Dim errorMessage As String = Nothing
            
            ' Nếu không kết nối được, thử lấy thông tin lỗi chi tiết
            If Not isConnected Then
                If info.IsConfigured Then
                    errorMessage = Await FirebaseService.GetConnectionErrorAsync()
                    If String.IsNullOrEmpty(errorMessage) Then
                        errorMessage = "Không thể kết nối đến Firebase. Kiểm tra URL và Secret trong Web.config"
                    End If
                Else
                    errorMessage = "Firebase chưa được cấu hình trong Web.config"
                End If
            Else
                ' Chỉ lấy totalEntries nếu đã kết nối được
                Try
                    totalEntries = Await FirebaseService.GetEntriesCountAsync()
                Catch ex As Exception
                    ' Nếu lỗi khi lấy count, vẫn hiển thị là connected nhưng không có count
                    errorMessage = "Kết nối thành công nhưng không thể đọc số lượng entries: " & ex.Message
                End Try
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
                .ErrorMessage = "Exception: " & ex.Message
            })
        End Try
    End Function
End Class

