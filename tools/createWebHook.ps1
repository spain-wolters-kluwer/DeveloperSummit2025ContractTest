param (
    [string]$instance,
    [string]$project,
    [string]$pactUrl,
    [string]$provider,
    [string]$consumer,
    [string]$username,
    [string]$password,
    [string]$pat
)

Write-Output "Pipeline Url: " + $pipelineUrl

$pipelineUrl = "https://api.github.com/repos/$instance/$project/dispatches"
Write-Output "Pipeline Url: " + $pipelineUrl


$body = @{
    events= @(
        @{
            name= "contract_requiring_verification_published"
        }
    )    
    request= @{
      method= "POST"
      url= $pipelineUrl
      headers= @{
        "Content-Type"= "application/json"
        "Accept"= "application/vnd.github.everest-preview+json"
        "Authorization"= "Bearer $pat"
      }
      body= @{
        "event_type"= "contract_requiring_verification_published"
        "client_payload"= @{
          "pact_url"= '${pactbroker.pactUrl}'
          "sha"= '${pactbroker.providerVersionNumber}'
          "branch"= '${pactbroker.providerVersionBranch}'
          "message"= 'Verify changed pact for ${pactbroker.consumerName} version ${pactbroker.consumerVersionNumber} branch ${pactbroker.consumerVersionBranch} by ${pactbroker.providerVersionNumber} (${pactbroker.providerVersionDescriptions})'
        }
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