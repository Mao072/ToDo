using Microsoft.EntityFrameworkCore;
using TodoPro.Api.Models; 

namespace TodoPro.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<TodoItem> Todos { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<UserGroup> UserGroups { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- 1. User 與 Department 的一對多關係 ---
        modelBuilder.Entity<User>()
            .HasOne(u => u.Department) 
            .WithMany(d => d.Users) 
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull); 

        // --- 2. TodoItem 與 User 的一對多關係 ---
        modelBuilder.Entity<TodoItem>()
            .HasOne(t => t.User)
            .WithMany(u => u.Todos)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // --- 3. Comment -> TodoItem 關係 (級聯刪除 Comment) ---
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.TodoItem)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TodoItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- 4. Comment -> User 關係 ---
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull); 

        // --- 5. 修正後的 TodoItem <-> Group 關係 (Group 擁有 FK，啟用級聯刪除) ---
        // 當 TodoItem (Principal) 被刪除時，連帶刪除 Group (Dependent)
        modelBuilder.Entity<Group>() 
            .HasOne(g => g.RelatedTodoItem) 
            .WithOne(t => t.DiscussionGroup!) 
            .HasForeignKey<Group>(g => g.TodoItemId) // 外來鍵設在 Group 模型上
            .IsRequired(false) 
            .OnDelete(DeleteBehavior.Cascade); 
            // *** 關鍵：當 TodoItem 刪除時，Group 將被刪除 ***

        // --- 6. UserGroup (多對多連接表) 的複合主鍵配置 ---
        modelBuilder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId }); 
        
        // --- 7. UserGroup -> LastReadMessage ---
        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.LastReadMessage)
            .WithOne() 
            .HasForeignKey<UserGroup>(ug => ug.LastReadMessageId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // --- 8. Group -> LatestMessage ---
        modelBuilder.Entity<Group>()
            .HasOne(g => g.LatestMessage)
            .WithOne() 
            .HasForeignKey<Group>(g => g.LatestMessageId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        // --- 9. Message 與 Group 的主要一對多關係 (Group 刪除會連帶刪除 Message) ---
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Group)
            .WithMany(g => g.Messages)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
            // *** 由於 Group 被刪除，這裡會級聯刪除所有 Messages ***
    }
}