# Script fix port reservation (HTTP.sys)
# Phai chay PowerShell voi quyen Administrator

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FIX PORT RESERVATION (HTTP.SYS)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "NGUYEN NHAN:" -ForegroundColor Yellow
Write-Host "Port 44332 va 59277 dang bi System (HTTP.sys) reserve" -ForegroundColor White
Write-Host "IIS Express khong the bind vao port da bi reserve" -ForegroundColor White
Write-Host ""

Write-Host "GIAI PHAP:" -ForegroundColor Yellow
Write-Host ""

# 1. Kiem tra port reservation hien tai
Write-Host "1. Kiem tra port reservation hien tai..." -ForegroundColor Cyan
$urlReservations = netsh http show urlacl | Select-String -Pattern "44332|59277"
if ($urlReservations) {
    Write-Host "   Tim thay reservation:" -ForegroundColor Yellow
    $urlReservations | ForEach-Object {
        Write-Host "   $_" -ForegroundColor White
    }
} else {
    Write-Host "   Khong tim thay reservation cho port 44332/59277" -ForegroundColor Green
}
Write-Host ""

# 2. Tat IIS Express
Write-Host "2. Tat IIS Express..." -ForegroundColor Cyan
$iisExpress = Get-Process -Name "iisexpress*" -ErrorAction SilentlyContinue
if ($iisExpress) {
    Write-Host "   Dang tat IIS Express..." -ForegroundColor Yellow
    Stop-Process -Name "iisexpress*" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Write-Host "   Da tat IIS Express" -ForegroundColor Green
} else {
    Write-Host "   IIS Express khong dang chay" -ForegroundColor Green
}
Write-Host ""

# 3. Xoa port reservation (neu can)
Write-Host "3. Xoa port reservation (neu can)..." -ForegroundColor Cyan
Write-Host "   Neu port bi reserve, se xoa reservation..." -ForegroundColor Yellow
Write-Host ""

# Kiem tra quyen Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "   CANH BAO: Ban can chay PowerShell voi quyen Administrator!" -ForegroundColor Red
    Write-Host "   Right-click PowerShell -> Run as administrator" -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host "   Quyen Administrator: OK" -ForegroundColor Green
    Write-Host ""
    
    # Thu xoa reservation cho port 44332 (HTTPS)
    Write-Host "   Thu xoa reservation cho https://localhost:44332/..." -ForegroundColor Yellow
    netsh http delete urlacl url=https://localhost:44332/ 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   Da xoa reservation cho port 44332" -ForegroundColor Green
    } else {
        Write-Host "   Khong co reservation cho port 44332 (hoac da xoa)" -ForegroundColor Yellow
    }
    
    # Thu xoa reservation cho port 59277 (HTTP)
    Write-Host "   Thu xoa reservation cho http://localhost:59277/..." -ForegroundColor Yellow
    netsh http delete urlacl url=http://localhost:59277/ 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   Da xoa reservation cho port 59277" -ForegroundColor Green
    } else {
        Write-Host "   Khong co reservation cho port 59277 (hoac da xoa)" -ForegroundColor Yellow
    }
}
Write-Host ""

# 4. Huong dan tiep theo
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "HUONG DAN TIEP THEO:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Quay lai Visual Studio" -ForegroundColor White
Write-Host "2. Nhan F5 de chay project" -ForegroundColor White
Write-Host "3. IIS Express se co the bind vao port 44332/59277" -ForegroundColor White
Write-Host ""
Write-Host "NEU VAN LOI:" -ForegroundColor Yellow
Write-Host "- Thu doi sang HTTP (xem file CHAY_PROJECT_SIMPLE.md)" -ForegroundColor White
Write-Host "- Chay Visual Studio voi quyen Administrator" -ForegroundColor White
Write-Host "- Kiem tra firewall/antivirus" -ForegroundColor White
Write-Host ""





