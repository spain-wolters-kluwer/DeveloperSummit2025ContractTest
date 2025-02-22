namespace DevSummit.WeatherForecast.Api.Domain.Services
{
    public interface IUsersService
    {
        public Task<bool> HasAccess(string userName);
    }
}
