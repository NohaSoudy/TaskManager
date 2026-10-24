using System.Net.Http.Json;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TaskManager.Api.Data;
using Xunit;
namespace TaskManager.Api.Tests;
public sealed class PostgresIntegrationTests {
 [Fact] public async Task TaskEndpointsPersistValidateAndReturnNotFound() {
  DedicatedTestDatabase.GetRequiredConnectionString();
  await using var factory=new TaskApiFactory(); using var client=factory.CreateClient();
  using(var scope=factory.Services.CreateScope()) { var db=scope.ServiceProvider.GetRequiredService<TaskDbContext>(); await db.Database.MigrateAsync(); await db.Tasks.ExecuteDeleteAsync(); }
  Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync("/api/tasks",new { title=" " })).StatusCode);
  var response=await client.PostAsJsonAsync("/api/tasks",new { title="Learn EF",description="Practice" }); Assert.Equal(HttpStatusCode.Created,response.StatusCode); var json=await response.Content.ReadAsStringAsync(); Assert.Contains("Learn EF",json); Assert.Contains("Todo",json);
  var list=await client.GetStringAsync("/api/tasks"); Assert.Contains("Learn EF",list); using var doc=System.Text.Json.JsonDocument.Parse(json); var id=doc.RootElement.GetProperty("id").GetGuid();
  Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/api/tasks/"+id)).StatusCode); Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync("/api/tasks/"+Guid.NewGuid())).StatusCode);
  Assert.Equal(HttpStatusCode.NotFound,(await client.PutAsJsonAsync("/api/tasks/"+Guid.NewGuid(),new { title="missing",status="Todo" })).StatusCode);
  Assert.Equal(HttpStatusCode.BadRequest,(await client.PatchAsJsonAsync("/api/tasks/"+id+"/status",new { status="Blocked" })).StatusCode);
  Assert.Equal(HttpStatusCode.OK,(await client.PatchAsJsonAsync("/api/tasks/"+id+"/status",new { status="InProgress" })).StatusCode); Assert.Equal(HttpStatusCode.OK,(await client.PutAsJsonAsync("/api/tasks/"+id,new { title="Updated",description="Done",status="Done" })).StatusCode);
  Assert.Equal(HttpStatusCode.NotFound,(await client.PatchAsJsonAsync("/api/tasks/"+Guid.NewGuid()+"/status",new { status="Done" })).StatusCode); Assert.Equal(HttpStatusCode.NoContent,(await client.DeleteAsync("/api/tasks/"+id)).StatusCode); Assert.Equal(HttpStatusCode.NotFound,(await client.DeleteAsync("/api/tasks/"+id)).StatusCode);
 }
}
internal static class DedicatedTestDatabase
{
 public static string GetRequiredConnectionString()
 {
  var connection = Environment.GetEnvironmentVariable("TASKMANAGER_TEST_CONNECTION_STRING");
  if (string.IsNullOrWhiteSpace(connection))
   throw new InvalidOperationException("Set TASKMANAGER_TEST_CONNECTION_STRING to a local, dedicated TaskManagerTestDb before running PostgreSQL integration tests.");
  return Validate(connection);
 }

 public static string Validate(string connection)
 {
  if (string.IsNullOrWhiteSpace(connection))
   throw new InvalidOperationException("Set TASKMANAGER_TEST_CONNECTION_STRING to a local, dedicated TaskManagerTestDb before running PostgreSQL integration tests.");
  NpgsqlConnectionStringBuilder parsed;
  try { parsed = new NpgsqlConnectionStringBuilder(connection); }
  catch (ArgumentException) { throw new InvalidOperationException("TASKMANAGER_TEST_CONNECTION_STRING is malformed; configure a local TaskManagerTestDb connection."); }
  if (!string.Equals(parsed.Database, "TaskManagerTestDb", StringComparison.Ordinal))
   throw new InvalidOperationException("PostgreSQL integration tests require the dedicated database named TaskManagerTestDb; no database was modified.");
  if (parsed.Host is not ("localhost" or "127.0.0.1" or "::1"))
   throw new InvalidOperationException("PostgreSQL integration tests require a loopback PostgreSQL host; no database was modified.");
  return connection;
 }
}

internal sealed class TaskApiFactory : WebApplicationFactory<Program> { protected override void ConfigureWebHost(IWebHostBuilder builder) { var connection=DedicatedTestDatabase.GetRequiredConnectionString(); builder.UseSetting("ConnectionStrings:TaskManager",connection); builder.ConfigureServices(services=>{ services.Remove(services.Single(d=>d.ServiceType==typeof(DbContextOptions<TaskDbContext>))); services.AddDbContext<TaskDbContext>(o=>o.UseNpgsql(connection)); }); } }
