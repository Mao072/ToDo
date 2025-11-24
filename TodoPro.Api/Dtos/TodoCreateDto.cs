
namespace TodoPro.Api.Dtos
{
    public class TodoCreateDto
    {
        // 用戶輸入的標題，必須提供
        public string Title { get; set; } = string.Empty; 
        
        // 用戶輸入的描述，可選
        public string? Description { get; set; }
        
        // 用戶選取指派的多個成員 ID 列表
        public List<int> AssignedUserIds { get; set; } = new List<int>();
    }
}