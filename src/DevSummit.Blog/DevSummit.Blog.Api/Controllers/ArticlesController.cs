using DevSummit.Blog.Api.Domain.Entities;
using DevSummit.Blog.Api.Domain.Repositories;
using DevSummit.Blog.Api.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevSummit.Blog.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArticlesController :ControllerBase
{
    private readonly IArticlesRepository repository;    
    private readonly IArticlesService service;
    private readonly ILogger<ArticlesController> logger;

    private const int summaryLenght = 20;

    public ArticlesController(ILogger<ArticlesController> logger, IArticlesRepository repository, IArticlesService service)
    {
        this.logger = logger;
        this.repository = repository;
        this.service = service;
    }

    // GET: api/<ArticlesController>
    [HttpGet]
    public ActionResult<IEnumerable<ArticleViewDto>> Get()
    {
        var username = HttpContext.Request.Headers["Username"].ToString();
        logger.LogInformation("Getting articles by user: {Username}", username);
        return base.Ok(repository.Get().Select(s => new ArticleViewDto(s.Id, s.Title, GetSummary(s))));
    }

    private static string? GetSummary(Article article)
    {
        if (article.Content?.Length < summaryLenght)
        {
            return article.Content;
        }
        return article.Content?.Substring(0, summaryLenght);
    }

    // GET api/<ArticlesController>/5
    [HttpGet("{id}")]
    public ActionResult<ArticleDto> Get(string id)
    {
        var username = HttpContext.Request.Headers["Username"].ToString();
        logger.LogInformation("Getting article by id by user: {Username}", username);
        var article = repository.GetById(Guid.Parse(id));
        if (article == null)
        {
            logger.LogError("Article not found");
            return NotFound("Article not found");
        }
        return new ArticleDto(article.Title, article.Content);
    }

    // POST api/<ArticlesController>
    [HttpPost]
    public ActionResult<string> Post([FromBody] ArticleDto article)
    {
        var username = HttpContext.Request.Headers["Username"].ToString();
        logger.LogInformation("Adding article by user: {Username}", username);
        var result = service.AddArticle(new Article { Title = article.Title, Content = article.Content });
        if (!result.IsValid)
        {
            logger.LogError(result.Message);
            return BadRequest(result.Message);
        }
        return Ok(result.Message);
    }

    // PUT api/<ArticlesController>/5
    [HttpPut("{id}")]
    public ActionResult<string> Put(string id, [FromBody] ArticleDto article)
    {
        var username = HttpContext.Request.Headers["Username"].ToString();
        logger.LogInformation("Updating article by user: {Username}", username);
        var result = service.UpdateArticle(new Article { Id = Guid.Parse(id), Title = article.Title, Content = article.Content });
        if (!result.IsValid)
        {
            logger.LogError(result.Message);
            if (result.Message == "Article not found")
            {
                return NotFound(result.Message);
            }
            return BadRequest(result.Message);
        }
        return NoContent();
    }

    // DELETE api/<ArticlesController>/5
    [HttpDelete("{id}")]
    public ActionResult<string> Delete(string id)
    {
        var username = HttpContext.Request.Headers["Username"].ToString();
        logger.LogInformation("Deleting article by user: {Username}", username);
        var result = service.DeleteArticle(Guid.Parse(id));
        if (!result.IsValid)
        {
            logger.LogError(result.Message);
            return NotFound(result.Message);
        }
        return NoContent();
    }
}

public record ArticleViewDto(Guid Id, string Title, string Summary);

public record ArticleDto(string Title, string Content);
