using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using task_crud.Application.Contracts;
using task_crud.Application.DTOs;

namespace task_crud.API.Controllers
{
    [ApiController]
    [Route("todos")]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _service;
        private readonly IHttpClientFactory _httpClientFactory;

        public TodosController(ITodoService service, IHttpClientFactory httpClientFactory)
        {
            _service = service;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetTodosQuery query)
        {
            var result = await _service.GetAllAsync(query);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var item = await _service.GetByIdAsync(id);
            return Ok(item);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCompleted([FromRoute] int id, [FromBody] UpdateTodoDTO taskItem)
        {
            var result = await _service.UpdateAsync(id, taskItem);
            return result ? NoContent() : NotFound();
        }

        [HttpPost("sync")]
        public async Task<IActionResult> Sync()
        {
            var client = _httpClientFactory.CreateClient();
            var resp = await client.GetAsync("https://jsonplaceholder.typicode.com/todos");
            if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode);

            using var stream = await resp.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var external = await JsonSerializer.DeserializeAsync<IEnumerable<ExternalTodo>>(stream, options);
            if (external == null) return NoContent();

            var createDtos = external.Select(e => new CreateTodoDTO(e.id, e.userId, e.title ?? string.Empty, e.completed));
            await _service.CreateRangeAsync(createDtos);

            return NoContent();
        }

        private record ExternalTodo(int userId, int id, string title, bool completed);
    }
}
