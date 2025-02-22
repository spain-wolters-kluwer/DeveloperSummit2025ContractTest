using DevSummit.UsersPermissions.Api.Domain.Entities;
using DevSummit.UsersPermissions.Api.Domain.Repositories;
using DevSummit.UsersPermissions.Api.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevSummit.UsersPermissions.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUsersRepository repository;
    private readonly IUsersService service;
    private readonly ILogger<UsersController> logger;

    public UsersController(ILogger<UsersController> logger, IUsersRepository repository, IUsersService service)
    {
        this.logger = logger;
        this.repository = repository;
        this.service = service;
    }

    // GET: api/<UsersController>
    [HttpGet]
    public ActionResult<IEnumerable<UserViewDto>> Get([FromQuery] string? name)
    {
        logger.LogInformation("Getting users");
        var users = repository.Get(name);        
        return Ok(users.Select(u => new UserViewDto(u.Id, u.Name, u.Email)));
    }

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

    // DELETE api/<UsersController>/5
    [HttpDelete("{id}")]
    public ActionResult<string> Delete(string id)
    {
        var result = service.DeleteUser(Guid.Parse(id));
        if (!result.IsValid)
        {
            return NotFound(result.Message);
        }
        return NoContent();
    }
}

public record UserViewDto(Guid Id, string Name, string Email);
public record UserDto(string Name, string Email, int Role);
