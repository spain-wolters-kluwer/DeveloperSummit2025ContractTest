namespace DevSummit.Blog.Api.Domain.Entities;

public enum Permissions
{
    None = 0,
    Read = 1,
    Write = 2
}

public class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Permissions Access { get; set; }
}
