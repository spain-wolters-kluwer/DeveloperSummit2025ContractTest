using DevSummit.Blog.Api.Controllers;
using DevSummit.Blog.Api.Domain.Entities;
using DevSummit.Commons.Pact;
using PactNet;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit.Abstractions;

namespace DevSummit.Blog.Consumer.Tests.BlogApi;

public class BlogApiTests : IClassFixture<PactService>
{
    private readonly IPactBuilderV4 _pactBuilder;
    private readonly CustomWebApplicationFactory<Program> _factory;

    private const string permitedUserName = "PermitedUser";
    private const string notPermitedUserName = "NotPermitedUser";
    private const string notExistingUserName = "NotExistingUser";

    private const string getId = "2707f508-ffcf-43c4-994f-66099700df4e";
    private const string titleById = "Lorem ipsum";

    public BlogApiTests(PactService pactService, ITestOutputHelper output)
    {
        _factory = new CustomWebApplicationFactory<Program>();
        _pactBuilder = pactService.CreatePactBuilder("Blog", "UsersPermissions", output);
    }

    #region GetBlogPosts
    [Fact]
    public async Task GivenUserWithAccess_WhenGetBlogPosts_ThenReturnBlogPosts()
    {
        _pactBuilder.ConfigureRequestPactBuilder(permitedUserName, "User with access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", permitedUserName);

            // Act
            var response = await client.GetAsync("api/articles");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var blogPosts = JsonSerializer.Deserialize<IEnumerable<Article>>(responseString);
            Assert.True(blogPosts?.Count() >= 7);
        });
    }

    [Fact]
    public async Task GivenNotExistingUser_WhenGetBlogPosts_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notExistingUserName);

            // Act
            var response = await client.GetAsync("api/articles");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }

    [Fact]
    public async Task GivenUserWithoutAccess_WhenGetBlogPosts_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notPermitedUserName);

            // Act
            var response = await client.GetAsync("api/articles");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }
    #endregion

    #region GetBlogPost
    [Fact]
    public async Task GivenUserWithAccess_GetGetBlogPost_ThenReturnBlogPost()
    {
        _pactBuilder.ConfigureRequestPactBuilder(permitedUserName, "User with access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", permitedUserName);

            // Act
            var response = await client.GetAsync($"api/articles/{getId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var blogPost = JsonSerializer.Deserialize<ArticleDto>(responseString,
                new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            Assert.Equal(titleById, blogPost?.Title);
        });
    }

    [Fact]
    public async Task GivenNotExistingUser_GetGetBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notExistingUserName);

            // Act
            var response = await client.GetAsync($"api/articles/{getId}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }
    [Fact]
    public async Task GivenUserWithoutAccess_GetGetBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notPermitedUserName);

            // Act
            var response = await client.GetAsync($"api/articles/{getId}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }
    #endregion

    #region PostBlogPost
    [Fact]
    public async Task GivenUserWithAcces_PostBlogPost_ThenBlogPostIsAdded()
    {
        _pactBuilder.ConfigureRequestPactBuilder(permitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", permitedUserName);
            var articleDto = BlogData.GetDefaultArticleDto();

            // Act
            var response = await client.PostAsync("api/articles", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            var articleId = await response.Content.ReadAsStringAsync();
            var article = _factory.GetArticleById(Guid.Parse(articleId));

            Assert.NotNull(article);
            Assert.Equal(articleDto.Title, article?.Title);
            Assert.Equal(articleDto.Content, article?.Content);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }

    [Fact]
    public async Task GivenNotExistingUser_PostBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notExistingUserName);
            var articleDto = BlogData.GetDefaultArticleDto();

            // Act
            var response = await client.PostAsync("api/articles", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }

    [Fact]
    public async Task GivenUserWithoutAccess_PostBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notPermitedUserName);
            var articleDto = BlogData.GetDefaultArticleDto();

            // Act
            var response = await client.PostAsync("api/articles", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }
    #endregion

    #region PutBlogPost
    [Fact]
    public async Task GivenUserWithAccess_PutBlogPost_ThenBlogPostIsUpdated()
    {
        _pactBuilder.ConfigureRequestPactBuilder(permitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", permitedUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            dynamic articleDto = new { Title = "Updated Article", Content = "Updated Article Content" };
            // Act
            var response = await client.PutAsync($"api/articles/{article.Id}", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedArticle = _factory.GetArticleById(article.Id);

            Assert.NotNull(updatedArticle);
            Assert.Equal(articleDto.Title, updatedArticle?.Title);
            Assert.Equal(articleDto.Content, updatedArticle?.Content);

            //CleanUp
            _factory.DeleteArticle(updatedArticle.Id);
        });
    }
    [Fact]
    public async Task GivenNotExistingUser_PutBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notExistingUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            dynamic articleDto = new { Title = "Updated Article", Content = "Updated Article Content" };
            // Act
            var response = await client.PutAsync($"api/articles/{article.Id}", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }

    [Fact]
    public async Task GivenUserWithoutAccess_PutBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notPermitedUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            dynamic articleDto = new { Title = "Updated Article", Content = "Updated Article Content" };
            // Act
            var response = await client.PutAsync($"api/articles/{article.Id}", new StringContent(JsonSerializer.Serialize(articleDto), Encoding.UTF8, "application/json"));

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }
    #endregion

    #region DeleteBlogPost
    [Fact]
    public async Task GivenUserWithAccess_DeleteBlogPost_ThenBlogPostIsDeleted()
    {
        _pactBuilder.ConfigureRequestPactBuilder(permitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", permitedUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            // Act
            var response = await client.DeleteAsync($"api/articles/{article.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var deletedArticle = _factory.GetArticleById(article.Id);

            Assert.Null(deletedArticle);
        });
    }
    [Fact]
    public async Task GivenNotExistingUser_DeleteBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notExistingUserName, "Not existing User");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notExistingUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            // Act
            var response = await client.DeleteAsync($"api/articles/{article.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }
    [Fact]
    public async Task GivenUserWithoutAccess_DeleteBlogPost_ThenReturnUnauthorized()
    {
        _pactBuilder.ConfigureRequestPactBuilder(notPermitedUserName, "User without access");
        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var client = _factory.CreateClient(ctx.MockServerUri.ToString());
            client.DefaultRequestHeaders.Add("Username", notPermitedUserName);
            var article = BlogData.GetDefaultArticle();
            _factory.SaveArticle(article);

            // Act
            var response = await client.DeleteAsync($"api/articles/{article.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            //CleanUp
            _factory.DeleteArticle(article.Id);
        });
    }
    #endregion
}

