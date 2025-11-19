using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoPro.Api.Models
{
    // 新增 Comment 模型，用於 TodoItem 的簡單留言功能
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // 連結到 TodoItem (外來鍵)
        public int TodoItemId { get; set; }
        [ForeignKey("TodoItemId")]
        public TodoItem TodoItem { get; set; } = null!;
        
        // 連結到 User (誰發表的評論)
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}