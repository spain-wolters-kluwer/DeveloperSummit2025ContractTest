using DevSummit.Commons.Pact;
using DevSummit.WeatherForecast.Api.Domain.Entities;
using PactNet;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace DevSummit.WeatherForecast.Consumer.Tests.WeatherForecastApi
{
    public class WeatherForecastApiTests : IClassFixture<PactService>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly IPactBuilderV4 _pactBuilder;

        private const string permitedUserName = "PermitedUser";
        private const string notPermitedUserName = "NotPermitedUser";
        private const string notExistingUserName = "NotExistingUser";

        public WeatherForecastApiTests(PactService service, ITestOutputHelper output)
        {
            _factory = new CustomWebApplicationFactory<Program>();
            _pactBuilder = service.CreatePactBuilder("WeatherForecast", "UsersPermissions", output);
        }

        [Fact]
        public async Task GivingUserWithAccess_WhenGetWeatherForecast_ThenReturnWeatherForecast()
        {
            _pactBuilder.ConfigureRequestPactBuilder(permitedUserName, "User with access");
            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Arrange
                var client = _factory.CreateClient(ctx.MockServerUri.ToString());
                client.DefaultRequestHeaders.Add("Username", permitedUserName);

                // Act
                var response = await client.GetAsync("weatherforecast");

                //Assert
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var temperatures = JsonSerializer.Deserialize<IEnumerable<Forecast>>(responseString);
                Assert.Equal(5, temperatures?.Count());
            });
        }

        [Fact]
        public async Task GivingNotExistingUser_WhenGetWeatherForecast_ThenReturnUnauthorized()
        {
            _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Arrange
                var client = _factory.CreateClient(ctx.MockServerUri.ToString());
                client.DefaultRequestHeaders.Add("Username", notExistingUserName);

                // Act
                var response = await client.GetAsync("weatherforecast");

                //Assert
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            });
        }

        [Fact]
        public async Task GivingUserWithoutAccess_WhenGetWeatherForecast_ThenReturnUnauthorized()
        {
            _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
            await _pactBuilder.VerifyAsync(async ctx =>
            {
                // Arrange
                var client = _factory.CreateClient(ctx.MockServerUri.ToString());
                client.DefaultRequestHeaders.Add("Username", notPermitedUserName);

                // Act
                var response = await client.GetAsync("weatherforecast");

                //Assert
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            });
        }
    }
}

