using MediatR;

namespace Application.TaskItems.Commands.DeleteTask
{
    public record DeleteTaskCommand(int Id) : IRequest;
}
