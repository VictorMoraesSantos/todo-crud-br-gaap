using Microsoft.EntityFrameworkCore;
using task_crud.Application.Contracts;
using task_crud.Application.DTOs;
using task_crud.Application.Mapping;
using task_crud.Domain.Repositories;
using System.Text.Json;
using System.Linq;
using System.Net.Http;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using todo.Domain.Exception;

namespace task_crud.Infrastructure.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _repository;

        public TodoService(ITodoRepository repository)
        {
            _repository = repository;
        }

        public async Task<TodoDTO?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetById(id);
            if (entity == null) return null;
            return TodoMapper.ToDTO(entity);
        }

        public async Task<PagedResult<TodoDTO>> GetAllAsync(GetTodosQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var items = await _repository.GetAll(query.Page, query.PageSize, query.Title, query.Sort, query.Order);
            var pagedItems = await items.ToListAsync();
            var dtos = pagedItems.Select(TodoMapper.ToDTO).Where(d => d != null).Select(d => d!).ToList();

            return new PagedResult<TodoDTO>(dtos, query.Page, query.PageSize);
        }

        public async Task CreateRangeAsync(IEnumerable<TodoDTO> dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entities = dto.Select(d =>
            {
                if (d == null) throw new ArgumentNullException(nameof(d));
                if (string.IsNullOrWhiteSpace(d.Title)) throw new ArgumentException("Title is required.", nameof(d));
                return TodoMapper.ToEntity(d);
            });

            await _repository.CreateRange(entities);
        }

        public async Task<bool> UpdateAsync(int id, UpdateTodoDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!dto.Completed)
            {
                var existing = await _repository.GetByUserId(dto.UserId);
                var count = existing.Count(e => e.Completed);
                if (count > 5)
                    throw new DomainException("O máximo de tarefas incompletas por usuário é 5.");
            }

            var entity = await _repository.GetById(id);
            if (entity == null) return false;

            entity.Update(dto.UserId, dto.Title, dto.Completed);
            await _repository.Update(entity);
            return true;
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.Delete(id);
        }

        public async Task SyncAsync()
        {
            using var client = new HttpClient();

            var resp = await client.GetAsync("https://jsonplaceholder.typicode.com/todos");
            if (!resp.IsSuccessStatusCode) return;

            await using var stream = await resp.Content.ReadAsStreamAsync();

            var dtos = await JsonSerializer.DeserializeAsync<IEnumerable<TodoDTO?>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dtos == null) return;

            var validDtos = dtos
                .Where(d => d != null)
                .ToList();

            if (validDtos.Count == 0) return;

            await CreateRangeAsync(validDtos);
        }
    }
}
