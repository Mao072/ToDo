using System.Collections.Generic;

namespace TodoPro.Api.Dtos
{
    /// <summary>
    /// 使用者登入資料傳輸物件
    /// </summary>
    public record LoginDto(string Account, string Password);

    /// <summary>
    /// 使用者註冊資料傳輸物件，包含 DepartmentId
    /// </summary>
    public record RegisterDto(string Account, string Password, string Name, int? DepartmentId, bool Supervisor);
}