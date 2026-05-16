namespace SummerBornInfo.Infrastructure.Persistence.LargeObjects;

public sealed class LargeObjectWriter(ApplicationDbContext context) : ILargeObjectWriter
{
    public async Task<uint> StreamContentToNewLargeObjectAsync(Stream content, CancellationToken cancellationToken = default)
    {
        await using var transaction = await StartTransactionIfNoneExistsAsync(cancellationToken);

        var npgsqlConnection = context.GetNpgsqlConnection();

        var largeObjectId = await CreateLargeObjectAsync(npgsqlConnection, cancellationToken);

        var fileDescriptor = await OpenLargeObjectAsync(npgsqlConnection, largeObjectId, cancellationToken);

        await WriteToLargeObjectAsync(npgsqlConnection, content, fileDescriptor, cancellationToken);

        await CloseLargeObjectAsync(npgsqlConnection, fileDescriptor, cancellationToken);

        if (transaction != null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        return largeObjectId;
    }

    private async Task<IDbContextTransaction?> StartTransactionIfNoneExistsAsync(CancellationToken cancellationToken)
    {
        if (context.Database.CurrentTransaction != null)
        {
            return null;
        }
        return await context.Database.BeginTransactionAsync(cancellationToken);
    }

    private static async Task<uint> CreateLargeObjectAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using NpgsqlCommand createCmd = new("SELECT lo_create(0)", connection);
        var createLargeObjectResult = await createCmd.ExecuteScalarAsync(cancellationToken)
            ?? throw new LargeObjectCreationException("Unable to create large object");

        if (createLargeObjectResult is not uint largeObjectId)
        {
            throw new LargeObjectCreationException("Unable to create large object");
        }
        return largeObjectId;
    }

    private static async Task<int> OpenLargeObjectAsync(NpgsqlConnection connection, uint largeObjectId, CancellationToken cancellationToken)
    {
        // Open the large object for writing (INV_WRITE = 0x20000)
        await using NpgsqlCommand openCommand = new("SELECT lo_open($1, 131072)", connection);
        openCommand.Parameters.AddWithValue(NpgsqlDbType.Oid, largeObjectId);
        var openLargeObjectResult = await openCommand.ExecuteScalarAsync(cancellationToken)!;

        if (openLargeObjectResult is not int fileDescriptor || fileDescriptor < 0)
        {
            throw new Exceptions.LargeObjectOpenException("Unable to open large object");
        }

        return fileDescriptor;
    }

    private static async Task WriteToLargeObjectAsync(NpgsqlConnection connection, Stream content, int fileDescriptor, CancellationToken cancellationToken)
    {
        const int chunkSize = 8192;
        var buffer = new byte[chunkSize];
        int bytesRead;

        while ((bytesRead = await content.ReadAsync(buffer.AsMemory(0, chunkSize), cancellationToken)) > 0)
        {
            var chunk = bytesRead == chunkSize ? buffer : buffer[..bytesRead];

            await using NpgsqlCommand writeCommand = new("SELECT lowrite($1, $2)", connection);
            writeCommand.Parameters.AddWithValue(fileDescriptor);
            writeCommand.Parameters.AddWithValue(chunk);
            await writeCommand.ExecuteScalarAsync(cancellationToken);
        }
    }

    private static async Task CloseLargeObjectAsync(NpgsqlConnection connection, int fileDescriptor, CancellationToken cancellationToken)
    {
        await using NpgsqlCommand closeCommand = new("SELECT lo_close($1)", connection);
        closeCommand.Parameters.AddWithValue(fileDescriptor);
        await closeCommand.ExecuteScalarAsync(cancellationToken);
    }
}
