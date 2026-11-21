using MineOps.Planning.Application.Exceptions;

namespace MineOps.Planning.Application.Common;

internal static class Pagination
{
    public static void Validate(int page, int pageSize)
    {
        var errors = new List<string>();

        if (page < 1)
        {
            errors.Add("Page must be greater than or equal to 1.");
        }

        if (pageSize is < 1 or > 100)
        {
            errors.Add("Page size must be between 1 and 100.");
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException("Invalid pagination parameters.", errors);
        }
    }
}
