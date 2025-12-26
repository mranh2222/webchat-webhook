# Script tat IIS Express
# Chay PowerShell

Write-Host "Dang tat IIS Express..." -ForegroundColor Yellow

$iisExpress = Get-Process -Name "iisexpress*" -ErrorAction SilentlyContinue
if ($iisExpress) {
    Write-Host "Tim thay IIS Express:" -ForegroundColor Cyan
    $iisExpress | ForEach-Object {
        Write-Host "  - PID: $($_.Id) - $($_.ProcessName)" -ForegroundColor White
    }
    Write-Host ""
    Write-Host "Dang kill IIS Express..." -ForegroundColor Yellow
    Stop-Process -Name "iisexpress*" -Force
    Start-Sleep -Seconds 2
    Write-Host "Da tat IIS Express!" -ForegroundColor Green
} else {
    Write-Host "IIS Express khong dang chay" -ForegroundColor Green
}

Write-Host ""
Write-Host "Ban co the chay lai project tu Visual Studio (F5)" -ForegroundColor Cyan





