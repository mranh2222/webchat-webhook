# Script đơn giản để chạy ngrok
Write-Host "=== CHẠY NGROK ===" -ForegroundColor Cyan
Write-Host ""

# Đường dẫn ngrok
$ngrokPath = "C:\Users\Ketca\Downloads\ngrok-v3-stable-windows-amd64\ngrok.exe"
$configPath = "C:\Users\Ketca\Desktop\WEBTEST2\ngrok.yml"

# Kiểm tra ngrok
if (-not (Test-Path $ngrokPath)) {
    Write-Host "❌ Không tìm thấy ngrok.exe tại: $ngrokPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Vui lòng kiểm tra đường dẫn ngrok!" -ForegroundColor Yellow
    exit 1
}

# Kiểm tra config
if (-not (Test-Path $configPath)) {
    Write-Host "❌ Không tìm thấy ngrok.yml tại: $configPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Đã tìm thấy ngrok và config" -ForegroundColor Green
Write-Host ""
Write-Host "📌 Config:" -ForegroundColor Yellow
Write-Host "   - Forward đến: http://localhost:59277" -ForegroundColor White
Write-Host "   - Protocol: HTTP" -ForegroundColor White
Write-Host ""
Write-Host "📌 Sau khi chạy, copy URL từ ngrok và dán vào Facebook:" -ForegroundColor Yellow
Write-Host "   Facebook Developer Console → Webhooks → Callback URL" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  LƯU Ý:" -ForegroundColor Yellow
Write-Host "   1. Đảm bảo project đang chạy (F5 trong Visual Studio)" -ForegroundColor White
Write-Host "   2. Ngrok sẽ chạy cho đến khi bạn nhấn Ctrl+C" -ForegroundColor White
Write-Host "   3. Mở http://127.0.0.1:4040 để xem web interface" -ForegroundColor White
Write-Host ""
Write-Host "Đang khởi động ngrok..." -ForegroundColor Cyan
Write-Host ""

# Chạy ngrok
cd "C:\Users\Ketca\Downloads\ngrok-v3-stable-windows-amd64"
& .\ngrok.exe start --config $configPath webhook

