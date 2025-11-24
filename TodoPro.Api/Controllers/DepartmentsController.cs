using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoPro.Api.Data;
using TodoPro.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace TodoPro.Api.Controllers
{
    // DTOs for Department Management (放在 Controller 檔案尾部)
    public record DepartmentCreateUpdateDto(string Name);

    [ApiController]
    [Route("api/[controller]")]
    // 移除頂層的 [Authorize]，改為在需要授權的方法上單獨添加
    public class DepartmentsController : ControllerBase 
    {
        private readonly AppDbContext _db;

        public DepartmentsController(AppDbContext db)
        {
            _db = db;
        }

        // 輔助方法：檢查 Supervisor 權限 (此方法仍需要 [Authorize] 的上下文)
        private bool IsCurrentUserSupervisor()
        {
            // 由於控制器沒有頂層 Authorize，這裡需要檢查 User.Identity.IsAuthenticated
            if (!User.Identity.IsAuthenticated) return false;
            return User.FindFirst("supervisor")?.Value == "True";
        }

        /// <summary>
        /// 列出所有部門 (公開 API，無需授權)
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // *** 關鍵修正：允許所有人訪問 (解決 401 錯誤) ***
        public async Task<IActionResult> Get()
        {
            var departments = await _db.Departments
                .OrderBy(d => d.Name)
                .Select(d => new 
                {
                    d.Id,
                    d.Name,
                    UserCount = d.Users.Count // 可選：回傳該部門的使用者數量 (需要注意 EF Core 載入效率)
                })
                .ToListAsync();

            return Ok(departments);
        }

        /// <summary>
        /// 新增部門 (限 Supervisor，需要授權)
        /// </summary>
        [HttpPost]
        [Authorize] // 確保需要登入才能執行
        public async Task<IActionResult> Create([FromBody] DepartmentCreateUpdateDto dto)
        {
            if (!IsCurrentUserSupervisor()) return Forbid("只有 Supervisor 才能新增部門。");
            
            if (await _db.Departments.AnyAsync(d => d.Name == dto.Name))
            {
                return BadRequest("部門名稱已存在。");
            }

            var department = new Department { Name = dto.Name };
            _db.Departments.Add(department);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { department.Id, department.Name });
        }

        /// <summary>
        /// 更改部門名稱 (限 Supervisor，需要授權)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentCreateUpdateDto dto)
        {
            if (!IsCurrentUserSupervisor()) return Forbid("只有 Supervisor 才能更改部門。");
            
            var department = await _db.Departments.FindAsync(id);
            if (department == null) return NotFound("找不到該部門。");

            if (await _db.Departments.AnyAsync(d => d.Name == dto.Name && d.Id != id))
            {
                return BadRequest("新部門名稱已被其他部門使用。");
            }

            department.Name = dto.Name;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// 刪除部門 (限 Supervisor，需要授權)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsCurrentUserSupervisor()) return Forbid("只有 Supervisor 才能刪除部門。");
            
            var department = await _db.Departments.FindAsync(id);
            if (department == null) return NotFound("找不到該部門。");
            
            _db.Departments.Remove(department);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// 列出某部門下的所有使用者 (需要授權)
        /// </summary>
        [HttpGet("{id}/users")]
        [Authorize]
        public async Task<IActionResult> GetUsersByDepartment(int id)
        {
            var users = await _db.Users
                .Where(u => u.DepartmentId == id)
                .Select(u => new
                {
                    u.Id,
                    u.Account,
                    u.Name,
                    u.Supervisor
                })
                .ToListAsync();

            if (!users.Any() && !await _db.Departments.AnyAsync(d => d.Id == id))
            {
                return NotFound("找不到該部門。");
            }

            return Ok(users);
        }
    }
}