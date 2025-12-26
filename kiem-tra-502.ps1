# Script kiểm tra 502 Bad Gateway
Write-Host "=== KIEM TRA 502 BAD GATEWAY ===" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra 1: Port 59277 có đang lắng nghe không
Write-Host "1. Kiem tra port 59277..." -ForegroundColor Yellow
try {
    $connection = Test-NetConnection -ComputerName localhost -Port 59277 -WarningAction SilentlyContinue
    if ($connection.TcpTestSucceeded) {
        Write-Host "   ✅ Port 59277 dang lang nghe" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Port 59277 KHONG lang nghe" -ForegroundColor Red
        Write-Host "   → Chay project trong Visual Studio (F5)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ❌ Port 59277 KHONG lang nghe" -ForegroundColor Red
    Write-Host "   → Chay project trong Visual Studio (F5)" -ForegroundColor Yellow
}

Write-Host ""

# Kiểm tra 2: Test localhost
Write-Host "2. Test localhost..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:59277/api/Webhook?test=1" -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop
    Write-Host "   ✅ Localhost tra ve status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Localhost KHONG tra ve" -ForegroundColor Red
    Write-Host "   → Kiem tra project co dang chay khong (F5)" -ForegroundColor Yellow
    Write-Host "   → Kiem tra port co dung khong" -ForegroundColor Yellow
}

Write-Host ""

# Kiểm tra 3: Kiểm tra ngrok config
Write-Host "3. Kiem tra ngrok config..." -ForegroundColor Yellow
$configPath = "C:\Users\Ketca\Desktop\WEBTEST2\ngrok.yml"
if (Test-Path $configPath) {
    $config = Get-Content $configPath -Raw
    if ($config -match "addr:\s*http://localhost:59277") {
        Write-Host "   ✅ ngrok.yml cau hinh dung port 59277" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  ngrok.yml co the sai port" -ForegroundColor Yellow
        Write-Host "   → Kiem tra file ngrok.yml" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ❌ Khong tim thay ngrok.yml" -ForegroundColor Red
}

Write-Host ""

# Kiểm tra 4: Kiểm tra ngrok có đang chạy không
Write-Host "4. Kiem tra ngrok..." -ForegroundColor Yellow
try {
    $ngrokStatus = Invoke-WebRequest -Uri "http://127.0.0.1:4040/api/tunnels" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
    Write-Host "   ✅ Ngrok dang chay" -ForegroundColor Green
    Write-Host "   → Mo http://127.0.0.1:4040 de xem web interface" -ForegroundColor Cyan
} catch {
    Write-Host "   ❌ Ngrok KHONG chay" -ForegroundColor Red
    Write-Host "   → Chay ngrok: .\CHAY_NGROK_DON_GIAN.ps1" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== KET LUAN ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Neu tat ca deu OK nhung van bi 502:" -ForegroundColor Yellow
Write-Host "  1. Restart project (Shift+F5 -> F5)" -ForegroundColor White
Write-Host "  2. Restart ngrok (Ctrl+C -> chay lai)" -ForegroundColor White
Write-Host "  3. Kiem tra Visual Studio Output (View -> Output -> Debug)" -ForegroundColor White
Write-Host "  4. Kiem tra ngrok web interface: http://127.0.0.1:4040" -ForegroundColor White

