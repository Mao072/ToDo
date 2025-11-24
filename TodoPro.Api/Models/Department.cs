using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TodoPro.Api.Models
{
    /// <summary>
    /// 部門實體模型
    /// </summary>
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // 導航屬性：一個部門可以有多個使用者
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}