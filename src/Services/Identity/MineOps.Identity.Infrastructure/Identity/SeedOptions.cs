namespace MineOps.Identity.Infrastructure.Identity;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string DefaultPassword { get; init; } = string.Empty;
}
