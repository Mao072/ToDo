using System.ComponentModel.DataAnnotations;

namespace TodoPro.Api.Models;

public class TodoLog
{
    [Key]
    public int Id { get; set; }

    public int TodoId { get; set; }
    public TodoItem? Todo { get; set; }

    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
