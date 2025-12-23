Imports System.IO
Imports System.Configuration
Imports System.Linq
Imports Newtonsoft.Json
Imports System.Collections.Generic

Public Class FileStorageService
    Private Shared ReadOnly lockObject As New Object()
    Private Shared ReadOnly dataFilePath As String

    Shared Sub New()
        ' Lấy đường dẫn file từ Web.config hoặc dùng đường dẫn mặc định
        Dim configPath As String = ConfigurationManager.AppSettings("WebhookDataFilePath")
        If String.IsNullOrEmpty(configPath) Then
            ' Mặc định lưu trong App_Data
            dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "webhook_data.json")
        Else
            dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configPath)
        End If

        ' Đảm bảo thư mục tồn tại
        Dim directoryPath As String = Path.GetDirectoryName(dataFilePath)
        If Not System.IO.Directory.Exists(directoryPath) Then
            System.IO.Directory.CreateDirectory(directoryPath)
        End If

        ' Tạo file nếu chưa tồn tại
        If Not File.Exists(dataFilePath) Then
            SaveData(New List(Of WebhookEntry)())
        End If
    End Sub

    ' Lưu dữ liệu vào file
    Public Shared Sub SaveData(data As List(Of WebhookEntry))
        SyncLock lockObject
            Try
                Dim json As String = JsonConvert.SerializeObject(data, Formatting.Indented)
                File.WriteAllText(dataFilePath, json, System.Text.Encoding.UTF8)
            Catch ex As Exception
                ' Log lỗi nếu cần
                Throw New Exception("Lỗi khi lưu dữ liệu vào file: " & ex.Message, ex)
            End Try
        End SyncLock
    End Sub

    ' Đọc dữ liệu từ file
    Public Shared Function LoadData() As List(Of WebhookEntry)
        SyncLock lockObject
            Try
                If Not File.Exists(dataFilePath) Then
                    Return New List(Of WebhookEntry)()
                End If

                Dim json As String = File.ReadAllText(dataFilePath, System.Text.Encoding.UTF8)
                If String.IsNullOrWhiteSpace(json) Then
                    Return New List(Of WebhookEntry)()
                End If

                Dim data As List(Of WebhookEntry) = JsonConvert.DeserializeObject(Of List(Of WebhookEntry))(json)
                If data Is Nothing Then
                    Return New List(Of WebhookEntry)()
                End If

                Return data
            Catch ex As Exception
                ' Nếu file bị lỗi, trả về list rỗng
                Return New List(Of WebhookEntry)()
            End Try
        End SyncLock
    End Function

    ' Thêm entry mới
    Public Shared Sub AddEntry(entry As WebhookEntry)
        SyncLock lockObject
            Dim data As List(Of WebhookEntry) = LoadData()
            data.Insert(0, entry)

            ' Giới hạn số lượng entries (giữ 1000 entries gần nhất)
            If data.Count > 1000 Then
                data = data.Take(1000).ToList()
            End If

            SaveData(data)
        End SyncLock
    End Sub

    ' Xóa tất cả dữ liệu
    Public Shared Sub ClearData()
        SyncLock lockObject
            SaveData(New List(Of WebhookEntry)())
        End SyncLock
    End Sub

    ' Lấy dữ liệu với giới hạn
    Public Shared Function GetData(limit As Integer) As List(Of WebhookEntry)
        SyncLock lockObject
            Dim data As List(Of WebhookEntry) = LoadData()
            Return data.OrderByDescending(Function(x) x.Timestamp).Take(limit).ToList()
        End SyncLock
    End Function

    ' Lấy đường dẫn file hiện tại
    Public Shared Function GetDataFilePath() As String
        Return dataFilePath
    End Function
End Class

