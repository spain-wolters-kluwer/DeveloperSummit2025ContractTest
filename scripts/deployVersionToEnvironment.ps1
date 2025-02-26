param (
    [string]$pactUrl,
    [string]$pacticipant,
    [string]$version,
    [string]$environment,
    [string]$username,
    [string]$password   
)


$url = $pactUrl + "environments"

$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/hal+json, application/json, */*; q=0.01"
    "X-Interface" = "HAL Browser"
    "Authorization" = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${username}:${password}"))
}

$responseEnv = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
$environmentUuid = $responseEnv._embedded.environments | Where-Object { $_.name -eq $environment } | Select-Object -ExpandProperty uuid
Write-Output "Environment UUID: $environmentUuid"

$url = $pactUrl + "pacticipants/" + $pacticipant + "/versions/" + $version + "/deployed-versions/environment/" + $environmentUuid
$response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers
$response