# Script kiem tra va huong dan chay project
# Chay trong PowerShell

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "KIEM TRA VA HUONG DAN CHAY PROJECT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Kiem tra Visual Studio
Write-Host "1. Kiem tra Visual Studio..." -ForegroundColor Yellow
$vsProcess = Get-Process -Name "devenv" -ErrorAction SilentlyContinue
if ($vsProcess) {
    Write-Host "   Visual Studio dang mo" -ForegroundColor Green
} else {
    Write-Host "   Visual Studio chua mo" -ForegroundColor Red
    Write-Host "   Hay mo Visual Studio va mo Solution WEBTEST2.sln" -ForegroundColor Cyan
}
Write-Host ""

# 2. Kiem tra IIS Express
Write-Host "2. Kiem tra IIS Express..." -ForegroundColor Yellow
$iisExpress = Get-Process -Name "iisexpress*" -ErrorAction SilentlyContinue
if ($iisExpress) {
    Write-Host "   IIS Express dang chay (PID: $($iisExpress.Id))" -ForegroundColor Green
} else {
    Write-Host "   IIS Express KHONG dang chay" -ForegroundColor Red
    Write-Host "   Ban can chay project tu Visual Studio (F5)" -ForegroundColor Cyan
}
Write-Host ""

# 3. Kiem tra port 44332 (HTTPS)
Write-Host "3. Kiem tra port 44332 (HTTPS)..." -ForegroundColor Yellow
$port44332Open = $false
$connection44332 = Get-NetTCPConnection -LocalPort 44332 -State Listen -ErrorAction SilentlyContinue
if ($connection44332) {
    Write-Host "   Port 44332 dang mo" -ForegroundColor Green
    $port44332Open = $true
} else {
    Write-Host "   Port 44332 KHONG dang mo" -ForegroundColor Red
}
Write-Host ""

# 4. Kiem tra port 59277 (HTTP)
Write-Host "4. Kiem tra port 59277 (HTTP)..." -ForegroundColor Yellow
$port59277Open = $false
$connection59277 = Get-NetTCPConnection -LocalPort 59277 -State Listen -ErrorAction SilentlyContinue
if ($connection59277) {
    Write-Host "   Port 59277 dang mo" -ForegroundColor Green
    $port59277Open = $true
} else {
    Write-Host "   Port 59277 KHONG dang mo" -ForegroundColor Red
}
Write-Host ""

# 5. Tong ket va huong dan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TONG KET VA HUONG DAN" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (-not $iisExpress) {
    Write-Host "PROJECT CHUA DUOC CHAY!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Cac buoc de chay project:" -ForegroundColor Yellow
    Write-Host "1. Mo Visual Studio" -ForegroundColor White
    Write-Host "2. Mo Solution: WEBTEST2.sln" -ForegroundColor White
    Write-Host "3. Nhan F5 (hoac click nut Start mau xanh)" -ForegroundColor White
    Write-Host "4. Doi browser tu dong mo" -ForegroundColor White
    Write-Host ""
    Write-Host "LUU Y QUAN TRONG:" -ForegroundColor Red
    Write-Host "   - Build (Ctrl+Shift+B) KHAC Run (F5)" -ForegroundColor Yellow
    Write-Host "   - Ban PHAI nhan F5 de CHAY project, khong chi Build" -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host "Project dang chay!" -ForegroundColor Green
    Write-Host ""
    if ($port44332Open) {
        Write-Host "Truy cap HTTPS: https://localhost:44332/" -ForegroundColor Cyan
    }
    if ($port59277Open) {
        Write-Host "Truy cap HTTP:  http://localhost:59277/" -ForegroundColor Cyan
    }
    Write-Host ""
    Write-Host "Test API endpoint:" -ForegroundColor Yellow
    if ($port44332Open) {
        Write-Host "   https://localhost:44332/api/Webhook?hub.mode=subscribe&hub.verify_token=my_facebook_verify_token_12345&hub.challenge=test123" -ForegroundColor White
    } elseif ($port59277Open) {
        Write-Host "   http://localhost:59277/api/Webhook?hub.mode=subscribe&hub.verify_token=my_facebook_verify_token_12345&hub.challenge=test123" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
