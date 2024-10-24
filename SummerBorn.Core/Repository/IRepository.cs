namespace SummerBorn.Core.Repository;

public interface IRepository<T> where T : class, IEntity
{
    public IQueryable<T> GetAll();
    public Task<T?> GetById(Guid id);
    public Task Add(T entity);
    public Task Update(T entity);
}
