using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TaskManager.Api.Data;
#nullable disable
namespace TaskManager.Api.Migrations;
[DbContext(typeof(TaskDbContext))]
[Migration("202610130001_InitialCreate")]
public sealed class InitialCreate : Migration {
 protected override void Up(MigrationBuilder migrationBuilder) { migrationBuilder.CreateTable(name:"tasks", columns: table => new { Id=table.Column<Guid>(type:"uuid",nullable:false), Title=table.Column<string>(type:"text",nullable:false), Description=table.Column<string>(type:"text",nullable:true), Status=table.Column<string>(type:"character varying(16)",maxLength:16,nullable:false), CreatedAt=table.Column<DateTimeOffset>(type:"timestamp with time zone",nullable:false), DueDate=table.Column<DateTimeOffset>(type:"timestamp with time zone",nullable:true) }, constraints: table => table.PrimaryKey("PK_tasks",x=>x.Id)); }
 protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name:"tasks");
}
