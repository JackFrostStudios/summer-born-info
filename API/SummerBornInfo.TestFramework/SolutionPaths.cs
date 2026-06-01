namespace SummerBornInfo.TestFramework;

internal static class SolutionPaths
{
    private const string SolutionFileName = "SummerBornInfo.sln";
    private static readonly string SolutionDirectory = FindSolutionDirectory();

    public static string PostgreSqlDockerfileDirectory { get; } = Path.Combine(
        SolutionDirectory,
        "SummerBornInfo.AppHost",
        "Postgres");

    private static string FindSolutionDirectory()
    {
        for (var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);
             currentDirectory is not null;
             currentDirectory = currentDirectory.Parent)
        {
            if (File.Exists(Path.Combine(currentDirectory.FullName, SolutionFileName)))
            {
                return currentDirectory.FullName;
            }
        }

        throw new InvalidOperationException(
            $"Could not locate {SolutionFileName} from base directory '{AppContext.BaseDirectory}'.");
    }
}
