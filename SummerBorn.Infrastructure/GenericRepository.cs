namespace SummerBorn.Infrastructure;
public class GenericRepository<T>(SchoolContext schoolContext) : IRepository<T> where T : class, IEntity
{
    private SchoolContext SchoolContext { get; } = schoolContext;

    public async Task Add(T entity)
    {
        SchoolContext.Set<T>().Add(entity);
        await SchoolContext.SaveChangesAsync();

    }
    public IQueryable<T> GetAll()
    {
        return SchoolContext.Set<T>().AsNoTracking();
    }

    public async Task<T?> GetById(Guid id)
    {
        return await SchoolContext.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task Update(T entity)
    {
        SchoolContext.Set<T>().Update(entity);
        await SchoolContext.SaveChangesAsync();
    }
}
