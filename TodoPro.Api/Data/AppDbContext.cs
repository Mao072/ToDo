using Microsoft.EntityFrameworkCore;
using TodoPro.Api.Models; 

namespace TodoPro.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> Todos { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<TodoLog> TodoLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure user - todo relationship
        modelBuilder.Entity<TodoItem>()
            .HasOne(t => t.User)
            .WithMany(u => u.Todos)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Optional: user -> single todo reference
        modelBuilder.Entity<User>()
            .HasOne(u => u.Todo)
            .WithMany()
            .HasForeignKey(u => u.TodoId)
            .OnDelete(DeleteBehavior.SetNull);

        // Comment -> Todo relationship
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Todo)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TodoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Comment -> User (optional) relationship
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // TodoLog -> Todo relationship
        modelBuilder.Entity<TodoLog>()
            .HasOne(l => l.Todo)
            .WithMany(t => t.Logs)
            .HasForeignKey(l => l.TodoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
