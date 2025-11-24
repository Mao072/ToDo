namespace TodoPro.Api.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int UserCount { get; set; }
    // Optional user ownership (Foreign Key)
    public int? UserId { get; set; }
    public User? User { get; set; }

    // 註：使用 [ForeignKey] 特性可能需要您在 DbContext 中進一步配置唯一約束 (Unique Constraint)
    public Group? DiscussionGroup { get; set; } 

    public List<Comment> Comments { get; set; } = new();
    
    // 移除：public List<TodoLog> Logs { get; set; } = new();
}
