using DevSummit.WeatherForecast.Api.Domain.Entities;

namespace DevSummit.WeatherForecast.Api.Domain.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private static readonly string[] Summaries = [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public IEnumerable<Forecast> GetWeatherForecast()
    {
        return Enumerable.Range(1, 5).Select(index => new Forecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
