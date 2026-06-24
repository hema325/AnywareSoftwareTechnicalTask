using Application.Common.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.CreateUser
{
    internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator(IAppDbContext context)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MustAsync(async (email, cancellationToken) =>
                    !await context.Users.AnyAsync(u => u.Email == email, cancellationToken))
                .WithMessage("Email is already registered.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);

            RuleFor(x => x.Role)
                .IsInEnum();
        }
    }
}
