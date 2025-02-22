using DevSummit.UsersPermissions.Api;

var builder = WebApplication.CreateBuilder(args);

HostConfiguration.ConfigureBuilder(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
HostConfiguration.ConfigureApp(app);

app.Run();
