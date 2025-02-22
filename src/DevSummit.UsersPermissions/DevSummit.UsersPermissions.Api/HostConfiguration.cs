using DevSummit.UsersPermissions.Api.Domain.Services;
using DevSummit.UsersPermissions.Api.Domain.Repositories;
using DevSummit.UsersPermissions.Api.Infrastructure.Repositories;

namespace DevSummit.UsersPermissions.Api;
public static class HostConfiguration
{
    public static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddSingleton<IUsersRepository, UsersRepository>();
        builder.Services.AddScoped<IUsersService, UsersService>();

        var assembly = typeof(HostConfiguration).Assembly;
        builder.Services.AddControllers().AddApplicationPart(assembly);
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    public static void ConfigureApp(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthorization();

        app.MapControllers();
    }
}

