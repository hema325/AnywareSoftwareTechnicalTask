using Application.TaskItems.Dtos;
using MediatR;

namespace Application.TaskItems.Queries.GetTasks
{
    public record GetTasksQuery : IRequest<IReadOnlyList<TaskItemDto>>;
}
