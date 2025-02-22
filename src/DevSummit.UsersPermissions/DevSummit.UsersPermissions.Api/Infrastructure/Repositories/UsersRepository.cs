using DevSummit.UsersPermissions.Api.Domain.Entities;
using DevSummit.UsersPermissions.Api.Domain.Repositories;

namespace DevSummit.UsersPermissions.Api.Infrastructure.Repositories;
public class UsersRepository : IUsersRepository
{
    private readonly List<User> users;

    public UsersRepository()
    {
        users = new List<User>
        {
            new User
            {
                Id = Guid.Parse("cba1b9da-7664-4022-9267-1de95f456865"),
                Name = "John Doe",
                Email = "John.Doe@email.com",
                Role = UserRoles.FullAccess
            },
            new User
            {
                Id = Guid.Parse("ebf28d90-3c1e-4e0a-89a3-32e8f84dc703"),
                Name = "Jane Doe",
                Email = "Jane.Doe@email.com",
                Role = UserRoles.ReadOnly
            },
            new User
            {
                Id = Guid.Parse("f1b1b9da-7664-4022-9267-1de95f456865"),
                Name = "John Smith",
                Email = "Johm Smith@email.com",
                Role = UserRoles.NoAccess,
            }
        };
    }   

    public void Add(User user)
    {
        users.Add(user);        
    }

    public void Delete(Guid id)
    {
        var userToDelete = users.Find(u => u.Id == id);
        if (userToDelete != null)
        {
            users.Remove(userToDelete);
        }
    }

    public User? GetById(Guid id)
    {
        return users.Find(u => u.Id == id);
    }

    public IEnumerable<User> Get(string? name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            return users.Where(u => u.Name != null && u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        return users;
    }

    public void Update(User user)
    {
        var userToUpdate = users.Find(u => u.Id == user.Id);
        if (userToUpdate != null)
        {            
            userToUpdate.Email = user.Email;
            userToUpdate.Role = user.Role;         
        }        
    }
}
