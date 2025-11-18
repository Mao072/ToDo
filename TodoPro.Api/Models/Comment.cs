using System.ComponentModel.DataAnnotations;

namespace TodoPro.Api.Models;

public class Comment
{
    [Key]
    public int Id { get; set; }

    public int TodoId { get; set; }
    public TodoItem? Todo { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
