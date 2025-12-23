@Code
    ViewData("Title") = "Hướng dẫn cấu hình Webhook"
    Layout = "~/Views/Shared/_Layout.vbhtml"
End Code

<div class="container mt-4">
    <div class="row">
        <div class="col-12">
            <h2>Hướng dẫn cấu hình Facebook Webhook</h2>
            <hr />
        </div>
    </div>

    <div class="row">
        <div class="col-md-12">
            <div class="card mb-3">
                <div class="card-header bg-primary text-white">
                    <h5 class="mb-0">Thông tin Webhook</h5>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="form-label"><strong>Webhook URL:</strong></label>
                        <div class="input-group">
                            <input type="text" class="form-control" id="webhookUrl" value="@(Request.Url.Scheme & "://" & Request.Url.Authority & "/api/Webhook")" readonly>
                            <button class="btn btn-outline-secondary" type="button" onclick="copyToClipboard('webhookUrl')">Copy</button>
                        </div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><strong>Verify Token:</strong></label>
                        <div class="input-group">
                            <input type="text" class="form-control" id="verifyToken" value="@System.Configuration.ConfigurationManager.AppSettings("FacebookVerifyToken")" readonly>
                            <button class="btn btn-outline-secondary" type="button" onclick="copyToClipboard('verifyToken')">Copy</button>
                        </div>
                        <small class="form-text text-muted">Token này được cấu hình trong Web.config</small>
                    </div>
                </div>
            </div>

            <div class="card mb-3">
                <div class="card-header bg-success text-white">
                    <h5 class="mb-0">Cấu hình Firebase (Bước 1 - Bắt buộc)</h5>
                </div>
                <div class="card-body">
                    <ol>
                        <li class="mb-3">
                            <strong>Tạo Firebase Project</strong>
                            <ul>
                                <li>Đi tới <a href="https://console.firebase.google.com" target="_blank">https://console.firebase.google.com</a></li>
                                <li>Đăng nhập bằng tài khoản Google</li>
                                <li>Click "Thêm dự án" (Add project) hoặc chọn project có sẵn</li>
                                <li>Nhập tên project và làm theo hướng dẫn</li>
                            </ul>
                        </li>
                        <li class="mb-3">
                            <strong>Tạo Realtime Database</strong>
                            <ul>
                                <li>Trong Firebase Console, vào mục "Realtime Database" ở menu bên trái</li>
                                <li>Click "Tạo database" (Create Database)</li>
                                <li>Chọn vị trí (location) gần bạn nhất</li>
                                <li>Chọn chế độ bảo mật: "Test mode" (để test) hoặc "Production mode" (cho production)</li>
                            </ul>
                        </li>
                        <li class="mb-3">
                            <strong>Lấy thông tin kết nối</strong>
                            <ul>
                                <li><strong>Database URL:</strong> Copy URL từ trang Realtime Database
                                    <br><code>Ví dụ: https://myproject-12345.firebaseio.com</code></li>
                                <li><strong>Database Secret (nếu cần):</strong> 
                                    <ul>
                                        <li>Vào Project Settings (⚙️) > Service Accounts</li>
                                        <li>Copy "Database secrets" hoặc tạo secret mới</li>
                                        <li><strong>Lưu ý:</strong> Nếu dùng Firebase Rules, có thể không cần secret</li>
                                    </ul>
                                </li>
                            </ul>
                        </li>
                        <li class="mb-3">
                            <strong>Cấu hình trong Web.config</strong>
                            <ul>
                                <li>Mở file <code>Web.config</code> trong project</li>
                                <li>Tìm section <code>&lt;appSettings&gt;</code></li>
                                <li>Cập nhật các giá trị:
                                    <pre class="bg-light p-2 mt-2"><code>&lt;add key="FirebaseBaseUrl" value="https://your-project-id.firebaseio.com" /&gt;
&lt;add key="FirebaseAuthSecret" value="your-firebase-secret-key" /&gt;</code></pre>
                                </li>
                                <li>Thay <code>your-project-id</code> bằng Database URL của bạn</li>
                                <li>Thay <code>your-firebase-secret-key</code> bằng Database Secret (hoặc để trống nếu dùng Rules)</li>
                            </ul>
                        </li>
                        <li class="mb-3">
                            <strong>Cấu hình Firebase Rules (Quan trọng cho bảo mật)</strong>
                            <ul>
                                <li>Vào Realtime Database > Rules tab</li>
                                <li>Để test, có thể dùng:
                                    <pre class="bg-light p-2 mt-2"><code>{
  "rules": {
    "webhooks": {
      ".read": true,
      ".write": true
    }
  }
}</code></pre>
                                </li>
                                <li><strong>Cảnh báo:</strong> Rules trên cho phép mọi người đọc/ghi. Chỉ dùng để test!</li>
                                <li>Cho production, nên dùng authentication hoặc IP whitelist</li>
                            </ul>
                        </li>
                    </ol>
                </div>
            </div>

            <div class="card mb-3">
                <div class="card-header bg-info text-white">
                    <h5 class="mb-0">Cấu hình Facebook Webhook (Bước 2)</h5>
                </div>
                <div class="card-body">
                    <ol>
                        <li class="mb-3">
                            <strong>Truy cập Facebook Developer Console</strong>
                            <ul>
                                <li>Đi tới <a href="https://developers.facebook.com" target="_blank">https://developers.facebook.com</a></li>
                                <li>Chọn ứng dụng của bạn hoặc tạo ứng dụng mới</li>
                            </ul>
                        </li>
                        <li class="mb-3">
                            <strong>Cấu hình Webhook</strong>
                            <ul>
                                <li>Vào mục "Webhooks" trong menu bên trái</li>
                                <li>Click "Add Callback URL" hoặc "Edit Subscription"</li>
                                <li>Nhập Webhook URL từ trên</li>
                                <li>Nhập Verify Token từ trên</li>
                                <li>Click "Verify and Save"</li>
                            </ul>
                        </li>
                        <li class="mb-3">
                            <strong>Đăng ký các subscription</strong>
                            <ul>
                                <li><strong>Để nhận Comments trên bài viết:</strong>
                                    <ul>
                                        <li>Vào "Webhooks" > "Page" hoặc "Instagram"</li>
                                        <li>Subscribe vào "feed" hoặc "comments"</li>
                                        <li>Chọn các fields: <code>feed</code>, <code>comments</code></li>
                                    </ul>
                                </li>
                                <li><strong>Để nhận Messages từ Messenger:</strong>
                                    <ul>
                                        <li>Vào "Messenger" trong menu bên trái</li>
                                        <li>Vào "Webhooks" section</li>
                                        <li>Subscribe vào "messages"</li>
                                        <li>Chọn các fields: <code>messages</code>, <code>messaging_postbacks</code></li>
                                    </ul>
                                </li>
                            </ul>
                        </li>
                        <li class="mb-3">
                            <strong>Cấu hình Messenger (Quan trọng cho nhận tin nhắn)</strong>
                            <ul>
                                <li>Vào "Messenger" > "Settings"</li>
                                <li>Chọn hoặc tạo Page của bạn</li>
                                <li>Copy "Page Access Token" (cần để gửi tin nhắn phản hồi)</li>
                                <li>Trong "Webhooks", đảm bảo đã subscribe "messages"</li>
                                <li>Chọn các subscription fields:
                                    <ul>
                                        <li><code>messages</code> - Nhận tin nhắn text</li>
                                        <li><code>messaging_postbacks</code> - Nhận postback events</li>
                                        <li><code>messaging_optins</code> - Nhận opt-in events</li>
                                        <li><code>messaging_deliveries</code> - Nhận delivery confirmations</li>
                                    </ul>
                                </li>
                            </ul>
                        </li>
                        <li class="mb-3">
                            <strong>Kiểm tra Webhook</strong>
                            <ul>
                                <li>Quay lại trang <a href="@Url.Action("Test", "Facebook")">Test</a> để xem dữ liệu real-time</li>
                                <li>Thử comment trên một bài viết hoặc gửi message để test</li>
                            </ul>
                        </li>
                    </ol>
                </div>
            </div>

            <div class="card mb-3">
                <div class="card-header bg-warning text-dark">
                    <h5 class="mb-0">Lưu ý quan trọng</h5>
                </div>
                <div class="card-body">
                    <ul>
                        <li>Webhook URL phải có thể truy cập được từ internet (không phải localhost)</li>
                        <li>Nếu test local, bạn cần dùng ngrok hoặc công cụ tương tự để expose localhost ra internet</li>
                        <li>Verify Token phải khớp với token trong Web.config</li>
                        <li>Đảm bảo ứng dụng Facebook của bạn đã được approved các permissions cần thiết</li>
                    </ul>
                </div>
            </div>

            <div class="card">
                <div class="card-header bg-success text-white">
                    <h5 class="mb-0">Test Webhook</h5>
                </div>
                <div class="card-body">
                    <p>Bạn có thể test webhook bằng cách:</p>
                    <ul>
                        <li>Comment trên một bài viết của page/group bạn đã subscribe</li>
                        <li>Gửi message đến page của bạn (nếu đã cấu hình Messenger)</li>
                        <li>Quay lại trang <a href="@Url.Action("Test", "Facebook")" class="btn btn-sm btn-primary">Test</a> để xem dữ liệu</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</div>

<script>
    function copyToClipboard(elementId) {
        const element = document.getElementById(elementId);
        element.select();
        element.setSelectionRange(0, 99999); // For mobile devices
        document.execCommand('copy');
        alert('Đã copy vào clipboard!');
    }
</script>

