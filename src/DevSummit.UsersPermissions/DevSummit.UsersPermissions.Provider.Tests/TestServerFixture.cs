using DevSummit.UsersPermissions.Api;
using DevSummit.UsersPermissions.Provider.Tests.Middleware;
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
        _app.UseProviderStateMiddleware();
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