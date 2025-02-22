# Use case Implementation - UsersPersmissions Service

## Implement new contract in Provider service

1. Modify the **User** entity to add *Role* property.
```csharp
public enum UserRoles
{
    NoAccess = 0,
    ReadOnly = 1,
    FullAccess = 2
}
public class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public UserRoles Role { get; set; }
}
```

2. Modify **UsersRepository** to save the new property. Really we modify the constructor to add new user with no access and adapt the existent users. And the **update* method.

* Constructor:
```csharp
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
            Role = UserRoles.ReadOnly,
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
```

* **Update** method
```csharp
    public void Update(User user)
    {
        var userToUpdate = users.Find(u => u.Id == user.Id);
        if (userToUpdate != null)
        {            
            userToUpdate.Email = user.Email;
            userToUpdate.Role = user.Role;         
        }        
    }
```

3. Adapt the Controller to send the **Role** when necessary

* Modify the *DTO*.

```csharp
public record UserDto(string Name, string Email, int Role);
```

* And the Mappings.
```csharp
    // GET api/<UsersController>/5
    [HttpGet("{id}")]
    public ActionResult<UserDto> GetById(string id)
    {
        logger.LogInformation("Getting user by id");
        var user = repository.GetById(Guid.Parse(id));
        if (user == null)
        {
            logger.LogError("User not found");
            return NotFound();
        }
        return new UserDto(user.Name, user.Email, (int)user.Role);
    }

    // POST api/<UsersController>
    [HttpPost]
    public ActionResult<string> Post([FromBody] UserDto user)
    {
        logger.LogInformation("Adding user");   
        var result = service.AddUser(new User { Name = user.Name, Email = user.Email, Role = (UserRoles)user.Role });
        if (!result.IsValid)
        {
            logger.LogError(result.Message);
            return BadRequest(result.Message);
        }
        return Ok(result.Message);
    }

    // PUT api/<UsersController>/5
    [HttpPut("{id}")]
    public ActionResult<string> Put(string id, [FromBody] UserDto user)
    {
        logger.LogInformation("Updating user");
        var result = service.UpdateUser(new User { Id = Guid.Parse(id), Name = user.Name, Email = user.Email, Role = (UserRoles)user.Role });
        if (!result.IsValid)
        {
            logger.LogError(result.Message);    
            if (result.Message == "Usuario no encontrado")
            {
                return NotFound(result.Message);
            }
            return BadRequest(result.Message);
        }
        return NoContent();
    }
```

4. Finally we add the new users for contract testing in the **ProviderStateMidleware** and set *PermitedUser* in *WeatherForecast* services with ReadoOnly Role
```csharp
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
```

## Backward compatibility in UsersPermissions Controller

1. Add **HasAccess** field to *UsersDto*
```csharp
public record UserDto(string Name, string Email, bool HasAccess, int Role);
```

2. Update **Get/{id}** operation to fullfill the DTO 
```csharp
 // GET api/<UsersController>/5
 [HttpGet("{id}")]
 public ActionResult<UserCardDto> GetById(string id)
 {
     logger.LogInformation("Getting user by id");
     var user = repository.GetById(Guid.Parse(id));
     if (user == null)
     {
         logger.LogError("User not found");
         return NotFound();
     }
     return new UserCardDto(user.Name, user.Email, HasAccess(user.Role), (int)user.Role);
 }

private static bool HasAccess(UserRoles role) => role != UserRoles.NoAccess;
 ```

