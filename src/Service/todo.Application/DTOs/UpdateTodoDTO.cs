namespace task_crud.Application.DTOs
{
    public record UpdateTodoDTO(
        int UserId,
        string Title,
        bool Completed);
}
