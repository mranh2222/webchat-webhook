@Code
    ViewData("Title") = "Facebook Webhook Test"
    Layout = "~/Views/Shared/_Layout.vbhtml"
End Code

<div class="container mt-4">
    <div class="row">
        <div class="col-12">
            <h2>Facebook & Messenger Webhook Test</h2>
            <p class="text-muted">Theo dõi comments và messages theo thời gian thực</p>
        </div>
    </div>

    <div class="row mb-3">
        <div class="col-md-6">
            <div class="card">
                <div class="card-header bg-primary text-white">
                    <h5 class="mb-0">Trạng thái Webhook</h5>
                </div>
                <div class="card-body">
                    <div class="mb-2">
                        <strong>Status:</strong> 
                        <span id="statusIndicator" class="badge bg-secondary">Đang kiểm tra...</span>
                    </div>
                    <div class="mb-2">
                        <strong>Tổng số events:</strong> 
                        <span id="totalEvents" class="badge bg-info">0</span>
                    </div>
                    <div class="mb-2">
                        <strong>Comments:</strong> 
                        <span id="totalComments" class="badge bg-success">0</span>
                    </div>
                    <div class="mb-2">
                        <strong>Messages:</strong> 
                        <span id="totalMessages" class="badge bg-warning">0</span>
                    </div>
                    <button id="btnClear" class="btn btn-danger btn-sm mt-2">Xóa dữ liệu</button>
                    <button id="btnRefresh" class="btn btn-secondary btn-sm mt-2">Làm mới</button>
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="card">
                <div class="card-header bg-info text-white">
                    <h5 class="mb-0">Thông tin Webhook</h5>
                </div>
                <div class="card-body">
                    <p><strong>Webhook URL:</strong></p>
                    <code id="webhookUrl" class="d-block mb-2 p-2 bg-light">Đang tải...</code>
                    <p class="mt-3"><strong>Verify Token:</strong></p>
                    <code id="verifyToken" class="d-block mb-2 p-2 bg-light">Đang tải...</code>
                    <p class="mt-3"><strong>Firebase Storage:</strong></p>
                    <div id="firebaseInfo" class="small text-muted">Đang tải...</div>
                    <a href="@Url.Action("WebhookInfo", "Facebook")" class="btn btn-sm btn-outline-info mt-2 d-block">Xem hướng dẫn cấu hình</a>
                </div>
            </div>
        </div>
    </div>

    <div class="row">
        <div class="col-12">
            <div class="card">
                <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">Events theo thời gian thực</h5>
                    <div>
                        <label class="text-white me-2">
                            <input type="checkbox" id="autoRefresh" checked> Tự động làm mới
                        </label>
                        <span class="badge bg-light text-dark" id="lastUpdate">Chưa cập nhật</span>
                    </div>
                </div>
                <div class="card-body">
                    <div id="eventsContainer" style="max-height: 600px; overflow-y: auto;">
                        <div class="text-center text-muted py-5">
                            <p>Chưa có dữ liệu. Đang chờ webhook từ Facebook...</p>
                            <small>Đảm bảo bạn đã cấu hình webhook URL trong Facebook Developer Console</small>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts
    <script>
        let refreshInterval = null;
        const refreshIntervalTime = 2000; // 2 giây

        $(document).ready(function() {
            loadWebhookInfo();
            loadWebhookData();
            loadFirebaseInfo();
            
            // Thiết lập auto refresh
            $('#autoRefresh').change(function() {
                if ($(this).is(':checked')) {
                    startAutoRefresh();
                } else {
                    stopAutoRefresh();
                }
            });

            $('#btnRefresh').click(function() {
                loadWebhookData();
                loadFirebaseInfo();
            });

            $('#btnClear').click(function() {
                if (confirm('Bạn có chắc muốn xóa tất cả dữ liệu từ Firebase?')) {
                    $.ajax({
                        url: '/api/Webhook/Clear',
                        type: 'GET',
                        success: function() {
                            loadWebhookData();
                            loadFirebaseInfo();
                            alert('Đã xóa dữ liệu');
                        },
                        error: function() {
                            alert('Lỗi khi xóa dữ liệu. Kiểm tra kết nối Firebase.');
                        }
                    });
                }
            });

            startAutoRefresh();
        });

        function loadWebhookInfo() {
            $('#webhookUrl').text(window.location.origin + '/api/Webhook');
        }

        function loadFirebaseInfo() {
            $.ajax({
                url: '/api/Webhook/FirebaseInfo',
                type: 'GET',
                dataType: 'json',
                success: function(data) {
                    if (data) {
                        const statusClass = data.IsConnected ? 'text-success' : 'text-danger';
                        const statusText = data.IsConnected ? 'Đã kết nối' : 'Chưa kết nối';
                        const configStatus = data.IsConfigured ? 'text-success' : 'text-warning';
                        const configText = data.IsConfigured ? 'Đã cấu hình' : 'Chưa cấu hình';
                        
                        let html = `
                            <div class="small">
                                <div><strong>Trạng thái:</strong> <span class="${configStatus}">${configText}</span></div>
                                <div><strong>Kết nối:</strong> <span class="${statusClass}">${statusText}</span></div>
                                <div><strong>Tổng entries:</strong> ${data.TotalEntries || 0}</div>
                                ${data.BaseUrl ? '<div class="mt-1"><small><strong>URL:</strong> ' + data.BaseUrl + '</small></div>' : ''}
                            </div>
                        `;
                        
                        if (data.ErrorMessage) {
                            html += '<div class="alert alert-danger mt-2 small">' + data.ErrorMessage + '</div>';
                        }
                        
                        if (!data.IsConfigured) {
                            html += '<div class="alert alert-warning mt-2 small">Vui lòng cấu hình Firebase trong Web.config</div>';
                        }
                        
                        $('#firebaseInfo').html(html);
                    }
                },
                error: function(xhr, status, error) {
                    $('#firebaseInfo').html('<span class="text-danger">Không thể tải thông tin Firebase: ' + error + '</span>');
                }
            });
        }

        function loadWebhookData() {
            $.ajax({
                url: '/api/Webhook/GetData',
                type: 'GET',
                dataType: 'json',
                success: function(data) {
                    displayEvents(data);
                    updateStatistics(data);
                    $('#lastUpdate').text('Cập nhật: ' + new Date().toLocaleTimeString('vi-VN'));
                },
                error: function(xhr, status, error) {
                    console.error('Error loading webhook data:', error);
                    $('#statusIndicator').removeClass('bg-success').addClass('bg-danger').text('Lỗi kết nối');
                }
            });
        }

        function displayEvents(events) {
            const container = $('#eventsContainer');
            
            if (!events || events.length === 0) {
                container.html(`
                    <div class="text-center text-muted py-5">
                        <p>Chưa có dữ liệu. Đang chờ webhook từ Facebook...</p>
                        <small>Đảm bảo bạn đã cấu hình webhook URL trong Facebook Developer Console</small>
                    </div>
                `);
                return;
            }

            let html = '<div class="list-group">';
            
            events.forEach(function(event) {
                const typeClass = event.Type === 'Comment' ? 'success' : 'warning';
                const typeIcon = event.Type === 'Comment' ? '💬' : '📨';
                const timeAgo = getTimeAgo(new Date(event.Timestamp));
                
                html += `
                    <div class="list-group-item">
                        <div class="d-flex w-100 justify-content-between align-items-start">
                            <div class="flex-grow-1">
                                <h6 class="mb-1">
                                    <span class="badge bg-${typeClass} me-2">${typeIcon} ${event.Type}</span>
                                    <small class="text-muted">${timeAgo}</small>
                                </h6>
                                <p class="mb-1">${escapeHtml(event.Message || '(Không có nội dung)')}</p>
                                <small class="text-muted">
                                    <strong>From:</strong> ${escapeHtml(event.FromName || event.FromId || 'Unknown')} 
                                    ${event.PostId ? ' | <strong>Post ID:</strong> ' + event.PostId : ''}
                                    ${event.CommentId ? ' | <strong>ID:</strong> ' + event.CommentId : ''}
                                </small>
                            </div>
                            <button class="btn btn-sm btn-outline-secondary" onclick="showRawData('${event.Id}')">
                                Raw Data
                            </button>
                        </div>
                        <div id="raw-${event.Id}" style="display:none;" class="mt-2">
                            <pre class="bg-light p-2 small"><code>${escapeHtml(event.RawData)}</code></pre>
                        </div>
                    </div>
                `;
            });
            
            html += '</div>';
            container.html(html);
        }

        function updateStatistics(events) {
            if (!events) {
                $('#totalEvents').text('0');
                $('#totalComments').text('0');
                $('#totalMessages').text('0');
                $('#statusIndicator').removeClass('bg-danger').addClass('bg-secondary').text('Chưa có dữ liệu');
                return;
            }

            const comments = events.filter(e => e.Type === 'Comment').length;
            const messages = events.filter(e => e.Type === 'Message').length;
            
            $('#totalEvents').text(events.length);
            $('#totalComments').text(comments);
            $('#totalMessages').text(messages);
            
            if (events.length > 0) {
                $('#statusIndicator').removeClass('bg-secondary bg-danger').addClass('bg-success').text('Đang hoạt động');
            } else {
                $('#statusIndicator').removeClass('bg-success bg-danger').addClass('bg-secondary').text('Chưa có dữ liệu');
            }
        }

        function getTimeAgo(date) {
            const now = new Date();
            const diff = Math.floor((now - date) / 1000);
            
            if (diff < 60) return diff + ' giây trước';
            if (diff < 3600) return Math.floor(diff / 60) + ' phút trước';
            if (diff < 86400) return Math.floor(diff / 3600) + ' giờ trước';
            return Math.floor(diff / 86400) + ' ngày trước';
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

        function showRawData(id) {
            const element = $('#raw-' + id);
            if (element.is(':visible')) {
                element.hide();
            } else {
                element.show();
            }
        }

        function startAutoRefresh() {
            if (refreshInterval) {
                clearInterval(refreshInterval);
            }
            refreshInterval = setInterval(function() {
                loadWebhookData();
            }, refreshIntervalTime);
        }

        function stopAutoRefresh() {
            if (refreshInterval) {
                clearInterval(refreshInterval);
                refreshInterval = null;
            }
        }
    </script>
End Section

