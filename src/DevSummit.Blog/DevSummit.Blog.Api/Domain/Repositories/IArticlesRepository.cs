namespace DevSummit.Blog.Api.Domain.Repositories;
using DevSummit.Blog.Api.Domain.Entities;
public interface IArticlesRepository
{
    Article? GetById(Guid id);
    IEnumerable<Article> Get();
    void Add(Article article);
    void Update(Article article);
    void Delete(Guid id);
}
