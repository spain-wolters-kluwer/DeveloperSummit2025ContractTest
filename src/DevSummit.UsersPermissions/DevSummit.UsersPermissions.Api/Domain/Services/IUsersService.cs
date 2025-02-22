
using DevSummit.UsersPermissions.Api.Domain.Entities;

namespace DevSummit.UsersPermissions.Api.Domain.Services;
public interface IUsersService
{
    ValidationResult AddUser(User user);
    ValidationResult DeleteUser(Guid id);
    ValidationResult UpdateUser(User user);
}