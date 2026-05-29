namespace SummerBornInfo.Web.OpenApi;

public static class SchoolEndpointOpenApiExtensions
{
    private static readonly string MaximumNearbyRadiusMilesText =
        SummerBornInfo.Features.Schools.Queries.GetNearbySchools.GetNearbySchoolsRequest.MaximumRadiusMiles.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static readonly string MaximumNearbyPageSizeText =
        SummerBornInfo.Features.Schools.Queries.GetNearbySchools.GetNearbySchoolsRequest.MaximumPageSize.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private const string InvalidNearbySearchRequestDescription =
        "Bad Request. Returned when latitude, longitude, radiusMiles, cursor, or pageSize are missing or invalid for nearby search.";

    public static RouteHandlerBuilder AddSchoolCollectionOpenApiMetadata(this RouteHandlerBuilder builder)
    {
        return builder
            .Produces<SchoolsResponse>(StatusCodes.Status200OK);
    }

    public static RouteHandlerBuilder AddNearbySchoolOpenApiMetadata(this RouteHandlerBuilder builder)
    {
        return builder
            .Produces<SchoolsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .AddOpenApiOperationTransformer(TransformNearbySchoolOperationAsync);
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

    private static async Task TransformNearbySchoolOperationAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schoolsResponseSchema = await context.GetOrCreateSchemaAsync(
            typeof(SchoolsResponse),
            parameterDescription: null,
            cancellationToken);

        DescribeQueryParameter(
            operation,
            "latitude",
            "Latitude of the search origin in decimal degrees. Must be between -90 and 90.",
            isRequired: true);
        DescribeQueryParameter(
            operation,
            "longitude",
            "Longitude of the search origin in decimal degrees. Must be between -180 and 180.",
            isRequired: true);
        DescribeQueryParameter(
            operation,
            "radiusMiles",
            $"Search radius in miles. Must be greater than 0 and no more than {MaximumNearbyRadiusMilesText}.",
            isRequired: true);
        DescribeQueryParameter(
            operation,
            "cursor",
            "Opaque nearby-search continuation token returned as nextCursor from a previous response.",
            isRequired: false);
        DescribeQueryParameter(
            operation,
            "pageSize",
            $"Optional page size. Must be between 1 and {MaximumNearbyPageSizeText}.",
            isRequired: false);

        operation.Responses ??= [];
        operation.Responses["200"] = new OpenApiResponse
        {
            Description = "OK",
            Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
            {
                ["application/json"] = new()
                {
                    Schema = schoolsResponseSchema,
                },
            },
        };

        DescribeProblemResponse(
            operation,
            StatusCodes.Status400BadRequest,
            InvalidNearbySearchRequestDescription);
    }

    private static void DescribeQueryParameter(
        OpenApiOperation operation,
        string name,
        string description,
        bool isRequired)
    {
        if (operation.Parameters is null)
        {
            return;
        }

        for (var index = 0; index < operation.Parameters.Count; index++)
        {
            var parameter = operation.Parameters[index];
            if (!string.Equals(parameter.Name, name, StringComparison.Ordinal))
            {
                if (!string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            operation.Parameters[index] = new OpenApiParameter
            {
                Name = name,
                In = parameter.In,
                Description = description,
                Required = isRequired,
                Schema = parameter.Schema,
            };

            return;
        }
    }

    private static void DescribeProblemResponse(
        OpenApiOperation operation,
        int statusCode,
        string description)
    {
        if (operation.Responses is null)
        {
            return;
        }

        var responseCode = statusCode.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (!operation.Responses.TryGetValue(responseCode, out var response))
        {
            return;
        }

        response.Description = description;
    }
}
