using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoPro.Api.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    // Supervisor flag
    public bool Supervisor { get; set; }

    [Required]
    public string Account { get; set; } = string.Empty;

    // Store hashed password (do not store plaintext passwords or raw JWTs here)
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Department { get; set; }

    // Relation: a user can have many todos
    public List<TodoItem> Todos { get; set; } = new();

    // Relation: a user can have many comments
    public List<Comment> Comments { get; set; } = new();

    // Optional: a user can also reference a single Todo (foreign key) if needed
    public int? TodoId { get; set; }
    public TodoItem? Todo { get; set; }
}
