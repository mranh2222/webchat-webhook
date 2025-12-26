Imports System.Web.Http
Imports System.Threading.Tasks
Imports System.Configuration
Imports System.Net.Http

Public Class FacebookApiController
    Inherits ApiController

    ' POST: api/FacebookApi/SubscribePage
    <HttpPost>
    <Route("api/FacebookApi/SubscribePage")>
    Public Async Function SubscribePage(<FromBody> request As SubscribePageRequest) As Task(Of IHttpActionResult)
        Try
            If String.IsNullOrEmpty(request.PageId) OrElse String.IsNullOrEmpty(request.AccessToken) Then
                Return BadRequest("PageId và AccessToken là bắt buộc")
            End If

            Dim result = Await FacebookGraphService.SubscribePageWebhookAsync(request.PageId, request.AccessToken)

            If result.Success Then
                Return Ok(New With {
                    .success = True,
                    .message = result.Message,
                    .data = result.Data
                })
            Else
                Return BadRequest(New With {
                    .success = False,
                    .message = result.Message
                })
            End If
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

    ' DELETE: api/FacebookApi/UnsubscribePage
    <HttpDelete>
    <Route("api/FacebookApi/UnsubscribePage")>
    Public Async Function UnsubscribePage(pageId As String, accessToken As String) As Task(Of IHttpActionResult)
        Try
            If String.IsNullOrEmpty(pageId) OrElse String.IsNullOrEmpty(accessToken) Then
                Return BadRequest("PageId và AccessToken là bắt buộc")
            End If

            Dim result = Await FacebookGraphService.UnsubscribePageWebhookAsync(pageId, accessToken)

            If result.Success Then
                Return Ok(New With {
                    .success = True,
                    .message = result.Message
                })
            Else
                Return BadRequest(New With {
                    .success = False,
                    .message = result.Message
                })
            End If
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

    ' GET: api/FacebookApi/SubscriptionStatus
    <HttpGet>
    <Route("api/FacebookApi/SubscriptionStatus")>
    Public Async Function GetSubscriptionStatus(pageId As String, accessToken As String) As Task(Of IHttpActionResult)
        Try
            If String.IsNullOrEmpty(pageId) OrElse String.IsNullOrEmpty(accessToken) Then
                Return BadRequest("PageId và AccessToken là bắt buộc")
            End If

            Dim status = Await FacebookGraphService.GetSubscriptionStatusAsync(pageId, accessToken)

            Return Ok(New With {
                .isSubscribed = status.IsSubscribed,
                .subscribedFields = status.SubscribedFields,
                .errorMessage = status.ErrorMessage
            })
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

    ' GET: api/FacebookApi/PageInfo
    <HttpGet>
    <Route("api/FacebookApi/PageInfo")>
    Public Async Function GetPageInfo(pageId As String, accessToken As String) As Task(Of IHttpActionResult)
        Try
            If String.IsNullOrEmpty(pageId) OrElse String.IsNullOrEmpty(accessToken) Then
                Return BadRequest("PageId và AccessToken là bắt buộc")
            End If

            Dim pageInfo = Await FacebookGraphService.GetPageInfoAsync(pageId, accessToken)

            If String.IsNullOrEmpty(pageInfo.ErrorMessage) Then
                Return Ok(New With {
                    .id = pageInfo.Id,
                    .name = pageInfo.Name,
                    .hasAccessToken = Not String.IsNullOrEmpty(pageInfo.AccessToken)
                })
            Else
                Return BadRequest(New With {
                    .errorMessage = pageInfo.ErrorMessage
                })
            End If
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

    ' GET: api/FacebookApi/PostComments
    <HttpGet>
    <Route("api/FacebookApi/PostComments")>
    Public Async Function GetPostComments(postId As String, accessToken As String) As Task(Of IHttpActionResult)
        Try
            If String.IsNullOrEmpty(postId) OrElse String.IsNullOrEmpty(accessToken) Then
                Return BadRequest("PostId và AccessToken là bắt buộc")
            End If

            Dim comments = Await FacebookGraphService.GetPostCommentsAsync(postId, accessToken)

            Return Ok(comments)
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function
End Class

' Request model
Public Class SubscribePageRequest
    Public Property PageId As String
    Public Property AccessToken As String
    Public Property Fields As String
End Class


