param (
    [string]$pactUrl,
    [string]$name,
    [string]$title,
    [string]$username,
    [string]$password   
)

$body = @{
    
    "name" = $name
    "displayName" = $title
    "production" = $false
}

$bodyJson = $body | ConvertTo-Json -Depth 10

$url = "https://" + $pactUrl + "/environments"

$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/hal+json, application/json, */*; q=0.01"
    "X-Interface" = "HAL Browser"
    "Authorization" = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${username}:${password}"))
}

$response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $bodyJson
$response