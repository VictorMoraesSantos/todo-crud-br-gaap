using Microsoft.EntityFrameworkCore;
using task_crud.Domain.Entities;
using task_crud.Domain.Repositories;
using task_crud.Infrastructure.Data;

namespace task_crud.Infrastructure.Repository
{
    public class TodoRepository : ITodoRepository
    {
        private readonly ApplicationDbContext _context;

        public TodoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateRange(IEnumerable<Todo> taskItems)
        {
            await _context.Todos.AddRangeAsync(taskItems);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var entity = await _context.Todos.FindAsync(id);
            if (entity == null) return;

            _context.Todos.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IQueryable<Todo>> GetAll(int? page, int? pageSize, string? title, string? sort, string? order)
        {
            var query = _context.Todos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(i => i.Title != null && i.Title.ToLower().Contains(title.ToLower()));

            if (!string.IsNullOrWhiteSpace(sort))
            {
                if (sort.ToLower().Equals("title"))
                    query = order?.ToLower().Equals("desc") == true
                        ? query.OrderByDescending(i => i.Title)
                        : query.OrderBy(i => i.Title);

                if (sort.ToLower().Equals("id"))
                    query = order?.ToLower().Equals("desc") == true
                        ? query.OrderByDescending(i => i.Id)
                        : query.OrderBy(i => i.Id);

                if (sort.ToLower().Equals("userid"))
                    query = order?.ToLower().Equals("desc") == true
                        ? query.OrderByDescending(i => i.UserId)
                        : query.OrderBy(i => i.UserId);
            }

            if (page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0)
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);

            return query;
        }

        public async Task<Todo> GetById(int id)
        {
            return await _context.Todos.FindAsync(id);
        }

        public async Task<IEnumerable<Todo>> GetByUserId(int id)
        {
            return await _context.Todos.AsNoTracking().Where(t => t.UserId == id).ToListAsync();
        }

        public async Task Update(Todo taskItem)
        {
            _context.Todos.Update(taskItem);
            await _context.SaveChangesAsync();
        }
    }
}
