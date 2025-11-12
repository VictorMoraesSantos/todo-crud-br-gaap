namespace task_crud.Application.DTOs
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PagedResult(IEnumerable<T> items, int page, int pageSize)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
        }
    }
}