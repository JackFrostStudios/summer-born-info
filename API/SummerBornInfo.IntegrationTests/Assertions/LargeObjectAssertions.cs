namespace SummerBornInfo.TestFramework.Assertions;

public static class LargeObjectAssertions
{
    public static async Task AssertLargeObjectExistsAsync(uint objectId, string connectionString, CancellationToken cancellationToken)
    {
        var exists = await DoesLargeObjectExistAsync(objectId, connectionString, cancellationToken);
        Assert.True(exists);
    }

    public static async Task AssertLargeObjectDoesNotExistsAsync(uint objectId, string connectionString, CancellationToken cancellationToken)
    {
        var exists = await DoesLargeObjectExistAsync(objectId, connectionString, cancellationToken);
        Assert.False(exists);
    }

    private static async Task<bool> DoesLargeObjectExistAsync(uint objectId, string connectionString, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection conn = new(connectionString);
        await conn.OpenAsync(cancellationToken);
        await using NpgsqlCommand cmd = new(
        "SELECT EXISTS(SELECT 1 FROM pg_largeobject_metadata WHERE oid = @oid)",
        conn
        );
        cmd.Parameters.AddWithValue("oid", NpgsqlDbType.Oid, objectId);

        return (bool)(await cmd.ExecuteScalarAsync(cancellationToken))!;
    }

    public static async Task AssertLargeObjectEqualsOriginalAsync(uint objectId, Stream originalObject, string connectionString, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection conn = new(connectionString);
        await conn.OpenAsync(cancellationToken);

        await using NpgsqlCommand cmd = new("SELECT lo_get(@oid)", conn);
        cmd.Parameters.AddWithValue("oid", NpgsqlTypes.NpgsqlDbType.Oid, objectId);

        var data = (byte[])(await cmd.ExecuteScalarAsync(cancellationToken))!;

        if (originalObject.CanSeek)
        {
            originalObject.Seek(0, SeekOrigin.Begin);
        }

        byte[] originalData;
        using (MemoryStream memoryStream = new())
        {
            await originalObject.CopyToAsync(memoryStream, cancellationToken);
            originalData = memoryStream.ToArray();
        }

        Assert.Equal(originalData.Length, data.Length);

        Assert.Equal(originalData, data);
    }
}
