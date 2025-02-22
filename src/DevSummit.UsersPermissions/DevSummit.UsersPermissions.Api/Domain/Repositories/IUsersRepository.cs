using DevSummit.UsersPermissions.Api.Domain.Entities;
namespace DevSummit.UsersPermissions.Api.Domain.Repositories;
public interface IUsersRepository
{
    User? GetById(Guid id);
    IEnumerable<User> Get(string? name);
    void Add(User user);
    void Update(User user);
    void Delete(Guid id);
}