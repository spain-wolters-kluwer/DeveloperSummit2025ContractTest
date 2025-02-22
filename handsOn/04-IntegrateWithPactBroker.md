# Integration With the PactBroker
In this section we integrate the pact files generated with a local PactBroker as contract repository.

## Execute PactBroker
To execute the PactBroker in a docker.

```shell
docker compose -f ./DockerTools/PactBroker-Docker-Compose.yml up -d
```

Pact broker is hosted in (http://localhost:9292)

## Integration for Consumer Tests

In case the tests have passed, what we need to do is upload the contract file to the PactBroker, specifying the service version.

1. **PactFileHandler** class to manage the pact file.
```csharp
using System.Text;

namespace DevSummit.Blog.Consumer.Tests.Pact.Broker;

internal static class PactFileHandler
{
    public static void CleanPactDirectory(string? pactDir)
    {
        if (string.IsNullOrEmpty(pactDir))
        {
            return;
        }

        var directory = new DirectoryInfo(pactDir);
        if (!directory.Exists)
        {
            return;
        }

        foreach (var file in directory.GetFiles())
        {
            file.Delete();
        }
    }

    public static string GetContentFromPactFile(string pactDir, string provider, string consumer)
    {
        var fileName = GetPactPathFileName(pactDir, provider, consumer);
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException($"Pact File {fileName} doesn't exist.");
        }
        return File.ReadAllText(fileName, Encoding.UTF8);
    }

    private static string GetPactPathFileName(string pactDir, string provider, string consumer)
    {
        return Path.Combine(pactDir ?? string.Empty, $"{consumer}-{provider}.json");
    }
}
```

2. **PactBrokerClient**, this is the client to upload the contract
```csharp
using DevSummit.Commons.Pact.Pact.Broker.Entities;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DevSummit.Blog.Consumer.Tests.Pact.Broker;

internal class PactBrokerClient
{
    private readonly HttpClient httpClient;

    private const string publishContractResource = "/contracts/publish";

    public PactBrokerClient(IConfiguration configuration)
    {
        httpClient = new HttpClient()
        {
            BaseAddress = new Uri(configuration["PactBrokerUrl"])
        };
        httpClient.DefaultRequestHeaders.Add("Accept", "application/hal+json, application/json, */*; q=0.01");
        httpClient.DefaultRequestHeaders.Add("X-Interface", "HAL Browser");

        if (!string.IsNullOrEmpty(configuration["PactBrokerUserName"]))
        {
            var authenticationString = $"{configuration["PactBrokerUserName"]}:{configuration["PactBrokerPassword"]}";
            var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
        }
    }

    public async Task PublishPactContract(PactBrokerPublishContract contract)
    {            
        var request = new HttpRequestMessage(HttpMethod.Post, publishContractResource)
        {
            Content = new StringContent(JsonSerializer.Serialize(contract, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), Encoding.UTF8, "application/json")
        };
                    
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
```

And the entity **PactBrokerPublishContract** definition
```csharp
namespace DevSummit.Commons.Pact.Pact.Broker.Entities;
public class PactBrokerPublishContract
{
    public string? PacticipantName { get; set; }
    public string? PacticipantVersionNumber { get; set; }
    public string? Branch { get; set; }
    public string[]? Tags { get; set; }
    public string? BuildUrl { get; set; }
    public List<PactBrokerContract>? Contracts { get; set; }

}

public class PactBrokerContract
{
    public string? ConsumerName { get; set; }
    public string? ProviderName { get; set; }
    public string? Specification { get; set; }
    public string? ContentType { get; set; }
    public string? Content { get; set; }
}
```

And add the following nuget package: **Microsoft.Extensions.Configuration.EnvironmentVariables**

3. Create an builder for **PactBrokerPublishContract**

```csharp
using DevSummit.Blog.Consumer.Tests.Pact.Broker;
using DevSummit.Commons.Pact.Pact.Broker.Entities;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace DevSummit.Commons.Pact.Pact.Broker;

internal class PactBrokerContractBuilder
{
    private readonly IConfiguration configuration;
    private string? provider;
    private string? consumer;
    private string[] tags = Array.Empty<string>();

    public PactBrokerContractBuilder(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public PactBrokerPublishContract Build()
    {
        return new PactBrokerPublishContract()
        {
            PacticipantName = consumer,
            Contracts = [ CreatePactContract(provider) ],
            Tags = tags.Length == 0 ? null : tags,
            Branch = configuration[$"{consumer}Branch"],
            PacticipantVersionNumber = configuration[$"{consumer}AssemblyVersion"],
            BuildUrl = configuration[$"{consumer}PipelineUrl"]
        };
    }

    private PactBrokerContract CreatePactContract(string provider)
    {
        return new PactBrokerContract()
        {
            ConsumerName = consumer,
            ProviderName = provider,
            ContentType = "application/json",
            Specification = "pact",
            Content = GetContentInBase64(provider)
        };
    }

    private string GetContentInBase64(string provider)
    {
        var pactContent = PactFileHandler.GetContentFromPactFile(configuration["PactDir"], provider, consumer);
        byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(pactContent);
        return Convert.ToBase64String(toEncodeAsBytes);
    }

    public PactBrokerContractBuilder WithConsumer(string? consumer)
    {
        this.consumer = consumer;
        return this;
    }

    public PactBrokerContractBuilder WithTags(string[] tags)
    {
        this.tags = tags;
        return this;
    }

    public PactBrokerContractBuilder WithProvider(string? providers)
    {
        this.provider = provider;
        return this;
    }
}
```

4. And finally complete the class **PactService**

* Add private fields
```csharp
private readonly IConfiguration configuration;

private string? consumer;
private string? provider;
```

* Then load the configuration and clean the working folder in constructor
```csharp
public PactService()
{
    configuration = new ConfigurationBuilder()
       .AddEnvironmentVariables()
       .Build();

    pactConfig = new PactConfig()
    {
        DefaultJsonSettings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        },
        PactDir = configuration["PactDir"],
        LogLevel = PactLogLevel.Information,                
    };

    PactFileHandler.CleanPactDirectory(configuration["PactDir"]); 
}
```
* Register the consumer and provider in the method **CreatePactBuilder**
```csharp
this.consumer = consumer;
this.provider = provider;
```

* Add a method to publish the Pact file using the **PactBrokerClient**
```csharp
    public async Task PublishContracts()
    {
        var pactflowContract = new PactBrokerContractBuilder(configuration)
                .WithConsumer(consumer)
                .WithTags(new string[] { configuration[$"{consumer}EnvironmentTag"] ?? string.Empty })
                .WithProvider(provider)
                .Build();

        await new PactBrokerClient(configuration).PublishPactContract(pactflowContract);
    }
```

* At last make the class **IDisposable** and publish the contract in the dispose method.
```csharp
 public void Dispose()
 {
     Dispose(true);
     GC.SuppressFinalize(this);
 }

 protected virtual void Dispose(bool disposing)
 {
     if (disposing)
     {
         PublishContracts().Wait();
     }
 }
```

5. The last thing to do it is to configure the environment variables. For sample in a **pactconfig.runsettings** file.
```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
    <RunConfiguration>
        <EnvironmentVariables>
			<WeatherForecastBranch>development</WeatherForecastBranch>
			<WeatherForecastAssemblyVersion>3.0.0-rev.5</WeatherForecastAssemblyVersion>
			<WeatherForecastPipelineUrl>https://mypipelineurl.com</WeatherForecastPipelineUrl>
		    <WeatherForecastEnvironmentTag>development</WeatherForecastEnvironmentTag>
			<UsersPermissionsBranch>development</UsersPermissionsBranch>
			<UsersPermissionsAssemblyVersion>2.2.0-rev.9</UsersPermissionsAssemblyVersion>
			<UsersPermissionsPipelineUrl>https://mypipelineurl.com</UsersPermissionsPipelineUrl>
			<UsersPermissionsEnvironmentTag>development</UsersPermissionsEnvironmentTag>
			<BlogBranch>development</BlogBranch>
			<BlogAssemblyVersion>1.1.0-rev.2</BlogAssemblyVersion>
			<BlogPipelineUrl>https://mypipelineurl.com</BlogPipelineUrl>
			<BlogEnvironmentTag>development</BlogEnvironmentTag>
			<PactDir>../../../../../../pacts</PactDir>
			<PactBrokerUrl>http://localhost:9292/</PactBrokerUrl>
			<PactBrokerUser></PactBrokerUser>
			<PactBrokerPassword></PactBrokerPassword>
		</EnvironmentVariables>
    </RunConfiguration>
</RunSettings>
```

## Integration for provider tests
In this case, has to configure the **PactVerifier** to download the contract files to test.

1. Create a extension method in the common test project for **PactVerifier**
```csharp
using Microsoft.Extensions.Configuration;
using PactNet.Verifier;

namespace DevSummit.Commons.Pact.Pact.Extensions;
public static class PactVerifierExtensions
{
    public static IPactVerifierSource WithPactFromConfiguration(this IPactVerifier pactVerifier, string providerName, string consumerName, IConfiguration configuration)
    {
        return pactVerifier.WithPactBrokerSource(new Uri(configuration["PactBrokerUrl"]),
            options =>
            {

                // Authentication
                if (!string.IsNullOrEmpty(configuration["PactBrokerUserName"]))
                    options.BasicAuthentication(configuration["PactBrokerUserName"], configuration["PactBrokerPassword"]);

                options.PublishResults(configuration[$"{providerName}AssemblyVersion"], (options) =>
                {
                    if (!string.IsNullOrEmpty(configuration[$"{providerName}PipelineUrl"]))
                        options.BuildUri(new Uri(configuration[$"{providerName}PipelineUrl"]));
                    options.ProviderBranch(configuration[$"{providerName}Branch"]);
                    options.ProviderTags([configuration[$"{providerName}EnvironmentTag"]]);
                });

                options.ConsumerVersionSelectors(
                    new ConsumerVersionSelector { Consumer = consumerName, Latest = true },
                    new ConsumerVersionSelector { Consumer = consumerName, Deployed = true }
                );
            }
        );
    }
}
```

2. Load the configuration in the **TestServerFixture**
* Add a public readonly property
```csharp
public IConfiguration Configuration { get; private set; }
```

* And load the configuration in the constructor
```csharp
        Configuration = new ConfigurationBuilder().
            AddEnvironmentVariables().
            Build();
```

3. And substitute the call **WithFileSource(new FileInfo(pactPath))** by the new extension method (**WithPactFromConfiguration(providerName, consumerName, fixture.Configuration)**) in the tests classes.

