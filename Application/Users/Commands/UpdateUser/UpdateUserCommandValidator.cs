using Application.Common.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.UpdateUser
{
    internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator(IAppDbContext context)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MustAsync(async (command, email, cancellationToken) =>
                    !await context.Users.AnyAsync(u => u.Email == email && u.Id != command.Id, cancellationToken))
                .WithMessage("Email is already registered.");

            RuleFor(x => x.Role)
                .IsInEnum();
        }
    }
}
