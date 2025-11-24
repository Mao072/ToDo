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
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using TodoPro.Api.Dtos;

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
            DepartmentId = dto.DepartmentId,
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
            new Claim("name", user.Name),
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
        /// 獲取當前登入使用者的詳細資訊 
        private int? GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                  User.FindFirst("sub")?.Value;
        if (int.TryParse(sub, out var userId))
            return userId;
        return null;
    }
    [HttpGet("me")]
    [Authorize] // 確保只有持有有效 Token 的用戶才能訪問
    public async Task<IActionResult> GetMe()
    {
        var currentUserId = GetCurrentUserId();
        
        if (!currentUserId.HasValue) 
        {
            // 理論上 [Authorize] 已經會攔截無效請求，這裡作為雙重檢查
            return Unauthorized("無法從 Token 中獲取使用者 ID。"); 
        }

        // 從資料庫查詢該使用者
        var user = await _db.Users
            .Where(u => u.Id == currentUserId.Value)
            .Select(u => new
            {
                u.Id,
                u.Account,
                u.Name,       // 這裡回傳 Name，供前端顯示「你好! XXX」
                u.Department,
                u.Supervisor
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound("使用者不存在或已被刪除。");
        }

        return Ok(user);
    }
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        var user = await _db.Users.FindAsync(currentUserId.Value);

        if (user == null)
        {
            return NotFound("使用者不存在。");
        }
        
        // 檢查新名稱是否已經被其他人使用
        if (await _db.Users.AnyAsync(u => u.Name == dto.Name && u.Id != currentUserId.Value))
        {
            return BadRequest(new { message = "名稱已被其他使用者使用，請選擇新的名稱。" });
        }

        // 只更新名稱
        user.Name = dto.Name;

        await _db.SaveChangesAsync();
        
        // 返回更新後的資料 (可選)
        return Ok(new { user.Id, user.Account, user.Name });
    }
    
    [HttpPut("password")] // 对应前端的 changePassword
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        var user = await _db.Users.FindAsync(currentUserId.Value);
        
        if (user == null)
        {
            return NotFound("使用者不存在。");
        }

        // 1. 驗證舊密碼是否正確
        if (!PasswordHasher.Verify(user.PasswordHash, dto.OldPassword))
        {
            // 返回自訂錯誤訊息，供前端捕捉並提示
            return BadRequest(new { message = "舊密碼不正確。" });
        }


        // 2. 更新密碼
        user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        
        await _db.SaveChangesAsync();

        return Ok(new { message = "密碼更新成功。" });
    }
}
