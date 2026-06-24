using MediatR;

namespace Application.TaskItems.Commands.UpdateTaskStatus
{
    public record UpdateTaskStatusCommand(int Id, TaskItemStatus Status) : IRequest;
}
