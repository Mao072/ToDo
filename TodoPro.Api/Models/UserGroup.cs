using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoPro.Api.Models
{
    public class UserGroup
    {
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group Group { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // The ID of the last message read by this user in this group
        public int? LastReadMessageId { get; set; }
        [ForeignKey("LastReadMessageId")]
        public Message? LastReadMessage { get; set; }
    }
}