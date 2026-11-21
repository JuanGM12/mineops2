using MineOps.Planning.Application.Exceptions;

namespace MineOps.Planning.Application.Activities;

internal static class ActivityRequestValidator
{
    public static void ValidateRequired(string code, string title)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(code))
        {
            errors.Add("Code is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add("Title is required.");
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException("The request is invalid.", errors);
        }
    }

    public static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new RequestValidationException("Title is required.");
        }
    }
}
