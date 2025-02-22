using DevSummit.Blog.Api.Domain.Entities;
using PactNet;
using PactNet.Matchers;
using System.Net;

namespace DevSummit.Blog.Consumer.Tests.BlogApi;
internal static class BlogData
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

    public static Article GetDefaultArticle() => new Article
    {
        Id = Guid.Parse("f3b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b"),
        Title = "Default Article",
        Content = "Default Article Content",
    };

    public static dynamic GetDefaultArticleDto() => new
    {
        Title = "Default Article",
        Content = "Default Article Content",
    };

    public static void ConfigureRequestPactBuilder(this IPactBuilderV4 pactBuilder, string userName, string giving)
    {
        var mailRegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        pactBuilder
            .UponReceiving("Get Users filtered by name")
                .Given(giving)
                .WithRequest(HttpMethod.Get, "/api/users")
                .WithQuery("name", Match.Equality(userName))
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithJsonBody(users.Where(u => u.Name == userName).Select(u => new { id = u.Id, name = Match.Type(u.Name), email = Match.Regex(u.Email, mailRegexPattern) }));

        var user = users.FirstOrDefault(u => u.Name == userName);
        if (user != null)
        {
            pactBuilder
                .UponReceiving("Get User details by user id")
                    .Given(giving)
                    .WithRequest(HttpMethod.Get, $"/api/users/{user.Id}")
                .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithJsonBody(new { name = Match.Type(user.Name), email = Match.Regex(user.Email, mailRegexPattern), hasAccess = user.Access });
        }
    }
}
