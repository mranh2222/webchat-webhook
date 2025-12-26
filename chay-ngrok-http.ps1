# Chạy ngrok với HTTP port 59277
Write-Host "=== Chạy ngrok với HTTP port 59277 ===" -ForegroundColor Cyan

$ngrokPath = "C:\Users\Ketca\Downloads\ngrok-v3-stable-windows-amd64\ngrok.exe"
$configPath = "C:\Users\Ketca\Desktop\WEBTEST2\ngrok.yml"

if (-not (Test-Path $ngrokPath)) {
    Write-Host "❌ Không tìm thấy ngrok.exe tại: $ngrokPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $configPath)) {
    Write-Host "❌ Không tìm thấy ngrok.yml tại: $configPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Đã tìm thấy ngrok và config file" -ForegroundColor Green
Write-Host "Config: Forward đến http://localhost:59277" -ForegroundColor Yellow
Write-Host ""
Write-Host "Đang chạy ngrok..." -ForegroundColor Cyan
Write-Host ""

# Chạy ngrok
& $ngrokPath start --config $configPath webhook


