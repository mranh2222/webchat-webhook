# Script để chạy ngrok cho Facebook Webhook
# Port mặc định: 44332 (HTTPS)

param(
    [int]$Port = 44332
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Facebook Webhook - Ngrok Tunnel" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra ngrok
$ngrokPath = $null

# Tìm ngrok trong các vị trí thường dùng
$possiblePaths = @(
    "$env:LOCALAPPDATA\Microsoft\WindowsApps\ngrok.exe",
    "$env:ProgramFiles\ngrok\ngrok.exe",
    "$env:ProgramFiles(x86)\ngrok\ngrok.exe",
    ".\ngrok.exe",
    "ngrok.exe"
)

foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $ngrokPath = $path
        break
    }
}

# Nếu không tìm thấy, kiểm tra trong PATH
if (-not $ngrokPath) {
    $ngrokCmd = Get-Command ngrok -ErrorAction SilentlyContinue
    if ($ngrokCmd) {
        $ngrokPath = $ngrokCmd.Source
    }
}

if (-not $ngrokPath) {
    Write-Host "❌ Ngrok không tìm thấy!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Vui lòng cài đặt ngrok:" -ForegroundColor Yellow
    Write-Host "1. Tải từ: https://ngrok.com/download" -ForegroundColor Yellow
    Write-Host "2. Hoặc cài qua Chocolatey: choco install ngrok" -ForegroundColor Yellow
    Write-Host "3. Hoặc cài qua winget: winget install ngrok.ngrok" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Sau khi cài đặt, đặt ngrok.exe vào thư mục project hoặc thêm vào PATH" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Tìm thấy ngrok tại: $ngrokPath" -ForegroundColor Green
Write-Host ""

# Kiểm tra xem ứng dụng có đang chạy không
Write-Host "Đang kiểm tra ứng dụng trên port $Port..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "https://localhost:$Port" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
    Write-Host "✅ Ứng dụng đang chạy trên port $Port" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Không thể kết nối đến https://localhost:$Port" -ForegroundColor Yellow
    Write-Host "   Đảm bảo ứng dụng đang chạy (F5 trong Visual Studio)" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "Bạn có muốn tiếp tục chạy ngrok? (y/n)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        exit 0
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Đang khởi động ngrok..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📌 Webhook URL sẽ là: https://[ngrok-url]/api/Webhook" -ForegroundColor Yellow
Write-Host "📌 Verify Token: my_facebook_verify_token_12345" -ForegroundColor Yellow
Write-Host ""
Write-Host "Sau khi ngrok chạy, copy URL và cấu hình trong Facebook Developer Console" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra xem có file config không
$configFile = Join-Path $PSScriptRoot "ngrok.yml"
if (Test-Path $configFile) {
    Write-Host "✅ Tìm thấy file config: $configFile" -ForegroundColor Green
    Write-Host "   Đang chạy ngrok với config..." -ForegroundColor Yellow
    Write-Host ""
    & $ngrokPath start --config $configFile webhook
} else {
    Write-Host "⚠️  Không tìm thấy file ngrok.yml" -ForegroundColor Yellow
    Write-Host "   Chạy ngrok bình thường (có thể gặp warning page)" -ForegroundColor Yellow
    Write-Host ""
    & $ngrokPath http $Port
}

