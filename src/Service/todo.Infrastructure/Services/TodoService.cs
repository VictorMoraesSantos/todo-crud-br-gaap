using Microsoft.EntityFrameworkCore;
using task_crud.Application.Contracts;
using task_crud.Application.DTOs;
using task_crud.Application.Mapping;
using task_crud.Domain.Entities;
using task_crud.Domain.Repositories;

namespace task_crud.Infrastructure.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _repository;

        public TodoService(ITodoRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(CreateTodoDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = TodoMapper.ToEntity(dto);
            await _repository.Create(entity);
            return entity.Id;
        }

        public async Task CreateRangeAsync(IEnumerable<CreateTodoDTO> dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entities = dto.Select(d => TodoMapper.ToEntity(d));
            await _repository.CreateRange(entities);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.Delete(id);
        }

        public async Task<PagedResult<TodoDTO>> GetAllAsync(GetTodosQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var totalCount = await _repository.GetCount(query.Title);
            var items = await _repository.GetAll(query.Page, query.PageSize, query.Title, query.Sort, query.Order);
            var pagedItems = await items.ToListAsync();
            var dtos = pagedItems.Select(TodoMapper.ToDTO).ToList();

            return new PagedResult<TodoDTO>(dtos, totalCount, query.Page, query.PageSize);
        }

        public async Task<TodoDTO> GetByIdAsync(int id)
        {
            var entity = await _repository.GetById(id);
            return TodoMapper.ToDTO(entity);
        }

        public async Task<bool> UpdateAsync(int id, UpdateTodoDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = await _repository.GetById(id);
            if (entity == null) return false;

            entity.Update(dto.UserId, dto.Title, dto.Completed);
            await _repository.Update(entity);
            return true;
        }
    }
}
