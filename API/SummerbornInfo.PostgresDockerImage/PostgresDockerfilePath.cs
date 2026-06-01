namespace SummerbornInfo.PostgresDockerImage;

public static class PostgresDockerfilePath
{
    private const string SolutionFileName = "SummerBornInfo.sln";
    private static readonly string SolutionDirectory = FindSolutionDirectory();

    public static string PostgreSqlDockerfileDirectory { get; } = Path.Combine(
        SolutionDirectory,
        "SummerbornInfo.PostgresDockerImage");

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
