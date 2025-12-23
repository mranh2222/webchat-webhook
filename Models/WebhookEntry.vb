Public Class WebhookEntry
    Public Property Id As String
    Public Property Type As String ' "Comment" hoặc "Message"
    Public Property PostId As String
    Public Property CommentId As String
    Public Property Message As String
    Public Property FromName As String
    Public Property FromId As String
    Public Property Timestamp As DateTime
    Public Property RawData As String
End Class



