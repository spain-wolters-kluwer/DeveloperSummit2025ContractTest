# Consumer Contract Test

## WeatherForecast service
We implement the following use cases:

- Giving an user with access when he get for weather forecast then the API returns OK.
- Giving an user without access when he get for weather forecast then the API returns unauthorized.
- Giving an inexistent user when he get for weather forecast then the API returns unauthorized.

### Create Test project and common test project.

A class library for all common pact stuff: **DevSummit.Commons.Pact** and install the nuget packages: **PactNet** version 5.0.0 and **xunit.abstractions**  version 2.0.3.
Test Project in XUnit for **DevSummit.WeatherForecast.Consumer.Tests** and install the nuget package: **Microsoft.AspNetCore.Mvc.Testing** version 8.0.13.

### Implement PactService in Commons
The next step is create all initializations for Pact. 
In the folder **Pact** we write the class *PactService*

```csharp
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
```
And the logger for xunit in folder **Pact/Logger**
```csharp
namespace DevSummit.Commons.Pact.Logger;

using PactNet.Infrastructure.Outputters;
using Xunit.Abstractions;
public class XUnitLogger : IOutput
{
    private readonly ITestOutputHelper _output;
    public XUnitLogger(ITestOutputHelper output)
    {
        _output = output;
    }
    public void WriteLine(string line)
    {
        _output.WriteLine(line);
    }
}
```

### Implement the tests

1. **WebApplicationFactory** to create tests servers and access to the API of WeatherForecast Service

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DevSummit.WeatherForecast.Consumer.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
    }

    public HttpClient CreateClient(string urlPermissions)
    {
        Environment.SetEnvironmentVariable("USERS_PERMISSIONS_API_URL", urlPermissions);
        return CreateClient();
    }
}
```

And add the following line at the end of **Program** class in API project
```csharp
public partial class Program {}
```

2. **WeatherForecastApiTests** Create the tests class for the three use cases.
```csharp
using DevSummit.WeatherForecast.Api.Domain.Entities;
using DevSummit.Commons.Pact;
using PactNet;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace DevSummit.WeatherForecast.Consumer.Tests.WeatherForecastApi
{
    public class WeatherForecastApiTests : IClassFixture<PactService>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly IPactBuilderV4 _pactBuilder;

        private const string permitedUserName = "PermitedUser";
        private const string notPermitedUserName = "NotPermitedUser";
        private const string notExistingUserName = "NotExistingUser";

        public WeatherForecastApiTests(PactService service, ITestOutputHelper output)
        {
            _factory = new CustomWebApplicationFactory<Program>();
            _pactBuilder = service.CreatePactBuilder("WeatherForecast", "UsersPermissions", output);
        }

        [Fact]
        public async Task GivingUserWithAccess_WhenGetWeatherForecast_ThenReturnWeatherForecast()
        {
            _pactBuilder.ConfigureRequestPactBuilder(permitedUserName, "User with access");
            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Arrange
                var client = _factory.CreateClient(ctx.MockServerUri.ToString());
                client.DefaultRequestHeaders.Add("Username", permitedUserName);

                // Act
                var response = await client.GetAsync("weatherforecast");

                //Assert
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var temperatures = JsonSerializer.Deserialize<IEnumerable<Forecast>>(responseString);
                Assert.Equal(5, temperatures?.Count());
            });
        }

        [Fact]
        public async Task GivingNotExistingUser_WhenGetWeatherForecast_ThenReturnUnauthorized()
        {
            _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Arrange
                var client = _factory.CreateClient(ctx.MockServerUri.ToString());
                client.DefaultRequestHeaders.Add("Username", notExistingUserName);

                // Act
                var response = await client.GetAsync("weatherforecast");

                //Assert
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            });
        }

        [Fact]
        public async Task GivingUserWithoutAccess_WhenGetWeatherForecast_ThenReturnUnauthorized()
        {
            _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Arrange
                var client = _factory.CreateClient(ctx.MockServerUri.ToString());
                client.DefaultRequestHeaders.Add("Username", notPermitedUserName);

                // Act
                var response = await client.GetAsync("weatherforecast");

                //Assert
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            });
        }
    }
}
```

Now we create a class to implement the extension of the Pactbuilder where we configure all the request to **UserPersmissions** services.

```csharp
using DevSummit.WeatherForecast.Api.Domain.Entities;
using PactNet;
using PactNet.Matchers;
using System.Net;

namespace DevSummit.WeatherForecast.Consumer.Tests.WeatherForecastApi;
internal static class WeatherForecastData
{
    private static User[] users = [
        new User
        {
            Id = Guid.Parse("757d4594-79b2-480c-8fc4-ddd7061c18cb"),
            Name = "PermitedUser",
            Email = "PermitedUser@user.com",
            Access = true
        },
        new User
        {
            Id = Guid.Parse("2a69e26a-d392-41b1-82e6-68b3e4a869fb"),
            Name = "NotPermitedUser",
            Email = "NotPermitedUser@user.com",
            Access = false
        }
    ];

    public static void ConfigureRequestPactBuilder(this IPactBuilderV4 pactBuilder, string userName, string giving)
    {
        var mailRegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        pactBuilder
            .UponReceiving("Get Users filtered by name")
                .Given(giving)
                .WithRequest(HttpMethod.Get, "/api/users")
                .WithQuery("name", Match.Equality(userName))
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithJsonBody(users.Where(u => u.Name == userName).Select(u => new { id = u.Id, name = Match.Type(u.Name), email = Match.Regex(u.Email, mailRegexPattern) }));

        var user = users.FirstOrDefault(u => u.Name == userName);
        if (user != null)
        {
            pactBuilder
                .UponReceiving("Get User details by user id")
                    .Given(giving)
                    .WithRequest(HttpMethod.Get, $"/api/users/{user.Id}")
                .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithJsonBody(new { name = Match.Type(user.Name), email = Match.Regex(user.Email, mailRegexPattern), hasAccess = user.Access });
        }
    }
}
```

## Blog service

Implement the following use cases:

- Giving an user with access when he get all the articles then the API returns OK.
- Giving an user without access when he get all the articles then the API returns unauthorized.
- Giving an inexistent user when he get all the articles then the API returns unauthorized.



