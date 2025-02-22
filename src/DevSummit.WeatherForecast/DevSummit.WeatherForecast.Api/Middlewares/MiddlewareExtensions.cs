using DevSummit.WeatherForecast.Api.Domain.Services;

namespace DevSummit.WeatherForecast.Api.Middlewares;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseUsernameHeaderMiddleware(this IApplicationBuilder builder)
    {
        var service = builder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IUsersService>();
        return builder.UseMiddleware<UsernameHeaderMiddleware>(service);
    }
}
