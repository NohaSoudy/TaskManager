using Microsoft.EntityFrameworkCore;
namespace TaskManager.Api.Data;
public sealed class TaskDbContext(DbContextOptions<TaskDbContext> options) : DbContext(options)
{
 public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
 protected override void OnModelCreating(ModelBuilder modelBuilder)
 {
  var task = modelBuilder.Entity<TaskEntity>(); task.ToTable("tasks"); task.HasKey(x => x.Id);
  task.Property(x => x.Title).IsRequired(); task.Property(x => x.Status).HasConversion<string>().HasMaxLength(16).IsRequired(); task.Property(x => x.CreatedAt).IsRequired();
 }
}
