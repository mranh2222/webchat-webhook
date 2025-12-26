# Chạy ngrok với host_header fix
Write-Host "=== Chạy ngrok với host_header fix ===" -ForegroundColor Cyan

$ngrokPath = "C:\Users\Ketca\Downloads\ngrok-v3-stable-windows-amd64\ngrok.exe"
$configPath = "C:\Users\Ketca\Desktop\WEBTEST2\ngrok.yml"

if (-not (Test-Path $ngrokPath)) {
    Write-Host "❌ Không tìm thấy ngrok.exe tại: $ngrokPath" -ForegroundColor Red
    Write-Host "Vui lòng kiểm tra đường dẫn ngrok" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path $configPath)) {
    Write-Host "❌ Không tìm thấy ngrok.yml tại: $configPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Đã tìm thấy ngrok và config file" -ForegroundColor Green
Write-Host ""
Write-Host "Đang chạy ngrok với config..." -ForegroundColor Yellow
Write-Host "Config file: $configPath" -ForegroundColor Cyan
Write-Host ""

# Chạy ngrok
& $ngrokPath start --config $configPath webhook


