using DevSummit.Blog.Api.Domain.Services;

namespace DevSummit.Blog.Api.Middlewares;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseUsernameHeaderMiddleware(this IApplicationBuilder builder)
    {
        var service = builder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IUsersService>();
        return builder.UseMiddleware<UsernameHeaderMiddleware>(service);
    }
}
