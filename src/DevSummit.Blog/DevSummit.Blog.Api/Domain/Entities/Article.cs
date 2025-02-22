namespace DevSummit.Blog.Api.Domain.Entities;
public class Article
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
}
