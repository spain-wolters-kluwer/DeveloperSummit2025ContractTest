using DevSummit.WeatherForecast.Api.Domain.Entities;
using PactNet;
using PactNet.Matchers;
using System.Net;

namespace DevSummit.WeatherForecast.Consumer.Tests.WeatherForecastApi;
internal static class WeatherForecastData
{
    private static User[] users = [
        new User
        {
            Id = Guid.Parse("757d4594-79b2-480c-8fc4-ddd7061c18cb"),
            Name = "PermitedUser",
            Email = "PermitedUser@user.com",
            Access = true
        },
        new User
        {
            Id = Guid.Parse("2a69e26a-d392-41b1-82e6-68b3e4a869fb"),
            Name = "NotPermitedUser",
            Email = "NotPermitedUser@user.com",
            Access = false
        }
    ];

    public static void ConfigureRequestPactBuilder(this IPactBuilderV4 pactBuilder, string userName, string giving)
    {
        var mailRegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        

    }
}

