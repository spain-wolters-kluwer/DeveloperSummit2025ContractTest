using DevSummit.Blog.Api.Domain.Clients;
using DevSummit.Blog.Api.Domain.Repositories;
using DevSummit.Blog.Api.Domain.Services;
using DevSummit.Blog.Api.Infrastructure.Clients;
using DevSummit.Blog.Api.Infrastructure.Repositories;
using DevSummit.Blog.Api.Middlewares;
using DevSummit.Blog.Api.OperationFilters;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IArticlesRepository, ArticlesRepository>();
builder.Services.AddScoped<IArticlesService, ArticlesService>();
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blog API", Version = "v1" });
    c.OperationFilter<AddUsernameHeaderParameter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog API v1"));

app.UseUsernameHeaderMiddleware();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
