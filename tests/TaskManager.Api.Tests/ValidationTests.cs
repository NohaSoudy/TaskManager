using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManager.Api.Contracts;
using TaskManager.Api.Data;
using Xunit;
namespace TaskManager.Api.Tests;
public sealed class ValidationTests {
 [Theory]
 [InlineData(null)]
 [InlineData("")]
 [InlineData("   ")]
 public void CreateTaskRequiresNonWhitespaceTitle(string? title) { var request=new CreateTaskRequest { Title=title! }; var results=new List<ValidationResult>(); Assert.False(Validator.TryValidateObject(request,new ValidationContext(request),results,true)); Assert.Contains(results,r=>r.MemberNames.Contains(nameof(CreateTaskRequest.Title))); }
 [Fact] public void StatusRequestRequiresAllowedValue() { var results=new List<ValidationResult>(); var request=new ChangeTaskStateRequest(); Assert.False(Validator.TryValidateObject(request,new ValidationContext(request),results,true)); var update = new UpdateTaskRequest { Title="x" }; Assert.False(Validator.TryValidateObject(update,new ValidationContext(update),new List<ValidationResult>(),true)); Assert.Equal(new[]{"Todo","InProgress","Done"},Enum.GetNames<TaskState>()); }
 [Fact] public void UnknownStatusStringCannotBeDeserialized() { var options=new JsonSerializerOptions(); options.Converters.Add(new JsonStringEnumConverter<TaskState>(allowIntegerValues:false)); Assert.Throws<JsonException>(()=>JsonSerializer.Deserialize<TaskState>("\"Blocked\"",options)); }
}
