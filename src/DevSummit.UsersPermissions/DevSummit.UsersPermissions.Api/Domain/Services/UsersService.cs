using DevSummit.UsersPermissions.Api.Domain.Entities;
using DevSummit.UsersPermissions.Api.Domain.Repositories;

namespace DevSummit.UsersPermissions.Api.Domain.Services;

public class UsersService : IUsersService
{
    private readonly IUsersRepository repository;

    public UsersService(IUsersRepository repository)
    {
        this.repository = repository;
    }

    public ValidationResult AddUser(User user)
    {
        user.Id = Guid.NewGuid();
        var validationResult = ValidateAddUser(user);
        if (validationResult.IsValid)
        {
            repository.Add(user);
            validationResult.Message = user.Id.ToString();
        }
        return validationResult;        
    }

    public ValidationResult UpdateUser(User user)
    {
        var validationResult = ValidateUpdateUser(user);
        if (validationResult.IsValid)
        {
            repository.Update(user);
        }
        return validationResult;
    
    }

    public ValidationResult DeleteUser(Guid id)
    {
        var userToDelete = repository.GetById(id);
        if (userToDelete == null)
            return new ValidationResult { IsValid = false, Message = "Usuario no encontrado." }; ;
        repository.Delete(id);
        return new ValidationResult { IsValid = true };
    }

    public ValidationResult ValidateAddUser(User user)
    {
        if (string.IsNullOrEmpty(user.Name))
            return new ValidationResult { IsValid = false, Message = "El nombre del usuario no puede estar vacío." };

        if (string.IsNullOrEmpty(user.Email))
            return new ValidationResult { IsValid = false, Message = "El correo electrónico del usuario no puede estar vacío." };

        if (repository.Get(user.Name).Any())
            return new ValidationResult { IsValid = false, Message = "Ya existe un usuario con el mismo nombre." };

        return new ValidationResult { IsValid = true };
    }

    private ValidationResult ValidateUpdateUser(User user)
    {
        var userToUpdate = repository.GetById(user.Id);
        if (userToUpdate == null)
            return new ValidationResult { IsValid = false, Message = "Usuario no encontrado." };

        if (userToUpdate.Name != user.Name)
        {
            return new ValidationResult { IsValid = false, Message = "No se puede cambiar el nombre del usuario." };
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            return new ValidationResult { IsValid = false, Message = "El correo electrónico del usuario no puede estar vacío." };
        }

        return new ValidationResult { IsValid = true };
    }
}
