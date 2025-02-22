using DevSummit.WeatherForecast.Api.Domain.Entities;

namespace DevSummit.WeatherForecast.Api.Domain.Clients;

public interface IUsersClient
{
    Task<Guid> GetUserIdByName(string? name);
    Task<User> GetUserById(Guid id);

}
