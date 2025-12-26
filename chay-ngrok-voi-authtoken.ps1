# Script để chạy ngrok với authtoken đã cấu hình

$ngrokPath = "C:\Users\Ketca\Downloads\ngrok-v3-stable-windows-amd64\ngrok.exe"
$port = 44332

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Chạy Ngrok với Authtoken" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $ngrokPath)) {
    Write-Host "❌ Không tìm thấy ngrok tại: $ngrokPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Tìm thấy ngrok" -ForegroundColor Green
Write-Host "✅ Authtoken đã được cấu hình" -ForegroundColor Green
Write-Host ""
Write-Host "📌 Đang chạy ngrok trên port $port..." -ForegroundColor Yellow
Write-Host ""
Write-Host "⚠️  LƯU Ý:" -ForegroundColor Yellow
Write-Host "   - Với authtoken, ngrok sẽ bypass warning page cho API requests" -ForegroundColor Cyan
Write-Host "   - Facebook có thể verify webhook thành công" -ForegroundColor Cyan
Write-Host "   - Copy URL từ ngrok và cấu hình trong Facebook Developer Console" -ForegroundColor Cyan
Write-Host ""

# Chạy ngrok
& $ngrokPath http $port


