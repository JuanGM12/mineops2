using System.Text;
using Microsoft.Extensions.Options;

namespace MineOps.Identity.Infrastructure.Auth;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    private const int MinimumSigningKeyBytes = 32;

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add("Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Jwt:Audience is required.");
        }

        if (Encoding.UTF8.GetByteCount(options.SigningKey) < MinimumSigningKeyBytes)
        {
            failures.Add($"Jwt:SigningKey must contain at least {MinimumSigningKeyBytes} UTF-8 bytes.");
        }

        if (options.ExpirationMinutes <= 0)
        {
            failures.Add("Jwt:ExpirationMinutes must be greater than zero.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
