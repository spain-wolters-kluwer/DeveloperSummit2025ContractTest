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
            ["User with full access"] = InsertUserWithFullAccess,
            ["User with read only access"] = InsertUserWithReadOnlyAccess,
            ["User with access"] = InsertUserWithAccess,
            ["User without access"] = InsertUserWithoutAccess,
            ["Not existing User"] = InsertNoUser
        };
    }

    private async Task InsertUserWithFullAccess(IDictionary<string, object> parameters)
    {
        var user = new User
        {
            Id = Guid.Parse("365d9ed3-eabf-465a-b48a-fdf32110501f"),
            Name = "FullAccessUser",
            Email = "FullAccessUser@user.com",
            Role = UserRoles.FullAccess
        };
        InsertUserIfNotExists(user);
        await Task.CompletedTask;
    }

    private async Task InsertUserWithReadOnlyAccess(IDictionary<string, object> parameters)
    {
        var user = new User
        {
            Id = Guid.Parse("62d7a17a-6273-4863-bc5f-2e096e81e749"),
            Name = "ReadOnlyUser",
            Email = "ReadOnlyUser@user.com",
            Role = UserRoles.ReadOnly
        };
        InsertUserIfNotExists(user);
        await Task.CompletedTask;
    }


    private async Task InsertUserWithAccess(IDictionary<string, object> parameters)
    {
        var user = new User
        {
            Id = Guid.Parse("757d4594-79b2-480c-8fc4-ddd7061c18cb"),
            Name = "PermitedUser",
            Email = "PermitedUser@user.com",
            Role = UserRoles.ReadOnly
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
            Role = UserRoles.NoAccess
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

