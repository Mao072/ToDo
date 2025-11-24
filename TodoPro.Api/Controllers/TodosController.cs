using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoPro.Api.Data;
using TodoPro.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq; 
using System.Threading.Tasks;
using TodoPro.Api.Dtos; 
using System; 

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
    /// 獲取所有待辦事項 (包含儲存在模型中的 UserCount)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var todosQuery = _db.Todos
            .Include(t => t.User).ThenInclude(u => u!.Department) // 載入 User 和 Department
            .Include(t => t.Comments).ThenInclude(c => c.User) // 載入 Comments
            .Include(t => t.DiscussionGroup); // 載入群組

        var list = await todosQuery
            .Select(t => new 
            {
                t.Id,
                t.Title,
                t.Description,
                t.IsCompleted,
                t.CreatedAt,
                t.UserId,
                
                // 查詢模型中的 UserCount 欄位
                ParticipantCount = t.UserCount, 
                
                // User 投影
                User = t.User == null ? null : new 
                { 
                    t.User.Id, 
                    t.User.Account, 
                    t.User.Name, 
                    DepartmentId = t.User.DepartmentId,
                    DepartmentName = t.User.Department != null ? t.User.Department.Name : null
                }, 
                
                Comments = t.Comments.Select(c => new 
                { 
                    c.Id, c.Content, c.CreatedAt, c.UserId, 
                    User = c.User == null ? null : new { c.User.Id, c.User.Account, c.User.Name } 
                }).ToList(),
                
                DiscussionGroup = t.DiscussionGroup == null ? null : new 
                { 
                    t.DiscussionGroup.Id, 
                    t.DiscussionGroup.Name,
                }
            })
            .ToListAsync();

        return Ok(list);
    }
    
    /// <summary>
    /// 獲取單個待辦事項的詳細信息 (用於詳情頁和聊天)
    /// </summary>
    [HttpGet("{id}")]
public async Task<IActionResult> Get(int id)
{
    // 這裡我們不應該對 todo 進行任何基於當前用戶 ID 的過濾。
    // 任務是否存在的檢查應該只基於 'id'。
    var todo = await _db.Todos
        // 確保載入 DiscussionGroup 及其成員
        .Include(t => t.DiscussionGroup)
            .ThenInclude(g => g!.UserGroups)
                .ThenInclude(ug => ug.User)
        .Include(t => t.User).ThenInclude(u => u!.Department)
        // *** 關鍵：只根據 ID 進行查詢 ***
        .Where(t => t.Id == id) 
        .Select(t => new
        {
            t.Id,
            t.Title,
            t.Description,
            t.IsCompleted,
            t.CreatedAt,
            t.UserId,
            
            // 投影所有必要數據
            ParticipantCount = t.UserCount, 
            User = t.User == null ? null : new 
            { 
                t.User.Id, 
                t.User.Account, 
                t.User.Name, 
            },
            
            DiscussionGroup = t.DiscussionGroup == null ? null : new
            {
                t.DiscussionGroup.Id,
                t.DiscussionGroup.Name,
                Members = t.DiscussionGroup.UserGroups.Select(ug => new
                {
                    ug.User.Id,
                    ug.User.Account,
                    ug.User.Name
                }).ToList()
            }
        })
        .FirstOrDefaultAsync();

    if (todo == null)
    {
        // 任務不存在，返回 404
        return NotFound($"找不到 ID 為 {id} 的任務。");
    }

    // 任務存在，返回 200 OK
    return Ok(todo);
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

        var usersToAssign = dto.AssignedUserIds.Distinct().ToList();

        if (!usersToAssign.Contains(currentUserId.Value)) {
            usersToAssign.Add(currentUserId.Value);
        }

        // 1. 建立 TodoItem
        var newItem = new TodoItem
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = currentUserId, 
            IsCompleted = false,
            // 在創建時寫入 UserCount
            UserCount = usersToAssign.Count 
        };
        
        // 2. 建立專屬討論群組
        var discussionGroup = new Group
        {
            Name = $"{dto.Title} ({Guid.NewGuid().ToString().Substring(0, 4)})",
            Description = $"{dto.Title}",
        };
        _db.Groups.Add(discussionGroup);
        newItem.DiscussionGroup = discussionGroup; 
        _db.Todos.Add(newItem);

        // 3. 指派成員
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
        
        // 返回新增的項目及參與人數
        return Ok(new 
        { 
            newItem.Id, 
            newItem.Title, 
            DiscussionGroupId = discussionGroup.Id,
            ParticipantCount = newItem.UserCount 
        });
    }

    /// <summary>
    /// 獲取所有使用者列表 (供前端指派使用)
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Include(u => u.Department) // 確保 Department 載入
            .Select(u => new 
            {
                u.Id,
                u.Account,
                u.Name,
                u.Supervisor,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department != null ? u.Department.Name : null,
            })
            .ToListAsync();
        
        return Ok(users);
    }
}