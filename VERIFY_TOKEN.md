# Verify Token (Mã xác minh)

## 🔑 Verify Token là gì?

**Verify Token** (Mã xác minh) là một chuỗi bí mật dùng để xác minh rằng Facebook đang gửi request đến đúng server của bạn.

Khi Facebook gửi GET request để verify webhook, nó sẽ gửi kèm verify token này. Server của bạn phải kiểm tra xem token này có khớp với token đã cấu hình không.

---

## ✅ Verify Token của bạn

**Mã xác minh:** `my_facebook_verify_token_12345`

**Vị trí trong code:**
- File: `Web.config`
- Key: `FacebookVerifyToken`
- Value: `my_facebook_verify_token_12345`

---

## 📋 Cách sử dụng trong Facebook Developer Console

### Bước 1: Vào Webhooks
1. Vào **Facebook Developer Console**
2. Chọn app của bạn
3. Vào **Webhooks** (hoặc **Messenger** → **Webhooks**)

### Bước 2: Điền Verify Token
1. Tìm trường **"Xác minh mã"** (Verify Token)
2. **Điền:** `my_facebook_verify_token_12345`
3. **Lưu ý:** Phải điền chính xác, không có khoảng trắng thừa

### Bước 3: Điền Callback URL
1. Tìm trường **"URL gọi lại"** (Callback URL)
2. **Điền:** `https://[ngrok-url]/api/Webhook`
   - Ví dụ: `https://unbilious-autumn-taillessly.ngrok-free.dev/api/Webhook`

### Bước 4: Click "Xác minh và lưu"
1. Click nút **"Xác minh và lưu"** (Verify and Save)
2. Facebook sẽ gửi GET request đến server của bạn
3. Server sẽ kiểm tra verify token
4. Nếu khớp → Trả về challenge code → Facebook xác minh thành công ✅

---

## 🔍 Cách hoạt động

### 1. Facebook gửi GET request:
```
GET /api/Webhook?hub.mode=subscribe&hub.verify_token=my_facebook_verify_token_12345&hub.challenge=abc123
```

### 2. Server kiểm tra:
- `hub.mode` = `"subscribe"` ✅
- `hub.verify_token` = `"my_facebook_verify_token_12345"` ✅ (khớp với Web.config)

### 3. Server trả về:
- Status: `200 OK`
- Body: `abc123` (challenge code)

### 4. Facebook xác minh thành công ✅

---

## ⚠️ Lưu ý

### 1. Verify Token phải giống hệt
- Token trong **Facebook** phải **giống hệt** với token trong **Web.config**
- Không phân biệt hoa thường (case-insensitive)
- Không có khoảng trắng thừa

### 2. Có thể đổi Verify Token
- Nếu muốn đổi, phải đổi cả 2 nơi:
  - `Web.config` → `FacebookVerifyToken`
  - Facebook Developer Console → Verify Token

### 3. Verify Token không phải là Access Token
- **Verify Token:** Dùng để verify webhook (bạn tự đặt)
- **Access Token:** Dùng để gọi Facebook API (lấy từ Facebook)

---

## 🧪 Test Verify Token

### Test bằng browser:
```
https://localhost:59277/api/Webhook?hub.mode=subscribe&hub.verify_token=my_facebook_verify_token_12345&hub.challenge=test123
```

**Kết quả mong đợi:** Hiển thị `test123` ✅

### Test bằng PowerShell:
```powershell
.\test-webhook.ps1
```

---

## ❌ Nếu verify fail

### Kiểm tra:
1. **Token có đúng không?**
   - Facebook: `my_facebook_verify_token_12345`
   - Web.config: `my_facebook_verify_token_12345`

2. **URL có đúng không?**
   - Phải là: `https://[ngrok-url]/api/Webhook`
   - Không có `/` ở cuối

3. **Server có đang chạy không?**
   - Visual Studio → F5
   - Ngrok đang chạy

4. **Xem logs:**
   - Visual Studio Output → Debug
   - Tìm: `=== WebhookController.VerifyWebhook CALLED ===`

---

## ✅ Checklist

- [ ] **Đã điền Verify Token:** `my_facebook_verify_token_12345`
- [ ] **Đã điền Callback URL:** `https://[ngrok-url]/api/Webhook`
- [ ] **Đã click "Xác minh và lưu"**
- [ ] **Facebook hiển thị "Đã xác minh"** (màu xanh)
- [ ] **Test bằng browser:** Hiển thị challenge code ✅

---

## 📝 Tóm tắt

**Verify Token của bạn:** `my_facebook_verify_token_12345`

**Điền vào Facebook:** `my_facebook_verify_token_12345`

**Xong!** 🎉

