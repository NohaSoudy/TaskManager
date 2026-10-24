using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using TaskManager.Api.Contracts;
using TaskManager.Api.Data;
using WorkStatus = TaskManager.Api.Data.TaskState;
namespace TaskManager.Api.Controllers;
/// <summary>Creates and manages tasks.</summary>
[ApiController, Route("api/tasks")]
public sealed class TasksController(TaskDbContext db) : ControllerBase {
 /// <summary>Create a task. Title is required; description and due date are optional.</summary>
 /// <response code="201">Returns the created task and its location.</response>
 /// <response code="400">The request failed validation.</response>
 /// <response code="500">An unexpected server error occurred.</response>
 [HttpPost]
 [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
 [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
 public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest r, CancellationToken ct) { var t = new TaskEntity { Id=Guid.NewGuid(), Title=r.Title.Trim(), Description=r.Description, DueDate=r.DueDate?.ToUniversalTime(), Status=WorkStatus.Todo, CreatedAt=DateTimeOffset.UtcNow }; db.Tasks.Add(t); await db.SaveChangesAsync(ct); return CreatedAtAction(nameof(GetById), new { id=t.Id }, TaskResponse.From(t)); }

 /// <summary>Retrieve all tasks ordered by creation time.</summary>
 /// <response code="200">Returns the task collection.</response>
 /// <response code="500">An unexpected server error occurred.</response>
 [HttpGet]
 [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
 public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll(CancellationToken ct) => Ok((await db.Tasks.AsNoTracking().OrderBy(x=>x.CreatedAt).ToListAsync(ct)).Select(TaskResponse.From).ToArray());

 /// <summary>Retrieve a task by ID.</summary>
 /// <response code="200">Returns the task.</response>
 /// <response code="404">No task has the requested ID.</response>
 /// <response code="500">An unexpected server error occurred.</response>
 [HttpGet("{id:guid}")]
 [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
 public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken ct) { var t=await db.Tasks.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct); return t is null ? NotFound() : Ok(TaskResponse.From(t)); }

 /// <summary>Replace the editable fields of a task.</summary>
 /// <response code="200">Returns the updated task.</response>
 /// <response code="400">The request failed validation.</response>
 /// <response code="404">No task has the requested ID.</response>
 /// <response code="500">An unexpected server error occurred.</response>
 [HttpPut("{id:guid}")]
 [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
 [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
 public async Task<ActionResult<TaskResponse>> Update(Guid id, UpdateTaskRequest r, CancellationToken ct) { var t=await db.Tasks.SingleOrDefaultAsync(x=>x.Id==id,ct); if(t is null)return NotFound(); t.Title=r.Title.Trim(); t.Description=r.Description; t.Status=r.Status!.Value; t.DueDate=r.DueDate?.ToUniversalTime(); try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return NotFound(); } return Ok(TaskResponse.From(t)); }

 /// <summary>Change a task status to Todo, InProgress, or Done.</summary>
 /// <response code="200">Returns the task with its updated status.</response>
 /// <response code="400">The request failed validation.</response>
 /// <response code="404">No task has the requested ID.</response>
 /// <response code="500">An unexpected server error occurred.</response>
 [HttpPatch("{id:guid}/status")]
 [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
 [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
 public async Task<ActionResult<TaskResponse>> ChangeStatus(Guid id, ChangeTaskStateRequest r, CancellationToken ct) { var t=await db.Tasks.SingleOrDefaultAsync(x=>x.Id==id,ct); if(t is null)return NotFound(); t.Status=r.Status!.Value; try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return NotFound(); } return Ok(TaskResponse.From(t)); }

 /// <summary>Delete a task.</summary>
 /// <response code="204">The task was deleted.</response>
 /// <response code="404">No task has the requested ID.</response>
 /// <response code="500">An unexpected server error occurred.</response>
 [HttpDelete("{id:guid}")]
 [ProducesResponseType(StatusCodes.Status204NoContent)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
 public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { var t=await db.Tasks.SingleOrDefaultAsync(x=>x.Id==id,ct); if(t is null)return NotFound(); db.Tasks.Remove(t); try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return NotFound(); } return NoContent(); }
}
