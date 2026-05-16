namespace SummerBornInfo.Infrastructure.Persistence.LargeObjects;

internal sealed class LargeObjectStream(NpgsqlConnection connection, uint largeObjectId) : Stream
{
    private readonly NpgsqlConnection _connection = connection;
    private readonly uint _largeObjectId = largeObjectId;
    private long _position;
    private long? _length;
    private const int ChunkSize = 1024 * 64; // 64KB chunks

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;

    public override long Length
    {
        get
        {
            _length ??= GetLength();
            return _length.Value;
        }
    }

    private long GetLength()
    {
        if (_length.HasValue)
        {
            return _length.Value;
        }

        using NpgsqlCommand cmd = new(
            "SELECT COALESCE(SUM(length(data)), 0) FROM pg_largeobject WHERE loid = @oid",
            _connection);
        cmd.Parameters.AddWithValue("oid", NpgsqlDbType.Oid, _largeObjectId);
        _length = (long)cmd.ExecuteScalar()!;
        return _length.Value;
    }

    public async Task<long> GetLengthAsync(CancellationToken cancellationToken = default)
    {
        if (_length.HasValue)
        {
            return _length.Value;
        }

        await using NpgsqlCommand cmd = new(
            "SELECT COALESCE(SUM(length(data)), 0) FROM pg_largeobject WHERE loid = @oid",
            _connection);
        cmd.Parameters.AddWithValue("oid", NpgsqlDbType.Oid, _largeObjectId);
        _length = (long)(await cmd.ExecuteScalarAsync(cancellationToken))!;
        return _length.Value;
    }

    public override long Position
    {
        get => _position;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(Position));
            _position = value;
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        _position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new NotSupportedException($"Unknown SeekOrigin: {origin}"),
        };
        return _position;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        Memory<byte> readBuffer = new(buffer, offset, count);
        var bytesToRead = Math.Min(readBuffer.Length, ChunkSize);

        using NpgsqlCommand cmd = new("SELECT lo_get(@oid, @offset, @length)", _connection);
        cmd.Parameters.AddWithValue("oid", NpgsqlDbType.Oid, _largeObjectId);
        cmd.Parameters.AddWithValue("offset", NpgsqlDbType.Bigint, _position);
        cmd.Parameters.AddWithValue("length", NpgsqlDbType.Integer, bytesToRead);

        var chunk = (byte[]?)cmd.ExecuteScalar();
        if (chunk == null || chunk.Length == 0)
        {
            return 0;
        }

        chunk.AsMemory().CopyTo(readBuffer);
        _position += chunk.Length;
        return chunk.Length;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var bytesToRead = Math.Min(buffer.Length, ChunkSize);

        await using NpgsqlCommand cmd = new("SELECT lo_get(@oid, @offset, @length)", _connection);
        cmd.Parameters.AddWithValue("oid", NpgsqlDbType.Oid, _largeObjectId);
        cmd.Parameters.AddWithValue("offset", NpgsqlDbType.Bigint, _position);
        cmd.Parameters.AddWithValue("length", NpgsqlDbType.Integer, bytesToRead);

        var chunk = (byte[]?)await cmd.ExecuteScalarAsync(cancellationToken);

        if (chunk == null || chunk.Length == 0)
        {
            return 0;
        }

        chunk.AsMemory().CopyTo(buffer);
        _position += chunk.Length;
        return chunk.Length;
    }
    public override void Flush() { }

    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();
}
