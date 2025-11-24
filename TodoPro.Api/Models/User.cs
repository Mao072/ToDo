using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace TodoPro.Api.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    public bool Supervisor { get; set; }

    [Required]
    public string Account { get; set; } = string.Empty;

    // Store hashed password
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }
    [ForeignKey("DepartmentId")]
    public Department? Department { get; set; }

    // Relation: a user can have many todos
    public List<TodoItem> Todos { get; set; } = new();

    // Relation: a user can have many comments
    public List<Comment> Comments { get; set; } = new();


}