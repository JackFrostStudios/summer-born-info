using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using SummerBornInfo.Features.Schools.Queries.GetAllSchools.Response;

namespace SummerBornInfo.Web.OpenApi;

public static class SchoolEndpointOpenApiExtensions
{
    public static RouteHandlerBuilder AddSchoolCollectionOpenApiMetadata(this RouteHandlerBuilder builder)
    {
        return builder
            .Produces<SchoolsResponse>(StatusCodes.Status200OK);
    }

    public static RouteHandlerBuilder AddSchoolSearchOpenApiMetadata(this RouteHandlerBuilder builder)
    {
        return builder
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AddOpenApiOperationTransformer(TransformSchoolSearchOperationAsync);
    }

    private static async Task TransformSchoolSearchOperationAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schoolsResponseSchema = await context.GetOrCreateSchemaAsync(
            typeof(SchoolsResponse),
            parameterDescription: null,
            cancellationToken);
        var schoolResponseSchema = await context.GetOrCreateSchemaAsync(
            typeof(SchoolResponse),
            parameterDescription: null,
            cancellationToken);

        operation.Responses ??= [];
        operation.Responses["200"] = new OpenApiResponse
        {
            Description = "OK",
            Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
            {
                ["application/json"] = new()
                {
                    Schema = new OpenApiSchema
                    {
                        OneOf =
                        [
                            schoolsResponseSchema,
                            schoolResponseSchema,
                        ],
                    },
                },
            },
        };
    }
}
