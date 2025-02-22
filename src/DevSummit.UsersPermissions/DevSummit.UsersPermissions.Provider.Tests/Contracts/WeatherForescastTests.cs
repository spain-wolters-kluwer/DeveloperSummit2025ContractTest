using DevSummit.Commons.Pact.Logger;
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace DevSummit.UsersPermissions.Provider.Tests.Contracts;

[Collection("TestServer collection")]
public class WeatherForescastTests
{
    private readonly TestServerFixture _fixture;
    private readonly PactVerifierConfig _pactConfig;
    private const string pactPath = "../../../../../../pacts/WeatherForecast-UsersPermissions.json";
    public WeatherForescastTests(TestServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _pactConfig = new PactVerifierConfig
        {
            Outputters = new List<IOutput>
            {
                new XUnitLogger(output)
            },
            LogLevel = PactLogLevel.Information
        };
    }

    [Fact]
    public void EnsureUsersPermissionsApiHonoursWithWeatherForecast()
    {
        using var pactVerifier = new PactVerifier("UsersPermissions", _pactConfig);
        pactVerifier.WithHttpEndpoint(new Uri(_fixture.Url))
            .WithFileSource(new FileInfo(pactPath))
            .WithProviderStateUrl(new Uri(_fixture.Url + "/provider-states"))
            .Verify();
    }

}
