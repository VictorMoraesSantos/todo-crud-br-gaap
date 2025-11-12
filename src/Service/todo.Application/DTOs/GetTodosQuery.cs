namespace task_crud.Application.DTOs
{
    public record GetTodosQuery(
        int Page = 1,
        int PageSize = 10,
        string? Title = null,
        string? Sort = null,
        string? Order = "asc");
}