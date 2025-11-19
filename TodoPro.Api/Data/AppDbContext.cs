using Microsoft.EntityFrameworkCore;
using TodoPro.Api.Models; 

namespace TodoPro.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> Todos { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    
    // 新增群組與訊息相關的模型
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<UserGroup> UserGroups { get; set; } = null!;

protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- 1. TodoItem 與 User 的一對多關係 ---
        modelBuilder.Entity<TodoItem>()
            .HasOne(t => t.User)
            .WithMany(u => u.Todos)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // --- 2. 移除多餘的 User -> Todo 關係 (已刪除 User.TodoId) ---
        // 刪除以下代碼：
        /*
        modelBuilder.Entity<User>()
            .HasOne(u => u.Todo)
            .WithMany()
            .HasForeignKey(u => u.TodoId)
            .OnDelete(DeleteBehavior.SetNull);
        */

        // --- 3. Comment -> TodoItem 關係 (修正屬性名稱) ---
        // 註：模型中 Comment.TodoItemId 連結 TodoItem
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.TodoItem) // 修正：模型中應為 TodoItem
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TodoItemId) // 修正：模型中應為 TodoItemId
            .OnDelete(DeleteBehavior.Cascade);

        // --- 4. Comment -> User 關係 ---
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // --- 5. 移除已刪除的 TodoLog 配置 ---
        // 刪除以下代碼：
        /*
        modelBuilder.Entity<TodoLog>()
            .HasOne(l => l.Todo)
            .WithMany(t => t.Logs)
            .HasForeignKey(l => l.TodoId)
            .OnDelete(DeleteBehavior.Cascade);
        */

        // ------------------------------------------------------------------
        // --- 6. 新增 Group/Message/UserGroup 相關配置 ---

        // A. TodoItem 與 Group 的一對零/一關係 (每個 TodoItem 有一個專屬 Group)
        modelBuilder.Entity<TodoItem>()
            .HasOne(t => t.DiscussionGroup)
            .WithOne(g => g.RelatedTodoItem)
            .HasForeignKey<TodoItem>(t => t.GroupId) // TodoItem 擁有外來鍵 GroupId
            .IsRequired(false) // 允許 GroupId 為 Null
            .OnDelete(DeleteBehavior.SetNull); // 當 Group 被刪除，TodoItem.GroupId 設為 Null

        // B. UserGroup (多對多連接表) 的複合主鍵配置
        modelBuilder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId }); // 複合主鍵

        // C. UserGroup -> User
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.User)
            .WithMany() // User 模型中沒有 UserGroups 導航屬性，如果需要，應在 User 模型中添加
            .HasForeignKey(ug => ug.UserId);

        // D. UserGroup -> Group
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId);
        
        // E. Group -> LatestMessage (一對零/一)
        modelBuilder.Entity<Group>()
            .HasOne(g => g.LatestMessage)
            .WithOne() // Message 不需反向導航屬性
            .HasForeignKey<Group>(g => g.LatestMessageId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull); // 當 Message 被刪除，Group.LatestMessageId 設為 Null
        
        // F. UserGroup -> LastReadMessage (一對零/一)
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.LastReadMessage)
            .WithOne() // Message 不需反向導航屬性
            .HasForeignKey<UserGroup>(ug => ug.LastReadMessageId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

    