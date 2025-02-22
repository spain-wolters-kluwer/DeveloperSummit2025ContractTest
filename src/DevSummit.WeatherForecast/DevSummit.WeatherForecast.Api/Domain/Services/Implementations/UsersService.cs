using DevSummit.WeatherForecast.Api.Domain.Clients;

namespace DevSummit.WeatherForecast.Api.Domain.Services;

public class UsersService : IUsersService
{
    private readonly IUsersClient usersClient;
    private readonly ILogger<UsersService> logger;


    public UsersService(ILogger<UsersService> logger, IUsersClient usersClient)
    {
        this.usersClient = usersClient;
        this.logger = logger;
    }

    public async Task<bool> HasAccess(string userName)
    {
        try
        {
            var userId = await usersClient.GetUserIdByName(userName);
            if (userId == Guid.Empty)
            {
                return false;
            }

            var user = await usersClient.GetUserById(userId);
            return user.Access;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while checking access for user {UserName}", userName);
            return false;
        }
    }
}
