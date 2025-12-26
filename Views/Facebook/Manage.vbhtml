@Code
    ViewData("Title") = "Quản lý Facebook Webhook Subscriptions"
    Layout = "~/Views/Shared/_Layout.vbhtml"
End Code

<div class="container mt-4">
    <div class="row">
        <div class="col-12">
            <h2>Quản lý Facebook Webhook Subscriptions</h2>
            <p class="text-muted">Subscribe/Unsubscribe webhook để nhận comments và messages theo thời gian thực</p>
            <hr />
        </div>
    </div>

    <div class="row">
        <div class="col-md-6">
            <div class="card mb-3">
                <div class="card-header bg-primary text-white">
                    <h5 class="mb-0">Subscribe Webhook</h5>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="form-label"><strong>Page ID:</strong></label>
                        <input type="text" class="form-control" id="pageId" placeholder="Nhập Page ID (ví dụ: 123456789)">
                        <small class="form-text text-muted">Lấy từ Facebook Page Settings > Page Info > Page ID</small>
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><strong>Access Token:</strong></label>
                        <input type="text" class="form-control" id="accessToken" placeholder="Nhập Page Access Token">
                        <small class="form-text text-muted">Lấy từ Facebook Developer Console > Messenger > Settings > Page Access Token</small>
                    </div>
                    <button class="btn btn-primary" id="btnSubscribe">Subscribe Webhook</button>
                    <button class="btn btn-secondary" id="btnCheckStatus">Kiểm tra trạng thái</button>
                </div>
            </div>

            <div class="card mb-3">
                <div class="card-header bg-info text-white">
                    <h5 class="mb-0">Thông tin Page</h5>
                </div>
                <div class="card-body">
                    <div id="pageInfo" class="text-muted">Chưa có thông tin</div>
                    <button class="btn btn-info btn-sm mt-2" id="btnGetPageInfo">Lấy thông tin Page</button>
                </div>
            </div>
        </div>

        <div class="col-md-6">
            <div class="card mb-3">
                <div class="card-header bg-success text-white">
                    <h5 class="mb-0">Trạng thái Subscription</h5>
                </div>
                <div class="card-body">
                    <div id="subscriptionStatus" class="text-muted">Chưa kiểm tra</div>
                </div>
            </div>

            <div class="card mb-3">
                <div class="card-header bg-warning text-dark">
                    <h5 class="mb-0">Test Comments</h5>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="form-label"><strong>Post ID:</strong></label>
                        <input type="text" class="form-control" id="postId" placeholder="Nhập Post ID">
                        <small class="form-text text-muted">ID của bài viết muốn lấy comments</small>
                    </div>
                    <button class="btn btn-warning" id="btnGetComments">Lấy Comments</button>
                    <div id="commentsResult" class="mt-3"></div>
                </div>
            </div>

            <div class="card">
                <div class="card-header bg-danger text-white">
                    <h5 class="mb-0">Unsubscribe Webhook</h5>
                </div>
                <div class="card-body">
                    <p class="text-muted">Hủy đăng ký webhook cho Page</p>
                    <button class="btn btn-danger" id="btnUnsubscribe">Unsubscribe</button>
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts
    <script>
        $(document).ready(function() {
            $('#btnSubscribe').click(function() {
                const pageId = $('#pageId').val();
                const accessToken = $('#accessToken').val();

                if (!pageId || !accessToken) {
                    alert('Vui lòng nhập Page ID và Access Token');
                    return;
                }

                $.ajax({
                    url: '/api/FacebookApi/SubscribePage',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({
                        PageId: pageId,
                        AccessToken: accessToken,
                        Fields: 'feed,comments,messages'
                    }),
                    success: function(data) {
                        if (data.success) {
                            alert('Subscribe thành công!');
                            checkSubscriptionStatus();
                        } else {
                            alert('Lỗi: ' + data.message);
                        }
                    },
                    error: function(xhr) {
                        const error = xhr.responseJSON ? xhr.responseJSON.message : 'Lỗi không xác định';
                        alert('Lỗi: ' + error);
                    }
                });
            });

            $('#btnCheckStatus').click(function() {
                checkSubscriptionStatus();
            });

            $('#btnGetPageInfo').click(function() {
                const pageId = $('#pageId').val();
                const accessToken = $('#accessToken').val();

                if (!pageId || !accessToken) {
                    alert('Vui lòng nhập Page ID và Access Token');
                    return;
                }

                $.ajax({
                    url: '/api/FacebookApi/PageInfo',
                    type: 'GET',
                    data: {
                        pageId: pageId,
                        accessToken: accessToken
                    },
                    success: function(data) {
                        if (data.errorMessage) {
                            $('#pageInfo').html('<span class="text-danger">Lỗi: ' + data.errorMessage + '</span>');
                        } else {
                            $('#pageInfo').html(`
                                <div><strong>ID:</strong> ${data.id}</div>
                                <div><strong>Tên:</strong> ${data.name}</div>
                                <div><strong>Access Token:</strong> ${data.hasAccessToken ? 'Có' : 'Không'}</div>
                            `);
                        }
                    },
                    error: function(xhr) {
                        const error = xhr.responseJSON ? xhr.responseJSON.errorMessage : 'Lỗi không xác định';
                        $('#pageInfo').html('<span class="text-danger">Lỗi: ' + error + '</span>');
                    }
                });
            });

            $('#btnGetComments').click(function() {
                const postId = $('#postId').val();
                const accessToken = $('#accessToken').val();

                if (!postId || !accessToken) {
                    alert('Vui lòng nhập Post ID và Access Token');
                    return;
                }

                $.ajax({
                    url: '/api/FacebookApi/PostComments',
                    type: 'GET',
                    data: {
                        postId: postId,
                        accessToken: accessToken
                    },
                    success: function(comments) {
                        if (comments.length === 0) {
                            $('#commentsResult').html('<p class="text-muted">Không có comments</p>');
                        } else {
                            let html = '<div class="list-group">';
                            comments.forEach(function(comment) {
                                html += `
                                    <div class="list-group-item">
                                        <strong>${comment.fromName || comment.fromId}</strong>
                                        <p class="mb-1">${escapeHtml(comment.message)}</p>
                                        <small class="text-muted">${comment.createdTime}</small>
                                    </div>
                                `;
                            });
                            html += '</div>';
                            $('#commentsResult').html(html);
                        }
                    },
                    error: function(xhr) {
                        $('#commentsResult').html('<span class="text-danger">Lỗi khi lấy comments</span>');
                    }
                });
            });

            $('#btnUnsubscribe').click(function() {
                const pageId = $('#pageId').val();
                const accessToken = $('#accessToken').val();

                if (!pageId || !accessToken) {
                    alert('Vui lòng nhập Page ID và Access Token');
                    return;
                }

                if (!confirm('Bạn có chắc muốn unsubscribe webhook?')) {
                    return;
                }

                $.ajax({
                    url: '/api/FacebookApi/UnsubscribePage',
                    type: 'DELETE',
                    data: {
                        pageId: pageId,
                        accessToken: accessToken
                    },
                    success: function(data) {
                        if (data.success) {
                            alert('Unsubscribe thành công!');
                            checkSubscriptionStatus();
                        } else {
                            alert('Lỗi: ' + data.message);
                        }
                    },
                    error: function(xhr) {
                        const error = xhr.responseJSON ? xhr.responseJSON.message : 'Lỗi không xác định';
                        alert('Lỗi: ' + error);
                    }
                });
            });

            function checkSubscriptionStatus() {
                const pageId = $('#pageId').val();
                const accessToken = $('#accessToken').val();

                if (!pageId || !accessToken) {
                    $('#subscriptionStatus').html('<span class="text-warning">Vui lòng nhập Page ID và Access Token</span>');
                    return;
                }

                $.ajax({
                    url: '/api/FacebookApi/SubscriptionStatus',
                    type: 'GET',
                    data: {
                        pageId: pageId,
                        accessToken: accessToken
                    },
                    success: function(data) {
                        if (data.errorMessage) {
                            $('#subscriptionStatus').html('<span class="text-danger">Lỗi: ' + data.errorMessage + '</span>');
                        } else {
                            const statusClass = data.isSubscribed ? 'text-success' : 'text-danger';
                            const statusText = data.isSubscribed ? 'Đã subscribe' : 'Chưa subscribe';
                            let html = `
                                <div><strong>Trạng thái:</strong> <span class="${statusClass}">${statusText}</span></div>
                            `;
                            
                            if (data.isSubscribed && data.subscribedFields && data.subscribedFields.length > 0) {
                                html += `<div class="mt-2"><strong>Fields đã subscribe:</strong> ${data.subscribedFields.join(', ')}</div>`;
                            }
                            
                            $('#subscriptionStatus').html(html);
                        }
                    },
                    error: function(xhr) {
                        $('#subscriptionStatus').html('<span class="text-danger">Lỗi khi kiểm tra trạng thái</span>');
                    }
                });
            }

            function escapeHtml(text) {
                if (!text) return '';
                const map = {
                    '&': '&amp;',
                    '<': '&lt;',
                    '>': '&gt;',
                    '"': '&quot;',
                    "'": '&#039;'
                };
                return text.replace(/[&<>"']/g, m => map[m]);
            }
        });
    </script>
End Section


