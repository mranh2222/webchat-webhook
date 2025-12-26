Imports System.Net.Http
Imports System.Threading.Tasks
Imports System.Configuration
Imports Newtonsoft.Json
Imports System.Text
Imports System.Collections.Generic
Imports Newtonsoft.Json.Linq

Public Class FacebookGraphService
    Private Shared ReadOnly httpClient As HttpClient
    Private Shared ReadOnly graphApiBaseUrl As String = "https://graph.facebook.com/v18.0"

    Shared Sub New()
        httpClient = New HttpClient()
        httpClient.Timeout = TimeSpan.FromSeconds(30)
    End Sub

    ' Lấy Access Token từ Web.config hoặc từ parameter
    Private Shared Function GetAccessToken() As String
        Return ConfigurationManager.AppSettings("FacebookAccessToken")
    End Function

    ' Subscribe webhook cho Page
    Public Shared Async Function SubscribePageWebhookAsync(pageId As String, accessToken As String) As Task(Of SubscriptionResult)
        Try
            Dim url As String = $"{graphApiBaseUrl}/{pageId}/subscribed_apps"
            Dim content As New FormUrlEncodedContent(New Dictionary(Of String, String) From {
                {"subscribed_fields", "feed,comments,messages"},
                {"access_token", accessToken}
            })

            Dim response As HttpResponseMessage = Await httpClient.PostAsync(url, content)
            Dim responseContent As String = Await response.Content.ReadAsStringAsync()

            If response.IsSuccessStatusCode Then
                Dim result As JObject = JsonConvert.DeserializeObject(Of JObject)(responseContent)
                Return New SubscriptionResult With {
                    .Success = True,
                    .Message = "Đã subscribe thành công",
                    .Data = result
                }
            Else
                Return New SubscriptionResult With {
                    .Success = False,
                    .Message = $"Lỗi: {responseContent}",
                    .Data = Nothing
                }
            End If
        Catch ex As Exception
            Return New SubscriptionResult With {
                .Success = False,
                .Message = $"Exception: {ex.Message}",
                .Data = Nothing
            }
        End Try
    End Function

    ' Unsubscribe webhook cho Page
    Public Shared Async Function UnsubscribePageWebhookAsync(pageId As String, accessToken As String) As Task(Of SubscriptionResult)
        Try
            Dim url As String = $"{graphApiBaseUrl}/{pageId}/subscribed_apps"
            Dim request As New HttpRequestMessage(HttpMethod.Delete, url)
            request.Content = New FormUrlEncodedContent(New Dictionary(Of String, String) From {
                {"access_token", accessToken}
            })

            Dim response As HttpResponseMessage = Await httpClient.SendAsync(request)
            Dim responseContent As String = Await response.Content.ReadAsStringAsync()

            If response.IsSuccessStatusCode Then
                Return New SubscriptionResult With {
                    .Success = True,
                    .Message = "Đã unsubscribe thành công",
                    .Data = Nothing
                }
            Else
                Return New SubscriptionResult With {
                    .Success = False,
                    .Message = $"Lỗi: {responseContent}",
                    .Data = Nothing
                }
            End If
        Catch ex As Exception
            Return New SubscriptionResult With {
                .Success = False,
                .Message = $"Exception: {ex.Message}",
                .Data = Nothing
            }
        End Try
    End Function

    ' Kiểm tra subscription status
    Public Shared Async Function GetSubscriptionStatusAsync(pageId As String, accessToken As String) As Task(Of SubscriptionStatus)
        Try
            Dim url As String = $"{graphApiBaseUrl}/{pageId}/subscribed_apps?access_token={accessToken}"
            Dim response As HttpResponseMessage = Await httpClient.GetAsync(url)
            Dim responseContent As String = Await response.Content.ReadAsStringAsync()

            If response.IsSuccessStatusCode Then
                Dim data As JObject = JsonConvert.DeserializeObject(Of JObject)(responseContent)
                Dim subscribedApps As JArray = data("data")

                If subscribedApps IsNot Nothing AndAlso subscribedApps.Count > 0 Then
                    Dim appId As String = ConfigurationManager.AppSettings("FacebookAppId")
                    Dim isSubscribed As Boolean = False
                    Dim subscribedFields As New List(Of String)

                    For Each app As JObject In subscribedApps
                        If app("id").ToString() = appId Then
                            isSubscribed = True
                            If app("subscribed_fields") IsNot Nothing Then
                                For Each field As JToken In app("subscribed_fields")
                                    subscribedFields.Add(field.ToString())
                                Next
                            End If
                            Exit For
                        End If
                    Next

                    Return New SubscriptionStatus With {
                        .IsSubscribed = isSubscribed,
                        .SubscribedFields = subscribedFields,
                        .RawData = responseContent
                    }
                Else
                    Return New SubscriptionStatus With {
                        .IsSubscribed = False,
                        .SubscribedFields = New List(Of String),
                        .RawData = responseContent
                    }
                End If
            Else
                Return New SubscriptionStatus With {
                    .IsSubscribed = False,
                    .SubscribedFields = New List(Of String),
                    .RawData = responseContent,
                    .ErrorMessage = responseContent
                }
            End If
        Catch ex As Exception
            Return New SubscriptionStatus With {
                .IsSubscribed = False,
                .SubscribedFields = New List(Of String),
                .RawData = "",
                .ErrorMessage = ex.Message
            }
        End Try
    End Function

    ' Lấy thông tin Page
    Public Shared Async Function GetPageInfoAsync(pageId As String, accessToken As String) As Task(Of PageInfo)
        Try
            Dim fields As String = "id,name,access_token"
            Dim url As String = $"{graphApiBaseUrl}/{pageId}?fields={fields}&access_token={accessToken}"
            Dim response As HttpResponseMessage = Await httpClient.GetAsync(url)
            Dim responseContent As String = Await response.Content.ReadAsStringAsync()

            If response.IsSuccessStatusCode Then
                Dim data As JObject = JsonConvert.DeserializeObject(Of JObject)(responseContent)
                Return New PageInfo With {
                    .Id = If(data("id")?.ToString(), ""),
                    .Name = If(data("name")?.ToString(), ""),
                    .AccessToken = If(data("access_token")?.ToString(), ""),
                    .RawData = responseContent
                }
            Else
                Return New PageInfo With {
                    .ErrorMessage = responseContent
                }
            End If
        Catch ex As Exception
            Return New PageInfo With {
                .ErrorMessage = ex.Message
            }
        End Try
    End Function

    ' Lấy comments từ một post (để test)
    Public Shared Async Function GetPostCommentsAsync(postId As String, accessToken As String) As Task(Of List(Of CommentInfo))
        Try
            Dim url As String = $"{graphApiBaseUrl}/{postId}/comments?access_token={accessToken}"
            Dim response As HttpResponseMessage = Await httpClient.GetAsync(url)
            Dim responseContent As String = Await response.Content.ReadAsStringAsync()

            If response.IsSuccessStatusCode Then
                Dim data As JObject = JsonConvert.DeserializeObject(Of JObject)(responseContent)
                Dim comments As JArray = data("data")
                Dim result As New List(Of CommentInfo)

                If comments IsNot Nothing Then
                    For Each comment As JObject In comments
                        result.Add(New CommentInfo With {
                            .Id = If(comment("id")?.ToString(), ""),
                            .Message = If(comment("message")?.ToString(), ""),
                            .FromId = If(comment("from")("id")?.ToString(), ""),
                            .FromName = If(comment("from")("name")?.ToString(), ""),
                            .CreatedTime = If(comment("created_time")?.ToString(), "")
                        })
                    Next
                End If

                Return result
            Else
                Return New List(Of CommentInfo)
            End If
        Catch ex As Exception
            Return New List(Of CommentInfo)
        End Try
    End Function
End Class

' Các class hỗ trợ
Public Class SubscriptionResult
    Public Property Success As Boolean
    Public Property Message As String
    Public Property Data As JObject
End Class

Public Class SubscriptionStatus
    Public Property IsSubscribed As Boolean
    Public Property SubscribedFields As List(Of String)
    Public Property RawData As String
    Public Property ErrorMessage As String
End Class

Public Class PageInfo
    Public Property Id As String
    Public Property Name As String
    Public Property AccessToken As String
    Public Property RawData As String
    Public Property ErrorMessage As String
End Class

Public Class CommentInfo
    Public Property Id As String
    Public Property Message As String
    Public Property FromId As String
    Public Property FromName As String
    Public Property CreatedTime As String
End Class


