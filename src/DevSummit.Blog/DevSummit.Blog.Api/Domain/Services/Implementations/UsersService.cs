using DevSummit.Blog.Api.Domain.Clients;

namespace DevSummit.Blog.Api.Domain.Services;

public class UsersService : IUsersService
{
    private readonly IUsersClient usersClient;
    private readonly ILogger<UsersService> logger;


    public UsersService(ILogger<UsersService> logger, IUsersClient usersClient)
    {
        this.usersClient = usersClient;
        this.logger = logger;
    }

    public async Task<bool> HasAccess(string userName, string method)
    {
        try
        {
            var userId = await usersClient.GetUserIdByName(userName);
            if (userId == Guid.Empty)
            {
                return false;
            }

            var user = await usersClient.GetUserById(userId);
            if (user.Access == Entities.Permissions.None)
            {
                return false;
            }

            if (IsWriteMode(method) && user.Access == Entities.Permissions.Read)
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while checking access for user {UserName}", userName);
            return false;
        }
    }

    private static bool IsWriteMode(string method)
    {
        if (method == "post" || method == "put" || method == "delete")
        {
            return true;
        }
        return false;
    }
}
