using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoPro.Api.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

        // Foreign key to the latest message for quick unread status check
        public int? LatestMessageId { get; set; }
        [ForeignKey("LatestMessageId")]
        public Message? LatestMessage { get; set; }

        // *** 修正：新增外來鍵，指向其 Principal (TodoItem) ***
        public int? TodoItemId { get; set; } // Group 現在擁有 FK
        [ForeignKey("TodoItemId")]
        public TodoItem? RelatedTodoItem { get; set; } // 反向導航屬性
        // *** 修正結束 ***
    }
}