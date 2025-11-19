using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoPro.Api.Data;
using TodoPro.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq; 
using System.Threading.Tasks;
// 假設 TodoCreateDto 在 TodoPro.Api.Dtos 中
using TodoPro.Api.Dtos; 

[ApiController]
[Route("api/[controller]")]
[Authorize] // 整個 Controller 都要求授權
public class TodosController : ControllerBase
{
    private readonly AppDbContext _db;

    public TodosController(AppDbContext db)
    {
        _db = db;
    }
    
    // 輔助方法：獲取當前登入用戶的 Id
    private int? GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                  User.FindFirst("sub")?.Value;
        if (int.TryParse(sub, out var userId))
            return userId;
        return null;
    }

    /// <summary>
    /// 獲取所有待辦事項 (不進行用戶過濾)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // 移除所有用戶篩選邏輯，回傳所有待辦事項
        var list = await _db.Todos
            .Include(t => t.User) // 待辦事項擁有者
            .Include(t => t.Comments).ThenInclude(c => c.User) // 評論和評論者
            .Include(t => t.DiscussionGroup)
            
            // 由於不需要過濾，此處暫時不包含 DiscussionGroup.UserGroups，以減少數據負載
            
            .Select(t => new 
            {
                t.Id,
                t.Title,
                t.Description,
                t.IsCompleted,
                t.CreatedAt,
                t.UserId,
                User = t.User == null ? null : new { t.User.Id, t.Account, t.Name, t.Department },
                Comments = t.Comments.Select(c => new 
                { 
                    c.Id, c.Content, c.CreatedAt, c.UserId, 
                    User = c.User == null ? null : new { c.User.Id, c.User.Account, c.User.Name } 
                }).ToList(),
                DiscussionGroup = t.DiscussionGroup == null ? null : new 
                { 
                    t.DiscussionGroup.Id, 
                    t.DiscussionGroup.Name,
                    // 不再包含 Members 列表
                }
            })
            .ToListAsync();

        return Ok(list);
    }

    /// <summary>
    /// 創建新的待辦事項 (Supervisor 權限)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TodoCreateDto dto)
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue) return Unauthorized("無法驗證用戶身份。");
        
        // 檢查 Supervisor 權限
        var isSupervisor = User.FindFirst("supervisor")?.Value == "True";
        if (!isSupervisor) return Forbid("只有 Supervisor 才能新增工作。");

        // 1. 建立 TodoItem
        var newItem = new TodoItem
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = currentUserId, // 創建者是擁有者/指派者
            IsCompleted = false,
        };
        
        // 2. 建立專屬討論群組
        var discussionGroup = new Group
        {
            Name = $"Todo-{dto.Title}-Discussion",
            Description = $"Discussion group for Todo Item: {dto.Title}",
        };
        _db.Groups.Add(discussionGroup);
        newItem.DiscussionGroup = discussionGroup; 
        _db.Todos.Add(newItem);
        
        // 3. 將指派成員加入群組
        var usersToAssign = dto.AssignedUserIds.Distinct().ToList();

        // 確保創建者自己也在群組內
        if (!usersToAssign.Contains(currentUserId.Value)) {
            usersToAssign.Add(currentUserId.Value);
        }

        foreach (var assignedUserId in usersToAssign)
        {
            var userGroup = new UserGroup 
            { 
                UserId = assignedUserId, 
                Group = discussionGroup,
            };
            _db.UserGroups.Add(userGroup);
        }
        
        await _db.SaveChangesAsync();
        
        return Ok(new 
        { 
            newItem.Id, 
            newItem.Title, 
            DiscussionGroupId = discussionGroup.Id,
            AssignedMembers = usersToAssign
        });
    }

    /// <summary>
    /// 獲取所有使用者列表 (供前端指派使用)
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Select(u => new 
            {
                u.Id,
                u.Account,
                u.Name,
                u.Department,
                u.Supervisor // 可選：是否顯示 supervisor 狀態
            })
            .ToListAsync();

        return Ok(users);
    }
}