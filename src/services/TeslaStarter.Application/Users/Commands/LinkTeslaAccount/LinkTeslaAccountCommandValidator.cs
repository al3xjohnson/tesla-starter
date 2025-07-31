namespace TeslaStarter.Application.Users.Commands.LinkTeslaAccount;

public sealed class LinkTeslaAccountCommandValidator : AbstractValidator<LinkTeslaAccountCommand>
{
    public LinkTeslaAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.TeslaAccountId)
            .NotEmpty().WithMessage("Tesla account ID is required.")
            .MaximumLength(255).WithMessage("Tesla account ID must not exceed 255 characters.");
    }
}
