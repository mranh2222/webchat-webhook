# Fix lỗi E_FAIL (0x80004005) - Project Load Failed
Write-Host "=== Fix E_FAIL Error ===" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra Visual Studio có đang chạy không
$vsProcess = Get-Process -Name "devenv*" -ErrorAction SilentlyContinue
if ($vsProcess) {
    Write-Host "⚠️  Visual Studio đang chạy!" -ForegroundColor Yellow
    Write-Host "Vui lòng đóng Visual Studio trước khi chạy script này." -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "Bạn có muốn tiếp tục không? (y/n)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        Write-Host "Đã hủy." -ForegroundColor Red
        exit
    }
}

Write-Host "Đang xóa file cache..." -ForegroundColor Yellow
Write-Host ""

# Xóa file .suo (Visual Studio user options)
$suoPath = ".vs\WEBTEST2\v17\.suo"
if (Test-Path $suoPath) {
    Remove-Item $suoPath -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Đã xóa file .suo" -ForegroundColor Green
} else {
    Write-Host "ℹ️  File .suo không tồn tại" -ForegroundColor Gray
}

# Xóa folder bin
if (Test-Path "bin") {
    Remove-Item "bin" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Đã xóa folder bin" -ForegroundColor Green
} else {
    Write-Host "ℹ️  Folder bin không tồn tại" -ForegroundColor Gray
}

# Xóa folder obj
if (Test-Path "obj") {
    Remove-Item "obj" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Đã xóa folder obj" -ForegroundColor Green
} else {
    Write-Host "ℹ️  Folder obj không tồn tại" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== Hoàn thành ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Bước tiếp theo:" -ForegroundColor Yellow
Write-Host "1. Mở lại Visual Studio" -ForegroundColor White
Write-Host "2. Mở Solution: WEBTEST2.sln" -ForegroundColor White
Write-Host "3. Reload project nếu cần" -ForegroundColor White
Write-Host "4. Build project (Ctrl+Shift+B)" -ForegroundColor White


