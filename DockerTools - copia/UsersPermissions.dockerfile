# Use the official .NET 8.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app


# Copy the rest of the application code
COPY src/DevSummit.UsersPermissions/DevSummit.UsersPermissions.Api .

# Build the application
RUN dotnet publish -c Release -o out

# Use the official .NET 8.0 runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory
WORKDIR /app

# Copy the built application from the build image
COPY --from=build /app/out .

# Expose the port the app runs on
EXPOSE 8080

# Define the entry point for the application
ENTRYPOINT ["dotnet", "DevSummit.UsersPermissions.Api.dll"]