namespace SummerBornInfo.Features.Schools.Commands.Import;

public sealed class ImportSchoolsCommandHandler(ApplicationDbContext context)
{
    public async Task<ImportSchoolsResponse> ExecuteAsync(ImportSchoolsCommand command, CancellationToken cancellationToken)
    {
        await using var efTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        
        var npgsqlConnection = (NpgsqlConnection)context.Database.GetDbConnection();
        var npgsqlTransaction = (NpgsqlTransaction)efTransaction.GetDbTransaction();

        var largeObjectId = await StreamContentToNewLargeObjectAsync(npgsqlConnection, npgsqlTransaction, command.Content, cancellationToken);

        var schoolBulkImportRequest = await SaveNewSchoolBulkImportRequestAsync(largeObjectId, cancellationToken);

        await efTransaction.CommitAsync(cancellationToken);

        return new ImportSchoolsResponse(schoolBulkImportRequest.Id);
    }

    public static async Task<uint> StreamContentToNewLargeObjectAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Stream content, CancellationToken cancellationToken = default)
    {
        var largeObjectId = await CreateLargeObjectAsync(connection, transaction, cancellationToken);

        var fileDescriptor = await OpenLargeObjectAsync(connection, transaction, largeObjectId, cancellationToken);

        await WriteToLargeObjectAsync(connection, transaction, content, fileDescriptor, cancellationToken);
        
        await CloseLargeObjectAsync(connection, transaction, fileDescriptor, cancellationToken);
        
        return largeObjectId;
    }

    private static async Task<uint> CreateLargeObjectAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        await using var createCmd = new NpgsqlCommand("SELECT lo_create(0)", connection, transaction);
        var createLargeObjectResult = await createCmd.ExecuteScalarAsync(cancellationToken) ?? throw new Exception("Unable to create large object");

        if (createLargeObjectResult is not uint largeObjectId)
        {
            throw new Exception("Unable to create large object");
        }
        return largeObjectId;
    }

    private static async Task<int> OpenLargeObjectAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, uint largeObjectId, CancellationToken cancellationToken)
    {
        // Open the large object for writing (INV_WRITE = 0x20000)
        await using var openCommand = new NpgsqlCommand("SELECT lo_open($1, 131072)", connection, transaction);
        openCommand.Parameters.AddWithValue(NpgsqlDbType.Oid, largeObjectId);
        var openLargeObjectResult = await openCommand.ExecuteScalarAsync(cancellationToken)!;

        if (openLargeObjectResult is not int fileDescriptor || fileDescriptor < 0)
        {
            throw new Exception("Unable to open large object");
        }

        return fileDescriptor;
    }

    private static async Task WriteToLargeObjectAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Stream content, int fileDescriptor, CancellationToken cancellationToken)
    {
        const int chunkSize = 8192;
        var buffer = new byte[chunkSize];
        int bytesRead;

        while ((bytesRead = await content.ReadAsync(buffer.AsMemory(0, chunkSize), cancellationToken)) > 0)
        {
            var chunk = bytesRead == chunkSize ? buffer : buffer[..bytesRead];

            await using var writeCommand = new NpgsqlCommand("SELECT lowrite($1, $2)", connection, transaction);
            writeCommand.Parameters.AddWithValue(fileDescriptor);
            writeCommand.Parameters.AddWithValue(chunk);
            await writeCommand.ExecuteScalarAsync(cancellationToken);
        }
    }

    private static async Task CloseLargeObjectAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int fileDescriptor, CancellationToken cancellationToken)
    {
        await using var closeCommand = new NpgsqlCommand("SELECT lo_close($1)", connection, transaction);
        closeCommand.Parameters.AddWithValue(fileDescriptor);
        await closeCommand.ExecuteScalarAsync(cancellationToken);
    }

    private async Task<SchoolBulkImportRequest> SaveNewSchoolBulkImportRequestAsync(uint largeObjectId, CancellationToken cancellationToken)
    {
        var schoolBulkImportRequest = new SchoolBulkImportRequest() { ContentId = largeObjectId };
        context.Add(schoolBulkImportRequest);
        await context.SaveChangesAsync(cancellationToken);
        return schoolBulkImportRequest;
    }
}