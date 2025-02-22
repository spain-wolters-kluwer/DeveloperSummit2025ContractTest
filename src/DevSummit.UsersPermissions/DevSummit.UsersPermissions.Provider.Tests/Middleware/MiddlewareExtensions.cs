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