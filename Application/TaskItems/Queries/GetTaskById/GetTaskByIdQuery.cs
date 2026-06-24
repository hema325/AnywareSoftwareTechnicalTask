using Application.TaskItems.Dtos;
using MediatR;

namespace Application.TaskItems.Queries.GetTaskById
{
    public record GetTaskByIdQuery(int Id) : IRequest<TaskItemDto>;
}
