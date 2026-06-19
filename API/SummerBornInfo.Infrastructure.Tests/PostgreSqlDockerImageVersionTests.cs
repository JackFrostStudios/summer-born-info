namespace SummerBornInfo.Infrastructure.Tests;

public sealed class PostgreSqlDockerImageVersionTests
{
    [Fact]
    public void GivenSharedDockerfile_WhenReadingVersion_ThenReturnsConfiguredVersion()
    {
        Assert.Equal("1.0.0", PostgreSqlDockerImageVersion.Version);
    }

    [Theory]
    [MemberData(nameof(GetInvalidDockerfileContents))]
    public void GivenInvalidDockerfileContents_WhenParsingVersion_ThenThrowsClearException(
        string dockerfileContents,
        string expectedMessageFragment)
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => PostgreSqlDockerImageVersion.ParseVersion(dockerfileContents));

        Assert.Contains(expectedMessageFragment, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GivenValidAndMalformedDuplicateVersionLabels_WhenParsingVersion_ThenThrowsClearException()
    {
        const string dockerfileContents = "FROM postgres:18\nLABEL org.opencontainers.image.version=\"1.0.0\"\nLABEL org.opencontainers.image.version";

        var exception = Assert.Throws<InvalidOperationException>(
            () => PostgreSqlDockerImageVersion.ParseVersion(dockerfileContents));

        Assert.Contains("to use the format", exception.Message, StringComparison.Ordinal);
    }

    public static TheoryData<string, string> GetInvalidDockerfileContents()
    {
        return new TheoryData<string, string>
        {
            { "FROM postgres:18", "to be present exactly once" },
            { "LABEL org.opencontainers.image.version=", "to use the format" },
            { "LABEL org.opencontainers.image.version=\"   \"", "must not be blank" },
            { "LABEL org.opencontainers.image.version=\"1.0.0\"\nLABEL org.opencontainers.image.version=\"1.0.1\"", "found 2 values" },
            { "LABEL org.opencontainers.image.version=\"1.0.0/rc1\"", "is not valid for use as a Docker tag" },
        };
    }
}
