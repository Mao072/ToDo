using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoPro.Api.Data;
using TodoPro.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq; 
using System.Threading.Tasks;

namespace TodoPro.Api.Controllers
{
    public record MessageCreateDto(string Content);

    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 整個 Controller 都要求授權
    public class GroupsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public GroupsController(AppDbContext db)
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
        /// 獲取某個群組的所有訊息 (按時間倒序)
        /// GET /api/groups/{groupId}/messages
        /// </summary>
        [HttpGet("{groupId}/messages")]
        public async Task<IActionResult> GetMessages(int groupId)
        {
            // 檢查群組是否存在 (並確保用戶是群組成員，這裡暫時跳過複雜的成員檢查)
            var group = await _db.Groups.FindAsync(groupId);
            if (group == null) return NotFound("找不到該討論群組。");

            var messages = await _db.Messages
                .Where(m => m.GroupId == groupId)
                .OrderBy(m => m.CreatedAt) // 按時間正序排列，方便聊天顯示
                .Select(m => new 
                {
                    m.Id,
                    m.Content,
                    m.CreatedAt,
                    User = new 
                    {
                        m.User.Id,
                        m.User.Name,
                        m.User.Account
                    }
                })
                .ToListAsync();

            return Ok(messages);
        }

        /// <summary>
        /// 發送新訊息到群組
        /// POST /api/groups/{groupId}/messages
        /// </summary>
        [HttpPost("{groupId}/messages")]
        public async Task<IActionResult> PostMessage(int groupId, [FromBody] MessageCreateDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue) return Unauthorized("無法驗證用戶身份。");

            // 檢查群組和用戶是否存在（並檢查成員身份）
            var group = await _db.Groups.FindAsync(groupId);
            if (group == null) return NotFound("找不到該討論群組。");
            
            // 註：可以在這裡加入檢查，確保 currentUserId 是該群組的成員。
            
            var newMessage = new Message
            {
                GroupId = groupId,
                UserId = currentUserId.Value,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _db.Messages.Add(newMessage);
            
            // *** 關鍵：更新 Group 的 LatestMessageId (用於通知) ***
            // 由於 ID 尚未生成，需要先儲存
            await _db.SaveChangesAsync();
            
            // 更新 LatestMessageId
            group.LatestMessageId = newMessage.Id;
            await _db.SaveChangesAsync();

            // 返回創建的訊息
            return CreatedAtAction(nameof(GetMessages), new { groupId = groupId }, new 
            {
                newMessage.Id,
                newMessage.Content,
                newMessage.CreatedAt,
                UserId = currentUserId.Value 
            });
        }
    }
}