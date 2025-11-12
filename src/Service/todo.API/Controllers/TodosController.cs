using Microsoft.AspNetCore.Mvc;
using task_crud.Application.Contracts;
using task_crud.Application.DTOs;
using todo.Domain.Exception;

namespace task_crud.API.Controllers
{
    [ApiController]
    [Route("api/todos")]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _service;

        public TodosController(ITodoService service)
        {
            _service = service;
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
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCompleted([FromRoute] int id, [FromBody] UpdateTodoDTO taskItem)
        {
            try
            {

                var result = await _service.UpdateAsync(id, taskItem);
                return NoContent();
            }
            catch (DomainException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("sync")]
        public async Task<IActionResult> Sync()
        {
            await _service.SyncAsync();
            return NoContent();
        }
    }
}
