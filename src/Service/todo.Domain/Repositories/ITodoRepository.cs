using task_crud.Domain.Entities;

namespace task_crud.Domain.Repositories
{
    public interface ITodoRepository
    {
        Task<Todo> GetById(int id);
        Task<IEnumerable<Todo>> GetByUserId(int id);
        Task<IQueryable<Todo>> GetAll(int? page, int? pageSize, string? title, string? sort, string? order);
        Task Update(Todo taskItem);
        Task Delete(int id);
        Task CreateRange(IEnumerable<Todo> taskItems);
    }
}
