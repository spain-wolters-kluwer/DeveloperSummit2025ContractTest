param (
    [string]$pactUrl,
    [string]$pacticipant,
    [string]$version,
    [string]$environment,
    [string]$username,
    [string]$password
)

Write-Output "Checking if $pacticipant version $version can be deployed to $environment"

$url = $pactUrl + "can-i-deploy?pacticipant=$pacticipant&version=$version&environment=$environment"

$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/hal+json, application/json, */*; q=0.01"
    "X-Interface" = "HAL Browser"
    "Authorization" = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${username}:${password}"))
}

$response = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
if ($response.summary.deployable -eq "true") {
    Write-Output "Success: "$response.summary.reason
    exit 0
} else {
    Write-Output "Fail: "$response.summary.reason
    exit 1
}


