# Complete CI and CD pipelines

## Complete CI
To prevent merging breaking contract changes in Consumer, you have to do few changes to your pipeline:
1. Configure a **webhook** in the Pact Broker to call a pipeline in **Github** or **Azure Pipelines** when there are changes in the contract file:

```shell
./scripts/github/createWebHook.ps1 [organization] [repository] [pactBrokerUrl] [provider] [consumer] [pactBrokerUser] [pactBrokerPassword]
```
**Notes:**
>* [Script for GitHub Actions](../scripts/github/createWebHook.ps1)
>* [Script for Azure Devops Pipelines](../scripts/azuredevops/createWebHook.ps1)

2. Add a step to check wheather the provider is compatible with the changes of the contract.

```yaml
    - name: Check if PR can be completed      
      run: pwsh scripts/canIcompletePR.ps1 "$Env:PactBrokerUrl" Blog "$Env:Version" "$Env:EnvironmentTag" "$Env:PactBrokerUserName" "$Env:PactBrokerPassword"
      shell: pwsh
```
**Note:** [Can I Complete PR Script](../scripts/canIcompletePR.ps1)

## Complete CD
To prevent deploy incompatible versions between Consumers and Providers, you can use the **CanIDeploy** tool. This tool checks the if the version you want to deploy in the environment is compatible with the already installed services.

1. Create the **[Environment](../scripts/createEnvironment.ps1)** in the Pact Broker 

```shell
./scripts/createEnvironment.ps1 [pactBrokerUrl] [environmentName] [environmentDisplayName] [pactBrokerUser] [pactBrokerPassword]
```

2. Complete the CD pipeline asking first **CanIDeploy** and when the enviroment is deployed, set the environment in the PactBroker app.

```yaml
    - name: Check if I can deploy to Development
      run: pwsh scripts/canIdeploy.ps1 ${{ secrets.PACT_BROKER_URL }} ${{ env.Pacticipant }} ${{ env.Version }} development ${{ secrets.PACT_BROKER_USER }} ${{ secrets.PACT_BROKER_PWD }}
      shell: pwsh
    
    ...

    - name: Set Deploy to Development
      run: pwsh scripts/deployVersionToEnvironment.ps1 ${{ secrets.PACT_BROKER_URL }} ${{ env.Pacticipant }} ${{ env.Version }} development ${{ secrets.PACT_BROKER_USER }} ${{ secrets.PACT_BROKER_PWD }}
      shell: pwsh
```

**Notes:**
>* [Can I Deploy Script](../scripts/canIdeploy.ps1)
>* [Set environment Script](../scripts/deployVersionToEnvironment.ps1)



