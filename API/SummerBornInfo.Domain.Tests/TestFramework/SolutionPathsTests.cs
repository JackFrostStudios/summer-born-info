namespace SummerBornInfo.Domain.Tests.TestFramework;

public sealed class SolutionPathsTests
{
    [Fact]
    public void PostgreSqlDockerfileDirectory_ShouldResolveToTheAppHostPostgresFolder()
    {
        var dockerfileDirectory = SummerBornInfo.TestFramework.SolutionPaths.PostgreSqlDockerfileDirectory;

        Assert.EndsWith(
            Path.Combine("SummerBornInfo.AppHost", "Postgres"),
            dockerfileDirectory,
            StringComparison.Ordinal);
        Assert.True(Directory.Exists(dockerfileDirectory));
        Assert.True(File.Exists(Path.Combine(dockerfileDirectory, "Dockerfile")));
    }
}
