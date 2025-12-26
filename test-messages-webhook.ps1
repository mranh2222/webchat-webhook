# Script test messages webhook
param(
    [string]$NgrokUrl = "https://unbilious-autumn-taillessly.ngrok-free.dev"
)

Write-Host "=== TEST MESSAGES WEBHOOK ===" -ForegroundColor Cyan
Write-Host ""

$webhookUrl = "$NgrokUrl/api/Webhook"

Write-Host "📌 Webhook URL: $webhookUrl" -ForegroundColor Yellow
Write-Host ""

# Test format giống Facebook
$testPayload = @{
    field = "messages"
    value = @{
        sender = @{
            id = "12334"
        }
        recipient = @{
            id = "23245"
        }
        timestamp = "1527459824"
        message = @{
            mid = "test_message_id"
            text = "test_message"
            commands = @(
                @{
                    name = "command123"
                },
                @{
                    name = "command456"
                }
            )
        }
    }
} | ConvertTo-Json -Depth 10

Write-Host "📤 Sending POST request..." -ForegroundColor Cyan
Write-Host "Payload:" -ForegroundColor Yellow
Write-Host $testPayload
Write-Host ""

try {
    $response = Invoke-WebRequest -Uri $webhookUrl -Method POST -Body $testPayload -ContentType "application/json" -UseBasicParsing
    Write-Host "✅ Status Code: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "✅ Response: $($response.Content)" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode
        Write-Host "   Status Code: $statusCode" -ForegroundColor Red
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "   Response: $responseBody" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Kiểm tra Visual Studio Output để xem logs!" -ForegroundColor Cyan

