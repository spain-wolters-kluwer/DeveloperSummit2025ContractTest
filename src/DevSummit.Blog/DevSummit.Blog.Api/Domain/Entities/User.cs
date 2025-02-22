namespace DevSummit.Blog.Api.Domain.Entities;
public enum UserRoles
{
    NotPermited = 0,
    ReadOnly = 1,
    FullAccess = 2
}
public class User
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public UserRoles Role { get; set; }
}
