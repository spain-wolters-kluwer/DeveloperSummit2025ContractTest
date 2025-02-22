# Integrate in your CI pipelines

In the solution there are two examples that they explain how to integrate this tests in your pipelines.

## Github Actions

Example for **WeatherForecast** service:

```yaml
name: CI for WeatherForecast Service

on:
  push:
    branches: 
      - main
      - development
    paths:
      - 'src/DevSummit.WeatherForecast/**'
  pull_request:
    branches:
      - main
      - development
    paths:
      - 'src/DevSummit.WeatherForecast/**'

env:
  TestResultsPath: src/DevSummit.Blog/TestsResults
  PactBrokerUrl: ${{ secrets.PACT_BROKER_URL }}
  PactBrokerUserName: ${{ secrets.PACT_BROKER_USER }}
  PactBrokerPassword: ${{ secrets.PACT_BROKER_PWD }}
  WeatherForecastBranch: ${{ github.ref_name }}
  WeatherForecastAssemblyVersion: ${{ github.sha }}
  WeatherForecastPipelineUrl: ${{ github.event.pull_request.html_url }}
  WeatherForecastEnvironmentTag: development
  PactDir: src/DevSummit.Blog/pacts

jobs:
  build:

    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore src/DevSummit.sln

    - name: Build
      run: dotnet build  src/DevSummit.WeatherForecast/DevSummit.WeatherForecast.Consumer.Tests/DevSummit.WeatherForecast.Consumer.Tests.csproj --configuration Release --no-restore

    - name: Test
      run: dotnet test src/DevSummit.WeatherForecast/DevSummit.WeatherForecast.Consumer.Tests/DevSummit.WeatherForecast.Consumer.Tests.csproj --configuration Release --no-restore --no-build --verbosity normal --results-directory $TestResultsPath --logger trx --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
```

The complete workflow files are:

[Blog Service](../.github/workflows/devsummit.blog.CI.yml)
[WeatherForecast Service](../.github/workflows/devsummit.weatherforecast.CI.yml)
[UsersPermissions Service](../.github/workflows/devsummit.userspermissions.CI.yml)

## Azure Devops pipelines

Example for **WeatherForecast** service:
```yaml
trigger:
  branches:
    include:
      - main
  paths:
    include:
      - src/DevSummit.WeatherForecast/**
      
pool:
  vmImage: 'ubuntu-latest'

variables:
  BuildConfiguration: 'Release'
  BuildPlatform: 'any cpu'
  netCoreVersion: 'net8.0'
  pactDir: './Pacts'
  prefix: 'weatherforecast'
  projectName: 'DevSummit.WeatherForecast'

stages:
##################
##  Stage_Test
##################
- stage: 'Stage_Test'
  jobs:
  ##################
  ##  Job_Test
  ##################
  - job: Job_Test
    displayName: 'Job_Test'
    pool:
      vmImage: 'ubuntu-latest'
    variables:
      - name: WeatherForecastAssemblyVersion
        value: $(Build.SourceVersion)
      - name: WeatherForecastBranch
        value: $(Build.SourceBranchName)
      - name: WeatherForecastPipelineUrl
        value: $(System.CollectionUri)$(System.TeamProject)/_build/results?buildId=$(Build.BuildId)
      - name: WeatherForecastEnvironmentTag
        value: 'development'

    timeoutInMinutes: 120
    cancelTimeoutInMinutes: 1

    steps:   
    
    - task: DotNetCoreCLI@2
      displayName: 'dotnet restore'
      inputs:
        command: 'restore'
        projects: '**/src/$(projectName)/**/**.csproj'
        sdkVersion: '$(netCoreVersion)'
        noCache: true
        verbosityRestore : 'normal'

    - task: DotNetCoreCLI@2
      displayName: 'dotnet build'
      inputs:
        command: 'build'
        projects: '**/src/$(projectName)/**/**Tests.csproj'     
        sdkVersion: '$(netCoreVersion)'   
        arguments: '--configuration $(BuildConfiguration) --no-restore'      

    - task: DotNetCoreCLI@2
      displayName: '$(prefix) Consumer Contract test'
      inputs:
        command: 'test'
        projects: '**/src/$(projectName)/**/**Tests.csproj'
        sdkVersion: '$(netCoreVersion)'
        arguments: '-c $(BuildConfiguration) --no-build --no-restore --logger trx /p:CollectCoverage=true /p:CoverletOutputFormat=opencover'
        testRunTitle: '$(prefix) Consumer Contract Test'

```
The complete pipelines files are:

[Blog Service](../.pipelines/blog-ci.yml)
[WeatherForecast Service](../.pipelines/weatherforecast-ci.yml)
[UsersPermissions Service](../.pipelines/userspermissions-ci.yml)
