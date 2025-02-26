param (
    [string]$pactUrl,
    [string]$pacticipant,
    [string]$version,
    [string]$environment,
    [string]$username,
    [string]$password   
)

Write-Output "Checking if $pacticipant version $version can be deployed to $environment"
Write-Output "Url to check: $pactUrl"
Write-Output "Username: $username"
Write-Output "Password: $password"