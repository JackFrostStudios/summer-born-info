namespace SummerBornInfo.Infrastructure.Persistence.LargeObjects;

public class LargeObjectReader(ApplicationDbContext context) : ILargeObjectReader
{
    public async Task<Stream?> ReadLargeObjectAsStreamAsync(uint largeObjectId, CancellationToken cancellationToken = default)
    {
        return await LargeObjectExistsAsync(largeObjectId, cancellationToken)
            ? new LargeObjectStream(context.GetNpgsqlConnection(), largeObjectId)
            : null;
    }

    private async Task<bool> LargeObjectExistsAsync(uint objectId, CancellationToken cancellationToken)
    {
        await context.Database.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand cmd = new(
        "SELECT EXISTS(SELECT 1 FROM pg_largeobject_metadata WHERE oid = @oid)",
        context.GetNpgsqlConnection()
        );
        cmd.Parameters.AddWithValue("oid", NpgsqlDbType.Oid, objectId);

        return (bool)(await cmd.ExecuteScalarAsync(cancellationToken))!;
    }
}
