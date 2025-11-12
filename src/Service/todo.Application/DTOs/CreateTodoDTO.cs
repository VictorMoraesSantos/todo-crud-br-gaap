namespace task_crud.Application.DTOs
{
    public record CreateTodoDTO(
        int Id,
        int UserId,
        string Title,
        bool Completed);
}
