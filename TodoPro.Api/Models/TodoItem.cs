namespace TodoPro.Api.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional user ownership (foreign key)
    public int? UserId { get; set; }
    public User? User { get; set; }

    public List<Comment> Comments { get; set; } = new();
    public List<TodoLog> Logs { get; set; } = new();
}