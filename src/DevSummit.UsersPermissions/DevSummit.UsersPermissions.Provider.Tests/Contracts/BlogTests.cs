using DevSummit.Commons.Pact.Logger;
using DevSummit.Commons.Pact.Pact.Extensions;
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace DevSummit.UsersPermissions.Provider.Tests.Contracts;

[Collection("TestServer collection")]
public class BlogTests
{
    private readonly TestServerFixture fixture;
    private readonly PactVerifierConfig pactConfig;
    public BlogTests(TestServerFixture fixture, ITestOutputHelper output)
    {
       this.fixture = fixture;
        pactConfig = new PactVerifierConfig
        {
            Outputters = new List<IOutput>
            {
                new XUnitLogger(output)
            },
            LogLevel = PactLogLevel.Information
        };
    }

    [Fact]
    public void EnsureUsersPermissionsApiHonoursWithBlog()
    {
        using var pactVerifier = new PactVerifier("UsersPermissions", pactConfig);
        pactVerifier.WithHttpEndpoint(new Uri(fixture.Url))
            .WithPactFromConfiguration("UsersPermissions", "Blog", fixture.Configuration)
            .WithProviderStateUrl(new Uri(fixture.Url + "/provider-states"))
            .Verify();
    }
}
