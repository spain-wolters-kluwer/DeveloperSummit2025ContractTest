using DevSummit.Blog.Consumer.Tests.Pact.Broker;
using DevSummit.Commons.Pact.Logger;
using DevSummit.Commons.Pact.Pact.Broker;
using Microsoft.Extensions.Configuration;
using PactNet;
using PactNet.Infrastructure.Outputters;
using System.Text.Json;
using Xunit.Abstractions;

namespace DevSummit.Commons.Pact;
public class PactService : IDisposable
{
    private readonly PactConfig pactConfig;

    private readonly IConfiguration configuration;

    private string? consumer;
    private string? provider;

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

    public IPactBuilderV4 CreatePactBuilder(string consumer, string provider, ITestOutputHelper output)
    {
        this.consumer = consumer;
        this.provider = provider;

        pactConfig.Outputters = new List<IOutput> { new XUnitLogger(output) };
        var pact = PactNet.Pact.V4(consumer, provider, pactConfig);
        return pact.WithHttpInteractions();
    }

    public async Task PublishContracts()
    {
        var pactflowContract = new PactBrokerContractBuilder(configuration)
                .WithConsumer(consumer)
                .WithTags(new string[] { configuration[$"{consumer}EnvironmentTag"] ?? string.Empty })
                .WithProvider(provider)
                .Build();

        await new PactBrokerClient(configuration).PublishPactContract(pactflowContract);
    }

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
}

