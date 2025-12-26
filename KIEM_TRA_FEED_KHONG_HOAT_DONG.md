# Kiểm tra: Trường feed không hoạt động

## 🔍 Vấn đề

Test trường `feed` nhưng không thấy logs trong Visual Studio Output.

**Từ logs hiện tại:**
- ✅ Chỉ thấy GET requests đến `/api/Webhook/GetData` (từ trang Test)
- ❌ KHÔNG thấy POST request đến `/api/Webhook` (từ Facebook)
- ❌ KHÔNG thấy logs từ `WebhookController.ReceiveWebhook`

**Kết luận:** Request từ Facebook chưa đến server!

---

## ✅ Đã cải thiện logging

### 1. Logging trong Application_BeginRequest

**Giờ sẽ log:**
- ✅ Tất cả requests đến `/api/Webhook` (không phân biệt hoa thường)
- ✅ Method (GET/POST)
- ✅ ContentType, ContentLength
- ✅ UserAgent
- ✅ Đặc biệt log khi thấy POST request

### 2. Logging trong WebhookController

**Đã có logging chi tiết:**
- ✅ Đầu hàm `ReceiveWebhook`
- ✅ Tất cả các bước xử lý
- ✅ Kết quả save Firebase

---

## 📋 Các bước kiểm tra

### Bước 1: Chạy lại project

1. **Stop project** (Shift+F5)
2. **Start lại** (F5)
3. **Đảm bảo ngrok đang chạy**

---

### Bước 2: Kiểm tra ngrok web interface

**Mở:** `http://127.0.0.1:4040`

**Sau khi test trường feed:**
1. **Refresh trang** ngrok web interface
2. **Tìm POST request** đến `/api/Webhook`
3. **Kiểm tra:**
   - ✅ Có POST request không?
   - ✅ Status là gì? (200, 502, 404, etc.)
   - ✅ Click vào request → Xem JSON payload

**Nếu KHÔNG thấy POST request:**
- ❌ Facebook không gửi được request đến ngrok
- ❌ Cần kiểm tra:
  - Webhook đã verify chưa? (dấu tích xanh)
  - Trường `feed` đã SUBSCRIBE chưa? (toggle màu xanh)
  - URL trong Facebook có đúng không?

---

### Bước 3: Test lại trường feed

1. **Vào Facebook Developer Console** → Webhooks
2. **Tìm trường `feed`**
3. **Click "Thử nghiệm"** → "Gửi đến máy chủ v24.0"
4. **Ngay lập tức kiểm tra:**

   **A. Ngrok web interface:**
   - Refresh trang
   - Phải thấy POST request đến `/api/Webhook`
   - Status phải là **200 OK**

   **B. Visual Studio Output:**
   - Phải thấy: `=== Application_BeginRequest ===`
   - Phải thấy: `Method: POST`
   - Phải thấy: `>>> POST REQUEST DETECTED TO /api/Webhook`
   - Phải thấy: `=== WebhookController.ReceiveWebhook CALLED ===`

---

### Bước 4: Nếu vẫn không thấy POST request

**Kiểm tra trong Facebook:**

1. **Webhook đã verify chưa?**
   - Phải có dấu tích xanh "Đã xác minh"
   - Nếu không → Click "Xác minh và lưu" lại

2. **Trường `feed` đã SUBSCRIBE chưa?**
   - Toggle phải **màu xanh** (ON)
   - Không phải màu xám (OFF)
   - **Quan trọng:** Chỉ click "Thử nghiệm" KHÔNG đủ, phải SUBSCRIBE!

3. **URL có đúng không?**
   - Phải là: `https://unbilious-autumn-taillessly.ngrok-free.dev/api/Webhook`
   - Phải giống với URL trong ngrok

4. **Verify Token có đúng không?**
   - Phải là: `my_facebook_verify_token_12345`
   - Phải giống với Web.config

---

## 🔍 Debug nếu thấy POST request trong ngrok nhưng không thấy logs

### Nếu ngrok thấy POST request với status 200:

**Nhưng Visual Studio Output không thấy logs:**
- ❌ Request đến nhưng không match route
- ❌ Code có lỗi và crash trước khi log

**Cách debug:**
1. **Xem JSON payload trong ngrok:**
   - Click vào POST request
   - Copy JSON payload
   - Kiểm tra cấu trúc có đúng không

2. **Kiểm tra Visual Studio Output:**
   - Tìm các dòng có "ERROR" hoặc "Exception"
   - Xem có error nào không

---

### Nếu ngrok thấy POST request với status 502:

**Nguyên nhân:** Server không chạy hoặc port sai

**Cách fix:**
1. **Kiểm tra project có chạy không:**
   - Visual Studio → F5
   - Output phải hiển thị "Application started"

2. **Kiểm tra port:**
   - Xem port trong Visual Studio Output
   - So sánh với port trong `ngrok.yml`
   - Phải giống nhau!

---

### Nếu ngrok thấy POST request với status 404:

**Nguyên nhân:** Route không match

**Cách fix:**
1. **Kiểm tra URL trong Facebook:**
   - Phải là: `https://[ngrok-url]/api/Webhook`
   - Không có `/` ở cuối
   - Không có path thêm

2. **Kiểm tra routing:**
   - WebhookController có `[Route("api/Webhook")]`
   - WebApiConfig có `config.MapHttpAttributeRoutes()`

---

## ✅ Kết quả mong đợi

Sau khi fix:
- ✅ Ngrok web interface: Thấy POST request với status **200 OK**
- ✅ Visual Studio Output: Thấy `=== Application_BeginRequest ===` với Method: POST
- ✅ Visual Studio Output: Thấy `=== WebhookController.ReceiveWebhook CALLED ===`
- ✅ Visual Studio Output: Thấy logs chi tiết từ `ProcessTestFeed`
- ✅ Firebase: Events được lưu

---

## 🆘 Checklist đầy đủ

- [ ] **Ngrok đang chạy** (status "online")
- [ ] **Project đang chạy** (F5 trong Visual Studio)
- [ ] **Webhook đã verify** (dấu tích xanh trong Facebook)
- [ ] **Trường `feed` đã SUBSCRIBE** (toggle màu xanh)
- [ ] **URL trong Facebook đúng** với URL trong ngrok
- [ ] **Test lại** và kiểm tra ngrok web interface
- [ ] **Kiểm tra Visual Studio Output** có logs không

---

## 💡 Lưu ý quan trọng

### Về subscribe:

- **Chỉ click "Thử nghiệm" KHÔNG đủ!**
- **Phải SUBSCRIBE** (bật toggle) mới nhận được events
- **Toggle phải màu xanh** (ON), không phải màu xám (OFF)

### Về ngrok web interface:

- **Phải refresh trang** sau khi test
- **Phải tìm POST request**, không phải GET request
- **Phải click vào request** để xem chi tiết

### Về Visual Studio Output:

- **Phải chọn "Debug"** (không phải "Build")
- **Phải scroll xuống** để xem logs mới nhất
- **Tìm các dòng có "=== " để dễ nhìn**

