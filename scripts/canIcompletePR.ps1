param (
    [string]$pactUrl,
    [string]$consumer,
    [string]$version,
    [string]$tag,
    [string]$username,
    [string]$password,
    [int]$timeout=300,
    [int]$interval=5
)

$url = "$pactUrl/can-i-deploy?pacticipant=$consumer&version=$version&to=$tag"

$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/hal+json, application/json, */*; q=0.01"
    "X-Interface" = "HAL Browser"
    "Authorization" = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${username}:${password}"))
}

$elapsed = 0

do {
    $response = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
    Start-Sleep -Seconds $interval
    $elapsed += $interval

    if ($elapsed -ge $timeout) {
        Write-Output "Timeout reached: "$response.summary.reason
        exit 1
    }    
} while ($response.summary.deployable -eq $null)

if ($response.summary.deployable -eq "true") {
    Write-Output "Success: "$response.summary.reason
    exit 0
} else {
    Write-Output "Fail: "$response.summary.reason
    exit 1
}