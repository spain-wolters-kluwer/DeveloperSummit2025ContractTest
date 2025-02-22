using DevSummit.WeatherForecast.Api.Domain.Entities;
namespace DevSummit.WeatherForecast.Api.Domain.Services;
public interface IWeatherForecastService
{
    IEnumerable<Forecast> GetWeatherForecast();
}