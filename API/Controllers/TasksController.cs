using Application.TaskItems.Commands.CreateTask;
using Application.TaskItems.Commands.DeleteTask;
using Application.TaskItems.Commands.UpdateTaskStatus;
using Application.TaskItems.Dtos;
using Application.TaskItems.Queries.GetTaskById;
using Application.TaskItems.Queries.GetTasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ISender _sender;

        public TasksController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<TaskItemDto>>> GetTasks(CancellationToken cancellationToken)
        {
            var tasks = await _sender.Send(new GetTasksQuery(), cancellationToken);
            return Ok(tasks);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskItemDto>> GetTaskById(int id, CancellationToken cancellationToken)
        {
            var task = await _sender.Send(new GetTaskByIdQuery(id), cancellationToken);
            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateTask(CreateTaskCommand command, CancellationToken cancellationToken)
        {
            var id = await _sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetTaskById), new { id }, id);
        }

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateTaskStatusCommand command, CancellationToken cancellationToken)
        {
            await _sender.Send(command with { Id = id }, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTask(int id, CancellationToken cancellationToken)
        {
            await _sender.Send(new DeleteTaskCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
