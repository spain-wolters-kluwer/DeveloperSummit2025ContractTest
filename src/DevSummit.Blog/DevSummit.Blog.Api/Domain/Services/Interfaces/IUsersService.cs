namespace DevSummit.Blog.Api.Domain.Services;

public interface IUsersService
{
    public Task<bool> HasAccess(string userName, string operation);
}
