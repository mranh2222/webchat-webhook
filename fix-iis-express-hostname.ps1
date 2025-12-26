# Fix IIS Express Invalid Hostname Error
# Script này sẽ cấu hình IIS Express để chấp nhận requests từ ngrok

Write-Host "=== Fix IIS Express Invalid Hostname ===" -ForegroundColor Cyan

# Tìm file applicationhost.config
$vsConfigPath = ".vs\WEBTEST2\config\applicationhost.config"
$userConfigPath = "$env:USERPROFILE\Documents\IISExpress\config\applicationhost.config"
$globalConfigPath = "$env:PROGRAMFILES\IIS Express\config\templates\PersonalWebServer\applicationhost.config"

$configPath = $null

if (Test-Path $vsConfigPath) {
    $configPath = $vsConfigPath
    Write-Host "✅ Tìm thấy config trong .vs folder" -ForegroundColor Green
} elseif (Test-Path $userConfigPath) {
    $configPath = $userConfigPath
    Write-Host "✅ Tìm thấy config trong Documents folder" -ForegroundColor Green
} else {
    Write-Host "❌ Không tìm thấy applicationhost.config" -ForegroundColor Red
    Write-Host "Thử tìm trong các thư mục sau:" -ForegroundColor Yellow
    Write-Host "  - $vsConfigPath"
    Write-Host "  - $userConfigPath"
    Write-Host ""
    Write-Host "Giải pháp thay thế: Cấu hình ngrok để forward với host header localhost" -ForegroundColor Yellow
    exit 1
}

Write-Host "Config file: $configPath" -ForegroundColor Cyan

# Backup file
$backupPath = "$configPath.backup"
Copy-Item $configPath $backupPath -Force
Write-Host "✅ Đã backup config file" -ForegroundColor Green

# Đọc file config
[xml]$config = Get-Content $configPath

# Tìm site binding cho WEBTEST2
$site = $config.configuration.'system.applicationHost'.sites.site | Where-Object { $_.name -eq "WebSite1" -or $_.bindings.binding.protocol -eq "https" }

if ($null -eq $site) {
    Write-Host "❌ Không tìm thấy site binding" -ForegroundColor Red
    Write-Host "Thử giải pháp thay thế: Cấu hình ngrok" -ForegroundColor Yellow
    exit 1
}

# Kiểm tra xem đã có binding với host="" chưa
$hasWildcardBinding = $false
foreach ($binding in $site.bindings.binding) {
    if ($binding.bindingInformation -like "*:44332:*") {
        $parts = $binding.bindingInformation -split ":"
        if ($parts.Length -ge 3 -and $parts[2] -eq "") {
            $hasWildcardBinding = $true
            Write-Host "✅ Đã có wildcard binding" -ForegroundColor Green
            break
        }
    }
}

if (-not $hasWildcardBinding) {
    # Thêm binding mới với host="" (chấp nhận tất cả hostnames)
    $newBinding = $config.CreateElement("binding")
    $newBinding.SetAttribute("protocol", "https")
    $newBinding.SetAttribute("bindingInformation", "*:44332:")
    
    $site.bindings.AppendChild($newBinding)
    Write-Host "✅ Đã thêm wildcard binding" -ForegroundColor Green
    
    # Save file
    $config.Save($configPath)
    Write-Host "✅ Đã lưu config" -ForegroundColor Green
} else {
    Write-Host "ℹ️  Wildcard binding đã tồn tại" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Hoàn thành ===" -ForegroundColor Cyan
Write-Host "Bước tiếp theo:" -ForegroundColor Yellow
Write-Host "1. Tắt IIS Express (nếu đang chạy)" -ForegroundColor White
Write-Host "2. Chạy lại project từ Visual Studio (F5)" -ForegroundColor White
Write-Host "3. Test lại ngrok URL" -ForegroundColor White


