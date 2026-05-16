namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing.LookupImporter;

internal abstract class LookupImporterBase<TEntity, TContext>(TContext context)
    where TEntity : class
    where TContext : DbContext
{
    private readonly TContext _context = context;
    private readonly Dictionary<string, TEntity> _cache = new(StringComparer.OrdinalIgnoreCase);
    public async Task<TEntity> UpsertAsync(string code, string name, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(code, out var cached))
        {
            return cached;
        }

        var entity = await FindByCodeAsync(_context, code, cancellationToken);

        if (entity is null)
        {
            entity = Create(code, name);
            await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        }
        else
        {
            UpdateName(entity, name);
        }

        _cache[code] = entity;
        return entity;
    }

    protected abstract Task<TEntity?> FindByCodeAsync(TContext context, string code, CancellationToken cancellationToken);
    protected abstract TEntity Create(string code, string name);
    protected abstract void UpdateName(TEntity entity, string name);
}