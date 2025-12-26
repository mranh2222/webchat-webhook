Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Configuration
Imports Newtonsoft.Json
Imports System.Text
Imports System.Collections.Generic
Imports System.Linq

Public Class FirebaseService
    Private Shared ReadOnly lockObject As New Object()
    Private Shared httpClient As HttpClient
    Private Shared ReadOnly baseUrl As String
    Private Shared ReadOnly authSecret As String

    Shared Sub New()
        ' Lấy cấu hình từ Web.config
        baseUrl = ConfigurationManager.AppSettings("FirebaseBaseUrl")
        authSecret = ConfigurationManager.AppSettings("FirebaseAuthSecret")

        ' Khởi tạo HttpClient
        httpClient = New HttpClient()
        httpClient.Timeout = TimeSpan.FromSeconds(30)
    End Sub

    ' Lấy URL đầy đủ với auth
    Private Shared Function GetFullUrl(path As String) As String
        Dim url As String = baseUrl.TrimEnd("/"c) & "/" & path.TrimStart("/"c) & ".json"
        If Not String.IsNullOrEmpty(authSecret) Then
            url &= "?auth=" & authSecret
        End If
        Return url
    End Function

    ' Thêm entry mới vào Firebase
    Public Shared Async Function AddEntryAsync(entry As WebhookEntry) As Task(Of Boolean)
        Try
            ' Lưu với ID là key
            Dim path As String = "webhooks/" & entry.Id
            Dim json As String = JsonConvert.SerializeObject(entry)
            Dim content As New StringContent(json, Encoding.UTF8, "application/json")

            Dim response As HttpResponseMessage = Await httpClient.PutAsync(GetFullUrl(path), content)
            Return response.IsSuccessStatusCode
        Catch ex As Exception
            ' Log lỗi nếu cần
            Return False
        End Try
    End Function

    ' Lấy tất cả entries
    Public Shared Async Function GetAllEntriesAsync() As Task(Of List(Of WebhookEntry))
        Try
            Dim response As HttpResponseMessage = Await httpClient.GetAsync(GetFullUrl("webhooks"))
            
            If response.IsSuccessStatusCode Then
                Dim json As String = Await response.Content.ReadAsStringAsync()
                
                If String.IsNullOrWhiteSpace(json) OrElse json = "null" Then
                    Return New List(Of WebhookEntry)()
                End If

                ' Firebase trả về dạng object với keys là IDs
                Dim data As Dictionary(Of String, WebhookEntry) = JsonConvert.DeserializeObject(Of Dictionary(Of String, WebhookEntry))(json)
                
                If data Is Nothing Then
                    Return New List(Of WebhookEntry)()
                End If

                Return data.Values.ToList()
            End If

            Return New List(Of WebhookEntry)()
        Catch ex As Exception
            Return New List(Of WebhookEntry)()
        End Try
    End Function

    ' Lấy entries với giới hạn
    Public Shared Async Function GetEntriesAsync(limit As Integer) As Task(Of List(Of WebhookEntry))
        Try
            Dim allEntries As List(Of WebhookEntry) = Await GetAllEntriesAsync()
            Return allEntries.OrderByDescending(Function(x) x.Timestamp).Take(limit).ToList()
        Catch ex As Exception
            Return New List(Of WebhookEntry)()
        End Try
    End Function

    ' Xóa tất cả entries
    Public Shared Async Function ClearAllEntriesAsync() As Task(Of Boolean)
        Try
            Dim response As HttpResponseMessage = Await httpClient.DeleteAsync(GetFullUrl("webhooks"))
            Return response.IsSuccessStatusCode
        Catch ex As Exception
            Return False
        End Try
    End Function

    ' Lấy thông tin về số lượng entries
    Public Shared Async Function GetEntriesCountAsync() As Task(Of Integer)
        Try
            Dim entries As List(Of WebhookEntry) = Await GetAllEntriesAsync()
            Return entries.Count
        Catch ex As Exception
            Return 0
        End Try
    End Function

    ' Kiểm tra kết nối Firebase
    Public Shared Async Function TestConnectionAsync() As Task(Of Boolean)
        Try
            If String.IsNullOrEmpty(baseUrl) Then
                Return False
            End If
            
            ' Test bằng cách đọc root hoặc webhooks path
            Dim testUrl As String = baseUrl.TrimEnd("/"c) & "/.json"
            If Not String.IsNullOrEmpty(authSecret) Then
                testUrl &= "?auth=" & authSecret
            End If
            
            Dim response As HttpResponseMessage = Await httpClient.GetAsync(testUrl)
            ' Firebase trả về 200 ngay cả khi không có dữ liệu
            Return response.IsSuccessStatusCode
        Catch ex As Exception
            ' Log lỗi chi tiết
            System.Diagnostics.Debug.WriteLine("Firebase TestConnection Error: " & ex.Message)
            Return False
        End Try
    End Function
    
    ' Lấy thông tin lỗi chi tiết
    Public Shared Async Function GetConnectionErrorAsync() As Task(Of String)
        Try
            If String.IsNullOrEmpty(baseUrl) Then
                Return "FirebaseBaseUrl chưa được cấu hình trong Web.config"
            End If
            
            Dim testUrl As String = baseUrl.TrimEnd("/"c) & "/.json"
            If Not String.IsNullOrEmpty(authSecret) Then
                testUrl &= "?auth=" & authSecret
            End If
            
            Dim response As HttpResponseMessage = Await httpClient.GetAsync(testUrl)
            
            If Not response.IsSuccessStatusCode Then
                Dim errorContent As String = Await response.Content.ReadAsStringAsync()
                Return $"HTTP {CInt(response.StatusCode)}: {errorContent}"
            End If
            
            ' Nếu response thành công nhưng vẫn gọi hàm này, có thể do lỗi khác
            Return "Kết nối thành công nhưng có lỗi khi đọc dữ liệu"
        Catch ex As Exception
            Return $"Exception: {ex.Message} - {ex.GetType().Name}"
        End Try
    End Function

    ' Lấy thông tin Firebase
    Public Shared Function GetFirebaseInfo() As Object
        Return New With {
            .BaseUrl = baseUrl,
            .HasAuthSecret = Not String.IsNullOrEmpty(authSecret),
            .IsConfigured = Not String.IsNullOrEmpty(baseUrl)
        }
    End Function
End Class

