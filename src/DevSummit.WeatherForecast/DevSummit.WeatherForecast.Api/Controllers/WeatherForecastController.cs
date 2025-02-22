
using DevSummit.WeatherForecast.Api.Domain.Entities;
using DevSummit.WeatherForecast.Api.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevSummit.WeatherForecast.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IWeatherForecastService service;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastService service)
    {
        _logger = logger;
        this.service = service;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public ActionResult<IEnumerable<Forecast>> Get()
    {
        var username = HttpContext.Request.Headers["Username"].ToString();
        _logger.LogInformation($"Get WeatherForecast made by user: {username}");

        return Ok(service.GetWeatherForecast());
    }
}
