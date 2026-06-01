namespace SummerbornInfo.PostgresDockerImage;

public static class PostgresDockerfilePath
{
    public static string PostgreSqlDockerfileDirectory { get; } = Assembly.GetExecutingAssembly().Location;
}
