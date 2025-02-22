using DevSummit.Blog.Api.Domain.Entities;
using DevSummit.Blog.Api.Domain.Repositories;

namespace DevSummit.Blog.Api.Domain.Services;
public class ArticlesService : IArticlesService
{
    private readonly IArticlesRepository repository;

    public ArticlesService(IArticlesRepository repository)
    {
        this.repository = repository;
    }

    public ValidationResult AddArticle(Article article)
    {
        article.Id = Guid.NewGuid();
        var validationResult = ValidateArticle(article);
        if (validationResult.IsValid)
        {
            repository.Add(article);
            validationResult.Message = article.Id.ToString();
        }
        return validationResult;
    }

    public ValidationResult UpdateArticle(Article article)
    {
        var validationResult = ValidateUpdateArticle(article);
        if (validationResult.IsValid)
        {
            repository.Update(article);
        }
        return validationResult;
    }

    public ValidationResult DeleteArticle(Guid id)
    {
        var articleToDelete = repository.GetById(id);
        if (articleToDelete == null)
            return new ValidationResult { IsValid = false, Message = "Artículo no encontrado." };
        repository.Delete(id);
        return new ValidationResult { IsValid = true };
    }

    private ValidationResult ValidateUpdateArticle(Article article)
    {
        var existingArticle = repository.GetById(article.Id);
        if (existingArticle == null)
            return new ValidationResult { IsValid = false, Message = "Artículo no encontrado." };
        return ValidateArticle(article);
    }

    private ValidationResult ValidateArticle(Article article)
    {
        if (string.IsNullOrEmpty(article.Title))
            return new ValidationResult { IsValid = false, Message = "El título del artículo no puede estar vacío." };

        if (string.IsNullOrEmpty(article.Content))
            return new ValidationResult { IsValid = false, Message = "El contenido del artículo no puede estar vacío." };

        return new ValidationResult { IsValid = true };
    }

}
