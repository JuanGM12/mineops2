using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using MineOps.Planning.Api.Errors;

namespace MineOps.Planning.IntegrationTests.Api;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task UnexpectedExceptionReturnsSafeFiveHundredContract()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException("Sensitive internal detail."),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("An unexpected error occurred.", document.RootElement.GetProperty("message").GetString());
        Assert.DoesNotContain("Sensitive", document.RootElement.ToString(), StringComparison.Ordinal);
    }
}
