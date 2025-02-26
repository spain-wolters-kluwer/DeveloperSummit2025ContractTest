param (
    [string]$instance,
    [string]$project,
    [string]$buildDefinitionId,
    [string]$pactUrl,
    [string]$provider,
    [string]$consumer,
    [string]$username,
    [string]$password,
    [string]$pat
)

$pipelineUrl = "https://" + $instance + "/" + $project + "/_apis/pipelines/" + $buildDefinitionId + "/runs?api-version=6.0-preview.1"

Write-Output "Pipeline Url: " + $pipelineUrl

$body = @{
    request = @{
        method = "POST"
        url = $pipelineUrl
        
        headers = @{
            "Content-Type" = "application/json"
            "Accept" = "application/json"
            "Authorization" = "Basic  " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
        }
        body = @{
            resources = @{
                repositories = @{
                    self = @{
                        refName = "refs/heads/main"
                    }
                }
            }
            templateParameters = @{  }
        }
    }
}

$bodyJson = $body | ConvertTo-Json -Depth 10

$url = "https://" + $pactUrl + "/webhooks/provider/"+ $provider +"/consumer/" + $consumer
 
Write-Output "Pact Url: " + $url
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/hal+json, application/json, */*; q=0.01"
    "X-Interface" = "HAL Browser"
    "Authorization" = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${username}:${password}"))
}

$response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $bodyJson
$response