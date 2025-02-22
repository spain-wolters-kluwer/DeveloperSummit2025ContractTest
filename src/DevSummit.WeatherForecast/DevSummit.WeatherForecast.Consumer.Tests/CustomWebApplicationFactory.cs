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
