using DevSummit.Blog.Api.Domain.Entities;
using DevSummit.Blog.Api.Domain.Services;
using DevSummit.Blog.Api.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DevSummit.Blog.Consumer.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
    }

    public HttpClient CreateClient(string urlPermissions)
    {
        Environment.SetEnvironmentVariable("USERS_PERMISSIONS_API_URL", urlPermissions);
        return CreateClient();
    }

    public void SaveArticle(Article article)
    {
        using var scope = Services.CreateScope();
        var articlesService = scope.ServiceProvider.GetRequiredService<IArticlesService>();
        articlesService.AddArticle(article);
    }

    public void DeleteArticle(Guid id)
    {
        using var scope = Services.CreateScope();
        var articlesService = scope.ServiceProvider.GetRequiredService<IArticlesService>();
        articlesService.DeleteArticle(id);
    }

    public Article? GetArticleById(Guid id)
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IArticlesRepository>();
        return repository.GetById(id);
    }
}
