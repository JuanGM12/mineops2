using Microsoft.OpenApi;

namespace MineOps.Identity.Api.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddMineOpsOpenApi(this IServiceCollection services)
    {
        services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "MineOps Identity API",
            Version = "v1",
            Description = "Autenticación y emisión de tokens JWT para MineOps."
        }));
        return services;
    }
}
