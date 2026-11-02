$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5058'

function Assert-True($condition, $message) {
    if (-not $condition) { throw "ASSERT FAILED: $message" }
}

function Invoke-Json($method, $uri, $body = $null, $token = $null) {
    $headers = @{}
    if ($token) { $headers.Authorization = "Bearer $token" }
    $params = @{ Method = $method; Uri = "$base$uri"; Headers = $headers }
    if ($null -ne $body) {
        $params.Body = ($body | ConvertTo-Json -Depth 10)
        $params.ContentType = 'application/json'
    }
    try {
        return Invoke-WebRequest @params -UseBasicParsing
    } catch {
        return $_.Exception.Response
    }
}

function Read-Response($response) {
    if ($response -is [Microsoft.PowerShell.Commands.BasicHtmlWebResponseObject]) {
        return ($response.Content | ConvertFrom-Json)
    }
    $stream = $response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    try { return ($reader.ReadToEnd() | ConvertFrom-Json) } finally { $reader.Dispose(); $stream.Dispose() }
}

$adminResponse = Invoke-Json 'POST' '/api/auth/login' @{ email = 'admin@restaurant.com'; password = 'Admin@123456' }
$customerResponse = Invoke-Json 'POST' '/api/auth/login' @{ email = 'customer@restaurant.com'; password = 'Customer@123456' }
$admin = Read-Response $adminResponse
$customer = Read-Response $customerResponse
Assert-True ($admin.success -and $admin.data.token) 'Admin login must return a token'
Assert-True ($customer.success -and $customer.data.token) 'Customer login must return a token'

$refreshResponse = Invoke-Json 'POST' '/api/auth/refresh-token' @{ refreshToken = $admin.data.refreshToken }
$refresh = Read-Response $refreshResponse
Assert-True ($refresh.success -and $refresh.data.token -and $refresh.data.refreshToken) 'Refresh token must rotate into a new access/refresh pair'

$replayedRefreshResponse = Invoke-Json 'POST' '/api/auth/refresh-token' @{ refreshToken = $admin.data.refreshToken }
Assert-True ([int]$replayedRefreshResponse.StatusCode -eq 401) 'A rotated refresh token must not be reusable'

$futureDate = (Get-Date).AddDays(2).ToString('yyyy-MM-dd')
$availabilityResponse = Invoke-Json 'GET' "/api/Booking/availability?bookingDate=$futureDate&bookingTime=19:00&numGuests=2&serviceDurationMinutes=120"
$availability = Read-Response $availabilityResponse
Assert-True ($availability.success) 'Availability endpoint must succeed for a future valid slot'
Assert-True ($null -ne $availability.data) 'Availability endpoint must return data'

$unauthorizedResponse = Invoke-Json 'POST' '/api/payment/create-deposit' @{ bookingId = 999999; paymentMethod = 3 }
Assert-True ([int]$unauthorizedResponse.StatusCode -eq 401) 'Deposit creation without authentication must be rejected'

$lateResponse = Invoke-Json 'POST' '/api/staff/bookings/late-check-in' @{ bookingCode = 'BK-TEST'; acknowledgeDepositForfeit = $false } $admin.data.token
$late = Read-Response $lateResponse
Assert-True ([int]$lateResponse.StatusCode -eq 400) 'Late check-in without forfeiture acknowledgement must be rejected'
Assert-True (-not $late.success) 'Late check-in rejection must return success=false'

Write-Output 'Backend business smoke tests passed.'
