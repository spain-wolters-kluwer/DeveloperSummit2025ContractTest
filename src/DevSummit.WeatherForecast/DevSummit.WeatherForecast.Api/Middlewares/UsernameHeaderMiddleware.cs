using DevSummit.WeatherForecast.Api.Domain.Services;

namespace DevSummit.WeatherForecast.Api.Middlewares;

public class UsernameHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IUsersService _service;

    public UsernameHeaderMiddleware(RequestDelegate next, IUsersService service)
    {
        _next = next;
        _service = service;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("Username"))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Username header is missing.");
            return;
        }

        var username = context.Request.Headers["Username"].ToString();
        if (!await _service.HasAccess(username))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Access denied.");
            return;
        }

        await _next(context);
    }
}
