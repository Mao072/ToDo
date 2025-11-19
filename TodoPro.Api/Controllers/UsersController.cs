using Microsoft.AspNetCore.Mvc;
using TodoPro.Api.Utils; // 假設 PasswordHasher 在這裡
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoPro.Api.Data;
using Microsoft.EntityFrameworkCore;
using TodoPro.Api.Models;
using Microsoft.Extensions.Configuration; // 確保 IConfiguration 被正確引用

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    
    // 假設 PasswordHasher 在 TodoPro.Api.Utils.PasswordHasher

    public UsersController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Name == dto.Name))
            return BadRequest("名稱已重複");
        var user = new User
        {
            Account = dto.Account,
            Name = dto.Name,
            Department = dto.Department,
            Supervisor = dto.Supervisor
        };
        // 確保 PasswordHasher 類別可被存取
        // 這裡假設 PasswordHasher.Hash 方法存在
        user.PasswordHash = PasswordHasher.Hash(dto.Password ?? string.Empty); 
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.Account, user.Name });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // ... (登入邏輯保持不變)
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Account == dto.Account);
        if (user == null)
            return Unauthorized("Invalid credentials");

        // 確保 PasswordHasher.Verify 方法存在
        var valid = PasswordHasher.Verify(user.PasswordHash, dto.Password); 
        if (!valid)
            return Unauthorized("Invalid credentials");

        var token = CreateToken(user);
        return Ok(new { token });
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Select(u => new 
            {
                u.Id,
                u.Account,
                u.Name,
                u.Department,
                u.Supervisor,
                // 移除已刪除的 TodoId 屬性
                // u.TodoId 
            })
            .ToListAsync();

        return Ok(users);
    }

    private string CreateToken(User user)
    {
        // ... (Token 創建邏輯保持不變)
        var key = _config["Jwt:Key"];
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("account", user.Account),
            new Claim("supervisor", user.Supervisor.ToString())
        };

        var symmetric = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? string.Empty));
        var creds = new SigningCredentials(symmetric, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:DurationMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginDto(string Account, string Password);
public record RegisterDto(string Account, string Password, string Name, string? Department, bool Supervisor);