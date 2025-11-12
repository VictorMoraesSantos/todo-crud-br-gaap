using task_crud.Application.DTOs;
using task_crud.Domain.Entities;

namespace task_crud.Application.Mapping
{
    public static class TodoMapper
    {
        public static TodoDTO? ToDTO(Todo? todo)
        {
            if (todo == null) return null;

            var dto = new TodoDTO(
                todo.Id,
                todo.UserId,
                todo.Title,
                todo.Completed);

            return dto;
        }

        public static Todo ToEntity(TodoDTO? todoDTO)
        {
            var entity = new Todo(
                todoDTO.UserId,
                todoDTO.Title,
                todoDTO.Completed);

            return entity;
        }
    }
}
