using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TaskManager.Api.Data;
#nullable disable
namespace TaskManager.Api.Migrations;
[DbContext(typeof(TaskDbContext))]
public sealed class TaskDbContextModelSnapshot : ModelSnapshot {
 protected override void BuildModel(ModelBuilder modelBuilder) { modelBuilder.HasAnnotation("ProductVersion","9.0.4"); modelBuilder.Entity("TaskManager.Api.Data.TaskEntity", b => { b.Property<Guid>("Id").HasColumnType("uuid"); b.Property<string>("Title").IsRequired().HasColumnType("text"); b.Property<string>("Description").HasColumnType("text"); b.Property<string>("Status").IsRequired().HasMaxLength(16).HasColumnType("character varying(16)"); b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone"); b.Property<DateTimeOffset?>("DueDate").HasColumnType("timestamp with time zone"); b.HasKey("Id"); b.ToTable("tasks"); }); }
}
