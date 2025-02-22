# Provider Contract Test

## Implement Provider Contract Test in UserPermissions Service vs WeatherForecast Service
We use the contract test file saved in **./pacts** to test whether **UserPermissions** implements the **Contract** with the consumer service.
To do so we need to execute the test in the real API, not executed by a Test Server or similar process.

1. Extract the Host configuration of the API.

* Create **HostConfiguration** class
```csharp
using DevSummit.UsersPermissions.Api.Domain.Services;
using DevSummit.UsersPermissions.Api.Domain.Repositories;
using DevSummit.UsersPermissions.Api.Infrastructure.Repositories;

namespace DevSummit.UsersPermissions.Api;
public static class HostConfiguration
{
    public static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddSingleton<IUsersRepository, UsersRepository>();
        builder.Services.AddScoped<IUsersService, UsersService>();

        var assembly = typeof(HostConfiguration).Assembly;
        builder.Services.AddControllers().AddApplicationPart(assembly);        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    public static void ConfigureApp(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthorization();

        app.MapControllers();
    }
}

```
* And modify **Program** class as example:
```csharp
using DevSummit.UsersPermissions.Api;

var builder = WebApplication.CreateBuilder(args);

HostConfiguration.ConfigureBuilder(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
HostConfiguration.ConfigureApp(app);

app.Run();
```

2. Create Test Project in XUnit for **DevSummit.WeatherForecast.Consumer.Tests**

3. Create **TestServerFixture**
```csharp
using DevSummit.UsersPermissions.Api;
using Microsoft.AspNetCore.Builder;

namespace DevSummit.UsersPermissions.Provider.Tests;

public class TestServerFixture : IDisposable
{
    private readonly WebApplication _app;
    public WebApplication App => _app;
    private const string url = "http://localhost:9223";
    public string Url => url;

    public TestServerFixture()
    {
        var builder = WebApplication.CreateBuilder();

        HostConfiguration.ConfigureBuilder(builder);

        _app = builder.Build();
        HostConfiguration.ConfigureApp(_app);
        _app.Urls.Add(Url);
        _app.StartAsync().GetAwaiter().GetResult();
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
            _app.StopAsync().GetAwaiter().GetResult();
            _app.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}
```

And we configure it as Fixture Collection
```csharp
namespace DevSummit.UsersPermissions.Provider.Tests;

[CollectionDefinition("TestServer collection")]
public class TestServerFixtureCollection : ICollectionFixture<TestServerFixture>
{
}
```

4. Create the tests in the class **WeatherForecastTests**
```csharp
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
            .Verify();
    }           
}
```

5. Finally only has to create the data in **UsersPermissions** in order to pass the tests. This it is done in a middleware that receives the test scenario name and create the data that need this scenario.

* Create a **ProviderState** class to store the state of the provider
```csharp
namespace DevSummit.UsersPermissions.Provider.Tests.Middleware;

public record ProviderState(string State, IDictionary<string, object> Params);
```
* Create a **ProviderStateMiddleware** where the users are created depending the *Giving* scenario
```csharp
using DevSummit.UsersPermissions.Api.Domain.Entities;
using DevSummit.UsersPermissions.Api.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace DevSummit.UsersPermissions.Provider.Tests.Middleware;

public class ProviderStateMiddleware
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly RequestDelegate _next;

    private readonly IDictionary<string, Func<IDictionary<string, object>, Task>> _providerStates;

    private readonly IUsersRepository _usersRepository;

    public ProviderStateMiddleware(RequestDelegate next, IUsersRepository usersRepository)
    {
        _next = next;
        _usersRepository = usersRepository;
        _providerStates = new Dictionary<string, Func<IDictionary<string, object>, Task>>
        {

            ["User with access"] = InsertUserWithAccess,
            ["User without access"] = InsertUserWithoutAccess,
            ["Not existing User"] = InsertNoUser
        };
    }

    private async Task InsertUserWithAccess(IDictionary<string, object> parameters)
    {
        var user = new User
        {
            Id = Guid.Parse("757d4594-79b2-480c-8fc4-ddd7061c18cb"),
            Name = "PermitedUser",
            Email = "PermitedUser@user.com",
            HasAccess = true
        };
        InsertUserIfNotExists(user);
        await Task.CompletedTask;
    }
        
    private async Task InsertUserWithoutAccess(IDictionary<string, object> parameters)
    {
        var user = new User
        {
            Id = Guid.Parse("2a69e26a-d392-41b1-82e6-68b3e4a869fb"),
            Name = "NotPermitedUser",
            Email = "NotPermitedUser@user.com",
            HasAccess = false
        };
        InsertUserIfNotExists(user);
        await Task.CompletedTask;
    }

    private void InsertUserIfNotExists(User user)
    {
        var userExists = _usersRepository.GetById(user.Id);
        if (userExists == null)
        {
            _usersRepository.Add(user);
        }
    }

    private async Task InsertNoUser(IDictionary<string, object> parameters)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handle the request
    /// </summary>
    /// <param name="context">Request context</param>
    /// <returns>Awaitable</returns>
    public async Task InvokeAsync(HttpContext context)
    {

        if (!(context.Request.Path.Value?.StartsWith("/provider-states") ?? false))
        {
            await this._next.Invoke(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status200OK;


        if (context.Request.Method == HttpMethod.Post.ToString().ToUpper())
        {
            string jsonRequestBody;

            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
            {
                jsonRequestBody = await reader.ReadToEndAsync();
            }

            try
            {

                ProviderState providerState = JsonSerializer.Deserialize<ProviderState>(jsonRequestBody, Options);

                if (!string.IsNullOrEmpty(providerState?.State))
                {
                    await this._providerStates[providerState.State].Invoke(providerState.Params);
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Failed to deserialise JSON provider state body:");
                await context.Response.WriteAsync(jsonRequestBody);
                await context.Response.WriteAsync(string.Empty);
                await context.Response.WriteAsync(e.ToString());
            }
        }
    }
}

```
* Create a **MiddlewareExtensions** to configure the middleware.
```csharp
using DevSummit.UsersPermissions.Api.Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DevSummit.UsersPermissions.Provider.Tests.Middleware;
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseProviderStateMiddleware(this IApplicationBuilder builder)
    {
        var service = builder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IUsersRepository>();
        return builder.UseMiddleware<ProviderStateMiddleware>(service);
    }
}
```
* Edit the **TestServerFixture** to set the middleware
```csharp
public TestServerFixture()
{
    var builder = WebApplication.CreateBuilder();

    HostConfiguration.ConfigureBuilder(builder);

    _app = builder.Build();
    HostConfiguration.ConfigureApp(_app);
    _app.Urls.Add(Url);
    _app.UseProviderStateMiddleware();
    _app.StartAsync().GetAwaiter().GetResult();
}
```
* Edit the **WeatherForescastTests** class to configure the Pact Engine with the provider Middleware.
```csharp
        using var pactVerifier = new PactVerifier("UsersPermissions", _pactConfig);
        pactVerifier.WithHttpEndpoint(new Uri(_fixture.Url))
            .WithFileSource(new FileInfo(pactPath))
            .WithProviderStateUrl(new Uri(_fixture.Url + "/provider-states"))
            .Verify();
```

6. Fix error in **UsersPermissions.Repository***
```csharp
public IEnumerable<User> Get(string? name)
{
    if (!string.IsNullOrEmpty(name))
    {
        return users.Where(u => u.Name != null && u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    return users;
}
```


