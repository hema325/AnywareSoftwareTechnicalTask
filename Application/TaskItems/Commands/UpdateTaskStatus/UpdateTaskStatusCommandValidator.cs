using FluentValidation;

namespace Application.TaskItems.Commands.UpdateTaskStatus
{
    internal sealed class UpdateTaskStatusCommandValidator : AbstractValidator<UpdateTaskStatusCommand>
    {
        public UpdateTaskStatusCommandValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum();
        }
    }
}
