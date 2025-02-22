using DevSummit.Commons.Pact.Logger;
using PactNet;
using PactNet.Infrastructure.Outputters;
using System.Text.Json;
using Xunit.Abstractions;

namespace DevSummit.Commons.Pact;
public class PactService
{
    private readonly PactConfig _pactConfig;

    public PactService()
    {
        _pactConfig = new PactConfig()
        {
            DefaultJsonSettings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            },
            PactDir = "../../../../../../pacts",
            LogLevel = PactLogLevel.Information,
        };
    }

    public IPactBuilderV4 CreatePactBuilder(string consumer, string provider, ITestOutputHelper output)
    {
        _pactConfig.Outputters = new List<IOutput> { new XUnitLogger(output) };
        var pact = PactNet.Pact.V4(consumer, provider, _pactConfig);
        return pact.WithHttpInteractions();
    }
}

