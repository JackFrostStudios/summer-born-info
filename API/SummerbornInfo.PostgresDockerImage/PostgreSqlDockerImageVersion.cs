namespace SummerbornInfo.PostgresDockerImage;

public static class PostgreSqlDockerImageVersion
{
    private const string VersionLabelKey = "org.opencontainers.image.version";

    public static string Version { get; } = ReadVersionFromDockerfile(PostgresDockerfilePath.PostgreSqlDockerfileFilePath);

    internal static string ReadVersionFromDockerfile(string dockerfilePath)
    {
        if (string.IsNullOrWhiteSpace(dockerfilePath))
        {
            throw new ArgumentException("Dockerfile path must be provided.", nameof(dockerfilePath));
        }

        if (!File.Exists(dockerfilePath))
        {
            throw new InvalidOperationException(
                $"Could not find the PostgreSQL Dockerfile at '{dockerfilePath}'.");
        }

        var dockerfileContents = File.ReadAllText(dockerfilePath);

        try
        {
            return ParseVersion(dockerfileContents);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException(
                $"Could not read '{VersionLabelKey}' from PostgreSQL Dockerfile '{dockerfilePath}'. {exception.Message}",
                exception);
        }
    }

    internal static string ParseVersion(string dockerfileContents)
    {
        ArgumentNullException.ThrowIfNull(dockerfileContents);

        var discoveredVersions = new List<string>();
        var sawMalformedVersionLabel = false;

        foreach (var rawLine in dockerfileContents.Split(['\r', '\n'], StringSplitOptions.None))
        {
            var trimmedLine = rawLine.Trim();
            if (!trimmedLine.StartsWith("LABEL", StringComparison.OrdinalIgnoreCase) ||
                !trimmedLine.Contains(VersionLabelKey, StringComparison.Ordinal))
            {
                continue;
            }

            if (!TryExtractVersionValue(trimmedLine, out var discoveredVersion))
            {
                sawMalformedVersionLabel = true;
                continue;
            }

            discoveredVersions.Add(discoveredVersion);
        }

        if (sawMalformedVersionLabel)
        {
            throw new InvalidOperationException(
                $"Expected Dockerfile label '{VersionLabelKey}' to use the format LABEL {VersionLabelKey}=\"<version>\" exactly once.");
        }

        if (discoveredVersions.Count == 0)
        {
            throw new InvalidOperationException(
                $"Expected Dockerfile label '{VersionLabelKey}' to be present exactly once.");
        }

        if (discoveredVersions.Count > 1)
        {
            throw new InvalidOperationException(
                $"Expected Dockerfile label '{VersionLabelKey}' to be present exactly once, but found {discoveredVersions.Count} values.");
        }

        var version = discoveredVersions[0].Trim();
        if (version.Length == 0)
        {
            throw new InvalidOperationException(
                $"Dockerfile label '{VersionLabelKey}' must not be blank.");
        }

        if (!IsValidDockerTag(version))
        {
            throw new InvalidOperationException(
                $"Dockerfile label '{VersionLabelKey}' value '{version}' is not valid for use as a Docker tag. Expected 1-128 characters matching [A-Za-z0-9_][A-Za-z0-9_.-]*.");
        }

        return version;
    }

    private static bool TryExtractVersionValue(string trimmedLine, out string version)
    {
        version = string.Empty;

        var labelIndex = trimmedLine.IndexOf(VersionLabelKey, StringComparison.Ordinal);
        if (labelIndex < 0)
        {
            return false;
        }

        var valueStartIndex = labelIndex + VersionLabelKey.Length;
        while (valueStartIndex < trimmedLine.Length && char.IsWhiteSpace(trimmedLine[valueStartIndex]))
        {
            valueStartIndex++;
        }

        if (valueStartIndex >= trimmedLine.Length || trimmedLine[valueStartIndex] != '=')
        {
            return false;
        }

        valueStartIndex++;
        while (valueStartIndex < trimmedLine.Length && char.IsWhiteSpace(trimmedLine[valueStartIndex]))
        {
            valueStartIndex++;
        }

        if (valueStartIndex >= trimmedLine.Length)
        {
            return false;
        }

        if (trimmedLine[valueStartIndex] == '"')
        {
            var closingQuoteIndex = trimmedLine.IndexOf('"', valueStartIndex + 1);
            if (closingQuoteIndex < 0)
            {
                return false;
            }

            version = trimmedLine[(valueStartIndex + 1)..closingQuoteIndex];
            return true;
        }

        var valueEndIndex = valueStartIndex;
        while (valueEndIndex < trimmedLine.Length && !char.IsWhiteSpace(trimmedLine[valueEndIndex]))
        {
            valueEndIndex++;
        }

        version = trimmedLine[valueStartIndex..valueEndIndex];
        return true;
    }

    private static bool IsValidDockerTag(string version)
    {
        if (version.Length is 0 or > 128)
        {
            return false;
        }

        if (!IsValidTagCharacter(version[0], allowPeriod: false, allowHyphen: false))
        {
            return false;
        }

        for (var index = 1; index < version.Length; index++)
        {
            if (!IsValidTagCharacter(version[index], allowPeriod: true, allowHyphen: true))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidTagCharacter(char character, bool allowPeriod, bool allowHyphen)
    {
        return char.IsAsciiLetterOrDigit(character) ||
               character == '_' ||
               (allowPeriod && character == '.') ||
               (allowHyphen && character == '-');
    }
}
