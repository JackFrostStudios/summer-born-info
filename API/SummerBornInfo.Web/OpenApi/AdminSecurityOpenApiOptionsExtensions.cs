using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SummerBornInfo.Web.OpenApi;

public static class AdminSecurityOpenApiOptionsExtensions
{
    internal const string IdentityApplicationCookieSecuritySchemeName = "IdentityApplicationCookie";

    public static OpenApiOptions AddAdminSecurityMetadata(this OpenApiOptions options)
    {
        return options
            .AddDocumentTransformer(AddCookieSecuritySchemeAsync)
            .AddOperationTransformer(AddProtectedAdminOperationMetadataAsync);
    }

    private static Task AddCookieSecuritySchemeAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var optionsMonitor = context.ApplicationServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var cookieOptions = optionsMonitor.Get(IdentityConstants.ApplicationScheme);
        var cookieName = cookieOptions.Cookie.Name
            ?? throw new InvalidOperationException("Identity application cookie name is not configured.");

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
        document.Components.SecuritySchemes[IdentityApplicationCookieSecuritySchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Cookie,
            Name = cookieName,
            Description = "Identity application cookie used for protected admin operations.",
        };

        return Task.CompletedTask;
    }

    private static Task AddProtectedAdminOperationMetadataAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsProtectedAdminOperation(context.Description))
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];
        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(
                    IdentityApplicationCookieSecuritySchemeName,
                    context.Document,
                    externalResource: null)] = [],
            });

        operation.Responses ??= [];
        _ = operation.Responses.TryAdd(
            "401",
            new OpenApiResponse
            {
                Description = "Unauthorized",
            });
        _ = operation.Responses.TryAdd(
            "403",
            new OpenApiResponse
            {
                Description = "Forbidden",
            });

        return Task.CompletedTask;
    }

    private static bool IsProtectedAdminOperation(ApiDescription description)
    {
        if (description.RelativePath is null
            || !description.RelativePath.StartsWith("api/admin/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var endpointMetadata = description.ActionDescriptor.EndpointMetadata;
        if (endpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            return false;
        }

        return endpointMetadata.OfType<IAuthorizeData>().Any();
    }
}
