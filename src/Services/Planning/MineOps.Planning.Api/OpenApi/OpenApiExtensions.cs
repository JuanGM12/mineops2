using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MineOps.Planning.Api.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddMineOpsOpenApi(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "MineOps Planning API", Version = "v1" });
            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Token JWT emitido por Identity. Escriba únicamente el token."
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
            options.OperationFilter<AuthorizationRolesOperationFilter>();
        });
        return services;
    }
}

internal sealed class AuthorizationRolesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var attributes = context.MethodInfo.DeclaringType!
            .GetCustomAttributes(true)
            .Concat(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>()
            .ToArray();
        if (attributes.Length == 0) return;

        var roles = attributes.SelectMany(attribute => (attribute.Roles ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var access = roles.Length == 0
            ? "Requiere un usuario autenticado."
            : $"Roles permitidos: {string.Join(", ", roles)}.";
        operation.Description = string.IsNullOrWhiteSpace(operation.Description)
            ? access
            : $"{operation.Description} {access}";
    }
}
