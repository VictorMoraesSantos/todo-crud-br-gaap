namespace task_crud.Application.DTOs
{
    public record TodoDTO(
        int Id,
        int UserId,
        string Title,
        bool Completed);
}
