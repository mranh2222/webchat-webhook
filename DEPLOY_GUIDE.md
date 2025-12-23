# Hướng dẫn Deploy lên Azure App Service

## Bước 1: Đăng ký Azure (Nếu chưa có)

1. Vào https://azure.microsoft.com/free/
2. Click "Start free"
3. Đăng ký bằng email/Microsoft account
4. Xác minh thẻ tín dụng (không bị tính phí nếu dùng Free Tier)

## Bước 2: Cài đặt Azure Tools trong Visual Studio

1. Mở Visual Studio
2. Vào **Tools** > **Get Tools and Features**
3. Trong tab **Individual components**, tìm và tick:
   - ✅ **Azure development workload**
   - ✅ **Azure App Service tools**
4. Click **Modify** để cài đặt

## Bước 3: Deploy từ Visual Studio

1. **Mở project** trong Visual Studio
2. **Right-click** vào project `WEBTEST2` trong Solution Explorer
3. Chọn **"Publish"**
4. Trong cửa sổ Publish:
   - Chọn **"Azure"** (bên trái)
   - Chọn **"Azure App Service (Windows)"**
   - Click **Next**
5. **Đăng nhập Azure:**
   - Click **"Sign in"** hoặc **"Add an account"**
   - Đăng nhập bằng Azure account của bạn
6. **Tạo App Service mới:**
   - Click **"Create new"** (dấu +)
   - Điền thông tin:
     - **Name:** `webchat-webhook` (hoặc tên khác, phải unique)
     - **Subscription:** Chọn subscription của bạn
     - **Resource Group:** 
       - Click **"New"** → Đặt tên: `webchat-resources`
     - **Hosting Plan:**
       - Click **"New"**
       - **Name:** `webchat-plan`
       - **Location:** Chọn gần bạn (ví dụ: `Southeast Asia`)
       - **Size:** Chọn **"Free"** (F1)
     - **Application Insights:** Có thể bỏ qua (tắt)
   - Click **"Create"**
   - Đợi Azure tạo xong (1-2 phút)
7. **Publish:**
   - Sau khi tạo xong, click **"Publish"**
   - Visual Studio sẽ build và deploy project
   - Đợi deploy xong (2-5 phút)
   - Khi thành công, trình duyệt sẽ tự mở với URL: `https://webchat-webhook.azurewebsites.net`

## Bước 4: Cập nhật Webhook trong Facebook

1. Copy URL từ Azure: `https://webchat-webhook.azurewebsites.net`
2. Vào Facebook Developer Console
3. Vào **Webhooks** (hoặc **Messenger** > **Webhooks**)
4. Cập nhật **Callback URL:**
   - Cũ: `https://localhost:44332/api/Webhook`
   - Mới: `https://webchat-webhook.azurewebsites.net/api/Webhook`
5. **Verify Token:** Giữ nguyên `my_facebook_verify_token_12345`
6. Click **"Xác minh và lưu"**
7. Nếu thành công, bạn sẽ thấy **"Verified"** màu xanh

## Bước 5: Test

1. Mở URL: `https://webchat-webhook.azurewebsites.net/Facebook/Test`
2. Kiểm tra phần "Firebase Storage" - phải hiển thị "Đã kết nối"
3. Gửi tin nhắn đến Page hoặc comment vào bài viết
4. Xem dữ liệu real-time trong trang Test

## Lưu ý quan trọng

- **Free Tier giới hạn:** 1 app, 1GB storage, 165MB RAM
- **URL cố định:** `https://webchat-webhook.azurewebsites.net` (không đổi)
- **Deploy lại:** Right-click project > **Publish** > Click **"Publish"** lại
- **Xem logs:** Vào Azure Portal > App Service > **Log stream**

## Troubleshooting

### Lỗi: "Name already exists"
- Đổi tên app (ví dụ: `webchat-webhook-123`)

### Lỗi: "Deployment failed"
- Kiểm tra build errors trong Visual Studio
- Đảm bảo project build thành công trước khi publish

### Webhook không verify được
- Kiểm tra URL có đúng không
- Kiểm tra ứng dụng đang chạy trên Azure
- Kiểm tra Verify Token có khớp không


