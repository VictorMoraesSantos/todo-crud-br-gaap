using task_crud.Application.DTOs;
using task_crud.Domain.Entities;

namespace task_crud.Application.Mapping
{
    public static class TodoMapper
    {
        public static TodoDTO ToDTO(Todo todo)
        {
            if (todo == null) return null;

            var dto = new TodoDTO(
                todo.Id,
                todo.UserId,
                todo.Title,
                todo.Completed);

            return dto;
        }

        public static Todo ToEntity(TodoDTO todoDTO)
        {
            if (todoDTO == null) return null;

            var entity = new Todo(
                todoDTO.UserId,
                todoDTO.Title,
                todoDTO.Completed);

            try
            {
                var idProp = typeof(Todo).GetProperty("Id");
                if (idProp != null && idProp.CanWrite)
                {
                    idProp.SetValue(entity, todoDTO.Id);
                }
            }
            catch
            {
                // ignore
            }

            return entity;
        }

        public static Todo ToEntity(CreateTodoDTO dto)
        {
            if (dto == null) return null;

            var entity = new Todo(dto.UserId, dto.Title, dto.Completed);
            try
            {
                var idProp = typeof(Todo).GetProperty("Id");
                if (idProp != null && idProp.CanWrite)
                {
                    idProp.SetValue(entity, dto.Id);
                }
            }
            catch
            {
                // ignore
            }

            return entity;
        }
    }
}
