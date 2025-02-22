using DevSummit.Blog.Api.Domain.Clients;
using DevSummit.Blog.Api.Domain.Entities;

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

    public async Task<bool> HasAccess(string userName, string operation)
    {
        try
        {
            var userId = await usersClient.GetUserIdByName(userName);
            if (userId == Guid.Empty)
            {
                return false;
            }

            var user = await usersClient.GetUserById(userId);
            if (user.Role == UserRoles.FullAccess)
            {
                // Full access has access to all operations
                return true;
            }

            if (user.Role == UserRoles.ReadOnly && operation == "Read")
            {
                // ReadOnly has access to read operation
                return true;
            }

            // Other kind of combination operation and role has no access
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while checking access for user {UserName}", userName);
            return false;
        }
    }
}
