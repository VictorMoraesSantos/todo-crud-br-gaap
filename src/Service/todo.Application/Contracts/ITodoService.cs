using task_crud.Application.DTOs;

namespace task_crud.Application.Contracts
{
    public interface ITodoService
    {
        Task<TodoDTO?> GetByIdAsync(int id);
        Task<PagedResult<TodoDTO>> GetAllAsync(GetTodosQuery query);
        Task<bool> UpdateAsync(int id, UpdateTodoDTO dto);
        Task CreateRangeAsync(IEnumerable<TodoDTO> dto);
        Task SyncAsync();
    }
}
