# Introduction
This solution is a *monorepo* that it contains three API's:

* **DevSummit.UsersPermissions**: This API manage users and its permissions
* **DevSummit.WeatherForecast**: This API give a weather forecast only to user that they have access to it.
* **DevSummit.Blog**: This API is a simple blog. Also users that has access to it, can create, read, update or delete blog entries.

This solutions is only intended to show how **Contract Test** works.

# Prerrequisites
To follow this hands on you need to be installed:
- **Source Code Editor** as Visual Studio 2022(preferred) or Visual Studio Code (with c# plugins). Raider from JetBrains are allowed as well. 
- **Docker** such as Docker desktop or Rancher.

# Course

1. [Use Case Bad Implementation](./handsOn/01-UseCaseBadImplementation.md)
2. [Consumer Contract Test](./handsOn/02-ConsumerContractTest.md)
3. [Provider Contract Test](./handsOn/03-ProviderContractTest.md)

# Configuration

It is needed to configure the environment variable **USERS_PERMISSIONS_API_URL** with the *Users Permissions URL* before executing the projects **DevSummit.WeatherForecast** and **DevSummit.Blog**.

```shell
dotnet run -c Debug --project ./src/DevSummit.UsersPermissions/DevSummit.UsersPermissions.Api/DevSummit.UsersPermissions.Api.csproj
$Env:USERS_PERMISSIONS_API_URL = "http://localhost:5241"; dotnet run -c Debug --project ./src/DevSummit.Blog/DevSummit.Blog.Api/DevSummit.Blog.Api.csproj
$Env:USERS_PERMISSIONS_API_URL = "http://localhost:5241"; dotnet run -c Debug --project ./src/DevSummit.WeatherForecast/DevSummit.WeatherForecast.Api/WeatherForecast.Blog.Api.csproj
```

# Docker
There is a Docker Compose file to build and execute the API's.

```shell
docker compose -f ./app.dockercompose.yaml up -d
```

Afterward you can access to the swagger of the API's:
* [Users Permissions API] (http://localhost:50000/swagger/index.html)
* [Weather Forecast API] (http://localhost:50001/swagger/index.html)
* [Blog API] (http://localhost:50002/swagger/index.html)




