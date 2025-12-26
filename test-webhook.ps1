# Script để test Facebook Webhook

param(
    [string]$NgrokUrl = "https://unbilious-autumn-taillessly.ngrok-free.dev",
    [string]$VerifyToken = "my_facebook_verify_token_12345"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Test Facebook Webhook" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$webhookUrl = "$NgrokUrl/api/Webhook"

Write-Host "📌 Webhook URL: $webhookUrl" -ForegroundColor Yellow
Write-Host "📌 Verify Token: $VerifyToken" -ForegroundColor Yellow
Write-Host ""

# Test 1: Verify Webhook (GET request)
Write-Host "Test 1: Verify Webhook (GET request)..." -ForegroundColor Cyan
$challenge = "test_challenge_12345"
$verifyUrl = "$webhookUrl?hub.mode=subscribe&hub.verify_token=$VerifyToken&hub.challenge=$challenge"

try {
    $response = Invoke-WebRequest -Uri $verifyUrl -Method GET -UseBasicParsing
    Write-Host "✅ Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "✅ Response: $($response.Content)" -ForegroundColor Green
    
    if ($response.Content -eq $challenge) {
        Write-Host "✅ Verification SUCCESS! Webhook is working correctly." -ForegroundColor Green
    } else {
        Write-Host "⚠️  Response không khớp với challenge" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "   Status Code: $statusCode" -ForegroundColor Red
    }
}

Write-Host ""

# Test 2: Check if webhook endpoint is accessible
Write-Host "Test 2: Check webhook endpoint accessibility..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$webhookUrl?test=1" -Method GET -UseBasicParsing -ErrorAction Stop
    Write-Host "✅ Webhook endpoint is accessible" -ForegroundColor Green
} catch {
    Write-Host "❌ Cannot access webhook endpoint" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Thông tin để cấu hình Facebook:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Callback URL: $webhookUrl" -ForegroundColor Yellow
Write-Host "Verify Token: $VerifyToken" -ForegroundColor Yellow
Write-Host ""
Write-Host "Copy các thông tin trên và paste vào Facebook Developer Console" -ForegroundColor Cyan
Write-Host ""


