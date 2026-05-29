namespace SummerBornInfo.Web.Tests.OpenApi;

public sealed class SchoolEndpointsOpenApiDocumentTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : SummerBornInfo.Web.Tests.API.Schools.SchoolApiIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenSchoolsCollectionOperation_WhenFetchedFromOpenApi_ThenParametersAndResponseMetadataMatchImplementedBehavior()
    {
        using var document = await GetOpenApiDocumentAsync();
        var getOperation = GetOperation(document, "/api/schools", "get");
        var parameterNames = getOperation.GetProperty("parameters")
            .EnumerateArray()
            .Select(parameter => parameter.GetProperty("name").GetString())
            .ToArray();

        Assert.Contains("cursor", parameterNames);
        Assert.Contains("pageSize", parameterNames);

        var responses = getOperation.GetProperty("responses");
        Assert.True(responses.TryGetProperty("200", out var okResponse));
        Assert.False(responses.TryGetProperty("400", out _));

        var schemaReference = okResponse.GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();

        Assert.Equal("#/components/schemas/SchoolsResponse", schemaReference);
    }

    [Fact]
    public async Task GivenSchoolSearchOperation_WhenFetchedFromOpenApi_ThenParametersAndErrorMetadataArePresent()
    {
        using var document = await GetOpenApiDocumentAsync();
        var getOperation = GetOperation(document, "/api/schools/search", "get");
        var parameterNames = getOperation.GetProperty("parameters")
            .EnumerateArray()
            .Select(parameter => parameter.GetProperty("name").GetString())
            .ToArray();

        Assert.Contains("q", parameterNames);
        Assert.Contains("urn", parameterNames);
        Assert.Contains("cursor", parameterNames);
        Assert.Contains("pageSize", parameterNames);

        var responses = getOperation.GetProperty("responses");
        Assert.True(responses.TryGetProperty("200", out var okResponse));
        Assert.True(responses.TryGetProperty("400", out var badRequestResponse));
        Assert.True(responses.TryGetProperty("404", out var notFoundResponse));

        AssertProblemResponseSchema(badRequestResponse);
        AssertProblemResponseSchema(notFoundResponse);

        var oneOfSchemas = okResponse.GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("oneOf")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(2, oneOfSchemas.Length);
    }

    [Fact]
    public async Task GivenNearbySchoolOperation_WhenFetchedFromOpenApi_ThenParametersAndResponseMetadataMatchContract()
    {
        using var document = await GetOpenApiDocumentAsync();
        var getOperation = GetOperation(document, "/api/schools/nearby", "get");
        var parameters = getOperation.GetProperty("parameters")
            .EnumerateArray()
            .ToArray();

        AssertParameter(
            parameters,
            "latitude",
            "query",
            isRequired: true,
            schemaType: "number",
            descriptionContains: "-90");
        AssertParameter(
            parameters,
            "longitude",
            "query",
            isRequired: true,
            schemaType: "number",
            descriptionContains: "-180");
        AssertParameter(
            parameters,
            "radiusMiles",
            "query",
            isRequired: true,
            schemaType: "number",
            descriptionContains: "100");
        AssertParameter(
            parameters,
            "cursor",
            "query",
            isRequired: false,
            schemaType: "string",
            descriptionContains: "opaque");
        AssertParameter(
            parameters,
            "pageSize",
            "query",
            isRequired: false,
            schemaType: "integer",
            descriptionContains: "200");

        var responses = getOperation.GetProperty("responses");
        Assert.True(responses.TryGetProperty("200", out var okResponse));
        Assert.True(responses.TryGetProperty("400", out var badRequestResponse));
        Assert.False(responses.TryGetProperty("404", out _));

        var schema = okResponse.GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema");

        AssertSchoolsResponseSchema(schema);
        AssertProblemResponseSchema(badRequestResponse);
    }

    [Fact]
    public async Task GivenSchoolSchemas_WhenFetchedFromOpenApi_ThenCorrectedFullDtoShapeIsExposed()
    {
        using var document = await GetOpenApiDocumentAsync();
        var schemas = document.RootElement
            .GetProperty("components")
            .GetProperty("schemas");

        var schoolsResponseSchema = schemas.GetProperty("SchoolsResponse");
        AssertRequiredProperties(schoolsResponseSchema, "schools", "nextCursor");

        var schoolItemsReference = schoolsResponseSchema.GetProperty("properties")
            .GetProperty("schools")
            .GetProperty("items")
            .GetProperty("$ref")
            .GetString();
        Assert.Equal("#/components/schemas/SchoolResponse", schoolItemsReference);

        var schoolResponseSchema = schemas.GetProperty("SchoolResponse");
        AssertRequiredProperties(
            schoolResponseSchema,
            "id",
            "urn",
            "ukprn",
            "establishmentNumber",
            "name",
            "address",
            "openDate",
            "closeDate",
            "phaseOfEducation",
            "localAuthority",
            "establishmentType",
            "establishmentGroup",
            "establishmentStatus");

        var schoolResponseProperties = schoolResponseSchema.GetProperty("properties");
        Assert.True(schoolResponseProperties.TryGetProperty("address", out var addressProperty));
        Assert.Equal("#/components/schemas/SchoolAddressResponse", addressProperty.GetProperty("$ref").GetString());
        Assert.True(schoolResponseProperties.TryGetProperty("urn", out _));
        Assert.True(schoolResponseProperties.TryGetProperty("ukprn", out _));
        Assert.True(schoolResponseProperties.TryGetProperty("latitude", out var latitudeProperty));
        Assert.True(schoolResponseProperties.TryGetProperty("longitude", out var longitudeProperty));
        AssertSchemaType(latitudeProperty, "number");
        AssertSchemaType(longitudeProperty, "number");
    }

    private async Task<JsonDocument> GetOpenApiDocumentAsync()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        return await JsonDocument.ParseAsync(contentStream, cancellationToken: TestContext.Current.CancellationToken);
    }

    private static JsonElement GetOperation(JsonDocument document, string path, string method)
    {
        return document.RootElement
            .GetProperty("paths")
            .GetProperty(path)
            .GetProperty(method);
    }

    private static void AssertRequiredProperties(JsonElement schema, params string[] expectedProperties)
    {
        var requiredProperties = schema.GetProperty("required")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();

        foreach (var expectedProperty in expectedProperties)
        {
            Assert.Contains(expectedProperty, requiredProperties);
        }
    }

    private static void AssertProblemResponseSchema(JsonElement response)
    {
        var content = response.GetProperty("content")
            .GetProperty("application/problem+json");
        var schemaReference = content
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();

        Assert.Equal("#/components/schemas/ProblemDetails", schemaReference);
    }

    private static void AssertSchoolsResponseSchema(JsonElement schema)
    {
        if (schema.TryGetProperty("$ref", out var schemaReference))
        {
            Assert.Equal("#/components/schemas/SchoolsResponse", schemaReference.GetString());
            return;
        }

        if (schema.TryGetProperty("allOf", out var allOfSchemas))
        {
            var referencedSchema = allOfSchemas
                .EnumerateArray()
                .FirstOrDefault(item => item.TryGetProperty("$ref", out _));

            if (referencedSchema.ValueKind != JsonValueKind.Undefined)
            {
                Assert.Equal("#/components/schemas/SchoolsResponse", referencedSchema.GetProperty("$ref").GetString());
                return;
            }
        }

        var properties = schema.GetProperty("properties");
        Assert.True(properties.TryGetProperty("schools", out var schoolsProperty));
        Assert.True(properties.TryGetProperty("nextCursor", out _));
        Assert.True(schoolsProperty.TryGetProperty("type", out var schoolsType));
        Assert.Equal("array", schoolsType.GetString());
    }

    private static void AssertParameter(
        JsonElement[] parameters,
        string name,
        string location,
        bool isRequired,
        string schemaType,
        string descriptionContains)
    {
        var parameter = parameters.Single(parameter =>
            string.Equals(parameter.GetProperty("name").GetString(), name, StringComparison.Ordinal));

        Assert.Equal(location, parameter.GetProperty("in").GetString());
        var actualIsRequired = parameter.TryGetProperty("required", out var requiredProperty)
            && requiredProperty.GetBoolean();
        Assert.Equal(isRequired, actualIsRequired);
        Assert.Contains(
            descriptionContains,
            parameter.GetProperty("description").GetString(),
            StringComparison.OrdinalIgnoreCase);
        AssertSchemaType(parameter.GetProperty("schema"), schemaType);
    }

    private static void AssertSchemaType(JsonElement schema, string expectedType)
    {
        var typeProperty = schema.GetProperty("type");

        if (typeProperty.ValueKind == JsonValueKind.String)
        {
            Assert.Equal(expectedType, typeProperty.GetString());
            return;
        }

        Assert.Contains(
            typeProperty.EnumerateArray().Select(item => item.GetString()),
            typeName => string.Equals(typeName, expectedType, StringComparison.Ordinal));
    }
}
