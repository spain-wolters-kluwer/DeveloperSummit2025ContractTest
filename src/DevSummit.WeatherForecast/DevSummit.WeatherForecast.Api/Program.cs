using DevSummit.WeatherForecast.Api.Domain.Clients;
using DevSummit.WeatherForecast.Api.Domain.Services;
using DevSummit.WeatherForecast.Api.Infrastructure.Clients;
using DevSummit.WeatherForecast.Api.Middlewares;
using DevSummit.WeatherForecast.Api.OperationFilters;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddHttpClient<IUsersClient, UsersClient>(client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("USERS_PERMISSIONS_API_URL") ?? string.Empty);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WeatherForecast API", Version = "v1" });
    c.OperationFilter<AddUsernameHeaderParameter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherForecast API v1"));

app.UseUsernameHeaderMiddleware();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
