# Hướng dẫn Deploy lên Render.com

## Bước 1: Đăng ký Render

1. Vào https://render.com
2. Click "Get Started for Free"
3. Đăng nhập bằng GitHub (khuyến nghị) hoặc Google/Email
4. Xác nhận email nếu cần

## Bước 2: Chuẩn bị code cho Render

### Tạo file `render.yaml` (Tùy chọn - để tự động deploy)

Tạo file `render.yaml` trong root project:

```yaml
services:
  - type: web
    name: webchat-webhook
    runtime: dotnet
    buildCommand: dotnet build
    startCommand: dotnet run
    env: dotnet
```

### Hoặc deploy thủ công (Khuyến nghị cho ASP.NET Framework)

Render chủ yếu hỗ trợ .NET Core tốt hơn. Với ASP.NET Framework 4.8, có thể cần:
- Chuyển sang .NET Core (khuyến nghị)
- Hoặc dùng Docker (phức tạp hơn)

## Bước 3: Push code lên GitHub

1. Tạo repository mới trên GitHub:
   - Vào https://github.com/new
   - Đặt tên: `webchat-webhook` (hoặc tên khác)
   - Click "Create repository"

2. Push code lên GitHub:
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   git branch -M main
   git remote add origin https://github.com/your-username/webchat-webhook.git
   git push -u origin main
   ```

## Bước 4: Deploy trên Render

1. Vào Render Dashboard: https://dashboard.render.com
2. Click "New +" → "Web Service"
3. Connect GitHub:
   - Click "Connect GitHub" (nếu chưa connect)
   - Chọn repository `webchat-webhook`
   - Click "Connect"
4. Cấu hình:
   - **Name:** `webchat-webhook`
   - **Region:** Chọn gần bạn (ví dụ: Singapore)
   - **Branch:** `main`
   - **Runtime:** `Docker` hoặc `.NET` (nếu có)
   - **Build Command:** (để trống hoặc `dotnet build`)
   - **Start Command:** (để trống hoặc `dotnet run`)
5. Click "Create Web Service"
6. Đợi deploy xong (5-10 phút)

## Bước 5: Lấy URL

- Sau khi deploy xong, bạn sẽ có URL: `https://webchat-webhook.onrender.com`
- URL này cố định, không đổi

## Bước 6: Cập nhật Webhook trong Facebook

1. Vào Facebook Developer Console
2. Vào Webhooks (hoặc Messenger > Webhooks)
3. Cập nhật Callback URL: `https://webchat-webhook.onrender.com/api/Webhook`
4. Verify Token: giữ nguyên
5. Click "Xác minh và lưu"

## Lưu ý quan trọng:

⚠️ **Render chủ yếu hỗ trợ .NET Core, không phải ASP.NET Framework 4.8**

Nếu gặp lỗi, có 2 cách:

### Cách 1: Chuyển sang .NET Core (Khuyến nghị)
- Tạo project mới với .NET Core
- Port code sang .NET Core

### Cách 2: Dùng Railway thay vì Render
- Railway hỗ trợ .NET Framework tốt hơn
- Hoặc dùng Docker

## Troubleshooting:

- **Lỗi build:** Kiểm tra .NET version, có thể cần .NET Core
- **Lỗi runtime:** Kiểm tra Start Command
- **Webhook không hoạt động:** Kiểm tra URL và Verify Token

