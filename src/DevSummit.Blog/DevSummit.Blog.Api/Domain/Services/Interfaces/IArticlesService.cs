using DevSummit.Blog.Api.Domain.Entities;

namespace DevSummit.Blog.Api.Domain.Services;
public interface IArticlesService
{
    ValidationResult AddArticle(Article article);
    ValidationResult DeleteArticle(Guid id);
    ValidationResult UpdateArticle(Article article);
}