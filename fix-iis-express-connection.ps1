# Script fix IIS Express connection issue
# Chay PowerShell voi quyen Administrator

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "KIEM TRA VA FIX IIS EXPRESS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Kiem tra IIS Express
Write-Host "1. Kiem tra IIS Express..." -ForegroundColor Yellow
$iisExpress = Get-Process -Name "iisexpress*" -ErrorAction SilentlyContinue
if ($iisExpress) {
    Write-Host "   IIS Express dang chay:" -ForegroundColor Green
    $iisExpress | ForEach-Object {
        Write-Host "   - PID: $($_.Id) - $($_.ProcessName)" -ForegroundColor White
    }
} else {
    Write-Host "   IIS Express KHONG dang chay" -ForegroundColor Red
}
Write-Host ""

# 2. Kiem tra port dang duoc su dung
Write-Host "2. Kiem tra port IIS Express dang su dung..." -ForegroundColor Yellow
if ($iisExpress) {
    $ports = Get-NetTCPConnection | Where-Object {$_.OwningProcess -in $iisExpress.Id} | Select-Object -Unique LocalPort, State
    if ($ports) {
        Write-Host "   IIS Express dang su dung cac port:" -ForegroundColor Green
        $ports | ForEach-Object {
            Write-Host "   - Port $($_.LocalPort) (State: $($_.State))" -ForegroundColor White
        }
    } else {
        Write-Host "   IIS Express KHONG bind vao port nao (co the bi loi)" -ForegroundColor Red
    }
}
Write-Host ""

# 3. Kiem tra port 44332 va 59277
Write-Host "3. Kiem tra port 44332 va 59277..." -ForegroundColor Yellow
$port44332 = Get-NetTCPConnection -LocalPort 44332 -ErrorAction SilentlyContinue
$port59277 = Get-NetTCPConnection -LocalPort 59277 -ErrorAction SilentlyContinue

if ($port44332) {
    $pid44332 = $port44332.OwningProcess
    $proc44332 = Get-Process -Id $pid44332 -ErrorAction SilentlyContinue
    Write-Host "   Port 44332 dang duoc su dung boi: $($proc44332.ProcessName) (PID: $pid44332)" -ForegroundColor Yellow
} else {
    Write-Host "   Port 44332 KHONG dang duoc su dung" -ForegroundColor Red
}

if ($port59277) {
    $pid59277 = $port59277.OwningProcess
    $proc59277 = Get-Process -Id $pid59277 -ErrorAction SilentlyContinue
    Write-Host "   Port 59277 dang duoc su dung boi: $($proc59277.ProcessName) (PID: $pid59277)" -ForegroundColor Yellow
} else {
    Write-Host "   Port 59277 KHONG dang duoc su dung" -ForegroundColor Red
}
Write-Host ""

# 4. Tat IIS Express cu (neu can)
Write-Host "4. Giai phap:" -ForegroundColor Yellow
Write-Host ""
if ($iisExpress) {
    Write-Host "   IIS Express dang chay nhung KHONG bind vao port 44332/59277" -ForegroundColor Red
    Write-Host "   Co the IIS Express dang chay project khac" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   CACH 1: Tat IIS Express cu va chay lai project" -ForegroundColor Cyan
    Write-Host "   1. Right-click icon IIS Express trong System Tray" -ForegroundColor White
    Write-Host "   2. Chon 'Exit' de tat tat ca sites" -ForegroundColor White
    Write-Host "   3. Quay lai Visual Studio, nhan F5 de chay lai" -ForegroundColor White
    Write-Host ""
    Write-Host "   CACH 2: Kill IIS Express bang PowerShell (neu can):" -ForegroundColor Cyan
    Write-Host "   Stop-Process -Name 'iisexpress*' -Force" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "   IIS Express KHONG dang chay" -ForegroundColor Red
    Write-Host "   Hay chay project tu Visual Studio (F5)" -ForegroundColor Cyan
}

Write-Host "========================================" -ForegroundColor Cyan





