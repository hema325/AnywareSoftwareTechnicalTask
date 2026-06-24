using MediatR;

namespace Application.TaskItems.Commands.CreateTask
{
    public record CreateTaskCommand(string Title, string Description, TaskItemPriority Priority) : IRequest<int>;
}
