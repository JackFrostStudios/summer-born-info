namespace SummerBornInfo.Web.Tests.OpenApi;

public sealed class SchoolEndpointsOpenApiDocumentTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenSchoolsCollectionOperation_WhenFetchedFromOpenApi_ThenParametersAndBadRequestMetadataArePresent()
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
        Assert.True(responses.TryGetProperty("400", out _));

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
        Assert.True(responses.TryGetProperty("400", out _));
        Assert.True(responses.TryGetProperty("404", out _));

        var oneOfSchemas = okResponse.GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("oneOf")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(2, oneOfSchemas.Length);
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

}
