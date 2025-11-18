using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoPro.Api.Data;
using TodoPro.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly AppDbContext _db;

    public TodosController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var list = await _db.Todos
            .Include(t => t.User)
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .Include(t => t.Logs)
            .Select(t => new {
                t.Id,
                t.Title,
                t.Description,
                t.IsCompleted,
                t.CreatedAt,
                t.UserId,
                User = t.User == null ? null : new { t.User.Id, t.User.Account, t.User.Name, t.User.Department },
                Comments = t.Comments.Select(c => new { c.Id, c.Content, c.CreatedAt, c.UserId, User = c.User == null ? null : new { c.User.Id, c.User.Account, c.User.Name } }).ToList(),
                Logs = t.Logs.Select(l => new { l.Id, l.Message, l.CreatedAt }).ToList()
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(TodoItem item)
    {
        // If user is authenticated, assign this todo to the user
        var sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ??
                  User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(sub, out var userId))
            item.UserId = userId;
        _db.Todos.Add(item);
        await _db.SaveChangesAsync();
        return Ok(item);
    }
}
