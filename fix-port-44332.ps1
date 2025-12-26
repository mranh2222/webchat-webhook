# Script để fix port 44332 bị chiếm
# Chạy PowerShell với quyền Administrator

Write-Host "=== Kiểm tra port 44332 ===" -ForegroundColor Cyan

# Kiểm tra connection
$connection = Get-NetTCPConnection -LocalPort 44332 -ErrorAction SilentlyContinue

if ($connection) {
    $pid = $connection.OwningProcess
    $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
    
    Write-Host "Port 44332 đang được sử dụng bởi:" -ForegroundColor Yellow
    Write-Host "  Process ID: $pid"
    
    if ($process) {
        Write-Host "  Process Name: $($process.ProcessName)"
        Write-Host "  Process Path: $($process.Path)"
        
        # Nếu là IIS Express hoặc process khác (không phải System)
        if ($process.ProcessName -like "*iisexpress*" -or $pid -ne 4) {
            Write-Host "`nĐang kill process $pid..." -ForegroundColor Yellow
            Stop-Process -Id $pid -Force
            Write-Host "✅ Đã kill process $pid" -ForegroundColor Green
            Start-Sleep -Seconds 2
        } else {
            Write-Host "`n⚠️  Port đang được sử dụng bởi System (PID 4)" -ForegroundColor Red
            Write-Host "   Điều này có thể là do HTTP.sys hoặc service khác" -ForegroundColor Red
            Write-Host "`n💡 Giải pháp:" -ForegroundColor Cyan
            Write-Host "   1. Mở Visual Studio" -ForegroundColor White
            Write-Host "   2. Mở Solution WEBTEST2.sln" -ForegroundColor White
            Write-Host "   3. Nhấn F5 để chạy project" -ForegroundColor White
            Write-Host "   4. IIS Express sẽ tự động xử lý" -ForegroundColor White
        }
    }
} else {
    Write-Host "✅ Port 44332 không bị chiếm" -ForegroundColor Green
}

Write-Host "`n=== Kiểm tra IIS Express ===" -ForegroundColor Cyan
$iisExpress = Get-Process -Name "iisexpress*" -ErrorAction SilentlyContinue
if ($iisExpress) {
    Write-Host "IIS Express đang chạy:" -ForegroundColor Green
    $iisExpress | ForEach-Object {
        Write-Host "  PID: $($_.Id) - $($_.Path)"
    }
} else {
    Write-Host "IIS Express không đang chạy" -ForegroundColor Yellow
    Write-Host "💡 Bạn cần chạy project từ Visual Studio (F5)" -ForegroundColor Cyan
}

Write-Host "`n=== Hoàn thành ===" -ForegroundColor Cyan





