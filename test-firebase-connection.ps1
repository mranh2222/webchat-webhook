# Test Firebase Connection
Write-Host "=== Test Firebase Connection ===" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "https://webchat-6ab22-default-rtdb.asia-southeast1.firebasedatabase.app"
$authSecret = "3YQ2VQRZrMLny2WSMiLrCXmz5f6x0rv2nBYha09m"

Write-Host "Firebase URL: $baseUrl" -ForegroundColor Yellow
Write-Host "Auth Secret: $authSecret" -ForegroundColor Yellow
Write-Host ""

# Test 1: Test connection
Write-Host "Test 1: Kiểm tra kết nối Firebase..." -ForegroundColor Cyan
$testUrl = "$baseUrl/.json"
if ($authSecret) {
    $testUrl += "?auth=$authSecret"
}

try {
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
    $response = Invoke-WebRequest -Uri $testUrl -Method GET -UseBasicParsing -ErrorAction Stop
    Write-Host "✅ Kết nối thành công! Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ Lỗi kết nối: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Kiểm tra webhooks path
Write-Host "Test 2: Kiểm tra path 'webhooks'..." -ForegroundColor Cyan
$webhooksUrl = "$baseUrl/webhooks.json"
if ($authSecret) {
    $webhooksUrl += "?auth=$authSecret"
}

try {
    $response = Invoke-WebRequest -Uri $webhooksUrl -Method GET -UseBasicParsing -ErrorAction Stop
    $content = $response.Content
    
    if ($content -eq "null" -or [string]::IsNullOrWhiteSpace($content)) {
        Write-Host "✅ Path 'webhooks' tồn tại nhưng chưa có dữ liệu" -ForegroundColor Yellow
        Write-Host "   → Firebase sẽ tự động tạo khi có data" -ForegroundColor Gray
    } else {
        Write-Host "✅ Path 'webhooks' đã có dữ liệu!" -ForegroundColor Green
        Write-Host "   Content length: $($content.Length) bytes" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Lỗi khi đọc path 'webhooks': $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 3: Test write (tạo test entry)
Write-Host "Test 3: Test ghi dữ liệu vào Firebase..." -ForegroundColor Cyan
$testId = [System.Guid]::NewGuid().ToString()
$testData = @{
    Id = $testId
    Type = "Test"
    Message = "Test message from PowerShell"
    Timestamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")
} | ConvertTo-Json

$writeUrl = "$baseUrl/webhooks/$testId.json"
if ($authSecret) {
    $writeUrl += "?auth=$authSecret"
}

try {
    $response = Invoke-WebRequest -Uri $writeUrl -Method PUT -Body $testData -ContentType "application/json" -UseBasicParsing -ErrorAction Stop
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ Ghi dữ liệu thành công!" -ForegroundColor Green
        Write-Host "   Test ID: $testId" -ForegroundColor Gray
        
        # Đọc lại để xác nhận
        Start-Sleep -Seconds 1
        $readResponse = Invoke-WebRequest -Uri $writeUrl -Method GET -UseBasicParsing -ErrorAction Stop
        Write-Host "✅ Đọc lại dữ liệu thành công!" -ForegroundColor Green
        Write-Host "   Content: $($readResponse.Content)" -ForegroundColor Gray
        
        # Xóa test data
        Write-Host ""
        Write-Host "Đang xóa test data..." -ForegroundColor Yellow
        $deleteResponse = Invoke-WebRequest -Uri $writeUrl -Method DELETE -UseBasicParsing -ErrorAction Stop
        Write-Host "✅ Đã xóa test data" -ForegroundColor Green
    } else {
        Write-Host "❌ Ghi dữ liệu thất bại. Status: $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Lỗi khi ghi dữ liệu: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errorContent = $reader.ReadToEnd()
        Write-Host "   Error details: $errorContent" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Kết luận ===" -ForegroundColor Cyan
Write-Host "Firebase đã được cấu hình và kết nối thành công!" -ForegroundColor Green
Write-Host "Path 'webhooks' sẽ tự động được tạo khi có data." -ForegroundColor Yellow
Write-Host ""
Write-Host "Bước tiếp theo:" -ForegroundColor Cyan
Write-Host "   1. Test webhook với message thực tế" -ForegroundColor White
Write-Host "   2. Kiểm tra trang Test: http://localhost:59277/Facebook/Test" -ForegroundColor White
Write-Host "   3. Xem data trong Firebase Console: https://console.firebase.google.com" -ForegroundColor White


