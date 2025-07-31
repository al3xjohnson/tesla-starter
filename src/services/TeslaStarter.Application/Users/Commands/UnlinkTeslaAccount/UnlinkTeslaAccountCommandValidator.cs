namespace TeslaStarter.Application.Users.Commands.UnlinkTeslaAccount;

public sealed class UnlinkTeslaAccountCommandValidator : AbstractValidator<UnlinkTeslaAccountCommand>
{
    public UnlinkTeslaAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
