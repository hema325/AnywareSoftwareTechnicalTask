using Application.Common.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Authentication.Commands.Register
{
    internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator(IAppDbContext context)
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
        }
    }
}
