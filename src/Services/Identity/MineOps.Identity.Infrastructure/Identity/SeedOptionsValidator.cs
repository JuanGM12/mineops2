using Microsoft.Extensions.Options;

namespace MineOps.Identity.Infrastructure.Identity;

public sealed class SeedOptionsValidator : IValidateOptions<SeedOptions>
{
    public ValidateOptionsResult Validate(string? name, SeedOptions options) =>
        string.IsNullOrWhiteSpace(options.DefaultPassword)
            ? ValidateOptionsResult.Fail("Seed:DefaultPassword is required.")
            : ValidateOptionsResult.Success;
}
